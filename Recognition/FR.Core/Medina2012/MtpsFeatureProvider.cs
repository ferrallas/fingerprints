/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2012
{
    public class MtpsFeatureProvider
    {
        private readonly MinutiaListProvider _mtiaListProvider;
        private readonly MTripletsExtractor mTripletsCalculator = new MTripletsExtractor();


        public MtpsFeatureProvider(MinutiaListProvider mtiaListProvider)
        {
            _mtiaListProvider = mtiaListProvider;
        }

        public byte NeighborsCount
        {
            set => mTripletsCalculator.NeighborsCount = value;
            get => mTripletsCalculator.NeighborsCount;
        }

        public MtripletsFeature Extract(string fingerprint, ResourceRepository repository)
        {
            try
            {
                var mtiae = _mtiaListProvider.Extract(fingerprint, repository);
                return mTripletsCalculator.ExtractFeatures(mtiae);
            }
            catch (Exception e)
            {
                if (_mtiaListProvider == null)
                    throw new InvalidOperationException(
                        "Unable to extract PNFeatures: Unassigned minutia list provider!", e);
                throw;
            }
        }
    }
}