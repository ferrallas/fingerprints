/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Milton García Borroto (milton.garcia@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2011
{
    [Serializable]
    internal class MTriplet
    {
        #region Public

        public Minutia this[int i] => _minutiae[MtiaIdxs[i]];

        public MTriplet(short[] mIdxs, List<Minutia> minutiae)
        {
            this._minutiae = minutiae;

            MtiaIdxs = ArrangeClockwise(mIdxs, minutiae);

            var mtiaArr = new Minutia[3];
            mtiaArr[0] = minutiae[MtiaIdxs[0]];
            mtiaArr[1] = minutiae[MtiaIdxs[1]];
            mtiaArr[2] = minutiae[MtiaIdxs[2]];

            // Computing distances and maxBeta angle
            _d[0] = MtiaEuclideanDistance.Compare(mtiaArr[0], mtiaArr[1]);
            _d[1] = MtiaEuclideanDistance.Compare(mtiaArr[1], mtiaArr[2]);
            _d[2] = MtiaEuclideanDistance.Compare(mtiaArr[0], mtiaArr[2]);
            _maxBeta = double.MinValue;
            for (byte i = 0; i < 2; i++)
            for (var j = (byte) (i + 1); j < 3; j++)
            {
                if (_d[_sortedDistIdxs[i]] > _d[_sortedDistIdxs[j]])
                {
                    var temp = _sortedDistIdxs[i];
                    _sortedDistIdxs[i] = _sortedDistIdxs[j];
                    _sortedDistIdxs[j] = temp;
                }
                var currBeta = Angle.DifferencePi(mtiaArr[i].Angle, mtiaArr[j].Angle);
                _maxBeta = Math.Max(_maxBeta, Math.Max(currBeta, currBeta));
            }
        }

        public static double DistanceThreshold
        {
            get => _dThr;
            set => _dThr = value;
        }

        public static double AngleThreshold
        {
            get => _aThr;
            set => _aThr = value;
        }

        public double MinDistance => _d[_sortedDistIdxs[0]];

        public double MidDistance => _d[_sortedDistIdxs[1]];

        public double MaxDistance => _d[_sortedDistIdxs[2]];

        public short[] MtiaIdxs { get; } = new short[3];

        public override int GetHashCode()
        {
            int max = Math.Max(MtiaIdxs[0], Math.Max(MtiaIdxs[1], MtiaIdxs[2]));
            int min = Math.Min(MtiaIdxs[0], Math.Min(MtiaIdxs[1], MtiaIdxs[2]));
            var med = MtiaIdxs[0] + MtiaIdxs[1] + MtiaIdxs[2] - max - min;
            return max * 1000000 + med * 1000 + min;
        }

        public override string ToString()
        {
            return $"{MtiaIdxs[0]},{MtiaIdxs[1]},{MtiaIdxs[2]}";
        }

        public double Match(MTriplet target, out byte[] mtiaOrder)
        {
            byte[] matchOrder = null;
            double maxSimil = 0;
            if (Math.Abs(MaxDistance - target.MaxDistance) < _dThr &&
                Math.Abs(MidDistance - target.MidDistance) < _dThr && Math.Abs(MinDistance - target.MinDistance) < _dThr)
                foreach (var order in Orders)
                {
                    var dirSim = MatchMtiaDirections(target, order);
                    if (dirSim == 0)
                        continue;

                    var distSim = MatchDistances(target, order);
                    if (distSim == 0)
                        continue;

                    var betaSim = MatchBetaAngles(target, order);
                    if (betaSim == 0)
                        continue;

                    var alphaSim = MatchAlphaAngles(target, order);
                    if (alphaSim == 0)
                        continue;

                    var currentSimil = 1 - (1 - distSim) * (1 - alphaSim) * (1 - betaSim);

                    if (currentSimil > maxSimil)
                    {
                        matchOrder = order;
                        maxSimil = currentSimil;
                    }
                }
            mtiaOrder = matchOrder;
            return maxSimil;
        }

        #endregion

        #region private methods

        private double MatchDistances(MTriplet compareTo, byte[] order)
        {
            var diff0 = Math.Abs(_d[0] - compareTo._d[order[0]]);
            if (diff0 > _dThr)
                return 0;
            var diff1 = Math.Abs(_d[1] - compareTo._d[order[1]]);
            if (diff1 > _dThr)
                return 0;
            var diff2 = Math.Abs(_d[2] - compareTo._d[order[2]]);
            if (diff2 > _dThr)
                return 0;
            return 1 - Math.Max(diff0, Math.Max(diff1, diff2)) / _dThr;
        }

        private double MatchBetaAngles(MTriplet compareTo, byte[] order)
        {
            var idxArr = new[] {0, 1, 2, 0};
            double maxdiff = 0;
            for (var i = 0; i < 3; i++)
            {
                var j = idxArr[i + 1];
                var qMtiai = _minutiae[MtiaIdxs[i]];
                var qMtiaj = _minutiae[MtiaIdxs[j]];
                var qbeta = Angle.Difference2Pi(qMtiai.Angle, qMtiaj.Angle);

                var tMtiai = compareTo._minutiae[compareTo.MtiaIdxs[order[i]]];
                var tMtiaj = compareTo._minutiae[compareTo.MtiaIdxs[order[j]]];
                var tbeta = Angle.Difference2Pi(tMtiai.Angle, tMtiaj.Angle);

                var diff = Angle.DifferencePi(qbeta, tbeta);
                if (diff >= _aThr)
                    return 0;
                if (diff > maxdiff)
                    maxdiff = diff;
            }

            return 1 - maxdiff / _aThr;
        }

        private double MatchMtiaDirections(MTriplet compareTo, byte[] order)
        {
            double maxdiff = 0;
            for (var i = 0; i < 3; i++)
            {
                var qMtiai = _minutiae[MtiaIdxs[i]];
                var tMtiai = compareTo._minutiae[compareTo.MtiaIdxs[order[i]]];
                var diff = Angle.DifferencePi(qMtiai.Angle, tMtiai.Angle);
                if (diff >= Math.PI / 4)
                    return 0;
                if (diff > maxdiff)
                    maxdiff = diff;
            }

            return 1;
        }

        private double MatchAlphaAngles(MTriplet compareTo, byte[] order)
        {
            double maxdiff = 0;
            for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                if (i != j)
                {
                    var qMtiai = _minutiae[MtiaIdxs[i]];
                    var qMtiaj = _minutiae[MtiaIdxs[j]];
                    double x = qMtiai.X - qMtiaj.X;
                    double y = qMtiai.Y - qMtiaj.Y;
                    var angleij = Angle.ComputeAngle(x, y);
                    var qAlpha = Angle.Difference2Pi(qMtiai.Angle, angleij);

                    var tMtiai = compareTo._minutiae[compareTo.MtiaIdxs[order[i]]];
                    var tMtiaj = compareTo._minutiae[compareTo.MtiaIdxs[order[j]]];
                    x = tMtiai.X - tMtiaj.X;
                    y = tMtiai.Y - tMtiaj.Y;
                    angleij = Angle.ComputeAngle(x, y);
                    var tAlpha = Angle.Difference2Pi(tMtiai.Angle, angleij);

                    var diff = Angle.DifferencePi(qAlpha, tAlpha);
                    if (diff >= _aThr)
                        return 0;
                    if (diff > maxdiff)
                        maxdiff = diff;
                }

            return 1 - maxdiff / _aThr;
        }

        private class DoubleComparer : IComparer<double>
        {
            public int Compare(double x, double y)
            {
                return x > y ? 1 : -1;
            }
        }

        private short[] ArrangeClockwise(short[] idxs, List<Minutia> minutiae)
        {
            var centerX = ((1.0 * minutiae[idxs[0]].X + minutiae[idxs[1]].X) / 2 + minutiae[idxs[2]].X) / 2;
            var centerY = ((1.0 * minutiae[idxs[0]].Y + minutiae[idxs[1]].Y) / 2 + minutiae[idxs[2]].Y) / 2;
            var list = new SortedList<double, short>(3, new DoubleComparer());
            for (var i = 0; i < 3; i++)
            {
                var minutia = minutiae[idxs[i]];
                var dx = minutia.X - centerX;
                var dy = minutia.Y - centerY;
                if (dx == 0 && dy == 0)
                    return idxs;
                list.Add(Angle.ComputeAngle(dx, dy), idxs[i]);
            }
            return new[] {list.Values[0], list.Values[1], list.Values[2]};
        }

        #endregion

        #region private fields

        private readonly List<Minutia> _minutiae;

        private readonly double[] _d = new double[3];

        private readonly byte[] _sortedDistIdxs = {0, 1, 2};

        private readonly double _maxBeta;

        [NonSerialized] public static readonly byte[][] Orders =
        {
            new[] {(byte) 0, (byte) 1, (byte) 2},
            new[] {(byte) 1, (byte) 2, (byte) 0},
            new[] {(byte) 2, (byte) 0, (byte) 1}
        };

        [NonSerialized] private static double _aThr = Math.PI / 6;

        [NonSerialized] private static double _dThr = 12;

        #endregion
    }
}