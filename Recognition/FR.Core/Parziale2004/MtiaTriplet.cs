/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Parziale2004
{
    [Serializable]
    internal class MtiaTriplet
    {
        #region internal

        internal Minutia this[int i] => _minutiae[MtiaIdxs[i]];

        internal MtiaTriplet(short[] mIdxs, List<Minutia> minutiae)
        {
            this._minutiae = minutiae;
            MtiaIdxs = mIdxs;

            var mtiaArr = new Minutia[3];
            mtiaArr[0] = minutiae[MtiaIdxs[0]];
            mtiaArr[1] = minutiae[MtiaIdxs[1]];
            mtiaArr[2] = minutiae[MtiaIdxs[2]];

            _d[0] = MtiaEuclideanDistance.Compare(mtiaArr[0], mtiaArr[1]);
            _d[1] = MtiaEuclideanDistance.Compare(mtiaArr[1], mtiaArr[2]);
            _d[2] = MtiaEuclideanDistance.Compare(mtiaArr[0], mtiaArr[2]);
        }

        internal static double DistanceThreshold
        {
            get => _dThr;
            set => _dThr = value;
        }

        internal static double AlphaThreshold
        {
            get => _alphaThr;
            set => _alphaThr = value;
        }

        internal static double BetaThreshold
        {
            get => _betaThr;
            set => _betaThr = value;
        }

        internal short[] MtiaIdxs { get; } = new short[3];

        internal bool Match(MtiaTriplet target)
        {
            return MatchDistances(target) && MatchAlphaAngles(target) && MatchBetaAngles(target);
        }

        #endregion

        #region public

        public override int GetHashCode()
        {
            return MtiaIdxs[0] * 1000000 + MtiaIdxs[1] * 1000 + MtiaIdxs[2];
        }

        public override string ToString()
        {
            return $"{MtiaIdxs[0]},{MtiaIdxs[1]},{MtiaIdxs[2]}";
        }

        #endregion

        #region private methods

        private bool MatchDistances(MtiaTriplet compareTo)
        {
            var ratio = Math.Abs(_d[0] - compareTo._d[0]) / Math.Min(_d[0], compareTo._d[0]);
            if (ratio >= _dThr)
                return false;
            ratio = Math.Abs(_d[1] - compareTo._d[1]) / Math.Min(_d[1], compareTo._d[1]);
            if (ratio >= _dThr)
                return false;
            ratio = Math.Abs(_d[2] - compareTo._d[2]) / Math.Min(_d[2], compareTo._d[2]);
            if (ratio >= _dThr)
                return false;
            return true;
        }

        private bool MatchAlphaAngles(MtiaTriplet compareTo)
        {
            var idxArr = new[] {0, 1, 2, 0};
            for (var i = 0; i < 3; i++)
            {
                var j = idxArr[i + 1];
                var qMtiai = _minutiae[MtiaIdxs[i]];
                var qMtiaj = _minutiae[MtiaIdxs[j]];
                var qAlpha = Angle.DifferencePi(qMtiai.Angle, qMtiaj.Angle);

                var tMtiai = compareTo._minutiae[compareTo.MtiaIdxs[i]];
                var tMtiaj = compareTo._minutiae[compareTo.MtiaIdxs[j]];
                var tAlpha = Angle.DifferencePi(tMtiai.Angle, tMtiaj.Angle);

                var diff = Angle.DifferencePi(qAlpha, tAlpha);
                if (diff >= _alphaThr)
                    return false;
            }

            return true;
        }

        private bool MatchBetaAngles(MtiaTriplet compareTo)
        {
            for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                if (i != j)
                {
                    var qMtiai = _minutiae[MtiaIdxs[i]];
                    var qMtiaj = _minutiae[MtiaIdxs[j]];
                    double x = qMtiai.X - qMtiaj.X;
                    double y = qMtiai.Y - qMtiaj.Y;
                    var angleij = Angle.ComputeAngle(x, y);
                    var qBeta = Angle.DifferencePi(qMtiai.Angle, angleij);

                    var tMtiai = compareTo._minutiae[compareTo.MtiaIdxs[i]];
                    var tMtiaj = compareTo._minutiae[compareTo.MtiaIdxs[j]];
                    x = tMtiai.X - tMtiaj.X;
                    y = tMtiai.Y - tMtiaj.Y;
                    angleij = Angle.ComputeAngle(x, y);
                    var tBeta = Angle.DifferencePi(tMtiai.Angle, angleij);

                    var diff = Angle.DifferencePi(qBeta, tBeta);
                    if (diff >= _betaThr)
                        return false;
                }

            return true;
        }

        #endregion

        #region private fields

        private readonly List<Minutia> _minutiae;

        private readonly double[] _d = new double[3];

        [NonSerialized] private static readonly byte[][] Orders =
        {
            new[] {(byte) 0, (byte) 1, (byte) 2},
            new[] {(byte) 1, (byte) 2, (byte) 0},
            new[] {(byte) 2, (byte) 0, (byte) 1}
        };

        [NonSerialized] private static double _alphaThr = Math.PI / 12;

        [NonSerialized] private static double _betaThr = Math.PI / 9;

        [NonSerialized] private static double _dThr = 0.2;

        #endregion
    }
}