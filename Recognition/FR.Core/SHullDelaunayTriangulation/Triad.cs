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
        public int a, b, c;
        public int ab, bc, ac; // adjacent edges index to neighbouring triangle.

        // Position and radius squared of circumcircle
        public float circumcircleR2, circumcircleX, circumcircleY;

        public Triad(int x, int y, int z)
        {
            a = x;
            b = y;
            c = z;
            ab = -1;
            bc = -1;
            ac = -1;
            circumcircleR2 = -1;
            x = 0;
            y = 0;
        }

        public void Initialize(int a, int b, int c, int ab, int bc, int ac, List<Vertex> points)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.ab = ab;
            this.bc = bc;
            this.ac = ac;

            FindCircumcirclePrecisely(points);
        }


        public void MakeClockwise(List<Vertex> points)
        {
            var centroidX = (points[a].x + points[b].x + points[c].x) / 3.0f;
            var centroidY = (points[a].y + points[b].y + points[c].y) / 3.0f;

            float dr0 = points[a].x - centroidX, dc0 = points[a].y - centroidY;
            float dx01 = points[b].x - points[a].x, dy01 = points[b].y - points[a].y;

            var df = -dx01 * dc0 + dy01 * dr0;
            if (df > 0)
            {
                // Need to swap vertices b<->c and edges ab<->bc
                var t = b;
                b = c;
                c = t;

                t = ab;
                ab = ac;
                ac = t;
            }
        }


        public bool FindCircumcirclePrecisely(List<Vertex> points)
        {
            // Use coordinates relative to point `a' of the triangle
            Vertex pa = points[a], pb = points[b], pc = points[c];

            double xba = pb.x - pa.x;
            double yba = pb.y - pa.y;
            double xca = pc.x - pa.x;
            double yca = pc.y - pa.y;

            // Squares of lengths of the edges incident to `a'
            var balength = xba * xba + yba * yba;
            var calength = xca * xca + yca * yca;

            // Calculate the denominator of the formulae. 
            var D = xba * yca - yba * xca;
            if (D == 0)
            {
                circumcircleX = 0;
                circumcircleY = 0;
                circumcircleR2 = -1;
                return false;
            }

            var denominator = 0.5 / D;

            // Calculate offset (from pa) of circumcenter
            var xC = (yca * balength - yba * calength) * denominator;
            var yC = (xba * calength - xca * balength) * denominator;

            var radius2 = xC * xC + yC * yC;
            if (radius2 > 1e10 * balength || radius2 > 1e10 * calength)
            {
                circumcircleX = 0;
                circumcircleY = 0;
                circumcircleR2 = -1;
                return false;
            }

            circumcircleR2 = (float) radius2;
            circumcircleX = (float) (pa.x + xC);
            circumcircleY = (float) (pa.y + yC);

            return true;
        }


        public bool InsideCircumcircle(Vertex p)
        {
            var dx = circumcircleX - p.x;
            var dy = circumcircleY - p.y;
            var r2 = dx * dx + dy * dy;
            return r2 < circumcircleR2;
        }


        public void ChangeAdjacentIndex(int fromIndex, int toIndex)
        {
            if (ab == fromIndex)
                ab = toIndex;
            else if (bc == fromIndex)
                bc = toIndex;
            else if (ac == fromIndex)
                ac = toIndex;
            else
                Debug.Assert(false);
        }


        public void FindAdjacency(int vertexIndex, int triangleIndex, out int indexOpposite, out int indexLeft,
            out int indexRight)
        {
            if (ab == triangleIndex)
            {
                indexOpposite = c;

                if (vertexIndex == a)
                {
                    indexLeft = ac;
                    indexRight = bc;
                }
                else
                {
                    indexLeft = bc;
                    indexRight = ac;
                }
            }
            else if (ac == triangleIndex)
            {
                indexOpposite = b;

                if (vertexIndex == a)
                {
                    indexLeft = ab;
                    indexRight = bc;
                }
                else
                {
                    indexLeft = bc;
                    indexRight = ab;
                }
            }
            else if (bc == triangleIndex)
            {
                indexOpposite = a;

                if (vertexIndex == b)
                {
                    indexLeft = ab;
                    indexRight = ac;
                }
                else
                {
                    indexLeft = ac;
                    indexRight = ab;
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
            return $"Triad vertices {a} {b} {c} ; edges {ab} {ac} {bc}";
        }
    }
}