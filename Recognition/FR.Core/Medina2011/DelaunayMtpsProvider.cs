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


        protected MtripletsFeature Extract(string fingerprint, ResourceRepository repository)
        {
            try
            {
                var mtiae = MtiaListProvider.Extract(fingerprint, repository);
                return mTripletsCalculator.ExtractFeatures(mtiae);
            }
            catch (Exception e)
            {
                if (MtiaListProvider == null)
                    throw new InvalidOperationException(
                        "Unable to extract PNFeatures: Unassigned minutia list provider!", e);
                throw;
            }
        }

        public MinutiaListProvider MtiaListProvider { get; set; }

    }
}