/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Jiang2000
{
    public partial class Jy
    {
        private double _gAngThr = Math.PI / 6;

        private int GlobalDistThr { get; set; } = 8;


        public double GlobalAngleThr
        {
            get => _gAngThr * 180 / Math.PI;
            set => _gAngThr = value * Math.PI / 180;
        }

        public double Match(JyFeatures query, JyFeatures template, out List<MinutiaPair> matchingMtiae)
        {
            matchingMtiae = null;
            var localMatchingMtiae = GetLocalMatchingMtiae(query, template);
            if (localMatchingMtiae.Count == 0)
                return 0;
            matchingMtiae = GetGlobalMatchingMtiae(localMatchingMtiae, localMatchingMtiae[0]);

            if (matchingMtiae.Count < 6)
                return 0;

            double sum = 0;
            foreach (var mtiaPair in matchingMtiae)
                sum += 0.5 + 0.5 * mtiaPair.MatchingValue;

            return 100.0 * sum / Math.Max(query.Minutiae.Count, template.Minutiae.Count);
        }

        private static IList<MinutiaPair> GetLocalMatchingMtiae(JyFeatures query, JyFeatures template)
        {
            var triplets = new List<JyTriplet>(query.Minutiae.Count * template.Minutiae.Count);
            for (var i = 0; i < query.Minutiae.Count; i++)
            {
                var qMtia = query.Minutiae[i];
                for (var j = 0; j < template.Minutiae.Count; j++)
                {
                    var tMtia = template.Minutiae[j];
                    var currSim = qMtia.RotationInvariantMatch(tMtia);

                    if (currSim == 0) continue;

                    var currTriplet = new JyTriplet();
                    var currMtiaPair = new MinutiaPair
                    {
                        QueryMtia = qMtia.MainMinutia,
                        TemplateMtia = tMtia.MainMinutia,
                        MatchingValue = currSim
                    };
                    currTriplet.MainMinutia = currMtiaPair;

                    currMtiaPair = new MinutiaPair
                    {
                        QueryMtia = qMtia.NearestMtia,
                        TemplateMtia = tMtia.NearestMtia,
                        MatchingValue = currSim
                    };
                    currTriplet.NearestMtia = currMtiaPair;

                    currMtiaPair = new MinutiaPair
                    {
                        QueryMtia = qMtia.FarthestMtia,
                        TemplateMtia = tMtia.FarthestMtia,
                        MatchingValue = currSim
                    };
                    currTriplet.FarthestMtia = currMtiaPair;

                    currTriplet.MatchingValue = currSim;
                    triplets.Add(currTriplet);
                }
            }
            triplets.Sort(new MtiaTripletComparer());
            var mtiaPairs = new List<MinutiaPair>(3 * triplets.Count);
            foreach (var triplet in triplets)
            {
                mtiaPairs.Add(triplet.MainMinutia);
                mtiaPairs.Add(triplet.NearestMtia);
                mtiaPairs.Add(triplet.FarthestMtia);
            }

            return mtiaPairs;
        }

        private List<MinutiaPair> GetGlobalMatchingMtiae(IList<MinutiaPair> localMatchingPairs, MinutiaPair refMtiaPair)
        {
            var globalMatchingMtiae = new List<MinutiaPair>(localMatchingPairs.Count);
            var qMatches = new Dictionary<Minutia, Minutia>(localMatchingPairs.Count);
            var tMatches = new Dictionary<Minutia, Minutia>(localMatchingPairs.Count);
            qMatches.Add(refMtiaPair.QueryMtia, refMtiaPair.TemplateMtia);
            tMatches.Add(refMtiaPair.TemplateMtia, refMtiaPair.QueryMtia);

            var mm = new MtiaMapper(refMtiaPair.QueryMtia, refMtiaPair.TemplateMtia);
            var refQuery = mm.Map(refMtiaPair.QueryMtia);
            var refTemplate = refMtiaPair.TemplateMtia;
            for (var i = 1; i < localMatchingPairs.Count; i++)
            {
                var mtiaPair = localMatchingPairs[i];
                if (!qMatches.ContainsKey(mtiaPair.QueryMtia) && !tMatches.ContainsKey(mtiaPair.TemplateMtia))
                {
                    var query = mm.Map(mtiaPair.QueryMtia);
                    var template = mtiaPair.TemplateMtia;
                    if (MatchDistance(refQuery, refTemplate, query, template) && MatchDirections(query, template) &&
                        MatchPosition(refQuery, refTemplate, query, template))
                    {
                        globalMatchingMtiae.Add(mtiaPair);
                        qMatches.Add(mtiaPair.QueryMtia, mtiaPair.TemplateMtia);
                        tMatches.Add(mtiaPair.TemplateMtia, mtiaPair.QueryMtia);
                    }
                }
            }
            globalMatchingMtiae.Add(refMtiaPair);
            return globalMatchingMtiae;
        }

        private bool MatchDistance(Minutia refQuery, Minutia refTemplate, Minutia query, Minutia template)
        {
            var d0 = MtiaEuclideanDistance.Compare(refQuery, query);
            var d1 = MtiaEuclideanDistance.Compare(refTemplate, template);
            return Math.Abs(d0 - d1) <= GlobalDistThr;
        }

        private bool MatchPosition(Minutia refQuery, Minutia refTemplate, Minutia query, Minutia template)
        {
            var qMtiai = refQuery;
            var qMtiaj = query;
            double x = qMtiai.X - qMtiaj.X;
            double y = qMtiai.Y - qMtiaj.Y;
            var qAngle = Angle.ComputeAngle(x, y);

            var tMtiai = refTemplate;
            var tMtiaj = template;
            x = tMtiai.X - tMtiaj.X;
            y = tMtiai.Y - tMtiaj.Y;
            var tAngle = Angle.ComputeAngle(x, y);

            return Angle.DifferencePi(qAngle, tAngle) <= _gAngThr;
        }

        private bool MatchDirections(Minutia query, Minutia template)
        {
            return Angle.DifferencePi(query.Angle, template.Angle) <= _gAngThr;
        }

        private class MtiaTripletComparer : IComparer<JyTriplet>
        {
            public int Compare(JyTriplet x, JyTriplet y)
            {
                return x == y ? 0 : x.MatchingValue < y.MatchingValue ? 1 : -1;
            }
        }
    }
}