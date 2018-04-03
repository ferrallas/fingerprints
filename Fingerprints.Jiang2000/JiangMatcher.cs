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

namespace Fingerprints.Jiang2000
{
    public class JiangMatcher : BaseMatcher<JiangFeatures>
    {
        private const double GAngThr = Math.PI / 6;

        private const int GlobalDistThr = 8;

        private const byte NeighborsCount = 2;

        public override JiangFeatures Extract(Bitmap image)
        {
            var minutiae = MinutiaeExtractor.ExtractFeatures(image);
            var skeletonImg = SkeletonImageExtractor.ExtractFeatures(image);
            var descriptorsList = new List<JiangMinutiaDescriptor>();

            if (minutiae.Count <= 3) return new JiangFeatures(descriptorsList);

            var mtiaIdx = new Dictionary<Minutia, int>();
            for (var i = 0; i < minutiae.Count; i++)
                mtiaIdx.Add(minutiae[i], i);
            for (short idx = 0; idx < minutiae.Count; idx++)
            {
                var query = minutiae[idx];
                var nearest = GetNearest(minutiae, query);
                for (var i = 0; i < nearest.Length - 1; i++)
                    for (var j = i + 1; j < nearest.Length; j++)
                    {
                        var newMTriplet = new JiangMinutiaDescriptor(skeletonImg, minutiae, idx, nearest[i],
                            nearest[j]);
                        descriptorsList.Add(newMTriplet);
                    }
            }
            descriptorsList.TrimExcess();
            return new JiangFeatures(descriptorsList);
        }

        public override double Match(JiangFeatures query, JiangFeatures template, out List<MinutiaPair> matchingMtiae)
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

        private static short[] GetNearest(IReadOnlyList<Minutia> minutiae, Minutia query)
        {
            var distances = new double[NeighborsCount];
            var nearestM = new short[NeighborsCount];
            for (var i = 0; i < distances.Length; i++)
                distances[i] = Double.MaxValue;

            for (short i = 0; i < minutiae.Count; i++)
                if (minutiae[i] != query)
                {
                    var currentDistance = MtiaEuclideanDistance.Compare(query, minutiae[i]);
                    var maxIdx = 0;
                    for (var j = 1; j < NeighborsCount; j++)
                        if (distances[j] > distances[maxIdx])
                            maxIdx = j;
                    if (currentDistance < distances[maxIdx])
                    {
                        distances[maxIdx] = currentDistance;
                        nearestM[maxIdx] = i;
                    }
                }
            return nearestM;
        }

        private static IList<MinutiaPair> GetLocalMatchingMtiae(JiangFeatures query, JiangFeatures template)
        {
            var triplets = new List<JiangMinutiaTriplet>(query.Minutiae.Count * template.Minutiae.Count);
            for (var i = 0; i < query.Minutiae.Count; i++)
            {
                var qMtia = query.Minutiae[i];
                for (var j = 0; j < template.Minutiae.Count; j++)
                {
                    var tMtia = template.Minutiae[j];
                    var currSim = qMtia.RotationInvariantMatch(tMtia);

                    if (Math.Abs(currSim) < Double.Epsilon) continue;

                    var currTriplet = new JiangMinutiaTriplet();
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

        private static List<MinutiaPair> GetGlobalMatchingMtiae(IList<MinutiaPair> localMatchingPairs, MinutiaPair refMtiaPair)
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

        private static bool MatchDistance(Minutia refQuery, Minutia refTemplate, Minutia query, Minutia template)
        {
            var d0 = MtiaEuclideanDistance.Compare(refQuery, query);
            var d1 = MtiaEuclideanDistance.Compare(refTemplate, template);
            return Math.Abs(d0 - d1) <= GlobalDistThr;
        }

        private static bool MatchPosition(Minutia refQuery, Minutia refTemplate, Minutia query, Minutia template)
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

            return Angle.DifferencePi(qAngle, tAngle) <= GAngThr;
        }

        private static bool MatchDirections(Minutia query, Minutia template)
        {
            return Angle.DifferencePi(query.Angle, template.Angle) <= GAngThr;
        }

        private class MtiaTripletComparer : IComparer<JiangMinutiaTriplet>
        {
            public int Compare(JiangMinutiaTriplet x, JiangMinutiaTriplet y)
            {
                return x == y ? 0 : x.MatchingValue < y.MatchingValue ? 1 : -1;
            }
        }
    }
}