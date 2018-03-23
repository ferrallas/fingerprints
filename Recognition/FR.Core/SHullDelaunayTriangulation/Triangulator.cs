using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

/*
  copyright s-hull.org 2011
  released under the contributors beerware license

 S-hull is free software and may be obtained from www.s-hull.org. It may be freely copied, modified, and redistributed under the following conditions which might loosely be termed a contribtors beerware license:

1. All copyright notices must remain intact in all files.

2. A copy of this text file must be distributed along with any copies of S-hull that you redistribute; this includes copies that you have modified, or copies of programs or other software products that include S-hull where distributed as source.

3. If you modify S-hull, you must include a notice giving the name of the person performing the modification, the date of modification, and the reason for such modification.

4. If you are distributing a binary or compiled version of s-hull it is not necessary to include any acknowledgement or reference to s-hull.

5. There is no warranty or other guarantee of fitness for S-hull, it is provided solely "as is". Bug reports or fixes may be sent to bugs@s-hull.org; the authors may or may not act on them as they desire.

6. By copying or compliing the code for S-hull you explicitly indemnify the copyright holder against any liability he may incur as a result of you copying the code.

7. If you meet any of the contributors to the code you used from s-hull.org in a pub or a bar, and you think the source code they contributed to is worth it, you can buy them a beer. If your principles run against beer a bacon-double-cheeseburger would do just as nicely or you could email david@s-hull.org and arrange to make a donation of 10 of your local currancy units to support s-hull.org.  
  
  contributors: Phil Atkin, Dr Sinclair.
*/
namespace PatternRecognition.FingerprintRecognition.Core.SHullDelaunayTriangulation
{
    public class SHullTriangulator
    {
        public float Fraction = 0.3f;
        private List<Vertex> _points;

