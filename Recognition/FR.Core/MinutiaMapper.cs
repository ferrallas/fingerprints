using System;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public class MinutiaMapper
    {
        private readonly double _dAngle;
        private readonly Minutia _query;
        private readonly Minutia _template;

        public MinutiaMapper(Minutia minutia)
        {
            var t = new Minutia(0, 0, 0);
            _dAngle = t.Angle - minutia.Angle;
            _template = t;
            _query = minutia;
        }

        public MinutiaMapper(Minutia query, Minutia template)
        {
            _dAngle = template.Angle - query.Angle;
            this._template = template;
            this._query = query;
        }

        public Minutia Map(Minutia m)
        {
            return new Minutia
            {
                Angle = m.Angle + _dAngle,
                X = Convert.ToInt16(Math.Round((m.X - _query.X) * Math.Cos(_dAngle) - (m.Y - _query.Y) * Math.Sin(_dAngle) +
                                               _template.X)),
                Y = Convert.ToInt16(Math.Round((m.X - _query.X) * Math.Sin(_dAngle) + (m.Y - _query.Y) * Math.Cos(_dAngle) +
                                               _template.Y))
            };
        }
    }
}