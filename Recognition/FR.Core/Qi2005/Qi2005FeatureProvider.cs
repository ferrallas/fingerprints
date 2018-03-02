/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Qi2005
{
    public class Qi2005FeatureProvider
    {
        private readonly Ratha1995OrImgExtractor _orientationImageExtractor = new Ratha1995OrImgExtractor();

        public Qi2005Features Extract(byte[] rawImage)
        {
            var image = ImageProvider.AdaptImage(rawImage);
            var mtiae = MinutiaeExtractor.ExtractFeatures(image);
            var dirImg = _orientationImageExtractor.ExtractFeatures(image);

            return Qi2005FeatureExtractor.ExtractFeatures(mtiae, dirImg);
        }
    }
}