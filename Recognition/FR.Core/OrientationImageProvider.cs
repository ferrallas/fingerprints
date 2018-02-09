/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */


using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public class OrientationImageProvider
    {
        private readonly Ratha1995OrImgExtractor _orientationImageExtractor;

        public OrientationImageProvider(Ratha1995OrImgExtractor provider)
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