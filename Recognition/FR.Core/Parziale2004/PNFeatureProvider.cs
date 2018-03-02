/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Parziale2004
{
    public static class PnFeatureProvider
    {
        public static PnFeatures Extract(byte[] image)
        {
            var mtiae = MinutiaeExtractor.ExtractFeatures(ImageProvider.AdaptImage(image));
            return PnFeatureExtractor.ExtractFeatures(mtiae);
        }
    }
}