/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Jiang2000
{
    public class MJY
    {
        #region Miembros de ISimilarity<JYFeatures>

        public double Match(JYFeatures query, JYFeatures template)
        {
            List<MinutiaPair> matchingMtiae;
            return Match(query, template, out matchingMtiae);
        }

        #endregion

        #region public

        public int GlobalDistThr { get; set; } = 8;


        public double GlobalAngleThr
        {
            get => gAngThr * 180 / Math.PI;
            set => gAngThr = value * Math.PI / 180;
        }

        public double Match(object query, object template, out List<MinutiaPair> matchingMtiae)
        {
            var qJYFeatures = query as JYFeatures;
            var tJYFeatures = template as JYFeatures;
            try
            {
                matchingMtiae = null;
                var localMatchingMtiae = GetLocalMatchingMtiae(qJYFeatures, tJYFeatures);
                if (localMatchingMtiae.Count == 0)
                    return 0;
                double max = 0;
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

                if (matchingMtiae == null)
                    return 0;

                double sum = 0;
                foreach (var mtiaPair in matchingMtiae)
                    sum += 0.5 + 0.5 * mtiaPair.MatchingValue;

                return 100.0 * max / Math.Max(qJYFeatures.Minutiae.Count, tJYFeatures.Minutiae.Count);
            }
            catch (Exception e)
            {
                if (query.GetType() != typeof(JYFeatures) || template.GetType() != typeof(JYFeatures))
                {
                    var msg = "Unable to match fingerprints: Invalid features type!";
                    throw new ArgumentOutOfRangeException(msg, e);
                }
                throw e;
            }
        }

        #endregion

        #region private

        private class JYTriplet
        {
            public MinutiaPair MainMinutia { set; get; }
            public MinutiaPair NearestMtia { set; get; }
            public MinutiaPair FarthestMtia { set; get; }
            public double MatchingValue { set; get; }
        }

        private static IList<MinutiaPair> GetLocalMatchingMtiae(JYFeatures query, JYFeatures template)
        {
            var triplets = new List<JYTriplet>(query.Minutiae.Count * template.Minutiae.Count);
            for (var i = 0; i < query.Minutiae.Count; i++)
            {
                var qMtia = query.Minutiae[i];
                for (var j = 0; j < template.Minutiae.Count; j++)
                {
                    var tMtia = template.Minutiae[j];
                    var currSim = qMtia.RotationInvariantMatch(tMtia);

                    if (currSim != 0)
                    {
                        var currTriplet = new JYTriplet();
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
            }
            triplets.Sort(new MtiaTripletComparer());
            var mtiaPairs = new List<MinutiaPair>(3 * triplets.Count);
            foreach (var triplet in triplets)
            {
                mtiaPairs.Add(triplet.MainMinutia);
                mtiaPairs.Add(triplet.NearestMtia);
                mtiaPairs.Add(triplet.FarthestMtia);
            }

            var qMatches = new Dictionary<Minutia, Minutia>(60);
            var tMatches = new Dictionary<Minutia, Minutia>(60);
            var matchingPairs = new List<MinutiaPair>(60);
            for (var i = 0; i < mtiaPairs.Count; i++)
            {
                var pair = mtiaPairs[i];
                if (!qMatches.ContainsKey(pair.QueryMtia) || !tMatches.ContainsKey(pair.TemplateMtia))
                {
                    matchingPairs.Add(pair);
                    if (!qMatches.ContainsKey(pair.QueryMtia))
                        qMatches.Add(pair.QueryMtia, pair.TemplateMtia);
                    if (!tMatches.ContainsKey(pair.TemplateMtia))
                        tMatches.Add(pair.TemplateMtia, pair.QueryMtia);
                }
            }

            return matchingPairs;
        }

        private List<MinutiaPair> GetGlobalMatchingMtiae(IList<MinutiaPair> localMatchingPairs, MinutiaPair refMtiaPair,
            ref int notMatchingCount)
        {
            var globalMatchingMtiae = new List<MinutiaPair>(localMatchingPairs.Count);
            var qMatches = new Dictionary<Minutia, Minutia>(localMatchingPairs.Count);
            var tMatches = new Dictionary<Minutia, Minutia>(localMatchingPairs.Count);
            qMatches.Add(refMtiaPair.QueryMtia, refMtiaPair.TemplateMtia);
            tMatches.Add(refMtiaPair.TemplateMtia, refMtiaPair.QueryMtia);

            var mm = new MtiaMapper(refMtiaPair.QueryMtia, refMtiaPair.TemplateMtia);
            var refQuery = mm.Map(refMtiaPair.QueryMtia);
            var refTemplate = refMtiaPair.TemplateMtia;
            var currNotMatchingMtiaCount = 0;
            int i;
            for (i = 0; i < localMatchingPairs.Count; i++)
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
                    else
                    {
                        currNotMatchingMtiaCount++;
                    }
                }
                if (currNotMatchingMtiaCount >= notMatchingCount)
                    break;
                //if (globalMatchingMtiae.Count + (localMatchingPairs.Count - i - 1) < MtiaCountThr)
                //    break;
            }
            if (i == localMatchingPairs.Count)
            {
                notMatchingCount = currNotMatchingMtiaCount;
                globalMatchingMtiae.Add(refMtiaPair);
                return globalMatchingMtiae;
            }
            return globalMatchingMtiae;
        }

        private class MtiaMapper
        {
            private readonly double dAngle;
            private readonly Minutia query;
            private readonly Minutia template;

            public MtiaMapper(Minutia query, Minutia template)
            {
                dAngle = template.Angle - query.Angle;
                this.template = template;
                this.query = query;
            }

            public Minutia Map(Minutia m)
            {
                return new Minutia
                {
                    Angle = m.Angle + dAngle,
                    X = Convert.ToInt16(Math.Round((m.X - query.X) * Math.Cos(dAngle) -
                                                   (m.Y - query.Y) * Math.Sin(dAngle) + template.X)),
                    Y = Convert.ToInt16(Math.Round((m.X - query.X) * Math.Sin(dAngle) +
                                                   (m.Y - query.Y) * Math.Cos(dAngle) + template.Y))
                };
            }
        }

        private bool MatchDistance(Minutia refQuery, Minutia refTemplate, Minutia query, Minutia template)
        {
            var d0 = dist.Compare(refQuery, query);
            var d1 = dist.Compare(refTemplate, template);
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

            return Angle.DifferencePi(qAngle, tAngle) <= gAngThr;
        }

        private bool MatchDirections(Minutia query, Minutia template)
        {
            return Angle.DifferencePi(query.Angle, template.Angle) <= gAngThr;
        }

        private class MtiaTripletComparer : IComparer<JYTriplet>
        {
            public int Compare(JYTriplet x, JYTriplet y)
            {
                return x == y ? 0 : x.MatchingValue < y.MatchingValue ? 1 : -1;
            }
        }

        private double gAngThr = Math.PI / 6;

        private readonly MtiaEuclideanDistance dist = new MtiaEuclideanDistance();

        #endregion
    }
}