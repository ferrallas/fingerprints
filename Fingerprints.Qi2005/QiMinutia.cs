/*
 * Created by: Andrés Eduardo Gutiérrez Rodríguez (andres@bioplantas.cu)
 *             Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: 
 */

using System;
using Fingerprints.Model;

namespace Fingerprints.Qi2005
{
    [Serializable]
    internal class QiMinutia
    {
        [NonSerialized] private const double Threshold = 125 * Math.PI;

        public Minutia Minutia { get; set; }

        public Segment[] Segments { get; set; }


        internal QiMinutia(Minutia mnt, OrientationImage dImg)
        {
            Minutia = mnt;
            Segments = new Segment[6];
            for (var i = 0; i < Segments.Length; i++)
                Segments[i] = new Segment(i * (2 * Math.PI / 6) + mnt.Angle, mnt, dImg);
        }

        internal double Compare(QiMinutia gOwMtia)
        {
            var sum = 0.0;
            for (var i = 0; i < Segments.Length; i++)
            {
                for (var j = Math.Min(Segments[i].Directions.Length, gOwMtia.Segments[i].Directions.Length) - 1;
                    j >= 0;
                    j--)
                    if (double.IsNaN(Segments[i].Directions[j]) && !double.IsNaN(gOwMtia.Segments[i].Directions[j]))
                    {
                        sum += Math.Pow(gOwMtia.Segments[i].Directions[j], 2);
                    }
                    else
                    {
                        if (!double.IsNaN(Segments[i].Directions[j]) && double.IsNaN(gOwMtia.Segments[i].Directions[j]))
                            sum += Math.Pow(Segments[i].Directions[j], 2);
                        else if (!double.IsNaN(Segments[i].Directions[j]) &&
                                 !double.IsNaN(gOwMtia.Segments[i].Directions[j]))
                            sum += Math.Pow(Segments[i].Directions[j] - gOwMtia.Segments[i].Directions[j], 2);
                    }
                if (Segments[i].Directions.Length > gOwMtia.Segments[i].Directions.Length)
                {
                    for (var k = Segments[i].Directions.Length - 1; k >= gOwMtia.Segments[i].Directions.Length; k--)
                        if (!double.IsNaN(Segments[i].Directions[k]))
                            sum += Math.Pow(Segments[i].Directions[k], 2);
                }
                else if (Segments[i].Directions.Length < gOwMtia.Segments[i].Directions.Length)
                {
                    for (var k = gOwMtia.Segments[i].Directions.Length - 1; k >= Segments[i].Directions.Length; k--)
                        if (!double.IsNaN(gOwMtia.Segments[i].Directions[k]))
                            sum += Math.Pow(gOwMtia.Segments[i].Directions[k], 2);
                }
            }
            if (Math.Sqrt(sum) < Threshold)
                return (Threshold - Math.Sqrt(sum)) / Threshold;
            return 0;
        }

        public static implicit operator Minutia(QiMinutia desc)
        {
            return desc.Minutia;
        }
    }
}