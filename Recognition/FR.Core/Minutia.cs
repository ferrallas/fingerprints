/*
 * Created by: Miguel Angel Medina P�rez (miguel.medina.perez@gmail.com)
 * Created: Thursday, December 21, 2007
 * Comments by: Miguel Angel Medina P�rez (miguel.medina.perez@gmail.com)
 */

using System;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public enum MinutiaType
    {
        Unknown,


        End,


        Bifurcation
    }


    [Serializable]
    public class Minutia
    {
        public Minutia(short x, short y, double angle)
        {
            X = x;
            Y = y;
            Angle = angle;
            MinutiaType = MinutiaType.Unknown;
            Flag = false;
        }

        public Minutia()
        {
            X = 0;
            Y = 0;
            Angle = 0;
            MinutiaType = MinutiaType.Unknown;
            Flag = false;
        }


        public short X { set; get; }


        public short Y { set; get; }


        public double Angle { set; get; }


        public MinutiaType MinutiaType { set; get; }


        public bool Flag { set; get; }


        public override int GetHashCode()
        {
            // Storing value X in the left most 11 bits.
            var blockX = (2047 & X) << 21;
            // Storing value Y in the next 11 bits.
            var blockY = (2047 & Y) << 10;
            // Storing value Angle in the next 8 bits.
            var blockAngle = Convert.ToByte(Math.Round(Angle * 255 / (2 * Math.PI))) << 2;
            // Storing value MinutiaType in the last 2 bits.
            var blockType = MinutiaType == MinutiaType.Unknown ? 0 : (MinutiaType == MinutiaType.End ? 1 : 2);

            return blockX | blockY | blockAngle | blockType;
        }


        public static bool operator ==(Minutia m1, Minutia m2)
        {
            return m1.X == m2.X && m1.Y == m2.Y && m1.Angle == m2.Angle;
        }


        public static bool operator !=(Minutia m1, Minutia m2)
        {
            return !Equals(m1, m2);
        }
    }
}