/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Jiang2000
{
    public class JYFeaturesProvider
    {
        private readonly MinutiaListProvider _mtiaListProvider;
        private readonly JYFeatureExtractor featureExtractor;
        private readonly SkeletonImageProvider SkeletonImgProvider;

        public JYFeaturesProvider(MinutiaListProvider minutiaListProvider)
        {
            _mtiaListProvider = minutiaListProvider;
            SkeletonImgProvider = new SkeletonImageProvider {SkeletonImageExtractor = new Ratha1995SkeImgExtractor()};
            featureExtractor = new JYFeatureExtractor();
        }


        protected JYFeatures Extract(string fingerprint, ResourceRepository repository)
        {
            try
            {
                var mtiae = _mtiaListProvider.Extract(fingerprint, repository);
                var skeletonImg = SkeletonImgProvider.GetResource(fingerprint, repository);

                return featureExtractor.ExtractFeatures(mtiae, skeletonImg);
            }
            catch (Exception)
            {
                if (_mtiaListProvider == null)
                    throw new InvalidOperationException(
                        "Unable to extract JYFeatures: Unassigned minutia list provider!");
                if (SkeletonImgProvider == null)
                    throw new InvalidOperationException(
                        "Unable to extract JYFeatures: Unassigned skeleton image provider!");
                throw;
            }
        }
    }
}