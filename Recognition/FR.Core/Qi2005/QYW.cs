/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Andrés Eduardo Gutiérrez Rodríguez (andres@bioplantas.cu)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PatternRecognition.FingerprintRecognition.Core.Qi2005
{
    public class Qyw
    {
        #region public

        public int GlobalDistThr { get; set; } = 8;


        public double GlobalAngleThr
        {
            get => _gAngThr * 180 / Math.PI;
            set => _gAngThr = value * Math.PI / 180;
        }


        public double Match(Qi2005Features query, Qi2005Features template)
        {
            List<MinutiaPair> matchingMtiae;
            return Match(query, template, out matchingMtiae);
        }


        public double Match(object query, object template, out List<MinutiaPair> matchingMtiae)
        {
            var qQi2005Features = query as Qi2005Features;
            var tQi2005Features = template as Qi2005Features;
            try
            {
                matchingMtiae = null;
                double[,] simArr;
                var localMatchingMtiae = GetLocalMatchingMtiae(qQi2005Features, tQi2005Features, out simArr);
                if (localMatchingMtiae.Count == 0)
                    return 0;
                matchingMtiae = GetGlobalMatchingMtiae(localMatchingMtiae, localMatchingMtiae[0]);

                return Eval(qQi2005Features, tQi2005Features, matchingMtiae, simArr);
            }
            catch (Exception e)
            {
                if (query.GetType() != typeof(Qi2005Features) || template.GetType() != typeof(Qi2005Features))
                {
                    var msg = "Unable to match fingerprints: Invalid features type!";
                    throw new ArgumentOutOfRangeException(msg, e);
                }
                throw e;
            }
        }

        #endregion

        #region private

        private IList<MinutiaPair> GetLocalMatchingMtiae(Qi2005Features query, Qi2005Features template,
            out double[,] simArr)
        {
            var mtiaPairs = new List<MinutiaPair>(query.Minutiae.Count * template.Minutiae.Count);
            simArr = new double[query.Minutiae.Count, template.Minutiae.Count];

            for (var i = 0; i < query.Minutiae.Count; i++)
            {
                var qMtia = query.Minutiae[i];
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
            mtiaPairs.Sort(new MtiaPairComparer());
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

        private class MtiaPairComparer : IComparer<MinutiaPair>
        {
            public int Compare(MinutiaPair x, MinutiaPair y)
            {
                return x == y ? 0 : x.MatchingValue < y.MatchingValue ? 1 : -1;
            }
        }

        private double Eval(Qi2005Features query, Qi2005Features template, List<MinutiaPair> matchingPair,
            double[,] simArr)
        {
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

        private Point[] GetBounds(Qi2005Features features)
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
                this._template = template;
                this._query = query;
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

        private double _gAngThr = Math.PI / 6;

        #endregion
    }
}