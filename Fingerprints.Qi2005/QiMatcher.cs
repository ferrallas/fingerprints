/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Fingerprints.Computation;
using Fingerprints.Model;

namespace Fingerprints.Qi2005
{
    public class QiMatcher : BaseMatcher<QiFeatures>
    {
        private const int GlobalDistThr = 8;
        private const double GAngThr = Math.PI / 6;

        public override QiFeatures Extract(Bitmap image)
        {
            var mtiae = MinutiaeExtractor.ExtractFeatures(image);
            var dirImg = ImageOrietantionExtractor.ExtractFeatures(image);

            return new QiFeatures(mtiae, dirImg);
        }

        public override double Match(QiFeatures qQi2005Features, QiFeatures tQi2005Features, out List<MinutiaPair> matchingMtiae)
        {
            try
            {
                matchingMtiae = null;
                var localMatchingMtiae = GetLocalMatchingMtiae(qQi2005Features, tQi2005Features, out var simArr);
                if (localMatchingMtiae.Count == 0)
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

                return Eval(qQi2005Features, tQi2005Features, matchingMtiae, simArr);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static IList<MinutiaPair> GetLocalMatchingMtiae(QiFeatures query, QiFeatures template,
            out double[,] simArr)
        {
            var qIndex = new Dictionary<Minutia, int>(query.Minutiae.Count);
            var tIndex = new Dictionary<Minutia, int>(template.Minutiae.Count);
            var mtiaPairs = new List<MinutiaPair>(query.Minutiae.Count * template.Minutiae.Count);

            simArr = new double[query.Minutiae.Count, template.Minutiae.Count];
            for (var i = 0; i < query.Minutiae.Count; i++)
            {
                var qMtia = query.Minutiae[i];
                qIndex.Add(qMtia, i);
                for (var j = 0; j < template.Minutiae.Count; j++)
                {
                    var tMtia = template.Minutiae[j];
                    var currSim = qMtia.Compare(tMtia);
                    simArr[i, j] = currSim;
                    if (currSim != 0)
                    {
                        var currMtiaPair = new MinutiaPair
                        {
                            QueryMtia = qMtia,
                            TemplateMtia = tMtia,
                            MatchingValue = currSim
                        };
                        mtiaPairs.Add(currMtiaPair);
                    }
                }
            }
            for (var j = 0; j < template.Minutiae.Count; j++)
                tIndex.Add(template.Minutiae[j], j);

            var qMatches = new Dictionary<Minutia, Minutia>(60);
            var tMatches = new Dictionary<Minutia, Minutia>(60);
            mtiaPairs.Sort(new MtiaPairComparer());

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
                    pair.MatchingValue = simArr[qIndex[pair.QueryMtia], tIndex[pair.TemplateMtia]];
                }
            }

            return matchingPairs;
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
            return null;
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

        private class MtiaPairComparer : IComparer<MinutiaPair>
        {
            public int Compare(MinutiaPair x, MinutiaPair y)
            {
                return x == y ? 0 : x.MatchingValue < y.MatchingValue ? 1 : -1;
            }
        }

        private static double Eval(QiFeatures query, QiFeatures template, List<MinutiaPair> matchingPair,
            double[,] simArr)
        {
            if (matchingPair == null)
                return 0;

            var qIndex = new Dictionary<Minutia, int>(query.Minutiae.Count);
            for (var i = 0; i < query.Minutiae.Count; i++)
                qIndex.Add(query.Minutiae[i], i);
            var tIndex = new Dictionary<Minutia, int>(template.Minutiae.Count);
            for (var i = 0; i < template.Minutiae.Count; i++)
                tIndex.Add(template.Minutiae[i], i);

            var qCenterMtia = matchingPair[matchingPair.Count - 1].QueryMtia;
            var tCenterMtia = matchingPair[matchingPair.Count - 1].TemplateMtia;

            // Computing query bounding region
            var qPolygon = GetBounds(query);
            qPolygon =
                new PolygonMapper(qCenterMtia, tCenterMtia).Map(qPolygon);
            var qBoundingRegion = new FingerprintRegion(qPolygon);
            // Computing template bounding region
            var tPolygon = GetBounds(template);
            var tBoundingRegion = new FingerprintRegion(tPolygon);

            // Mapping query minutiae
            var mtiaMapper = new MtiaMapper(qCenterMtia, tCenterMtia);
            var qMtiae = new List<Minutia>(query.Minutiae.Count);
            for (var i = 0; i < query.Minutiae.Count; i++)
            {
                var mtia = query.Minutiae[i];
                qMtiae.Add(mtiaMapper.Map(mtia));
            }

            var qCount = 0;
            for (var i = 0; i < qMtiae.Count; i++)
            {
                var mtia = qMtiae[i];
                if (tBoundingRegion.Contains(mtia))
                    qCount++;
            }
            var tCount = 0;
            foreach (var mtia in template.Minutiae)
                if (qBoundingRegion.Contains(mtia))
                    tCount++;

            double sum = 0;
            foreach (var mtiaPair in matchingPair)
            {
                var i = qIndex[mtiaPair.QueryMtia];
                var j = tIndex[mtiaPair.TemplateMtia];
                sum += simArr[i, j];
            }

            return qCount >= 6 && tCount >= 6 ? 1.0 * sum / Math.Max(qCount, tCount) : 0;
        }

        private class FingerprintRegion
        {
            private readonly GraphicsPath _path;

            public FingerprintRegion(Point[] polygon)
            {
                var types = new byte[polygon.Length];
                int i;
                for (i = 0; i < polygon.Length; i++)
                    types[i] = (byte) PathPointType.Line;
                _path = new GraphicsPath(polygon, types);
            }

            public bool Contains(Minutia m)
            {
                return _path.IsVisible(m.X, m.Y);
            }
        }

        private static Point[] GetBounds(QiFeatures features)
        {
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;
            foreach (var mtiaDesc in features.Minutiae)
            {
                if (minX > mtiaDesc.Minutia.X)
                    minX = mtiaDesc.Minutia.X;

                if (minY > mtiaDesc.Minutia.Y)
                    minY = mtiaDesc.Minutia.Y;

                if (maxX < mtiaDesc.Minutia.X)
                    maxX = mtiaDesc.Minutia.X;

                if (maxY < mtiaDesc.Minutia.Y)
                    maxY = mtiaDesc.Minutia.Y;
            }
            var resultingBound = new Point[5];
            resultingBound[0] = new Point(minX - 1, maxY + 1);
            resultingBound[1] = new Point(maxX + 1, maxY + 1);
            resultingBound[2] = new Point(maxX + 1, minY - 1);
            resultingBound[3] = new Point(minX - 1, minY - 1);
            resultingBound[4] = new Point(minX - 1, maxY + 1);
            return resultingBound;
        }

        private class PolygonMapper
        {
            private readonly double _dAngle;
            private readonly Minutia _query;
            private readonly Minutia _template;

            public PolygonMapper(Minutia query, Minutia template)
            {
                _dAngle = template.Angle - query.Angle;
                _template = template;
                _query = query;
            }

            public Point[] Map(Point[] polygon)
            {
                var newPolygon = new Point[polygon.Length];
                for (var i = 0; i < polygon.Length; i++)
                {
                    newPolygon[i].X =
                        Convert.ToInt16(
                            Math.Round((polygon[i].X - _query.X) * Math.Cos(_dAngle) -
                                       (polygon[i].Y - _query.Y) * Math.Sin(_dAngle) + _template.X));
                    newPolygon[i].Y =
                        Convert.ToInt16(
                            Math.Round((polygon[i].X - _query.X) * Math.Sin(_dAngle) +
                                       (polygon[i].Y - _query.Y) * Math.Cos(_dAngle) + _template.Y));
                }
                return newPolygon;
            }
        }
    }
}