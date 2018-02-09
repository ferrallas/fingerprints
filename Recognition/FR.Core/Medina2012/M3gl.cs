/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Milton García Borroto (milton.garcia@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2012
{
    public class M3gl
    {
        #region public

        public double LocalAngleThr
        {
            get => MTriplet.AngleThreshold * 180 / Math.PI;
            set => MTriplet.AngleThreshold = value * Math.PI / 180;
        }

        public double LocalDistThr
        {
            get => MTriplet.DistanceThreshold;
            set => MTriplet.DistanceThreshold = value;
        }

        public int GlobalDistThr { get; set; } = 12;

        public double GlobalAngleThr
        {
            get => gaThr * 180 / Math.PI;
            set => gaThr = value * Math.PI / 180;
        }

        public int MtiaCountThr { get; set; } = 2;

        public double Match(MtripletsFeature query, MtripletsFeature template)
        {
            List<MinutiaPair> matchingMtiae;
            return Match(query, template, out matchingMtiae);
        }

        public double Match(MtripletsFeature query, MtripletsFeature template, out List<MinutiaPair> matchingMtiae)
        {
            matchingMtiae = new List<MinutiaPair>();
            if (query.Minutiae.Count < MtiaCountThr || template.Minutiae.Count < MtiaCountThr)
                return 0;
            IList<MtripletPair> matchingTriplets = GetMatchingTriplets(query, template);
            if (matchingTriplets.Count == 0)
                return 0;
            var localMatchingMtiae = GetLocalMatchingMtiae(query, template, matchingTriplets);
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

        #endregion

        #region private

        private static List<MtripletPair> GetMatchingTriplets(MtripletsFeature t1, MtripletsFeature t2)
        {
            var mostSimilar = new List<MtripletPair>();
            foreach (var queryTriplet in t1.MTriplets)
            {
                var mtpPairs = t2.FindNoRotateAllSimilar(queryTriplet);
                if (mtpPairs != null)
                    mostSimilar.AddRange(mtpPairs);
            }
            mostSimilar.Sort(new MtpPairComparer());
            return mostSimilar;
        }

        private List<MinutiaPair> GetLocalMatchingMtiae(MtripletsFeature query, MtripletsFeature template,
            IList<MtripletPair> matchingTriplets)
        {
            var minutiaMatches = new List<MinutiaPair>();
            var qMatches = new Dictionary<Minutia, Minutia>(60);
            var tMatches = new Dictionary<Minutia, Minutia>(60);
            foreach (var pair in matchingTriplets)
            {
                var qMtia0 = pair.queryMTp[0];
                var qMtia1 = pair.queryMTp[1];
                var qMtia2 = pair.queryMTp[2];
                var tMtia0 = pair.templateMTp[pair.templateMtiaOrder[0]];
                var tMtia1 = pair.templateMTp[pair.templateMtiaOrder[1]];
                var tMtia2 = pair.templateMTp[pair.templateMtiaOrder[2]];

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

        private List<MinutiaPair> GetGlobalMatchingMtiae(List<MinutiaPair> localMatchingPairs, MinutiaPair refMtiaPair,
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
                    if (dist.Compare(query, template) <= GlobalDistThr && MatchDirections(query, template) &&
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

        private bool MatchDirections(Minutia query, Minutia template)
        {
            var alpha = query.Angle;
            var beta = template.Angle;
            var diff = Math.Abs(beta - alpha);
            return Math.Min(diff, 2 * Math.PI - diff) <= gaThr;
        }

        private bool MatchBetaAngle(MinutiaPair mtiaPair0, MinutiaPair mtiaPair1)
        {
            var qMtiai = mtiaPair0.QueryMtia;
            var qMtiaj = mtiaPair1.QueryMtia;
            var qbeta = Angle.Difference2Pi(qMtiai.Angle, qMtiaj.Angle);

            var tMtiai = mtiaPair0.TemplateMtia;
            var tMtiaj = mtiaPair1.TemplateMtia;
            var tbeta = Angle.Difference2Pi(tMtiai.Angle, tMtiaj.Angle);

            var diff = Math.Abs(tbeta - qbeta);
            return !(Math.Min(diff, 2 * Math.PI - diff) > gaThr);
        }

        private class MtpPairComparer : IComparer<MtripletPair>
        {
            public int Compare(MtripletPair x, MtripletPair y)
            {
                return x.matchingValue == y.matchingValue ? 0 : x.matchingValue < y.matchingValue ? 1 : -1;
            }
        }

        private double gaThr = Math.PI / 6;

        private readonly MtiaEuclideanDistance dist = new MtiaEuclideanDistance();

        #endregion
    }
}