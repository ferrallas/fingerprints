/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public class OrientationImageProvider
    {
        private readonly IFeatureExtractor<OrientationImage> _orientationImageExtractor;
        private readonly FingerprintImageProvider _imageProvider = new FingerprintImageProvider();

        public OrientationImageProvider(IFeatureExtractor<OrientationImage> provider)
        {
            _orientationImageExtractor = provider;
        }


        public OrientationImage GetResource(string fingerprint, ResourceRepository repository)
        {
            var image = _imageProvider.GetResource(fingerprint, repository);
            return _orientationImageExtractor.ExtractFeatures(image);
        }
    }
}