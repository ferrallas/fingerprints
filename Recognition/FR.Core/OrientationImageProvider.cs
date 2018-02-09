/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */


namespace PatternRecognition.FingerprintRecognition.Core
{
    public class OrientationImageProvider
    {
        private readonly IFeatureExtractor<OrientationImage> _orientationImageExtractor;

        public OrientationImageProvider(IFeatureExtractor<OrientationImage> provider)
        {
            _orientationImageExtractor = provider;
        }


        public OrientationImage Extract(byte[] rawImage)
        {
            var image = ImageProvider.GetResource(rawImage);
            return _orientationImageExtractor.ExtractFeatures(image);
        }
    }
}