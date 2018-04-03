using System;
using Fingerprints.Model;

namespace Fingerprints.Computation
{
    public class MtiaMapper
    {
        private readonly double _dAngle;
        private readonly Minutia _query;
        private readonly Minutia _template;

        public MtiaMapper(Minutia query, Minutia template)
        {
            _dAngle = template.Angle - query.Angle;
            _template = template;
            _query = query;
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