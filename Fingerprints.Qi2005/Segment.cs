using System;
using System.Collections.Generic;
using System.Drawing;
using Fingerprints.Computation;
using Fingerprints.Model;

namespace Fingerprints.Qi2005
{
    [Serializable]
    public class Segment
    {
        [NonSerialized]
        private const int Interval = 18;

        public double[] Directions { get; set; }

        public Segment()
        {

        }

        internal Segment(double ang, Minutia mnt, OrientationImage dImg)
        {
            var endOfPoints = false;
            var i = 1;
            var points = new List<double>();
            while (!endOfPoints)
            {
                var pnt = SetPosToSPoint(ang, i * Interval, new Point(mnt.X, mnt.Y));
                if (IsInBound(pnt, dImg))
                {
                    dImg.GetBlockCoordFromPixel(pnt.X, pnt.Y, out var row, out var col);
                    if (col < 0 || row < 0 || row >= dImg.Height ||
                        col >= dImg.Width || dImg.IsNullBlock(row, col))
                        points.Add(double.NaN);
                    else
                        points.Add(Math.Min(Angle.DifferencePi(mnt.Angle, dImg.AngleInRadians(row, col)),
                            Angle.DifferencePi(mnt.Angle, dImg.AngleInRadians(row, col) + Math.PI)));
                    i++;
                }
                else
                {
                    endOfPoints = true;
                }
            }
            var isLastNan = false;
            var j = points.Count - 1;
            while (!isLastNan && j >= 0)
                if (double.IsNaN(points[j]))
                {
                    points.RemoveAt(j);
                    j--;
                }
                else
                {
                    isLastNan = true;
                }
            Directions = points.ToArray();
        }

        internal Point SetPosToSPoint(double angle, int radio, Point p)
        {
            var point = new Point();
            var dx = radio * Math.Cos(angle);
            var dy = radio * Math.Sin(angle);
            point.X = p.X - Convert.ToInt32(Math.Round(dx));
            point.Y = p.Y - Convert.ToInt32(Math.Round(dy));
            return point;
        }

        internal bool IsInBound(Point pnt, OrientationImage dImg)
        {
            if (pnt.X > 0 && pnt.X < dImg.Width * dImg.WindowSize &&
                pnt.Y > 0 && pnt.Y < dImg.Height * dImg.WindowSize)
                return true;
            return false;
        }
    }
}