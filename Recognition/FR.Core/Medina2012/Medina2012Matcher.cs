/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Milton García Borroto (milton.garcia@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2012
{
    public static class Medina2012Matcher
    {
        private const double GaThr = Math.PI / 6;

        private const int GlobalDistThr  = 12;

        private const int MtiaCountThr = 2;

        private const byte NeighborsCount = 4;

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
            var minutiae = MinutiaeExtractor.ExtractFeatures(image);
            var result = new List<MTriplet>();
            var triplets = new Dictionary<int, int>();

            var nearest = new short[minutiae.Count, NeighborsCount];
            var distance = new double[minutiae.Count, NeighborsCount];

            // Initializing distances
            for (var i = 0; i < minutiae.Count; i++)
            for (var j = 0; j < NeighborsCount; j++)
            {
                distance[i, j] = double.MaxValue;
                nearest[i, j] = -1;
            }

            // Computing m-triplets
            for (short i = 0; i < minutiae.Count; i++)
            {
                // Updating nearest minutiae
                UpdateNearest(minutiae, i, nearest, distance);

                // Building m-triplets
                for (var j = 0; j < NeighborsCount - 1; j++)
                for (var k = j + 1; k < NeighborsCount; k++)
                    if (nearest[i, j] != -1 && nearest[i, k] != -1)
                    {
                        if (i == nearest[i, j] || i == nearest[i, k] || nearest[i, j] == nearest[i, k])
                            throw new Exception("Wrong mtp");

                        var newMTriplet = new MTriplet(new[] { i, nearest[i, j], nearest[i, k] }, minutiae);
                        var newHash = newMTriplet.GetHashCode();
                        if (!triplets.ContainsKey(newHash))
                        {
                            triplets.Add(newHash, 0);
                            result.Add(newMTriplet);
                        }
                    }
            }
            result.TrimExcess();
            return new MtripletsFeature(result, minutiae);
        }

        private static void UpdateNearest(IList<Minutia> minutiae, int idx, short[,] nearest, double[,] distance)
        {
            for (var i = idx + 1; i < minutiae.Count; i++)
            {
                var dValue = MtiaEuclideanDistance.Compare(minutiae[idx], minutiae[i]);

                var maxIdx = 0;
                for (var j = 1; j < NeighborsCount; j++)
                    if (distance[idx, j] > distance[idx, maxIdx])
                        maxIdx = j;
                if (dValue < distance[idx, maxIdx])
                {
                    distance[idx, maxIdx] = dValue;
                    nearest[idx, maxIdx] = Convert.ToInt16(i);
                }

                maxIdx = 0;
                for (var j = 1; j < NeighborsCount; j++)
                    if (distance[i, j] > distance[i, maxIdx])
                        maxIdx = j;
                if (dValue < distance[i, maxIdx])
                {
                    distance[i, maxIdx] = dValue;
                    nearest[i, maxIdx] = Convert.ToInt16(idx);
                }
            }
        }

        private static double Match(MtripletsFeature query, MtripletsFeature template, out List<MinutiaPair> matchingMtiae)
        {
            matchingMtiae = new List<MinutiaPair>();
            if (query.Minutiae.Count < MtiaCountThr || template.Minutiae.Count < MtiaCountThr)
                return 0;
            IList<MtripletPair> matchingTriplets = GetMatchingTriplets(query, template);
            if (matchingTriplets.Count == 0)
                return 0;
            var localMatchingMtiae = GetLocalMatchingMtiae(matchingTriplets);
            if (localMatchingMtiae.Count < MtiaCountThr)
                return 0;

            var max = 0;
            var notMatchingCount = int.MaxValue;
            for (var i = 0; i < localMatchingMtiae.Count; i++)
            {
                var currMatchingMtiae =
                    GetGlobalMatchingMtiae(localMatchingMtiae, localMatchingMtiae[i], ref notMatchingCount);
                if (currMatchingMtiae != null && currMatchingMtiae.Count > max)
                {
                    max = currMatchingMtiae.Count;
                    matchingMtiae = currMatchingMtiae;
                }
            }

            return 1.0 * max * max / (query.Minutiae.Count * template.Minutiae.Count);
        }

        private static List<MtripletPair> GetMatchingTriplets(MtripletsFeature t1, MtripletsFeature t2)
        {
            var mostSimilar = new List<MtripletPair>();
            foreach (var queryTriplet in t1.MTriplets)
            {
                var mtpPairs = t2.FindNoRotateAllSimilar(queryTriplet);
                if (mtpPairs != null)
                    mostSimilar.AddRange(mtpPairs);
            }
            mostSimilar.Sort(MtpPairComparer.Instance);
            return mostSimilar;
        }

        private static List<MinutiaPair> GetLocalMatchingMtiae(IEnumerable<MtripletPair> matchingTriplets)
        {
            var minutiaMatches = new List<MinutiaPair>();
            var qMatches = new Dictionary<Minutia, Minutia>(60);
            var tMatches = new Dictionary<Minutia, Minutia>(60);
            foreach (var pair in matchingTriplets)
            {
                var qMtia0 = pair.QueryMTp[0];
                var qMtia1 = pair.QueryMTp[1];
                var qMtia2 = pair.QueryMTp[2];
                var tMtia0 = pair.TemplateMTp[pair.TemplateMtiaOrder[0]];
                var tMtia1 = pair.TemplateMTp[pair.TemplateMtiaOrder[1]];
                var tMtia2 = pair.TemplateMTp[pair.TemplateMtiaOrder[2]];

                if (!qMatches.ContainsKey(qMtia0) || !tMatches.ContainsKey(tMtia0))
                {
                    if (!qMatches.ContainsKey(qMtia0))
                        qMatches.Add(qMtia0, tMtia0);
                    if (!tMatches.ContainsKey(tMtia0))
                        tMatches.Add(tMtia0, qMtia0);
                    minutiaMatches.Add(new MinutiaPair {QueryMtia = qMtia0, TemplateMtia = tMtia0});
                }
                if (!qMatches.ContainsKey(qMtia1) || !tMatches.ContainsKey(tMtia1))
                {
                    if (!qMatches.ContainsKey(qMtia1))
                        qMatches.Add(qMtia1, tMtia1);
                    if (!tMatches.ContainsKey(tMtia1))
                        tMatches.Add(tMtia1, qMtia1);
                    minutiaMatches.Add(new MinutiaPair {QueryMtia = qMtia1, TemplateMtia = tMtia1});
                }
                if (!qMatches.ContainsKey(qMtia2) || !tMatches.ContainsKey(tMtia2))
                {
                    if (!qMatches.ContainsKey(qMtia2))
                        qMatches.Add(qMtia2, tMtia2);
                    if (!tMatches.ContainsKey(tMtia2))
                        tMatches.Add(tMtia2, qMtia2);
                    minutiaMatches.Add(new MinutiaPair {QueryMtia = qMtia2, TemplateMtia = tMtia2});
                }
            }
            return minutiaMatches;
        }

        private static List<MinutiaPair> GetGlobalMatchingMtiae(IList<MinutiaPair> localMatchingPairs, MinutiaPair refMtiaPair,
            ref int notMatchingCount)
        {
            var globalMatchingMtiae = new List<MinutiaPair>(localMatchingPairs.Count);
            var qMatches = new Dictionary<Minutia, Minutia>(localMatchingPairs.Count);
            var tMatches = new Dictionary<Minutia, Minutia>(localMatchingPairs.Count);
            qMatches.Add(refMtiaPair.QueryMtia, refMtiaPair.TemplateMtia);
            tMatches.Add(refMtiaPair.TemplateMtia, refMtiaPair.QueryMtia);

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
                    if (MtiaEuclideanDistance.Compare(query, template) <= GlobalDistThr && MatchDirections(query, template) &&
                        MatchBetaAngle(refMtiaPair, mtiaPair))
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
                if (globalMatchingMtiae.Count + (localMatchingPairs.Count - i - 1) < MtiaCountThr)
                    break;
            }
            if (i == localMatchingPairs.Count)
            {
                notMatchingCount = currNotMatchingMtiaCount;
                globalMatchingMtiae.Add(refMtiaPair);
                return globalMatchingMtiae;
            }
            return null;
        }

        private static bool MatchDirections(Minutia query, Minutia template)
        {
            var alpha = query.Angle;
            var beta = template.Angle;
            var diff = Math.Abs(beta - alpha);
            return Math.Min(diff, 2 * Math.PI - diff) <= GaThr;
        }

        private static bool MatchBetaAngle(MinutiaPair mtiaPair0, MinutiaPair mtiaPair1)
        {
            var qMtiai = mtiaPair0.QueryMtia;
            var qMtiaj = mtiaPair1.QueryMtia;
            var qbeta = Angle.Difference2Pi(qMtiai.Angle, qMtiaj.Angle);

            var tMtiai = mtiaPair0.TemplateMtia;
            var tMtiaj = mtiaPair1.TemplateMtia;
            var tbeta = Angle.Difference2Pi(tMtiai.Angle, tMtiaj.Angle);

            var diff = Math.Abs(tbeta - qbeta);
            return !(Math.Min(diff, 2 * Math.PI - diff) > GaThr);
        }

        private class MtpPairComparer : IComparer<MtripletPair>
        {
            internal static readonly MtpPairComparer Instance = new MtpPairComparer();

            public int Compare(MtripletPair x, MtripletPair y)
            {
                return x.MatchingValue == y.MatchingValue ? 0 : x.MatchingValue < y.MatchingValue ? 1 : -1;
            }
        }
    }
}