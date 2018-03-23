using System.Collections.Generic;
using System.Diagnostics;

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
    public class Triad
    {
        public int A, B, C;
        public int Ab, Bc, Ac; // adjacent edges index to neighbouring triangle.

        // Position and radius squared of circumcircle
        public float CircumcircleR2, CircumcircleX, CircumcircleY;

        public Triad(int x, int y, int z)
        {
            A = x;
            B = y;
            C = z;
            Ab = -1;
            Bc = -1;
            Ac = -1;
            CircumcircleR2 = -1;
        }

        public void Initialize(int a, int b, int c, int ab, int bc, int ac, List<Vertex> points)
        {
            this.A = a;
            this.B = b;
            this.C = c;
            this.Ab = ab;
            this.Bc = bc;
            this.Ac = ac;

            FindCircumcirclePrecisely(points);
        }


        public void MakeClockwise(List<Vertex> points)
        {
            var centroidX = (points[A].X + points[B].X + points[C].X) / 3.0f;
            var centroidY = (points[A].Y + points[B].Y + points[C].Y) / 3.0f;

            float dr0 = points[A].X - centroidX, dc0 = points[A].Y - centroidY;
            float dx01 = points[B].X - points[A].X, dy01 = points[B].Y - points[A].Y;

            var df = -dx01 * dc0 + dy01 * dr0;
            if (df > 0)
            {
                // Need to swap vertices b<->c and edges ab<->bc
                var t = B;
                B = C;
                C = t;

                t = Ab;
                Ab = Ac;
                Ac = t;
            }
        }


        public bool FindCircumcirclePrecisely(List<Vertex> points)
        {
            // Use coordinates relative to point `a' of the triangle
            Vertex pa = points[A], pb = points[B], pc = points[C];

            double xba = pb.X - pa.X;
            double yba = pb.Y - pa.Y;
            double xca = pc.X - pa.X;
            double yca = pc.Y - pa.Y;

            // Squares of lengths of the edges incident to `a'
            var balength = xba * xba + yba * yba;
            var calength = xca * xca + yca * yca;

            // Calculate the denominator of the formulae. 
            var d = xba * yca - yba * xca;
            if (d == 0)
            {
                CircumcircleX = 0;
                CircumcircleY = 0;
                CircumcircleR2 = -1;
                return false;
            }

            var denominator = 0.5 / d;

            // Calculate offset (from pa) of circumcenter
            var xC = (yca * balength - yba * calength) * denominator;
            var yC = (xba * calength - xca * balength) * denominator;

            var radius2 = xC * xC + yC * yC;
            if (radius2 > 1e10 * balength || radius2 > 1e10 * calength)
            {
                CircumcircleX = 0;
                CircumcircleY = 0;
                CircumcircleR2 = -1;
                return false;
            }

            CircumcircleR2 = (float) radius2;
            CircumcircleX = (float) (pa.X + xC);
            CircumcircleY = (float) (pa.Y + yC);

            return true;
        }


        public bool InsideCircumcircle(Vertex p)
        {
            var dx = CircumcircleX - p.X;
            var dy = CircumcircleY - p.Y;
            var r2 = dx * dx + dy * dy;
            return r2 < CircumcircleR2;
        }


        public void ChangeAdjacentIndex(int fromIndex, int toIndex)
        {
            if (Ab == fromIndex)
                Ab = toIndex;
            else if (Bc == fromIndex)
                Bc = toIndex;
            else if (Ac == fromIndex)
                Ac = toIndex;
            else
                Debug.Assert(false);
        }


        public void FindAdjacency(int vertexIndex, int triangleIndex, out int indexOpposite, out int indexLeft,
            out int indexRight)
        {
            if (Ab == triangleIndex)
            {
                indexOpposite = C;

                if (vertexIndex == A)
                {
                    indexLeft = Ac;
                    indexRight = Bc;
                }
                else
                {
                    indexLeft = Bc;
                    indexRight = Ac;
                }
            }
            else if (Ac == triangleIndex)
            {
                indexOpposite = B;

                if (vertexIndex == A)
                {
                    indexLeft = Ab;
                    indexRight = Bc;
                }
                else
                {
                    indexLeft = Bc;
                    indexRight = Ab;
                }
            }
            else if (Bc == triangleIndex)
            {
                indexOpposite = A;

                if (vertexIndex == B)
                {
                    indexLeft = Ab;
                    indexRight = Ac;
                }
                else
                {
                    indexLeft = Ac;
                    indexRight = Ab;
                }
            }
            else
            {
                Debug.Assert(false);
                indexOpposite = indexLeft = indexRight = 0;
            }
        }

        public override string ToString()
        {
            return $"Triad vertices {A} {B} {C} ; edges {Ab} {Ac} {Bc}";
        }
    }
}