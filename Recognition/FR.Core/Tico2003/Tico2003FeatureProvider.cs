/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;

namespace PatternRecognition.FingerprintRecognition.Core.Tico2003
{
    public class Tico2003FeatureProvider 
    {
        private readonly Tico2003FeatureExtractor featureExtractor = new Tico2003FeatureExtractor();

        public MinutiaListProvider MtiaListProvider { get; set; }


        public OrientationImageProvider OrImgProvider { get; set; }

        public Tico2003Features Extract(byte[] image)
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
                        "Unable to extract Tico2003Features: Unassigned minutia list provider!");
                if (OrImgProvider == null)
                    throw new InvalidOperationException(
                        "Unable to extract Tico2003Features: Unassigned orientation image provider!");
                throw;
            }
        }
    }
}