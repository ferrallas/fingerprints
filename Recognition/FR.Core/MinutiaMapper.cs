using System;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public class MinutiaMapper
    {
        private readonly double dAngle;
        private readonly Minutia query;
        private readonly Minutia template;

        public MinutiaMapper(Minutia minutia)
        {
            var t = new Minutia(0, 0, 0);
            dAngle = t.Angle - minutia.Angle;
            template = t;
            query = minutia;
        }

        public MinutiaMapper(Minutia query, Minutia template)
        {
            dAngle = template.Angle - query.Angle;
            this.template = template;
            this.query = query;
        }

        public Minutia Map(Minutia m)
        {
            return new Minutia
            {
                Angle = m.Angle + dAngle,
                X = Convert.ToInt16(Math.Round((m.X - query.X) * Math.Cos(dAngle) - (m.Y - query.Y) * Math.Sin(dAngle) +
                                               template.X)),
                Y = Convert.ToInt16(Math.Round((m.X - query.X) * Math.Sin(dAngle) + (m.Y - query.Y) * Math.Cos(dAngle) +
                                               template.Y))
            };
        }
    }
}