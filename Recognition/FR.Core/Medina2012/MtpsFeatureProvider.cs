/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2012
{
    public class MtpsFeatureProvider
    {
        private readonly MinutiaListProvider _mtiaListProvider;
        private readonly MTripletsExtractor mTripletsCalculator = new MTripletsExtractor();


        public MtpsFeatureProvider()
        {
            _mtiaListProvider = new MinutiaListProvider(new Ratha1995MinutiaeExtractor());
        }

        public byte NeighborsCount
        {
            set => mTripletsCalculator.NeighborsCount = value;
            get => mTripletsCalculator.NeighborsCount;
        }

        public MtripletsFeature Extract(byte[] imageRaw)
        {
            var mtiae = _mtiaListProvider.Extract(imageRaw);
            return mTripletsCalculator.ExtractFeatures(mtiae);
        }
    }
}