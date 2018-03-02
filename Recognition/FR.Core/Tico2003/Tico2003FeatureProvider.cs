/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Tico2003
{
    public class Tico2003FeatureProvider 
    {
        private readonly Ratha1995OrImgExtractor _orientationImageExtractor = new Ratha1995OrImgExtractor();
        private readonly Tico2003FeatureExtractor _featureExtractor = new Tico2003FeatureExtractor();

        public Tico2003Features Extract(byte[] rawImage)
        {
            var image = ImageProvider.AdaptImage(rawImage);
            var mtiae = MinutiaeExtractor.ExtractFeatures(image);
            var dirImg = _orientationImageExtractor.ExtractFeatures(image);

            return _featureExtractor.ExtractFeatures(mtiae, dirImg);
        }
    }
}