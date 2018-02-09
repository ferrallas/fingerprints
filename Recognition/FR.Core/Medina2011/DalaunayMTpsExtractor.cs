/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2011
{
    public class DalaunayMTpsExtractor
    {
        public static MtripletsFeature ExtractFeatures(List<Minutia> minutiae)
        {
            var mtriplets = new List<MTriplet>();
            var triplets = new Dictionary<int, int>();

            foreach (var triangle in Delaunay2D.Triangulate(minutiae))
            {
                var idxArr = new[]
                {
                    (short) triangle.A,
                    (short) triangle.B,
                    (short) triangle.C
                };
                var newMTriplet = new MTriplet(idxArr, minutiae);
                var newHash = newMTriplet.GetHashCode();
                if (!triplets.ContainsKey(newHash))
                {
                    triplets.Add(newHash, 0);
                    mtriplets.Add(newMTriplet);
                }
            }

            mtriplets.TrimExcess();
            return new MtripletsFeature(mtriplets, minutiae);
        }
    }
}