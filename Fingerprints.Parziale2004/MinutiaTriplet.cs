/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using Fingerprints.Computation;
using Fingerprints.Model;

namespace Fingerprints.Parziale2004
{
    [Serializable]
    public class MinutiaTriplet
    {
        public List<Minutia> Minutiae { get; set; }

        public double[] D { get; set; } = new double[3];

        public Minutia this[int i] => Minutiae[MtiaIdxs[i]];

        public static double DistanceThreshold { get; set; } = 0.2;

        public static double AlphaThreshold { get; set; } = Math.PI / 12;

        public static double BetaThreshold { get; set; } = Math.PI / 9;

        public short[] MtiaIdxs { get; set; }

        public MinutiaTriplet()
        {

        }

        public MinutiaTriplet(short[] mIdxs, List<Minutia> minutiae)
        {
            Minutiae = minutiae;
            MtiaIdxs = mIdxs;

            var mtiaArr = new Minutia[3];
            mtiaArr[0] = minutiae[MtiaIdxs[0]];
            mtiaArr[1] = minutiae[MtiaIdxs[1]];
            mtiaArr[2] = minutiae[MtiaIdxs[2]];

            D[0] = MtiaEuclideanDistance.Compare(mtiaArr[0], mtiaArr[1]);
            D[1] = MtiaEuclideanDistance.Compare(mtiaArr[1], mtiaArr[2]);
            D[2] = MtiaEuclideanDistance.Compare(mtiaArr[0], mtiaArr[2]);
        }

        public bool Match(MinutiaTriplet target)
        {
            return MatchDistances(target) && MatchAlphaAngles(target) && MatchBetaAngles(target);
        }

        public override int GetHashCode()
        {
            return MtiaIdxs[0] * 1000000 + MtiaIdxs[1] * 1000 + MtiaIdxs[2];
        }

        public override string ToString()
        {
            return $"{MtiaIdxs[0]},{MtiaIdxs[1]},{MtiaIdxs[2]}";
        }

        #region private methods

        private bool MatchDistances(MinutiaTriplet compareTo)
        {
            var ratio = Math.Abs(D[0] - compareTo.D[0]) / Math.Min(D[0], compareTo.D[0]);
            if (ratio >= DistanceThreshold)
                return false;
            ratio = Math.Abs(D[1] - compareTo.D[1]) / Math.Min(D[1], compareTo.D[1]);
            if (ratio >= DistanceThreshold)
                return false;
            ratio = Math.Abs(D[2] - compareTo.D[2]) / Math.Min(D[2], compareTo.D[2]);
            return !(ratio >= DistanceThreshold);
        }

        private bool MatchAlphaAngles(MinutiaTriplet compareTo)
        {
            var idxArr = new[] {0, 1, 2, 0};
            for (var i = 0; i < 3; i++)
            {
                var j = idxArr[i + 1];
                var qMtiai = Minutiae[MtiaIdxs[i]];
                var qMtiaj = Minutiae[MtiaIdxs[j]];
                var qAlpha = Angle.DifferencePi(qMtiai.Angle, qMtiaj.Angle);

                var tMtiai = compareTo.Minutiae[compareTo.MtiaIdxs[i]];
                var tMtiaj = compareTo.Minutiae[compareTo.MtiaIdxs[j]];
                var tAlpha = Angle.DifferencePi(tMtiai.Angle, tMtiaj.Angle);

                var diff = Angle.DifferencePi(qAlpha, tAlpha);
                if (diff >= AlphaThreshold)
                    return false;
            }

            return true;
        }

        private bool MatchBetaAngles(MinutiaTriplet compareTo)
        {
            for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                if (i != j)
                {
                    var qMtiai = Minutiae[MtiaIdxs[i]];
                    var qMtiaj = Minutiae[MtiaIdxs[j]];
                    double x = qMtiai.X - qMtiaj.X;
                    double y = qMtiai.Y - qMtiaj.Y;
                    var angleij = Angle.ComputeAngle(x, y);
                    var qBeta = Angle.DifferencePi(qMtiai.Angle, angleij);

                    var tMtiai = compareTo.Minutiae[compareTo.MtiaIdxs[i]];
                    var tMtiaj = compareTo.Minutiae[compareTo.MtiaIdxs[j]];
                    x = tMtiai.X - tMtiaj.X;
                    y = tMtiai.Y - tMtiaj.Y;
                    angleij = Angle.ComputeAngle(x, y);
                    var tBeta = Angle.DifferencePi(tMtiai.Angle, angleij);

                    var diff = Angle.DifferencePi(qBeta, tBeta);
                    if (diff >= BetaThreshold)
                        return false;
                }

            return true;
        }

        #endregion
    }
}