        private void Analyse(List<Vertex> suppliedPoints, Hull hull, List<Triad> triads, bool rejectDuplicatePoints,
            bool hullOnly)
        {
            if (suppliedPoints.Count < 3)
                throw new ArgumentException("Number of points supplied must be >= 3");

            _points = suppliedPoints;
            var nump = _points.Count;

            var distance2ToCentre = new float[nump];
            var sortedIndices = new int[nump];

            // Choose first point as the seed
            for (var k = 0; k < nump; k++)
            {
                distance2ToCentre[k] = _points[0].Distance2To(_points[k]);
                sortedIndices[k] = k;
            }

            // Sort by distance to seed point
            Array.Sort(distance2ToCentre, sortedIndices);

            // Duplicates are more efficiently rejected now we have sorted the vertices
            if (rejectDuplicatePoints)
                for (var k = nump - 2; k >= 0; k--)
                    // If the points are identical then their distances will be the same,
                    // so they will be adjacent in the sorted list
                    if (_points[sortedIndices[k]].X == _points[sortedIndices[k + 1]].X &&
                        _points[sortedIndices[k]].Y == _points[sortedIndices[k + 1]].Y)
                    {
                        // Duplicates are expected to be rare, so this is not particularly efficient
                        Array.Copy(sortedIndices, k + 2, sortedIndices, k + 1, nump - k - 2);
                        Array.Copy(distance2ToCentre, k + 2, distance2ToCentre, k + 1, nump - k - 2);
                        nump--;
                    }

            Debug.WriteLine(_points.Count - nump + " duplicate points rejected");

            if (nump < 3)
                throw new ArgumentException("Number of unique points supplied must be >= 3");

            var mid = -1;
            float romin2 = float.MaxValue, circumCentreX = 0, circumCentreY = 0;

            // Find the point which, with the first two points, creates the triangle with the smallest circumcircle
            var tri = new Triad(sortedIndices[0], sortedIndices[1], 2);
            for (var kc = 2; kc < nump; kc++)
            {
                tri.C = sortedIndices[kc];
                if (tri.FindCircumcirclePrecisely(_points) && tri.CircumcircleR2 < romin2)
                {
                    mid = kc;
                    // Centre of the circumcentre of the seed triangle
                    romin2 = tri.CircumcircleR2;
                    circumCentreX = tri.CircumcircleX;
                    circumCentreY = tri.CircumcircleY;
                }
                else if (romin2 * 4 < distance2ToCentre[kc])
                {
                    break;
                }
            }

            // Change the indices, if necessary, to make the 2th point produce the smallest circumcircle with the 0th and 1th
            if (mid != 2)
            {
                var indexMid = sortedIndices[mid];
                var distance2Mid = distance2ToCentre[mid];

                Array.Copy(sortedIndices, 2, sortedIndices, 3, mid - 2);
                Array.Copy(distance2ToCentre, 2, distance2ToCentre, 3, mid - 2);
                sortedIndices[2] = indexMid;
                distance2ToCentre[2] = distance2Mid;
            }

            // These three points are our seed triangle
            tri.C = sortedIndices[2];
            tri.MakeClockwise(_points);
            tri.FindCircumcirclePrecisely(_points);

            // Add tri as the first triad, and the three points to the convex hull
            triads.Add(tri);
            hull.Add(new HullVertex(_points, tri.A));
            hull.Add(new HullVertex(_points, tri.B));
            hull.Add(new HullVertex(_points, tri.C));

            // Sort the remainder according to their distance from its centroid
            // Re-measure the points' distances from the centre of the circumcircle
            var centre = new Vertex(circumCentreX, circumCentreY);
            for (var k = 3; k < nump; k++)
                distance2ToCentre[k] = _points[sortedIndices[k]].Distance2To(centre);

            // Sort the _other_ points in order of distance to circumcentre
            Array.Sort(distance2ToCentre, sortedIndices, 3, nump - 3);

            // Add new points into hull (removing obscured ones from the chain)
            // and creating triangles....
            for (var k = 3; k < nump; k++)
            {
                var pointsIndex = sortedIndices[k];
                var ptx = new HullVertex(_points, pointsIndex);

                float dx = ptx.X - hull[0].X, dy = ptx.Y - hull[0].Y; // outwards pointing from hull[0] to pt.

                int numh = hull.Count;
                List<int> pidx = new List<int>(), tridx = new List<int>();
                int hidx; // new hull point location within hull.....

                if (hull.EdgeVisibleFrom(0, dx, dy))
                {
                    // starting with a visible hull facet !!!
                    hidx = 0;

                    // check to see if segment numh is also visible
                    if (hull.EdgeVisibleFrom(numh - 1, dx, dy))
                    {
                        // visible.
                        pidx.Add(hull[numh - 1].PointsIndex);
                        tridx.Add(hull[numh - 1].TriadIndex);

                        for (var h = 0; h < numh - 1; h++)
                        {
                            // if segment h is visible delete h
                            pidx.Add(hull[h].PointsIndex);
                            tridx.Add(hull[h].TriadIndex);
                            if (hull.EdgeVisibleFrom(h, ptx))
                            {
                                hull.RemoveAt(h);
                                h--;
                                numh--;
                            }
                            else
                            {
                                // quit on invisibility
                                hull.Insert(0, ptx);
                                numh++;
                                break;
                            }
                        }
                        // look backwards through the hull structure
                        for (var h = numh - 2; h > 0; h--)
                            // if segment h is visible delete h + 1
                            if (hull.EdgeVisibleFrom(h, ptx))
                            {
                                pidx.Insert(0, hull[h].PointsIndex);
                                tridx.Insert(0, hull[h].TriadIndex);
                                hull.RemoveAt(h + 1); // erase end of chain
                            }
                            else
                            {
                                break; // quit on invisibility
                            }
                    }
                    else
                    {
                        hidx = 1; // keep pt hull[0]
                        tridx.Add(hull[0].TriadIndex);
                        pidx.Add(hull[0].PointsIndex);

                        for (var h = 1; h < numh; h++)
                        {
                            // if segment h is visible delete h  
                            pidx.Add(hull[h].PointsIndex);
                            tridx.Add(hull[h].TriadIndex);
                            if (hull.EdgeVisibleFrom(h, ptx))
                            {
                                // visible
                                hull.RemoveAt(h);
                                h--;
                                numh--;
                            }
                            else
                            {
                                // quit on invisibility
                                hull.Insert(h, ptx);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    int e1 = -1, e2 = numh;
                    for (var h = 1; h < numh; h++)
                        if (hull.EdgeVisibleFrom(h, ptx))
                        {
                            if (e1 < 0)
                                e1 = h; // first visible
                        }
                        else
                        {
                            if (e1 > 0)
                            {
                                // first invisible segment.
                                e2 = h;
                                break;
                            }
                        }

                    // triangle pidx starts at e1 and ends at e2 (inclusive).	
                    if (e2 < numh)
                    {
                        for (var e = e1; e <= e2; e++)
                        {
                            pidx.Add(hull[e].PointsIndex);
                            tridx.Add(hull[e].TriadIndex);
                        }
                    }
                    else
                    {
                        for (var e = e1; e < e2; e++)
                        {
                            pidx.Add(hull[e].PointsIndex);
                            tridx.Add(hull[e].TriadIndex); // there are only n-1 triangles from n hull pts.
                        }
                        pidx.Add(hull[0].PointsIndex);
                    }

                    // erase elements e1+1 : e2-1 inclusive.
                    if (e1 < e2 - 1)
                        hull.RemoveRange(e1 + 1, e2 - e1 - 1);

                    // insert ptx at location e1+1.
                    hull.Insert(e1 + 1, ptx);
                    hidx = e1 + 1;
                }

                // If we're only computing the hull, we're done with this point
                if (hullOnly)
                    continue;

                int a = pointsIndex, t0;

                var npx = pidx.Count - 1;
                var numt = triads.Count;
                t0 = numt;

                for (var p = 0; p < npx; p++)
                {
                    var trx = new Triad(a, pidx[p], pidx[p + 1]);
                    trx.FindCircumcirclePrecisely(_points);

                    trx.Bc = tridx[p];
                    if (p > 0)
                        trx.Ab = numt - 1;
                    trx.Ac = numt + 1;

                    // index back into the triads.
                    var txx = triads[tridx[p]];
                    if ((trx.B == txx.A && trx.C == txx.B) | (trx.B == txx.B && trx.C == txx.A))
                        txx.Ab = numt;
                    else if ((trx.B == txx.A && trx.C == txx.C) | (trx.B == txx.C && trx.C == txx.A))
                        txx.Ac = numt;
                    else if ((trx.B == txx.B && trx.C == txx.C) | (trx.B == txx.C && trx.C == txx.B))
                        txx.Bc = numt;

                    triads.Add(trx);
                    numt++;
                }
                // Last edge is on the outside
                triads[numt - 1].Ac = -1;

                hull[hidx].TriadIndex = numt - 1;
                if (hidx > 0)
                {
                    hull[hidx - 1].TriadIndex = t0;
                }
                else
                {
                    numh = hull.Count;
                    hull[numh - 1].TriadIndex = t0;
                }
            }
        }


        public List<Vertex> ConvexHull(List<Vertex> points)
        {
            return ConvexHull(points, false);
        }


        public List<Vertex> ConvexHull(List<Vertex> points, bool rejectDuplicatePoints)
        {
            var hull = new Hull();
            var triads = new List<Triad>();

            Analyse(points, hull, triads, rejectDuplicatePoints, true);

            var hullVertices = new List<Vertex>();
            foreach (var hv in hull)
                hullVertices.Add(new Vertex(hv.X, hv.Y));

            return hullVertices;
        }


        public List<Triad> Triangulation(List<Vertex> points)
        {
            return Triangulation(points, false);
        }


        public List<Triad> Triangulation(List<Vertex> points, bool rejectDuplicatePoints)
        {
            var triads = new List<Triad>();
            var hull = new Hull();

            Analyse(points, hull, triads, rejectDuplicatePoints, false);

            // Now, need to flip any pairs of adjacent triangles not satisfying
            // the Delaunay criterion
            var numt = triads.Count;
            var idsA = new bool[numt];
            var idsB = new bool[numt];

            // We maintain a "list" of the triangles we've flipped in order to propogate any
            // consequent changes
            // When the number of changes is large, this is best maintained as a vector of bools
            // When the number becomes small, it's best maintained as a set
            // We switch between these regimes as the number flipped decreases
            var flipped = FlipTriangles(triads, idsA);

            var iterations = 1;
            while (flipped > (int) (Fraction * numt))
            {
                flipped = (iterations & 1) == 1 ? FlipTriangles(triads, idsA, idsB) : FlipTriangles(triads, idsB, idsA);

                iterations++;
            }

            Set<int> idSetA = new Set<int>(), idSetB = new Set<int>();
            flipped = FlipTriangles(triads,
                (iterations & 1) == 1 ? idsA : idsB, idSetA);

            iterations = 1;
            while (flipped > 0)
            {
                flipped = (iterations & 1) == 1 ? FlipTriangles(triads, idSetA, idSetB) : FlipTriangles(triads, idSetB, idSetA);

                iterations++;
            }

            return triads;
        }


        private bool FlipTriangle(IReadOnlyList<Triad> triads, int triadIndexToTest, out int triadIndexFlipped)
        {
            int oppositeVertex, edge1, edge2, edge3, edge4;
            triadIndexFlipped = 0;

            var tri = triads[triadIndexToTest];
            // test all 3 neighbours of tri 

            if (tri.Bc >= 0)
            {
                triadIndexFlipped = tri.Bc;
                var t2 = triads[triadIndexFlipped];
                // find relative orientation (shared limb).
                t2.FindAdjacency(tri.B, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (tri.InsideCircumcircle(_points[oppositeVertex]))
                {
                    // not valid in the Delaunay sense.
                    edge1 = tri.Ab;
                    edge2 = tri.Ac;
                    if (edge1 != edge3 && edge2 != edge4)
                    {
                        int tria = tri.A, trib = tri.B, tric = tri.C;
                        tri.Initialize(tria, trib, oppositeVertex, edge1, edge3, triadIndexFlipped, _points);
                        t2.Initialize(tria, tric, oppositeVertex, edge2, edge4, triadIndexToTest, _points);

                        // change knock on triangle labels.
                        if (edge3 >= 0)
                            triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
                        if (edge2 >= 0)
                            triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
                        return true;
                    }
                }
            }


            if (tri.Ab >= 0)
            {
                triadIndexFlipped = tri.Ab;
                var t2 = triads[triadIndexFlipped];
                // find relative orientation (shared limb).
                t2.FindAdjacency(tri.A, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (tri.InsideCircumcircle(_points[oppositeVertex]))
                {
                    // not valid in the Delaunay sense.
                    edge1 = tri.Ac;
                    edge2 = tri.Bc;
                    if (edge1 != edge3 && edge2 != edge4)
                    {
                        int tria = tri.A, trib = tri.B, tric = tri.C;
                        tri.Initialize(tric, tria, oppositeVertex, edge1, edge3, triadIndexFlipped, _points);
                        t2.Initialize(tric, trib, oppositeVertex, edge2, edge4, triadIndexToTest, _points);

                        // change knock on triangle labels.
                        if (edge3 >= 0)
                            triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
                        if (edge2 >= 0)
                            triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
                        return true;
                    }
                }
            }

            if (tri.Ac >= 0)
            {
                triadIndexFlipped = tri.Ac;
                var t2 = triads[triadIndexFlipped];
                // find relative orientation (shared limb).
                t2.FindAdjacency(tri.A, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (tri.InsideCircumcircle(_points[oppositeVertex]))
                {
                    // not valid in the Delaunay sense.
                    edge1 = tri.Ab; // .ac shared limb
                    edge2 = tri.Bc;
                    if (edge1 != edge3 && edge2 != edge4)
                    {
                        int tria = tri.A, trib = tri.B, tric = tri.C;
                        tri.Initialize(trib, tria, oppositeVertex, edge1, edge3, triadIndexFlipped, _points);
                        t2.Initialize(trib, tric, oppositeVertex, edge2, edge4, triadIndexToTest, _points);

                        // change knock on triangle labels.
                        if (edge3 >= 0)
                            triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
                        if (edge2 >= 0)
                            triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
                        return true;
                    }
                }
            }

            return false;
        }


        private int FlipTriangles(List<Triad> triads, bool[] idsFlipped)
        {
            var numt = triads.Count;
            Array.Clear(idsFlipped, 0, numt);

            var flipped = 0;
            for (var t = 0; t < numt; t++)
            {
                int t2;
                if (FlipTriangle(triads, t, out t2))
                {
                    flipped += 2;
                    idsFlipped[t] = true;
                    idsFlipped[t2] = true;
                }
            }

            return flipped;
        }

        private int FlipTriangles(List<Triad> triads, bool[] idsToTest, bool[] idsFlipped)
        {
            var numt = triads.Count;
            Array.Clear(idsFlipped, 0, numt);

            var flipped = 0;
            for (var t = 0; t < numt; t++)
                if (idsToTest[t])
                {
                    int t2;
                    if (FlipTriangle(triads, t, out t2))
                    {
                        flipped += 2;
                        idsFlipped[t] = true;
                        idsFlipped[t2] = true;
                    }
                }

            return flipped;
        }

        private int FlipTriangles(List<Triad> triads, bool[] idsToTest, Set<int> idsFlipped)
        {
            var numt = triads.Count;
            idsFlipped.Clear();

            var flipped = 0;
            for (var t = 0; t < numt; t++)
                if (idsToTest[t])
                {
                    int t2;
                    if (FlipTriangle(triads, t, out t2))
                    {
                        flipped += 2;
                        idsFlipped.Add(t);
                        idsFlipped.Add(t2);
                    }
                }

            return flipped;
        }

        private int FlipTriangles(List<Triad> triads, Set<int> idsToTest, Set<int> idsFlipped)
        {
            var flipped = 0;
            idsFlipped.Clear();

            foreach (var t in idsToTest)
            {
                int t2;
                if (FlipTriangle(triads, t, out t2))
                {
                    flipped += 2;
                    idsFlipped.Add(t);
                    idsFlipped.Add(t2);
                }
            }

            return flipped;
        }

        #region Debug verification routines: verify that triad adjacency and indeces are set correctly

#if DEBUG
        private void VerifyHullContains(Hull hull, int idA, int idB)
        {
            if (
                hull[0].PointsIndex == idA && hull[hull.Count - 1].PointsIndex == idB ||
                hull[0].PointsIndex == idB && hull[hull.Count - 1].PointsIndex == idA)
                return;

            for (var h = 0; h < hull.Count - 1; h++)
                if (hull[h].PointsIndex == idA)
                {
                    Debug.Assert(hull[h + 1].PointsIndex == idB);
                    return;
                }
                else if (hull[h].PointsIndex == idB)
                {
                    Debug.Assert(hull[h + 1].PointsIndex == idA);
                    return;
                }
        }

        private void VerifyTriadContains(Triad tri, int nbourTriad, int idA, int idB)
        {
            if (tri.Ab == nbourTriad)
                Debug.Assert(
                    tri.A == idA && tri.B == idB ||
                    tri.B == idA && tri.A == idB);
            else if (tri.Ac == nbourTriad)
                Debug.Assert(
                    tri.A == idA && tri.C == idB ||
                    tri.C == idA && tri.A == idB);
            else if (tri.Bc == nbourTriad)
                Debug.Assert(
                    tri.C == idA && tri.B == idB ||
                    tri.B == idA && tri.C == idB);
            else
                Debug.Assert(false);
        }

        private void VerifyTriads(List<Triad> triads, Hull hull)
        {
            for (var t = 0; t < triads.Count; t++)
            {
                if (t == 17840)
                    t = t + 0;

                var tri = triads[t];
                if (tri.Ac == -1)
                    VerifyHullContains(hull, tri.A, tri.C);
                else
                    VerifyTriadContains(triads[tri.Ac], t, tri.A, tri.C);

                if (tri.Ab == -1)
                    VerifyHullContains(hull, tri.A, tri.B);
                else
                    VerifyTriadContains(triads[tri.Ab], t, tri.A, tri.B);

                if (tri.Bc == -1)
                    VerifyHullContains(hull, tri.B, tri.C);
                else
                    VerifyTriadContains(triads[tri.Bc], t, tri.B, tri.C);
            }
        }

        private void WriteTriangles(List<Triad> triangles, string name)
        {
            using (var writer = new StreamWriter(name + ".dtt"))
            {
                writer.WriteLine(triangles.Count.ToString());
                for (var i = 0; i < triangles.Count; i++)
                {
                    var t = triangles[i];
                    writer.WriteLine($"{i + 1}: {t.A} {t.B} {t.C} - {t.Ab + 1} {t.Bc + 1} {t.Ac + 1}");
                }
            }
        }

#endif

        #endregion
    }
}