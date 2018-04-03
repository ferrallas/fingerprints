/*
 * Created by: Andrés Eduardo Gutiérrez Rodríguez (andres@bioplantas.cu)
 *             Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             
 *             
 * Created: 
 * Comments by: 
 */

using System;
using System.Drawing;
using Fingerprints.Computation;
using Fingerprints.Model;

namespace Fingerprints.Tico2003
{
    [Serializable]
    internal class MinutiaDescriptor
    {
        internal MinutiaDescriptor(Minutia mnt, OrientationImage dImg)
        {
            Minutia = mnt;
            //difRadio = (int) Math.Truncate((Resolution/25.4)*ridgePeriod*2);

            EmptyFeaturesCount = 0;
            Orientations = new double[72];
            for (int i = 0, j = 0; i < 4; i++)
            {
                var curr = GetOrientations(InitRadio + i * DifRadio, Minutia, dImg);
                for (var k = 0; k < curr.Length; k++)
                {
                    Orientations[j++] = curr[k];
                    if (double.IsNaN(curr[k]))
                        EmptyFeaturesCount++;
                }
            }
        }

        internal double[] Orientations { get; }

        internal Minutia Minutia { get; }

        internal byte EmptyFeaturesCount { get; }

        public static implicit operator Minutia(MinutiaDescriptor desc)
        {
            return desc.Minutia;
        }

        internal double Compare(MinutiaDescriptor mtiaDesc)
        {
            double sum = 0;
            for (var i = 0; i < 72; i++)
            {
                var or1 = Orientations[i];
                var or2 = mtiaDesc.Orientations[i];
                if (!double.IsNaN(or1) && !double.IsNaN(or2))
                {
                    var diffOr = Math.Abs(or1 - or2);

                    var difAng = 2 / Math.PI * diffOr;

                    sum += Math.Exp(-16 * difAng);
                }
            }

            return sum / 72;
        }

        #region private 

        [NonSerialized] private const int DifRadio = 18;

        [NonSerialized] private const int InitRadio = 27;

        private static double[] GetOrientations(int radio, Minutia mtia, OrientationImage dirImg)
        {
            var currOrientations = new double[radio / 3];
            var n = radio / 3;
            var incAng = 2 * Math.PI * 3.0 / radio;
            for (var i = 0; i < n; i++)
            {
                var myAng = mtia.Angle + i * incAng;
                if (myAng > 2 * Math.PI)
                    myAng -= 2 * Math.PI;
                var pnt = SetPosToSPoint(myAng, radio, new Point(mtia.X, mtia.Y));
                dirImg.GetBlockCoordFromPixel(pnt.X, pnt.Y, out var row, out var col);
                if (col < 0 || row < 0 || row >= dirImg.Height ||
                    col >= dirImg.Width || dirImg.IsNullBlock(row, col))
                    currOrientations[i] = double.NaN;
                else
                    currOrientations[i] =
                        Math.Min(Angle.DifferencePi(mtia.Angle, dirImg.AngleInRadians(row, col)),
                            Angle.DifferencePi(mtia.Angle, dirImg.AngleInRadians(row, col) + Math.PI));
            }
            return currOrientations;
        }

        private static Point SetPosToSPoint(double angle, int radio, Point p)
        {
            var point = new Point();
            var dx = radio * Math.Cos(angle);
            var dy = radio * Math.Sin(angle);
            point.X = p.X - Convert.ToInt32(Math.Round(dx));
            point.Y = p.Y - Convert.ToInt32(Math.Round(dy));
            return point;
        }

        #endregion
    }
}