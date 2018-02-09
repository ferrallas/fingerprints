/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Jiang2000
{
    public static class JyFeaturesProvider
    {
        public static JyFeatures Extract(byte[] imageRaw)
        {
            var mtiae = MinutiaeExtractor.ExtractFeatures(ImageProvider.GetResource(imageRaw));
            var skeletonImg = SkeletonImageExtractor.ExtractFeatures(ImageProvider.GetResource(imageRaw));

            return JyFeatureExtractor.ExtractFeatures(mtiae, skeletonImg);
        }
    }
}