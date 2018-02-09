/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;

namespace PatternRecognition.FingerprintRecognition.Core.Qi2005
{
    public class Qi2005FeatureProvider
    {
        private readonly Qi2005FeatureExtractor featureExtractor = new Qi2005FeatureExtractor();

        public MinutiaListProvider MtiaListProvider { get; set; }

        public OrientationImageProvider OrImgProvider { get; set; }

        public Qi2005Features Extract(byte[] image)
        {
            try
            {
                var mtiae = MtiaListProvider.Extract(image);
                var dirImg = OrImgProvider.Extract(image);

                return featureExtractor.ExtractFeatures(mtiae, dirImg);
            }
            catch (Exception)
            {
                if (MtiaListProvider == null)
                    throw new InvalidOperationException(
                        "Unable to extract Qi2005Features: Unassigned minutia list provider!");
                if (OrImgProvider == null)
                    throw new InvalidOperationException(
                        "Unable to extract Qi2005Features: Unassigned orientation image provider!");
                throw;
            }
        }
    }
}