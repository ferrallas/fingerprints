/*
 * Created by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 * Created: June 8, 2010
 * Comments by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 */

using System;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public class SkeletonImageProvider
    {
        public IFeatureExtractor<SkeletonImage> SkeletonImageExtractor { set; get; }

        public SkeletonImage Extract(byte[] imageRaw)
        {
            var image = ImageProvider.GetResource(imageRaw);
            return SkeletonImageExtractor.ExtractFeatures(image);
        }
    }
}