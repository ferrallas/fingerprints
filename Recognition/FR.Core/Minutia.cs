/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: Thursday, December 21, 2007
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
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


    public class MinutiaPair
    {
        public Minutia QueryMtia { set; get; }


        public Minutia TemplateMtia { set; get; }


        public double MatchingValue { set; get; }


        public override int GetHashCode()
        {
            unchecked
            {
                return (QueryMtia.GetHashCode() * 397) ^ TemplateMtia.GetHashCode();
            }
        }

        public override string ToString()
        {
            return GetHashCode().ToString();
        }


        public static bool operator ==(MinutiaPair mp1, MinutiaPair mp2)
        {
            if (!ReferenceEquals(null, mp1))
                return mp1.Equals(mp2);
            if (ReferenceEquals(null, mp1) && ReferenceEquals(null, mp2))
                return true;
            return false;
        }


        public static bool operator !=(MinutiaPair mp1, MinutiaPair mp2)
        {
            if (!ReferenceEquals(null, mp1))
                return !mp1.Equals(mp2);
            if (ReferenceEquals(null, mp1) && ReferenceEquals(null, mp2))
                return false;
            return true;
        }


        public bool Equals(MinutiaPair obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.QueryMtia.Equals(QueryMtia) && obj.TemplateMtia.Equals(TemplateMtia);
        }


        public override bool Equals(object obj)
        {
            var mp = obj as MinutiaPair;
            return Equals(mp);
        }
    }

    public class MtiaMapper
    {
        private readonly double _dAngle;
        private readonly Minutia _query;
        private readonly Minutia _template;

        public MtiaMapper(Minutia query, Minutia template)
        {
            _dAngle = template.Angle - query.Angle;
            this._template = template;
            this._query = query;
        }

        public Minutia Map(Minutia m)
        {
            var newAngle = m.Angle + _dAngle;
            var sin = Math.Sin(_dAngle);
            var cos = Math.Cos(_dAngle);
            return new Minutia
            {
                Angle = newAngle > 2 * Math.PI
                    ? newAngle - 2 * Math.PI
                    : newAngle < 0
                        ? newAngle + 2 * Math.PI
                        : newAngle,
                X = Convert.ToInt16(Math.Round((m.X - _query.X) * cos - (m.Y - _query.Y) * sin + _template.X)),
                Y = Convert.ToInt16(Math.Round((m.X - _query.X) * sin + (m.Y - _query.Y) * cos + _template.Y))
            };
        }
    }
}