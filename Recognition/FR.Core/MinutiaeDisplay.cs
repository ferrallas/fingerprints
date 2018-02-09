/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: Thursday, December 21, 2007
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public class MinutiaeDisplay : FeatureDisplay<List<Minutia>>
    {
        #region IFeatureDisplay<List<Minutia>> Members

        public override void Show(List<Minutia> features, Graphics g)
        {
            var mtiaRadius = 6;
            var lineLength = 18;
            var pen = new Pen(Brushes.Blue) {Width = 3};
            pen.Color = Color.Red;

            var whitePen = new Pen(Brushes.Blue) {Width = 5};
            whitePen.Color = Color.White;

            var i = 0;
            foreach (var mtia in (IList<Minutia>) features)
            {
                g.DrawEllipse(whitePen, mtia.X - mtiaRadius, mtia.Y - mtiaRadius, 2 * mtiaRadius + 1,
                    2 * mtiaRadius + 1);
                g.DrawLine(whitePen, mtia.X, mtia.Y, Convert.ToInt32(mtia.X + lineLength * Math.Cos(mtia.Angle)),
                    Convert.ToInt32(mtia.Y + lineLength * Math.Sin(mtia.Angle)));

                pen.Color = Color.Red;

                g.DrawEllipse(pen, mtia.X - mtiaRadius, mtia.Y - mtiaRadius, 2 * mtiaRadius + 1, 2 * mtiaRadius + 1);
                g.DrawLine(pen, mtia.X, mtia.Y, Convert.ToInt32(mtia.X + lineLength * Math.Cos(mtia.Angle)),
                    Convert.ToInt32(mtia.Y + lineLength * Math.Sin(mtia.Angle)));
                i++;
            }

            var lastMtia = ((IList<Minutia>) features)[((IList<Minutia>) features).Count - 1];
            //pen.Color = Color.Green;
            g.DrawEllipse(pen, lastMtia.X - mtiaRadius, lastMtia.Y - mtiaRadius, 2 * mtiaRadius + 1,
                2 * mtiaRadius + 1);
            g.DrawLine(pen, lastMtia.X, lastMtia.Y, Convert.ToInt32(lastMtia.X + lineLength * Math.Cos(lastMtia.Angle)),
                Convert.ToInt32(lastMtia.Y + lineLength * Math.Sin(lastMtia.Angle)));
        }

        #endregion
    }
}