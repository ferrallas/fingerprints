// Delaunay 2D triangulation algorithm.
// Salvatore Previti. http://www.salvatorepreviti.it - info@salvatorepreviti.it
// Optimized implementation of Delaunay triangulation algorithm by Paul Bourke (pbourke@swin.edu.au)
// See http://astronomy.swin.edu.au/~pbourke/terrain/triangulate/ for details.
//
// You can use this code or parts of this code providing that above credit remain intact.
//

using System;
using System.Collections.Generic;
using System.Drawing;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public struct IntegerTriangle
    {
        public int A;


        public int B;


        public int C;
    }


    public static class Delaunay2D
    {
        #region Triangulate

        public static IntegerTriangle[] Triangulate(ICollection<Minutia> minutiae)
        {
            ICollection<PointF> pointsCollection = new List<PointF>(minutiae.Count);
            foreach (var minutia in minutiae)
                pointsCollection.Add(new PointF(minutia.X, minutia.Y));
            var edges = new DelaunayTriangulator();
            if (edges.Initialize(pointsCollection, 0.00001f))
                edges.Process();

            return edges.ToArray();
        }

        #endregion

        #region Private types

        private struct DelaunayTriangulator
        {
            #region Structures

            private struct Triangle
            {
                internal int A;
                internal int B;
                internal int C;

                internal float CircumCirclecenterX;
                internal float CircumCirclecenterY;
                internal float CircumCircleRadius;

                internal int Previous;
                internal int Next;

                internal int PrevNonCompleted;
                internal int NextNonCompleted;

                #region ToIntegerTriangle

                internal IntegerTriangle ToIntegerTriangle()
                {
                    IntegerTriangle result;
                    result.A = A;
                    result.B = B;
                    result.C = C;
                    return result;
                }

                internal void ToIntegerTriangle(ref IntegerTriangle destination)
                {
                    destination.A = A;
                    destination.B = B;
                    destination.C = C;
                }

                #endregion
            }

            private struct EdgeEntry
            {
                internal int Next;
                internal int A;
                internal int B;
                internal int Count;
            }

            private struct EdgeBucketEntry
            {
                internal int Generation;
                internal int EntryIndex;
            }

            #endregion

            #region Points fields

            private int _pointsCount;
            private float _tolerance;
            private PointF[] _points;
            private int[] _pointsIndices;

            #endregion

            #region Triangles Fields

            private Triangle[] _triangles;

            private int _trianglesLast;
            private int _trianglesCount;
            private int _trianglesFirst;

            private int _firstNonCompletedTriangle;
            private int _lastNonCompletedTriangle;

            private int _firstFreeTriangle;

            #endregion

            #region Edges Fields

            private EdgeBucketEntry[] _edgesBuckets;
            private EdgeEntry[] _edgesEntries;

            private int _edgesGeneration;
            private int _edgesCount;

            #endregion

            #region Initialize

            internal bool Initialize(ICollection<PointF> pointsCollection, float tolerance)
            {
                _points = null;
                _pointsIndices = null;
                _edgesBuckets = null;
                _edgesEntries = null;
                _triangles = null;
                _triangles = null;

                // Initialize triangle table

                _trianglesFirst = -1;
                _trianglesLast = -1;
                _trianglesCount = 0;

                _firstNonCompletedTriangle = -1;
                _lastNonCompletedTriangle = -1;

                // Initialize edge table

                _edgesGeneration = 1;
                _edgesCount = 0;

                this._tolerance = tolerance > 0 ? tolerance : float.Epsilon; // Ensure tolerance is valid

                _pointsCount = pointsCollection == null ? 0 : pointsCollection.Count;

                if (pointsCollection.Count < 3)
                    return false; // We need a non null collection with at least 3 vertices!

                // Create the array of points.
                // We need 3 more items to add supertriangle vertices

                _points = new PointF[_pointsCount + 3];
                pointsCollection.CopyTo(_points, 0);

                // Create an array of indices to points sorted by Y (firstly), X (secondly) and insertion order (thirdly)
                _pointsIndices = GetSortedPointIndices(_points, _pointsCount, tolerance);

                // Calculate min and max X and Y coomponents of points

                PointF pointsMin, pointsMax;
                var d = new PointF();

                GetMinMaxPointCoordinates(_points, _pointsCount, out pointsMin, out pointsMax);

                // Create supertriangle vertices
                d.X = pointsMax.X - pointsMin.X;
                d.Y = pointsMax.Y - pointsMin.Y;

                var dmax = d.X > d.Y ? d.X : d.Y;
                var mid = new PointF();
                mid.X = (pointsMax.X + pointsMin.X) * 0.5f;
                mid.Y = (pointsMax.Y + pointsMin.Y) * 0.5f;

                _points[_pointsCount] = new PointF(mid.X - 2 * dmax, mid.Y - dmax);
                _points[_pointsCount + 1] = new PointF(mid.X, mid.Y + 2 * dmax);
                _points[_pointsCount + 2] = new PointF(mid.X + 2 * dmax, mid.Y - dmax);

                // Initialize triangle array

                _triangles = new Triangle[_pointsCount * 4 + 1];

                var triangleEntry = new Triangle();
                triangleEntry.PrevNonCompleted = -1;
                triangleEntry.NextNonCompleted = -1;

                // Initialized free triangles

                _triangles = new Triangle[_triangles.Length];

                _firstFreeTriangle = 0;
                for (var i = 0; i < _triangles.Length; ++i)
                {
                    triangleEntry.Previous = i - 1;
                    triangleEntry.Next = i + 1;

                    _triangles[i] = triangleEntry;
                }
                _triangles[_triangles.Length - 1].Next = -1;

                // Initialize edge table

                var size = SpMath.GetPrime(_triangles.Length * 3 + 1);
                _edgesBuckets = new EdgeBucketEntry[size];
                _edgesEntries = new EdgeEntry[size];

                // Add supertriangle

                AddTriangle(_pointsCount, _pointsCount + 1, _pointsCount + 2);

                return true;
            }

            #endregion

            #region Process

            internal void Process()
            {
                // Process all sorted points

                float pointX = 0, pointY = 0;

                for (var sortedIndex = 0; sortedIndex < _pointsIndices.Length; ++sortedIndex)
                {
                    var pointIndex = _pointsIndices[sortedIndex];

                    var point = _points[pointIndex];

                    if (sortedIndex != 0 && Math.Abs(point.X - pointX) < _tolerance &&
                        Math.Abs(point.Y - pointY) < _tolerance)
                        continue; // Ignore current point if equals to previous point. We check equality using tolerance.

                    pointX = point.X;
                    pointY = point.Y;
                    var pointYplusTolerance = pointY + _tolerance;

                    // Check if triangle contains current point in its circumcenter.
                    // If yes, add triangle edges to edges table and remove triangle.
                    for (int nextNonCompleted, triangleIndex = _firstNonCompletedTriangle;
                        triangleIndex >= 0;
                        triangleIndex = nextNonCompleted)
                    {
                        // Calculate distance between triancle circumcircle center and current point
                        // Compare that distance with radius of triangle circumcircle
                        // If is less, it means that the point is inside of circumcircle, else, it means it is outside.

                        var circumCirclecenterX = _triangles[triangleIndex].CircumCirclecenterX;
                        var circumCirclecenterY = _triangles[triangleIndex].CircumCirclecenterY;
                        var circumCircleRadius = _triangles[triangleIndex].CircumCircleRadius;
                        nextNonCompleted = _triangles[triangleIndex].NextNonCompleted;

                        var dx = pointX - circumCirclecenterX;
                        var dy = pointY - circumCirclecenterY;

                        if (dx * dx + dy * dy <= circumCircleRadius)
                            ReplaceTriangleWithEdges(triangleIndex, ref _triangles[triangleIndex]);
                        else if (circumCirclecenterY < pointYplusTolerance && dy > circumCircleRadius + _tolerance)
                            MarkAsComplete(ref _triangles[triangleIndex]);
                    }

                    // Form new triangles for the current point
                    // Edges used more than once will be skipped
                    // Triangle vertices are arranged in clockwise order

                    for (var j = 0; j < _edgesCount; ++j)
                    {
                        var edge = _edgesEntries[j];
                        if (_edgesEntries[j].Count == 1)
                            AddTriangle(edge.A, edge.B, pointIndex);
                    }

                    // Clear edges table

                    ++_edgesGeneration;
                    _edgesCount = 0;
                }

                _firstNonCompletedTriangle = -1;

                // Count valid triangles (triangles that don't share vertices with supertriangle) and find the last triangle.

                _trianglesLast = _trianglesFirst;
                _trianglesCount = 0;
                if (_trianglesLast != -1)
                    for (;;)
                    {
                        var triangle = _triangles[_trianglesLast];

                        if (triangle.A < _pointsCount && triangle.B < _pointsCount && triangle.C < _pointsCount)
                            ++_trianglesCount;
                        else
                            _triangles[_trianglesLast].A = -1;

                        var next = _triangles[_trianglesLast].Next;
                        if (next == -1)
                            break;

                        _trianglesLast = next;
                    }
            }

            #endregion

            #region CopyTo, AddTo, ToArray

            private void CopyTo(IntegerTriangle[] array, int arrayIndex)
            {
                for (var triangleIndex = _trianglesLast;
                    triangleIndex >= 0;
                    triangleIndex = _triangles[triangleIndex].Previous)
                    if (_triangles[triangleIndex].A >= 0)
                        _triangles[triangleIndex].ToIntegerTriangle(ref array[arrayIndex++]);
            }

            internal IntegerTriangle[] ToArray()
            {
                var result = new IntegerTriangle[_trianglesCount];
                CopyTo(result, 0);
                return result;
            }

            #endregion

            #region Edges table

            private void ReplaceTriangleWithEdges(int triangleIndex, ref Triangle triangle)
            {
                // Remove triangle from linked list

                if (triangle.Next >= 0)
                    _triangles[triangle.Next].Previous = triangle.Previous;

                if (triangle.Previous >= 0)
                    _triangles[triangle.Previous].Next = triangle.Next;
                else
                    _trianglesFirst = triangle.Next;

                // Remove triangle from non completed linked list

                MarkAsComplete(ref triangle);

                // Add triangle to free triangles linked list

                triangle.Previous = -1;
                triangle.Next = _firstFreeTriangle;

                _triangles[_firstFreeTriangle].Previous = triangleIndex;

                _firstFreeTriangle = triangleIndex;

                // Add triangle edges to edges table

                AddEdge(triangle.A, triangle.B);
                AddEdge(triangle.B, triangle.C);
                AddEdge(triangle.C, triangle.A);
            }

            private void AddEdge(int edgeA, int edgeB)
            {
                EdgeEntry entry;

                // Calculate bucked index using an hashcode of edge indices.
                // Hashcode is generated so order of edges is ignored, it means, edge 1, 2 is equals to edge 2, 1
                var targetBucket = ((edgeA < edgeB ? (edgeA << 8) ^ edgeB : (edgeB << 8) ^ edgeA) & 0x7FFFFFFF) %
                                   _edgesBuckets.Length;

                if (_edgesBuckets[targetBucket].Generation != _edgesGeneration)
                {
                    // Bucket generation doesn't match current generation.
                    // This means this bucket is empty.
                    // Generations are incremented each time edge table is cleared.

                    // This entry is in the head of this bucket
                    entry.Next = -1;

                    // Store the new generation
                    _edgesBuckets[targetBucket].Generation = _edgesGeneration;
                }
                else
                {
                    var entryIndex = _edgesBuckets[targetBucket].EntryIndex;

                    for (var i = entryIndex; i >= 0; i = entry.Next)
                    {
                        entry = _edgesEntries[i];
                        if (entry.A == edgeA && entry.B == edgeB || entry.A == edgeB && entry.B == edgeA)
                        {
                            ++_edgesEntries[i].Count;
                            return;
                        }
                    }

                    entry.Next = entryIndex;
                }

                entry.A = edgeA;
                entry.B = edgeB;
                entry.Count = 1;

                _edgesEntries[_edgesCount] = entry;
                _edgesBuckets[targetBucket].EntryIndex = _edgesCount;
                ++_edgesCount;
            }

            #endregion

            #region Triangles lists

            private void AddTriangle(int a, int b, int c)
            {
                // Acquire the first free triangle

                var result = _firstFreeTriangle;
                _firstFreeTriangle = _triangles[result].Next;
                _triangles[_firstFreeTriangle].Previous = -1;

                Triangle triangle;

                // Insert the triangle into triangles linked list

                triangle.Previous = -1;
                triangle.Next = _trianglesFirst;

                if (_trianglesFirst != -1)
                    _triangles[_trianglesFirst].Previous = result;

                _trianglesFirst = result;

                // Insert the triangle into non completed triangles linked list

                triangle.PrevNonCompleted = _lastNonCompletedTriangle;
                triangle.NextNonCompleted = -1;

                if (_firstNonCompletedTriangle == -1)
                    _firstNonCompletedTriangle = result;
                else
                    _triangles[_lastNonCompletedTriangle].NextNonCompleted = result;

                _lastNonCompletedTriangle = result;

                // Store new entry
                //this.Triangles[result] = triangle;

                // Create the new triangle

                triangle.A = a;
                triangle.B = b;
                triangle.C = c;

                // Compute the circum circle of the new triangle

                var pA = _points[a];
                var pB = _points[b];
                var pC = _points[c];

                float m1, m2;
                float mx1, mx2;
                float my1, my2;
                float cX, cY;

                if (Math.Abs(pB.Y - pA.Y) < _tolerance)
                {
                    m2 = -(pC.X - pB.X) / (pC.Y - pB.Y);
                    mx2 = (pB.X + pC.X) * 0.5f;
                    my2 = (pB.Y + pC.Y) * 0.5f;

                    cX = (pB.X + pA.X) * 0.5f;
                    cY = m2 * (cX - mx2) + my2;
                }
                else
                {
                    m1 = -(pB.X - pA.X) / (pB.Y - pA.Y);
                    mx1 = (pA.X + pB.X) * 0.5f;
                    my1 = (pA.Y + pB.Y) * 0.5f;

                    if (Math.Abs(pC.Y - pB.Y) < _tolerance)
                    {
                        cX = (pC.X + pB.X) * 0.5f;
                        cY = m1 * (cX - mx1) + my1;
                    }
                    else
                    {
                        m2 = -(pC.X - pB.X) / (pC.Y - pB.Y);
                        mx2 = (pB.X + pC.X) * 0.5f;
                        my2 = (pB.Y + pC.Y) * 0.5f;

                        cX = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
                        cY = m1 * (cX - mx1) + my1;
                    }
                }

                triangle.CircumCirclecenterX = cX;
                triangle.CircumCirclecenterY = cY;

                // Calculate circumcircle radius

                mx1 = pB.X - cX;
                my1 = pB.Y - cY;
                triangle.CircumCircleRadius = mx1 * mx1 + my1 * my1;

                // Store the new triangle

                _triangles[result] = triangle;
            }

            private void MarkAsComplete(ref Triangle triangle)
            {
                // Remove triangle from non completed linked list

                if (triangle.NextNonCompleted >= 0)
                    _triangles[triangle.NextNonCompleted].PrevNonCompleted = triangle.PrevNonCompleted;
                else
                    _lastNonCompletedTriangle = triangle.PrevNonCompleted;

                if (triangle.PrevNonCompleted >= 0)
                    _triangles[triangle.PrevNonCompleted].NextNonCompleted = triangle.NextNonCompleted;
                else
                    _firstNonCompletedTriangle = triangle.NextNonCompleted;
            }

            #endregion

            #region Static functions

            private static void GetMinMaxPointCoordinates(PointF[] points, int count, out PointF min, out PointF max)
            {
                if (count <= 0)
                    throw new InvalidOperationException("Array cannot be empty");

                min = points[0];
                max = points[0];

                for (var i = 1; i < count; ++i)
                {
                    var v = points[i];
                    if (v.X > max.X)
                        max.X = v.X;
                    else if (v.X < min.X)
                        min.X = v.X;
                    if (v.Y > max.Y)
                        max.Y = v.Y;
                    else if (v.Y < min.Y)
                        min.Y = v.Y;
                }
            }


            private static int[] GetSortedPointIndices(PointF[] points, int count, float tolerance)
            {
                var result = new int[count];

                // Store index in indices

                for (var i = 0; i < result.Length; ++i)
                    result[i] = i;

                // Sort indices by Y (firstly), X (secondly) and insertion order (thirdly)

                Array.Sort(result, delegate(int a, int b)
                {
                    var va = points[a];
                    var vb = points[b];

                    var f = va.Y - vb.Y;

                    if (f > tolerance)
                        return +1;
                    if (f < -tolerance)
                        return -1;

                    f = va.X - vb.X;

                    if (f > tolerance)
                        return +1;
                    if (f < -tolerance)
                        return -1;

                    return a - b;
                });

                return result;
            }

            #endregion
        }

        #endregion
    }

    internal static class SpMath
    {
        #region Prime numbers

        internal static readonly int[] Primes =
        {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
        };

        internal static int GetPrime(int min)
        {
            if (min < 0)
                return min;

            for (var i = 0; i < Primes.Length; i++)
            {
                var prime = Primes[i];
                if (prime >= min)
                    return prime;
            }

            //outside of our predefined table. 
            //compute the hard way.
            for (var i = min | 1; i < int.MaxValue; i += 2)
            {
                if ((i & 1) != 0)
                {
                    var limit = (int) Math.Sqrt(i);
                    for (var divisor = 3; divisor <= limit; divisor += 2)
                        if (i % divisor == 0)
                            continue;

                    return i;
                }

                if (i == 2)
                    return i;
            }

            return min;
        }

        #endregion
    }
}