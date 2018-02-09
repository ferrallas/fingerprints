/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2011
{
    public class DelaunayMtpsProvider
    {
        private readonly DalaunayMTpsExtractor mTripletsCalculator = new DalaunayMTpsExtractor();

        public MtripletsFeature Extract(byte[] image)
        {
            var mtiae = MtiaListProvider.Extract(image);
            return mTripletsCalculator.ExtractFeatures(mtiae);
        }

        public MinutiaListProvider MtiaListProvider { get; set; }

    }
}