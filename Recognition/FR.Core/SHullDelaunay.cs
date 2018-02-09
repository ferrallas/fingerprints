/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System.Collections.Generic;
using PatternRecognition.FingerprintRecognition.Core.SHullDelaunayTriangulation;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public static class SHullDelaunay
    {
        private static readonly SHullTriangulator triangulator = new SHullTriangulator();

        public static List<Minutia[]> Triangulate(List<Minutia> minutiae, out List<int[]> triplets)
        {
            var points = new List<Vertex>();
            foreach (var minutia in minutiae)
                points.Add(new Vertex(minutia.X, minutia.Y));

            var triangles = triangulator.Triangulation(points, true);

            var mTriplets = new List<Minutia[]>(minutiae.Count);
            triplets = new List<int[]>(minutiae.Count);
            foreach (var triangle in triangles)
            {
                mTriplets.Add(new[] {minutiae[triangle.a], minutiae[triangle.b], minutiae[triangle.c]});
                triplets.Add(new[] {triangle.a, triangle.b, triangle.c});
            }

            return mTriplets;
        }
    }
}