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

        public SkeletonImage GetResource(string fingerprint, ResourceRepository repository)
        {
            var isPersistent = IsResourcePersistent();
            var resourceName =
                $"{fingerprint}.{GetSignature()}";
            if (isPersistent && repository.ResourceExists(resourceName))
                return SkeletonImageSerializer.FromByteArray(repository.RetrieveResource(resourceName));

            var resource = Extract(fingerprint, repository);
            if (resource == null)
                return null;

            if (isPersistent)
                repository.StoreResource(resourceName, SkeletonImageSerializer.ToByteArray(resource));
            return resource;
        }


        public string GetSignature()
        {
            return $"{SkeletonImageExtractor.GetType().Name}.ski";
        }


        public bool IsResourcePersistent()
        {
            return true;
        }

        #region private

        private SkeletonImage Extract(string fingerprintLabel, ResourceRepository repository)
        {
            var image = imageProvider.GetResource(fingerprintLabel, repository);
            if (image == null)
                throw new ArgumentOutOfRangeException(nameof(fingerprintLabel),
                    "Unable to extract SkeletonImage: Invalid fingerprint!");
            if (SkeletonImageExtractor == null)
                throw new InvalidOperationException(
                    "Unable to extract SkeletonImage: Unassigned skeleton image extractor!");
            return SkeletonImageExtractor.ExtractFeatures(image);
        }

        private readonly FingerprintImageProvider imageProvider = new FingerprintImageProvider();

        #endregion
    }
}