/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Parziale2004
{
    public class Pn
    {
        #region public


        public int GlobalDistThr { get; } = 12;

        public double Match(PnFeatures query, PnFeatures template)
        {
            List<MinutiaPair> matchingMtiae;
            return Match(query, template, out matchingMtiae);
        }


        public double Match(object query, object template, out List<MinutiaPair> matchingMtiae)
        {
            var qPnFeatures = query as PnFeatures;
            var tPnFeatures = template as PnFeatures;
            try
            {
                matchingMtiae = new List<MinutiaPair>();
                IList<MtiaeTripletPair> matchingTriplets = GetMatchingTriplets(qPnFeatures, tPnFeatures);
                if (matchingTriplets.Count == 0)
                    return 0;

                var localMatchingMtiae = new List<MinutiaPair>(3600);
                foreach (var qMtia in qPnFeatures.Minutiae)
                foreach (var tMtia in tPnFeatures.Minutiae)
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

                return 100 * Math.Sqrt(1.0 * max * max / (qPnFeatures.Minutiae.Count * tPnFeatures.Minutiae.Count));
            }
            catch (Exception e)
            {
                if (query.GetType() != typeof(PnFeatures) || template.GetType() != typeof(PnFeatures))
                {
                    var msg = "Unable to match fingerprints: Invalid features type!";
                    throw new ArgumentOutOfRangeException(msg, e);
                }
                throw e;
            }
        }

        #endregion

        #region private

        private List<MtiaeTripletPair> GetMatchingTriplets(PnFeatures t1, PnFeatures t2)
        {
            var mostSimilar = new List<MtiaeTripletPair>();
            foreach (var queryTriplet in t1.MTriplets)
            {
                var mtpPairs = t2.FindAllSimilar(queryTriplet);
                if (mtpPairs != null)
                    mostSimilar.AddRange(mtpPairs);
            }
            return mostSimilar;
        }

        private List<MinutiaPair> GetReferenceMtiae(IList<MtiaeTripletPair> matchingTriplets)
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
            }
            return pairs;
        }

        private List<MinutiaPair> GetLocalMatchingMtiae(IList<MtiaeTripletPair> matchingTriplets)
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

        private List<MinutiaPair> GetGlobalMatchingMtiae(List<MinutiaPair> localMatchingPairs, MinutiaPair refMtiaPair,
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

        private bool MatchDirections(Minutia query, Minutia template)
        {
            return Angle.DifferencePi(query.Angle, template.Angle) <= _gaThr;
        }

        private double _gaThr = Math.PI / 6;

        #endregion
    }
}