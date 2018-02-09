/*
 * Created by: Miguel Angel Medina P�rez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina P�rez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PatternRecognition.FingerprintRecognition.Core.Tico2003
{
    public class Mtk
    {
        #region public

        public int MtiaCountThr { get; set; } = 6;


        public int GlobalDistThr { get; set; } = 12;


        public double GlobalAngleThr
        {
            get => gAngThr * 180 / Math.PI;
            set => gAngThr = value * Math.PI / 180;
        }


        public double Match(Tico2003Features query, Tico2003Features template)
        {
            List<MinutiaPair> matchingMtiae;
            return Match(query, template, out matchingMtiae);
        }


        public double Match(object query, object template, out List<MinutiaPair> matchingMtiae)
        {
            var qTico2003Features = query as Tico2003Features;
            var tTico2003Features = template as Tico2003Features;
            try
            {
                matchingMtiae = null;
                var localMatchingMtiae = GetLocalMatchingMtiae(qTico2003Features, tTico2003Features);
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

                return Eval(qTico2003Features, tTico2003Features, matchingMtiae);
            }
            catch (Exception e)
            {
                if (query.GetType() != typeof(Tico2003Features) || template.GetType() != typeof(Tico2003Features))
                {
                    var msg = "Unable to match fingerprints: Invalid features type!";
                    throw new ArgumentOutOfRangeException(msg, e);
                }
                throw e;
            }
        }

        #endregion

        #region private

        private List<MinutiaPair> GetLocalMatchingMtiae(Tico2003Features query, Tico2003Features template)
        {
            var qIndex = new Dictionary<Minutia, int>(query.Minutiae.Count);
            var tIndex = new Dictionary<Minutia, int>(template.Minutiae.Count);
            var mtiaPairs = new List<MinutiaPair>(query.Minutiae.Count * template.Minutiae.Count);
            var simArr = new double[query.Minutiae.Count + 1, template.Minutiae.Count + 1];
            simArr[query.Minutiae.Count, template.Minutiae.Count] = 0;
            for (var i = 0; i < query.Minutiae.Count; i++)
            {
                var qMtia = query.Minutiae[i];
                qIndex.Add(qMtia, i);
                for (var j = 0; j < template.Minutiae.Count; j++)
                {
                    var tMtia = template.Minutiae[j];
                    //var currSim = Angle.AngleDif180(qMtia.Minutia.Angle, tMtia.Minutia.Angle) >= Math.PI / 4 ? 0 : qMtia.Compare(tMtia);
                    var currSim = qMtia.Compare(tMtia);
                    simArr[i, j] = currSim;
                    simArr[i, template.Minutiae.Count] += currSim;
                    simArr[query.Minutiae.Count, j] += currSim;
                }
            }
            for (var j = 0; j < template.Minutiae.Count; j++)
                tIndex.Add(template.Minutiae[j], j);

            for (var i = 0; i < query.Minutiae.Count; i++)
            {
                var qMtia = query.Minutiae[i];
                for (var j = 0; j < template.Minutiae.Count; j++)
                {
                    var tMtia = template.Minutiae[j];
                    var currPos = simArr[i, j] * simArr[i, j] /
                                  (simArr[i, template.Minutiae.Count] + simArr[query.Minutiae.Count, j] -
                                   3 * simArr[i, j]);
                    if (currPos != 0)
                    {
                        var currMtiaPair = new MinutiaPair
                        {
                            QueryMtia = qMtia,
                            TemplateMtia = tMtia,
                            MatchingValue = currPos
                        };

                        mtiaPairs.Add(currMtiaPair);
                    }
                }
            }
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

        private bool MatchDirections(Minutia query, Minutia template)
        {
            return Angle.DifferencePi(query.Angle, template.Angle) <= gAngThr;
        }

        private class MtiaPairComparer : IComparer<MinutiaPair>
        {
            public int Compare(MinutiaPair x, MinutiaPair y)
            {
                return x == y ? 0 : x.MatchingValue < y.MatchingValue ? 1 : -1;
            }
        }

        private double Eval(Tico2003Features query, Tico2003Features template, List<MinutiaPair> matchingPair)
        {
            if (matchingPair == null)
                return 0;

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
                sum += mtiaPair.MatchingValue;

            return qCount >= MtiaCountThr && tCount >= MtiaCountThr ? 1.0 * sum * sum / (qCount * tCount) : 0;
        }

        private class FingerprintRegion
        {
            private readonly GraphicsPath path;

            public FingerprintRegion(Point[] polygon)
            {
                var types = new byte[polygon.Length];
                int i;
                for (i = 0; i < polygon.Length; i++)
                    types[i] = (byte) PathPointType.Line;
                path = new GraphicsPath(polygon, types);
            }

            public bool Contains(Minutia m)
            {
                return path.IsVisible(m.X, m.Y);
            }
        }

        private Point[] GetBounds(Tico2003Features features)
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
            private readonly double dAngle;
            private readonly Minutia query;
            private readonly Minutia template;

            public PolygonMapper(Minutia query, Minutia template)
            {
                dAngle = template.Angle - query.Angle;
                this.template = template;
                this.query = query;
            }

            public Point[] Map(Point[] polygon)
            {
                var newPolygon = new Point[polygon.Length];
                for (var i = 0; i < polygon.Length; i++)
                {
                    newPolygon[i].X =
                        Convert.ToInt16(
                            Math.Round((polygon[i].X - query.X) * Math.Cos(dAngle) -
                                       (polygon[i].Y - query.Y) * Math.Sin(dAngle) + template.X));
                    newPolygon[i].Y =
                        Convert.ToInt16(
                            Math.Round((polygon[i].X - query.X) * Math.Sin(dAngle) +
                                       (polygon[i].Y - query.Y) * Math.Cos(dAngle) + template.Y));
                }
                return newPolygon;
            }
        }

        private double gAngThr = Math.PI / 6;

        private readonly MtiaEuclideanDistance dist = new MtiaEuclideanDistance();

        #endregion
    }
}