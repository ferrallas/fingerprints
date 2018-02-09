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
        public OrientationImageProvider OrImgProvider { get; set; }

        public Qi2005Features Extract(byte[] image)
        {
            var mtiae = MinutiaeExtractor.ExtractFeatures(ImageProvider.GetResource(image));
            var dirImg = OrImgProvider.Extract(image);

            return Qi2005FeatureExtractor.ExtractFeatures(mtiae, dirImg);
        }
    }
}