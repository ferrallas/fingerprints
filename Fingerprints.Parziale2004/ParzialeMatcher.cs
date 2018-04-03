/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using Fingerprints.Computation;
using Fingerprints.Model;

namespace Fingerprints.Parziale2004
{
    public class ParzialeMatcher : BaseMatcher<PartialeFeatures>
    {
        private const double GaThr = Math.PI / 6;
        private const int GlobalDistThr = 12;

        public override PartialeFeatures Extract(Bitmap image)
        {
            var minutiae = MinutiaeExtractor.ExtractFeatures(image);
            var result = new List<MinutiaTriplet>();

            if (minutiae.Count > 3)
                foreach (var triangle in Delaunay2D.Triangulate(minutiae))
                {
                    var idxArr = new[]
                    {
                        (short) triangle.A,
                        (short) triangle.B,
                        (short) triangle.C
                    };
                    var newMTriplet = new MinutiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle.A,
                        (short) triangle.C,
                        (short) triangle.B
                    };
                    newMTriplet = new MinutiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle.B,
                        (short) triangle.A,
                        (short) triangle.C
                    };
                    newMTriplet = new MinutiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle.B,
                        (short) triangle.C,
                        (short) triangle.A
                    };
                    newMTriplet = new MinutiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle.C,
                        (short) triangle.A,
                        (short) triangle.B
                    };
                    newMTriplet = new MinutiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);

                    idxArr = new[]
                    {
                        (short) triangle.C,
                        (short) triangle.B,
                        (short) triangle.A
                    };
                    newMTriplet = new MinutiaTriplet(idxArr, minutiae);
                    result.Add(newMTriplet);
                }
            result.TrimExcess();
            return new PartialeFeatures(result, minutiae);
        }

        public override double Match(PartialeFeatures qPartialeFeatures, PartialeFeatures tPartialeFeatures, out List<MinutiaPair> matchingMtiae)
        {
            try
            {
                matchingMtiae = new List<MinutiaPair>();
                IList<MinutiaTripletPair> matchingTriplets = GetMatchingTriplets(qPartialeFeatures, tPartialeFeatures);
                if (matchingTriplets.Count == 0)
                    return 0;

                var localMatchingMtiae = new List<MinutiaPair>(3600);
                foreach (var qMtia in qPartialeFeatures.Minutiae)
                foreach (var tMtia in tPartialeFeatures.Minutiae)
                    localMatchingMtiae.Add(new MinutiaPair
                    {
                        QueryMtia = qMtia,
                        TemplateMtia = tMtia,
                        MatchingValue = 1
                    });

                var refMtiaePairs = GetReferenceMtiae(matchingTriplets);

                // Iterating over the reference Minutia pair
                var max = 0;
                var notMatchingCount = Int32.MaxValue;
                for (var i = 0; i < refMtiaePairs.Count; i++)
                {
                    var currMatchingMtiae =
                        GetGlobalMatchingMtiae(localMatchingMtiae, refMtiaePairs[i], ref notMatchingCount);
                    if (currMatchingMtiae != null && currMatchingMtiae.Count > max)
                    {
                        max = currMatchingMtiae.Count;
                        matchingMtiae = currMatchingMtiae;
                    }
                }

                return 100 * Math.Sqrt(1.0 * max * max / (qPartialeFeatures.Minutiae.Count * tPartialeFeatures.Minutiae.Count));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static List<MinutiaTripletPair> GetMatchingTriplets(PartialeFeatures t1, PartialeFeatures t2)
        {
            var mostSimilar = new List<MinutiaTripletPair>();
            foreach (var queryTriplet in t1.MTriplets)
            {
                var mtpPairs = t2.FindAllSimilar(queryTriplet);
                if (mtpPairs != null)
                    mostSimilar.AddRange(mtpPairs);
            }
            return mostSimilar;
        }

        private static List<MinutiaPair> GetReferenceMtiae(IEnumerable<MinutiaTripletPair> matchingTriplets)
        {
            var pairs = new List<MinutiaPair>();
            var matches = new Dictionary<MinutiaPair, byte>(60);
            foreach (var pair in matchingTriplets)
            {
                var qMtia0 = pair.QueryMTp[0];
                var qMtia1 = pair.QueryMTp[1];
                var qMtia2 = pair.QueryMTp[2];

                var tMtia0 = pair.TemplateMTp[0];
                var tMtia1 = pair.TemplateMTp[1];
                var tMtia2 = pair.TemplateMTp[2];

                var qRefMtia = new Minutia
                {
                    X = Convert.ToInt16(Math.Round((qMtia0.X + qMtia1.X + qMtia2.X) / 3.0)),
                    Y = Convert.ToInt16(Math.Round((qMtia0.Y + qMtia1.Y + qMtia2.Y) / 3.0))
                };
                var diffY = (Math.Sin(qMtia0.Angle) + Math.Sin(qMtia1.Angle) + Math.Sin(qMtia2.Angle)) / 3.0;
                var diffX = (Math.Cos(qMtia0.Angle) + Math.Cos(qMtia1.Angle) + Math.Cos(qMtia2.Angle)) / 3.0;
                qRefMtia.Angle = Angle.ComputeAngle(diffX, diffY);

                var tRefMtia = new Minutia
                {
                    X = Convert.ToInt16(Math.Round((tMtia0.X + tMtia1.X + tMtia2.X) / 3.0)),
                    Y = Convert.ToInt16(Math.Round((tMtia0.Y + tMtia1.Y + tMtia2.Y) / 3.0))
                };
                diffY = (Math.Sin(tMtia0.Angle) + Math.Sin(tMtia1.Angle) + Math.Sin(tMtia2.Angle)) / 3.0;
                diffX = (Math.Cos(tMtia0.Angle) + Math.Cos(tMtia1.Angle) + Math.Cos(tMtia2.Angle)) / 3.0;
                tRefMtia.Angle = Angle.ComputeAngle(diffX, diffY);

                var mPair = new MinutiaPair {QueryMtia = qRefMtia, TemplateMtia = tRefMtia};
                if (!matches.ContainsKey(mPair))
                {
                    matches.Add(mPair, 0);
                    pairs.Add(mPair);
                }
            }
            return pairs;
        }

        private static List<MinutiaPair> GetLocalMatchingMtiae(IEnumerable<MinutiaTripletPair> matchingTriplets)
        {
            var minutiaMatches = new List<MinutiaPair>();
            var matches = new Dictionary<MinutiaPair, byte>(60);
            foreach (var pair in matchingTriplets)
            {
                var qMtia0 = pair.QueryMTp[0];
                var qMtia1 = pair.QueryMTp[1];
                var qMtia2 = pair.QueryMTp[2];
                var tMtia0 = pair.TemplateMTp[0];
                var tMtia1 = pair.TemplateMTp[1];
                var tMtia2 = pair.TemplateMTp[2];

                var mPair0 = new MinutiaPair {QueryMtia = qMtia0, TemplateMtia = tMtia0};
                if (!matches.ContainsKey(mPair0))
                {
                    matches.Add(mPair0, 0);
                    minutiaMatches.Add(mPair0);
                }
                var mPair1 = new MinutiaPair {QueryMtia = qMtia1, TemplateMtia = tMtia1};
                if (!matches.ContainsKey(mPair1))
                {
                    matches.Add(mPair1, 0);
                    minutiaMatches.Add(mPair1);
                }
                var mPair2 = new MinutiaPair {QueryMtia = qMtia2, TemplateMtia = tMtia2};
                if (!matches.ContainsKey(mPair2))
                {
                    matches.Add(mPair2, 0);
                    minutiaMatches.Add(mPair2);
                }
            }
            return minutiaMatches;
        }

        private static List<MinutiaPair> GetGlobalMatchingMtiae(List<MinutiaPair> localMatchingPairs, MinutiaPair refMtiaPair,
            ref int notMatchingCount)
        {
            var globalMatchingMtiae = new List<MinutiaPair>(localMatchingPairs.Count);
            var qMatches = new Dictionary<Minutia, Minutia>(localMatchingPairs.Count);
            var tMatches = new Dictionary<Minutia, Minutia>(localMatchingPairs.Count);

            var mm = new MtiaMapper(refMtiaPair.QueryMtia, refMtiaPair.TemplateMtia);
            var currNotMatchingMtiaCount = 0;
            int i;
            for (i = 0; i < localMatchingPairs.Count; i++)
            {
                var mtiaPair = localMatchingPairs[i];
                if (!qMatches.ContainsKey(mtiaPair.QueryMtia) && !tMatches.ContainsKey(mtiaPair.TemplateMtia))
                {
                    var query = mm.Map(mtiaPair.QueryMtia);
                    var template = mtiaPair.TemplateMtia;
                    if (MtiaEuclideanDistance.Compare(query, template) <= GlobalDistThr && MatchDirections(query, template))
                    {
                        globalMatchingMtiae.Add(mtiaPair);
                        qMatches.Add(mtiaPair.QueryMtia, mtiaPair.TemplateMtia);
                        tMatches.Add(mtiaPair.TemplateMtia, mtiaPair.QueryMtia);
                    }
                    else
                    {
                        currNotMatchingMtiaCount++;
                    } 
                }
                if (currNotMatchingMtiaCount >= notMatchingCount)
                    break;
            }
            if (i == localMatchingPairs.Count)
            {
                notMatchingCount = currNotMatchingMtiaCount;
                return globalMatchingMtiae;
            }
            return null;
        }

        private static bool MatchDirections(Minutia query, Minutia template)
        {
            return Angle.DifferencePi(query.Angle, template.Angle) <= GaThr;
        }
    }
}