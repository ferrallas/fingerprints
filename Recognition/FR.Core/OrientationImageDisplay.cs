/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Drawing;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public class OrientationImageDisplay : FeatureDisplay<OrientationImage>
    {
        #region IFeatureDisplay<List<Minutia>> Members

        public override void Show(OrientationImage orImg, Graphics g)
        {
            var lineLength = orImg.WindowSize / 2;
            var greenPen = new Pen(Brushes.Green) {Width = 2};
            var redPen = new Pen(Brushes.Red) {Width = 2};
            Pen currentPen;

            for (var i = 0; i < orImg.Height; i++)
            for (var j = 0; j < orImg.Width; j++)
            {
                double angle;
                if (orImg.IsNullBlock(i, j))
                {
                    currentPen = redPen;
                    angle = 0;
                }
                else
                {
                    currentPen = greenPen;
                    angle = orImg.AngleInRadians(i, j);
                }
                //double angle = orImg.IsNullBlock(i, j) ? 0 : orImg.AngleInRadians(i, j);
                var x = j * orImg.WindowSize + orImg.WindowSize / 2;
                var y = i * orImg.WindowSize + orImg.WindowSize / 2;

                var p0 = new Point
                {
                    X = Convert.ToInt32(x - lineLength * Math.Cos(angle)),
                    Y = Convert.ToInt32(y - lineLength * Math.Sin(angle))
                };

                var p1 = new Point
                {
                    X = Convert.ToInt32(x + lineLength * Math.Cos(angle)),
                    Y = Convert.ToInt32(y + lineLength * Math.Sin(angle))
                };

                g.DrawLine(currentPen, p0, p1);
            }
        }

        #endregion
    }
}