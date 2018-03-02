/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Milton García Borroto (milton.garcia@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2012
{
    [Serializable]
    public class MTriplet
    {
        [NonSerialized]
        private static readonly byte[][] Orders =
        {
            new[] {(byte) 0, (byte) 1, (byte) 2},
            new[] {(byte) 1, (byte) 2, (byte) 0},
            new[] {(byte) 2, (byte) 0, (byte) 1}
        };

        private const double _aThr = Math.PI / 6;

        internal const double DistanceThreshold = 12;

        public List<Minutia> Minutiae { get; set; }

        public double[] D { get; set; } = new double[3];

        public  byte[] SortedDistIdxs { get; set; } = { 0, 1, 2 };

        public double MaxBeta { get; set; }

        public short[] MtiaIdxs { get; set; }

        private double MinDistance => D[SortedDistIdxs[0]];

        private double MidDistance => D[SortedDistIdxs[1]];

        public double MaxDistance => D[SortedDistIdxs[2]];

        public Minutia this[int i] => Minutiae[MtiaIdxs[i]];

        public MTriplet() { }

        public MTriplet(short[] mIdxs, List<Minutia> minutiae)
        {
            this.Minutiae = minutiae;

            MtiaIdxs = ArrangeClockwise(mIdxs, minutiae);

            var mtiaArr = new Minutia[3];
            mtiaArr[0] = minutiae[MtiaIdxs[0]];
            mtiaArr[1] = minutiae[MtiaIdxs[1]];
            mtiaArr[2] = minutiae[MtiaIdxs[2]];

            // Computing distances and maxBeta angle
            D[0] = MtiaEuclideanDistance.Compare(mtiaArr[0], mtiaArr[1]);
            D[1] = MtiaEuclideanDistance.Compare(mtiaArr[1], mtiaArr[2]);
            D[2] = MtiaEuclideanDistance.Compare(mtiaArr[0], mtiaArr[2]);
            MaxBeta = double.MinValue;
            for (byte i = 0; i < 2; i++)
            for (var j = (byte) (i + 1); j < 3; j++)
            {
                if (D[SortedDistIdxs[i]] > D[SortedDistIdxs[j]])
                {
                    var temp = SortedDistIdxs[i];
                    SortedDistIdxs[i] = SortedDistIdxs[j];
                    SortedDistIdxs[j] = temp;
                }
                var alpha = mtiaArr[i].Angle;
                var beta = mtiaArr[j].Angle;
                var diff = Math.Abs(beta - alpha);
                var currBeta = Math.Min(diff, 2 * Math.PI - diff);
                MaxBeta = Math.Max(MaxBeta, Math.Max(currBeta, currBeta));
            }
        }

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

        public double NoRotateMatch(MTriplet target, out byte[] mtiaOrder)
        {
            byte[] matchOrder = null;
            double maxSimil = 0;
            if (Math.Abs(MaxDistance - target.MaxDistance) < DistanceThreshold &&
                Math.Abs(MidDistance - target.MidDistance) < DistanceThreshold &&
                Math.Abs(MinDistance - target.MinDistance) <
                DistanceThreshold /* && Angle.AngleDif180(maxBeta, target.maxBeta) < aThr*/)
                foreach (var order in Orders)
                {
                    var dirSim = MatchMtiaDirections(target, order);
                    if (Math.Abs(dirSim) < double.Epsilon)
                        continue;

                    var distSim = MatchDistances(target, order);
                    if (Math.Abs(distSim) < double.Epsilon)
                        continue;

                    var betaSim = MatchBetaAngles(target, order);
                    if (Math.Abs(betaSim) < double.Epsilon)
                        continue;

                    var alphaSim = MatchAlphaAngles(target, order);
                    if (Math.Abs(alphaSim) < double.Epsilon)
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

        #region private methods

        private double MatchDistances(MTriplet compareTo, byte[] order)
        {
            var diff0 = Math.Abs(D[0] - compareTo.D[order[0]]);
            if (diff0 > DistanceThreshold)
                return 0;
            var diff1 = Math.Abs(D[1] - compareTo.D[order[1]]);
            if (diff1 > DistanceThreshold)
                return 0;
            var diff2 = Math.Abs(D[2] - compareTo.D[order[2]]);
            if (diff2 > DistanceThreshold)
                return 0;
            return 1 - Math.Max(diff0, Math.Max(diff1, diff2)) / DistanceThreshold;
        }

        private double MatchBetaAngles(MTriplet compareTo, byte[] order)
        {
            var idxArr = new[] {0, 1, 2, 0};
            double maxdiff = 0;
            for (var i = 0; i < 3; i++)
            {
                var j = idxArr[i + 1];
                var qMtiai = Minutiae[MtiaIdxs[i]];
                var qMtiaj = Minutiae[MtiaIdxs[j]];
                var qbeta = Angle.Difference2Pi(qMtiai.Angle, qMtiaj.Angle);

                var tMtiai = compareTo.Minutiae[compareTo.MtiaIdxs[order[i]]];
                var tMtiaj = compareTo.Minutiae[compareTo.MtiaIdxs[order[j]]];
                var tbeta = Angle.Difference2Pi(tMtiai.Angle, tMtiaj.Angle);

                var diff1 = Math.Abs(tbeta - qbeta);
                var diff = Math.Min(diff1, 2 * Math.PI - diff1);
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
                var qMtiai = Minutiae[MtiaIdxs[i]];
                var tMtiai = compareTo.Minutiae[compareTo.MtiaIdxs[order[i]]];
                var alpha = qMtiai.Angle;
                var beta = tMtiai.Angle;
                var diff1 = Math.Abs(beta - alpha);
                var diff = Math.Min(diff1, 2 * Math.PI - diff1);
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
                    var qMtiai = Minutiae[MtiaIdxs[i]];
                    var qMtiaj = Minutiae[MtiaIdxs[j]];
                    double x = qMtiai.X - qMtiaj.X;
                    double y = qMtiai.Y - qMtiaj.Y;
                    var angleij = Angle.ComputeAngle(x, y);
                    var qAlpha = Angle.Difference2Pi(qMtiai.Angle, angleij);

                    var tMtiai = compareTo.Minutiae[compareTo.MtiaIdxs[order[i]]];
                    var tMtiaj = compareTo.Minutiae[compareTo.MtiaIdxs[order[j]]];
                    x = tMtiai.X - tMtiaj.X;
                    y = tMtiai.Y - tMtiaj.Y;
                    angleij = Angle.ComputeAngle(x, y);
                    var tAlpha = Angle.Difference2Pi(tMtiai.Angle, angleij);

                    var diff1 = Math.Abs(tAlpha - qAlpha);
                    var diff = Math.Min(diff1, 2 * Math.PI - diff1);
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

        private static short[] ArrangeClockwise(short[] idxs, IList<Minutia> minutiae)
        {
            var centerX = ((1.0 * minutiae[idxs[0]].X + minutiae[idxs[1]].X) / 2 + minutiae[idxs[2]].X) / 2;
            var centerY = ((1.0 * minutiae[idxs[0]].Y + minutiae[idxs[1]].Y) / 2 + minutiae[idxs[2]].Y) / 2;
            var list = new SortedList<double, short>(3, new DoubleComparer());
            for (var i = 0; i < 3; i++)
            {
                var minutia = minutiae[idxs[i]];
                var dx = minutia.X - centerX;
                var dy = minutia.Y - centerY;
                //todo Arreglar, poner un órden ante los colineales
                if (Math.Abs(dx) < double.Epsilon && Math.Abs(dy) < double.Epsilon)
                    return idxs;
                list.Add(Angle.ComputeAngle(dx, dy), idxs[i]);
            }
            return new[] {list.Values[0], list.Values[1], list.Values[2]};
        }

        #endregion
    }
}