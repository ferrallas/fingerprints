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
    public static class Medina2011Matcher
    {
        private const int GlobalDistThr  = 12;

        private const double GaThr = Math.PI / 6;

        public static void Store(IStoreProvider<MtripletsFeature> storage, Bitmap bitmap, string subjectId)
        {
            var extract = Extract(ImageProvider.AdaptImage(bitmap));

            storage.Add(new Candidate<MtripletsFeature>
            {
                EntryId = subjectId,
                Feauture = extract
            });
        }

        public static IEnumerable<Match> Match(IStoreProvider<MtripletsFeature> storage, Bitmap bitmap, string subjectId = null)
        {
            var extract = Extract(ImageProvider.AdaptImage(bitmap));

            if (subjectId != null)
            {
                storage.Add(new Candidate<MtripletsFeature>
                {
                    EntryId = subjectId,
                    Feauture = extract
                });
            }

            var list = new List<Match>();
            foreach (var candidate in storage.Candidates)
            {
                var score = Match(extract, candidate.Feauture, out var matchingMtiae);

                if (Math.Abs(score) < double.Epsilon || matchingMtiae == null)
                    continue;

                if (matchingMtiae.Count > 10)
                    list.Add(new Match
                    {
                        Confidence = score,
                        EntryId = candidate.EntryId,
                        MatchingPoints = matchingMtiae.Count
                    });
            }

            return list;
        }

        private static MtripletsFeature Extract(Bitmap image)
        {
            var minutiae = MinutiaeExtractor.ExtractFeatures(ImageProvider.AdaptImage(image));
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

        private static double Match(MtripletsFeature query, MtripletsFeature template, out List<MinutiaPair> matchingMtiae)
        {
            matchingMtiae = new List<MinutiaPair>();
            IList<MtripletPair> matchingTriplets = GetMatchingTriplets(query, template);
            if (matchingTriplets.Count == 0)
                return 0;

            var localMatchingMtiae = new List<MinutiaPair>(3600);
            foreach (var qMtia in query.Minutiae)
            foreach (var tMtia in template.Minutiae)
                localMatchingMtiae.Add(new MinutiaPair
                {
                    QueryMtia = qMtia,
                    TemplateMtia = tMtia,
                    MatchingValue = 1
                });

            var refMtiaePairs = GetReferenceMtiae(matchingTriplets);

            // Iterating over the reference Minutia pair
            var max = 0;
            var notMatchingCount = int.MaxValue;
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

            return 100 * Math.Sqrt(1.0 * max * max / (query.Minutiae.Count * template.Minutiae.Count));
        }

        private static List<MtripletPair> GetMatchingTriplets(MtripletsFeature t1, MtripletsFeature t2)
        {
            var mostSimilar = new List<MtripletPair>();
            foreach (var queryTriplet in t1.MTriplets)
            {
                var mtpPairs = t2.FindSimilarMTriplets(queryTriplet);
                if (mtpPairs != null)
                    mostSimilar.AddRange(mtpPairs);
            }
            //mostSimilar.Sort(new MtpPairComparer());
            return mostSimilar;
        }

        private static List<MinutiaPair> GetReferenceMtiae(ICollection<MtripletPair> matchingTriplets)
        {
            var pairs = new List<MinutiaPair>();
            var matches = new Dictionary<MinutiaPair, byte>(60);
            var qMatches = new Dictionary<MTriplet, byte>(matchingTriplets.Count);
            var tMatches = new Dictionary<MTriplet, byte>(matchingTriplets.Count);
            foreach (var pair in matchingTriplets)
                if (!qMatches.ContainsKey(pair.QueryMTp) || !tMatches.ContainsKey(pair.TemplateMTp))
                {
                    var qMtia0 = pair.QueryMTp[0];
                    var qMtia1 = pair.QueryMTp[1];
                    var qMtia2 = pair.QueryMTp[2];

                    var tMtia0 = pair.TemplateMTp[0];
                    var tMtia1 = pair.TemplateMTp[1];
                    var tMtia2 = pair.TemplateMTp[2];

                    var qRefMtia = new Minutia();
                    qRefMtia.X = Convert.ToInt16(Math.Round((qMtia0.X + qMtia1.X + qMtia2.X) / 3.0));
                    qRefMtia.Y = Convert.ToInt16(Math.Round((qMtia0.Y + qMtia1.Y + qMtia2.Y) / 3.0));
                    var diffY = (Math.Sin(qMtia0.Angle) + Math.Sin(qMtia1.Angle) + Math.Sin(qMtia2.Angle)) / 3.0;
                    var diffX = (Math.Cos(qMtia0.Angle) + Math.Cos(qMtia1.Angle) + Math.Cos(qMtia2.Angle)) / 3.0;
                    qRefMtia.Angle = Angle.ComputeAngle(diffX, diffY);

                    var tRefMtia = new Minutia();
                    tRefMtia.X = Convert.ToInt16(Math.Round((tMtia0.X + tMtia1.X + tMtia2.X) / 3.0));
                    tRefMtia.Y = Convert.ToInt16(Math.Round((tMtia0.Y + tMtia1.Y + tMtia2.Y) / 3.0));
                    diffY = (Math.Sin(tMtia0.Angle) + Math.Sin(tMtia1.Angle) + Math.Sin(tMtia2.Angle)) / 3.0;
                    diffX = (Math.Cos(tMtia0.Angle) + Math.Cos(tMtia1.Angle) + Math.Cos(tMtia2.Angle)) / 3.0;
                    tRefMtia.Angle = Angle.ComputeAngle(diffX, diffY);

                    var mPair = new MinutiaPair {QueryMtia = qRefMtia, TemplateMtia = tRefMtia};
                    if (!matches.ContainsKey(mPair))
                    {
                        matches.Add(mPair, 0);
                        pairs.Add(mPair);
                    }
                    if (!qMatches.ContainsKey(pair.QueryMTp))
                        qMatches.Add(pair.QueryMTp, 0);
                    if (!tMatches.ContainsKey(pair.TemplateMTp))
                        tMatches.Add(pair.TemplateMTp, 0);
                }
            return pairs;
        }

        private static List<MinutiaPair> GetGlobalMatchingMtiae(IReadOnlyList<MinutiaPair> localMatchingPairs, MinutiaPair refMtiaPair,
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