/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: 
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Jiang2000
{
    [Serializable]
    internal class JyMtiaDescriptor
    {
        #region internal

        internal JyMtiaDescriptor(SkeletonImage skeletonImage, List<Minutia> minutiae, short mainMtiaIdx,
            short mtiaIdx0, short mtiaIdx1)
        {
            this._minutiae = minutiae;
            this._mainMtiaIdx = mainMtiaIdx;
            _nearestMtiaIdx = mtiaIdx0;
            _farthestMtiaIdx = mtiaIdx1;

            _dist0 = MtiaEuclideanDistance.Compare(MainMinutia, NearestMtia);
            _dist1 = MtiaEuclideanDistance.Compare(MainMinutia, FarthestMtia);
            if (_dist1 < _dist0)
            {
                _nearestMtiaIdx = mtiaIdx1;
                _farthestMtiaIdx = mtiaIdx0;
                var temp = _dist0;
                _dist0 = _dist1;
                _dist1 = temp;
            }

            _alpha0 = ComputeAlpha(MainMinutia, NearestMtia);
            _alpha1 = ComputeAlpha(MainMinutia, FarthestMtia);

            _beta0 = ComputeBeta(MainMinutia, NearestMtia);
            _beta1 = ComputeBeta(MainMinutia, FarthestMtia);

            _ridgeCount0 = ComputeRidgeCount(skeletonImage, MainMinutia, NearestMtia);
            _ridgeCount1 = ComputeRidgeCount(skeletonImage, MainMinutia, FarthestMtia);
        }

        public static implicit operator Minutia(JyMtiaDescriptor desc)
        {
            return desc.MainMinutia;
        }

        internal Minutia MainMinutia => _minutiae[_mainMtiaIdx];

        internal Minutia NearestMtia => _minutiae[_nearestMtiaIdx];

        internal Minutia FarthestMtia => _minutiae[_farthestMtiaIdx];

        internal static double DistanceThreshold
        {
            get => _dThr;
            set => _dThr = value;
        }

        internal static double AngleThreshold
        {
            get => _aThr;
            set => _aThr = value;
        }

        public override int GetHashCode()
        {
            return _mainMtiaIdx * 1000000 + _nearestMtiaIdx * 1000 + _farthestMtiaIdx;
        }

        public override string ToString()
        {
            return $"{_mainMtiaIdx},{_nearestMtiaIdx},{_farthestMtiaIdx}";
        }

        internal double RotationInvariantMatch(JyMtiaDescriptor target)
        {
            var distDiff = MatchDistances(target);
            var alphaDiff = MatchAlphaAngles(target);
            var betaDiff = MatchBetaAngles(target);
            var ridgeCountDiff = MatchRidgeCounts(target);
            var mtiaTypeDiff = MatchByType(target);

            var dist = Math.Sqrt(distDiff + alphaDiff + betaDiff + ridgeCountDiff + mtiaTypeDiff);

            return dist < 66 ? (66 - dist) / 66 : 0;
        }

        internal double NoRotateMatch(JyMtiaDescriptor target)
        {
            if (!MatchMtiaDirections(target))
                return 0;
            var distDiff = MatchDistances(target);
            var alphaDiff = MatchAlphaAngles(target);
            var betaDiff = MatchBetaAngles(target);
            var ridgeCountDiff = MatchRidgeCounts(target);
            var mtiaTypeDiff = MatchByType(target);

            var dist = Math.Sqrt(distDiff + alphaDiff + betaDiff + ridgeCountDiff + mtiaTypeDiff);

            return dist < 66 ? (66 - dist) / 66 : 0;
        }

        #endregion

        #region private methods

        private byte ComputeRidgeCount(SkeletonImage skeletonImage, Minutia mtia0, Minutia mtia1)
        {
            return skeletonImage.RidgeCount(mtia0.X, mtia0.Y, mtia1.X, mtia1.Y);
        }

        private double ComputeAlpha(Minutia mtia0, Minutia mtia1)
        {
            double x = mtia0.X - mtia1.X;
            double y = mtia0.Y - mtia1.Y;
            return Angle.Difference2Pi(mtia0.Angle, Angle.ComputeAngle(x, y));
        }

        private double ComputeBeta(Minutia mtia0, Minutia mtia1)
        {
            return Angle.Difference2Pi(mtia0.Angle, mtia1.Angle);
        }

        private double MatchDistances(JyMtiaDescriptor target)
        {
            var diff0 = Math.Abs(target._dist0 - _dist0);
            var diff1 = Math.Abs(target._dist1 - _dist1);

            return diff0 + diff1;
        }

        private bool MatchMtiaDirections(JyMtiaDescriptor target)
        {
            var diff = Angle.DifferencePi(target.MainMinutia.Angle, MainMinutia.Angle);
            if (diff >= Math.PI / 4)
                return false;
            diff = Angle.DifferencePi(target.NearestMtia.Angle, NearestMtia.Angle);
            if (diff >= Math.PI / 4)
                return false;
            diff = Angle.DifferencePi(target.FarthestMtia.Angle, FarthestMtia.Angle);
            if (diff >= Math.PI / 4)
                return false;

            return true;
        }

        private double MatchRidgeCounts(JyMtiaDescriptor target)
        {
            double diff0 = Math.Abs(target._ridgeCount0 - _ridgeCount0);
            double diff1 = Math.Abs(target._ridgeCount1 - _ridgeCount1);

            return 3 * (Math.Pow(diff0, 2) + Math.Pow(diff1, 2));
        }

        private double MatchAlphaAngles(JyMtiaDescriptor target)
        {
            var diff0 = Angle.DifferencePi(target._alpha0, _alpha0);
            var diff1 = Angle.DifferencePi(target._alpha1, _alpha1);

            return 54 * (Math.Pow(diff0, 2) + Math.Pow(diff1, 2)) / Math.PI;
        }

        private double MatchBetaAngles(JyMtiaDescriptor target)
        {
            var diff0 = Angle.DifferencePi(target._beta0, _beta0);
            var diff1 = Angle.DifferencePi(target._beta1, _beta1);

            return 54 * (Math.Pow(diff0, 2) + Math.Pow(diff1, 2)) / Math.PI;
        }

        private double MatchByType(JyMtiaDescriptor target)
        {
            var diff0 = target.MainMinutia.MinutiaType == MainMinutia.MinutiaType ? 0 : 1;
            var diff1 = target.NearestMtia.MinutiaType == NearestMtia.MinutiaType ? 0 : 1;
            var diff2 = target.FarthestMtia.MinutiaType == FarthestMtia.MinutiaType ? 0 : 1;
            return 3 * (diff0 + diff1 + diff2);
        }

        #endregion

        #region private fields

        private readonly short _mainMtiaIdx, _nearestMtiaIdx, _farthestMtiaIdx;

        private readonly double _dist0, _dist1;

        private readonly double _alpha0, _alpha1, _beta0, _beta1;

        private readonly byte _ridgeCount0;
        private readonly byte _ridgeCount1;

        private readonly List<Minutia> _minutiae;

        [NonSerialized] private static double _aThr = Math.PI / 6;

        [NonSerialized] private static double _dThr = 12;

        #endregion
    }
}