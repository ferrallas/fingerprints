/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2011
{
    public class MPN
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


        public double Match(MtripletsFeature query, MtripletsFeature template)
        {
            List<MinutiaPair> matchingMtiae;
            return Match(query, template, out matchingMtiae);
        }


        public double Match(object query, object template, out List<MinutiaPair> matchingMtiae)
        {
            var qMtripletsFeature = query as MtripletsFeature;
            var tMtripletsFeature = template as MtripletsFeature;
            try
            {
                matchingMtiae = new List<MinutiaPair>();
                IList<MtripletPair> matchingTriplets = GetMatchingTriplets(qMtripletsFeature, tMtripletsFeature);
                if (matchingTriplets.Count == 0)
                    return 0;

                var localMatchingMtiae = new List<MinutiaPair>(3600);
                foreach (var qMtia in qMtripletsFeature.Minutiae)
                foreach (var tMtia in tMtripletsFeature.Minutiae)
                    localMatchingMtiae.Add(new MinutiaPair
                    {
                        QueryMtia = qMtia,
                        TemplateMtia = tMtia,
                        MatchingValue = 1
                    });
                //List<MinutiaPair> localMatchingMtiae = GetLocalMatchingMtiae(qMtripletsFeature, tMtripletsFeature, matchingTriplets);
                //if (localMatchingMtiae.Count == 0)
                //    return 0;

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

                return 100 * Math.Sqrt(1.0 * max * max /
                                       (qMtripletsFeature.Minutiae.Count * tMtripletsFeature.Minutiae.Count));
            }
            catch (Exception e)
            {
                if (query.GetType() != typeof(MtripletsFeature) || template.GetType() != typeof(MtripletsFeature))
                {
                    var msg = "Unable to match fingerprints: Invalid features type!";
                    throw new ArgumentOutOfRangeException(msg, e);
                }
                throw e;
            }
        }

        #endregion

        #region private

        private List<MtripletPair> GetMatchingTriplets(MtripletsFeature t1, MtripletsFeature t2)
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

        private List<MinutiaPair> GetReferenceMtiae(IList<MtripletPair> matchingTriplets)
        {
            var pairs = new List<MinutiaPair>();
            var matches = new Dictionary<MinutiaPair, byte>(60);
            var qMatches = new Dictionary<MTriplet, byte>(matchingTriplets.Count);
            var tMatches = new Dictionary<MTriplet, byte>(matchingTriplets.Count);
            foreach (var pair in matchingTriplets)
                if (!qMatches.ContainsKey(pair.queryMTp) || !tMatches.ContainsKey(pair.templateMTp))
                {
                    var qMtia0 = pair.queryMTp[0];
                    var qMtia1 = pair.queryMTp[1];
                    var qMtia2 = pair.queryMTp[2];

                    var tMtia0 = pair.templateMTp[0];
                    var tMtia1 = pair.templateMTp[1];
                    var tMtia2 = pair.templateMTp[2];

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
                    if (!qMatches.ContainsKey(pair.queryMTp))
                        qMatches.Add(pair.queryMTp, 0);
                    if (!tMatches.ContainsKey(pair.templateMTp))
                        tMatches.Add(pair.templateMTp, 0);
                }
            return pairs;
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
                    if (dist.Compare(query, template) <= GlobalDistThr && MatchDirections(query, template))
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
            return Angle.DifferencePi(query.Angle, template.Angle) <= gaThr;
        }

        private double gaThr = Math.PI / 6;

        private readonly MtiaEuclideanDistance dist = new MtiaEuclideanDistance();

        #endregion
    }
}