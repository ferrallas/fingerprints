/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Parziale2004
{
    public class PnFeatureExtractor
    {
        public static PnFeatures ExtractFeatures(List<Minutia> minutiae)
        {
            var result = new List<MtiaTriplet>();
            if (minutiae.Count > 3)
                foreach (var triangle in Delaunay2D.Triangulate(minutiae))
                {
                    var idxArr = new[]
                    {
                        (short) triangle.A,
                        (short) triangle.B,
                        (short) triangle.C
                    };
                    var newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle.A,
                        (short) triangle.C,
                        (short) triangle.B
                    };
                    newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle.B,
                        (short) triangle.A,
                        (short) triangle.C
                    };
                    newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle.B,
                        (short) triangle.C,
                        (short) triangle.A
                    };
                    newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle.C,
                        (short) triangle.A,
                        (short) triangle.B
                    };
                    newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle.C,
                        (short) triangle.B,
                        (short) triangle.A
                    };
                    newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);
                }
            result.TrimExcess();
            return new PnFeatures(result, minutiae);
        }

        public PnFeatures ExtractFeatures1(List<Minutia> minutiae)
        {
            var result = new List<MtiaTriplet>();
            if (minutiae.Count > 3)
            {
                List<int[]> triplets;
                SHullDelaunay.Triangulate(minutiae, out triplets);
                foreach (var triangle in triplets)
                {
                    var idxArr = new[]
                    {
                        (short) triangle[0],
                        (short) triangle[1],
                        (short) triangle[2]
                    };
                    var newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle[0],
                        (short) triangle[1],
                        (short) triangle[2]
                    };
                    newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle[0],
                        (short) triangle[1],
                        (short) triangle[2]
                    };
                    newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle[0],
                        (short) triangle[1],
                        (short) triangle[2]
                    };
                    newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle[0],
                        (short) triangle[1],
                        (short) triangle[2]
                    };
                    newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle[0],
                        (short) triangle[1],
                        (short) triangle[2]
                    };
                    newMTriplet = new MtiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);
                }
            }

            result.TrimExcess();
            return new PnFeatures(result, minutiae);
        }
    }
}