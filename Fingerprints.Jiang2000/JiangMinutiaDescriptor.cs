/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: 
 */

using System;
using System.Collections.Generic;
using Fingerprints.Computation;
using Fingerprints.Model;

namespace Fingerprints.Jiang2000
{
    [Serializable]
    public class JiangMinutiaDescriptor
    {
        public readonly short MainMtiaIdx, NearestMtiaIdx, FarthestMtiaIdx;

        public double Dist0 { get; set; }
        public double Dist1 { get; set; }

        public double Alpha0 { get; set; }
        public double Alpha1 { get; set; }
        public double Beta0 { get; set; }
        public double Beta1 { get; set; }

        public byte RidgeCount0 { get; set; }
        public byte RidgeCount1 { get; set; }

        public List<Minutia> Minutiae { get; set; }

        public static double AngleThreshold { get; set; } = Math.PI / 6;

        public static double DistanceThreshold { get; set; } = 12;

        //for serialization purpose
        public JiangMinutiaDescriptor()
        {
        }

        #region internal

        internal JiangMinutiaDescriptor(SkeletonImage skeletonImage, List<Minutia> minutiae, short mainMtiaIdx,
            short mtiaIdx0, short mtiaIdx1)
        {
            Minutiae = minutiae;
            MainMtiaIdx = mainMtiaIdx;
            NearestMtiaIdx = mtiaIdx0;
            FarthestMtiaIdx = mtiaIdx1;

            Dist0 = MtiaEuclideanDistance.Compare(MainMinutia, NearestMtia);
            Dist1 = MtiaEuclideanDistance.Compare(MainMinutia, FarthestMtia);
            if (Dist1 < Dist0)
            {
                NearestMtiaIdx = mtiaIdx1;
                FarthestMtiaIdx = mtiaIdx0;
                var temp = Dist0;
                Dist0 = Dist1;
                Dist1 = temp;
            }

            Alpha0 = ComputeAlpha(MainMinutia, NearestMtia);
            Alpha1 = ComputeAlpha(MainMinutia, FarthestMtia);

            Beta0 = ComputeBeta(MainMinutia, NearestMtia);
            Beta1 = ComputeBeta(MainMinutia, FarthestMtia);

            RidgeCount0 = ComputeRidgeCount(skeletonImage, MainMinutia, NearestMtia);
            RidgeCount1 = ComputeRidgeCount(skeletonImage, MainMinutia, FarthestMtia);
        }

        public static implicit operator Minutia(JiangMinutiaDescriptor desc)
        {
            return desc.MainMinutia;
        }

        internal Minutia MainMinutia => Minutiae[MainMtiaIdx];

        internal Minutia NearestMtia => Minutiae[NearestMtiaIdx];

        internal Minutia FarthestMtia => Minutiae[FarthestMtiaIdx];

        public override int GetHashCode()
        {
            return MainMtiaIdx * 1000000 + NearestMtiaIdx * 1000 + FarthestMtiaIdx;
        }

        public override string ToString()
        {
            return $"{MainMtiaIdx},{NearestMtiaIdx},{FarthestMtiaIdx}";
        }

        internal double RotationInvariantMatch(JiangMinutiaDescriptor target)
        {
            var distDiff = MatchDistances(target);
            var alphaDiff = MatchAlphaAngles(target);
            var betaDiff = MatchBetaAngles(target);
            var ridgeCountDiff = MatchRidgeCounts(target);
            var mtiaTypeDiff = MatchByType(target);

            var dist = Math.Sqrt(distDiff + alphaDiff + betaDiff + ridgeCountDiff + mtiaTypeDiff);

            return dist < 66 ? (66 - dist) / 66 : 0;
        }

        internal double NoRotateMatch(JiangMinutiaDescriptor target)
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

        private static byte ComputeRidgeCount(SkeletonImage skeletonImage, Minutia mtia0, Minutia mtia1)
        {
            return skeletonImage.RidgeCount(mtia0.X, mtia0.Y, mtia1.X, mtia1.Y);
        }

        private static double ComputeAlpha(Minutia mtia0, Minutia mtia1)
        {
            double x = mtia0.X - mtia1.X;
            double y = mtia0.Y - mtia1.Y;
            return Angle.Difference2Pi(mtia0.Angle, Angle.ComputeAngle(x, y));
        }

        private static double ComputeBeta(Minutia mtia0, Minutia mtia1)
        {
            return Angle.Difference2Pi(mtia0.Angle, mtia1.Angle);
        }

        private double MatchDistances(JiangMinutiaDescriptor target)
        {
            var diff0 = Math.Abs(target.Dist0 - Dist0);
            var diff1 = Math.Abs(target.Dist1 - Dist1);

            return diff0 + diff1;
        }

        private bool MatchMtiaDirections(JiangMinutiaDescriptor target)
        {
            var diff = Angle.DifferencePi(target.MainMinutia.Angle, MainMinutia.Angle);
            if (diff >= Math.PI / 4)
                return false;
            diff = Angle.DifferencePi(target.NearestMtia.Angle, NearestMtia.Angle);
            if (diff >= Math.PI / 4)
                return false;
            diff = Angle.DifferencePi(target.FarthestMtia.Angle, FarthestMtia.Angle);
            return !(diff >= Math.PI / 4);
        }

        private double MatchRidgeCounts(JiangMinutiaDescriptor target)
        {
            double diff0 = Math.Abs(target.RidgeCount0 - RidgeCount0);
            double diff1 = Math.Abs(target.RidgeCount1 - RidgeCount1);

            return 3 * (Math.Pow(diff0, 2) + Math.Pow(diff1, 2));
        }

        private double MatchAlphaAngles(JiangMinutiaDescriptor target)
        {
            var diff0 = Angle.DifferencePi(target.Alpha0, Alpha0);
            var diff1 = Angle.DifferencePi(target.Alpha1, Alpha1);

            return 54 * (Math.Pow(diff0, 2) + Math.Pow(diff1, 2)) / Math.PI;
        }

        private double MatchBetaAngles(JiangMinutiaDescriptor target)
        {
            var diff0 = Angle.DifferencePi(target.Beta0, Beta0);
            var diff1 = Angle.DifferencePi(target.Beta1, Beta1);

            return 54 * (Math.Pow(diff0, 2) + Math.Pow(diff1, 2)) / Math.PI;
        }

        private double MatchByType(JiangMinutiaDescriptor target)
        {
            var diff0 = target.MainMinutia.MinutiaType == MainMinutia.MinutiaType ? 0 : 1;
            var diff1 = target.NearestMtia.MinutiaType == NearestMtia.MinutiaType ? 0 : 1;
            var diff2 = target.FarthestMtia.MinutiaType == FarthestMtia.MinutiaType ? 0 : 1;
            return 3 * (diff0 + diff1 + diff2);
        }

        #endregion
    }
}