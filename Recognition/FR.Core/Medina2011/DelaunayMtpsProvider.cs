/*
 * Created by: Miguel Angel Medina P�rez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina P�rez (miguel.medina.perez@gmail.com)
 */

using System;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2011
{
    public static class DelaunayMtpsProvider
    {
        public static MtripletsFeature Extract(byte[] image)
        {
            var mtiae = MinutiaeExtractor.ExtractFeatures(ImageProvider.GetResource(image));
            return DalaunayMTpsExtractor.ExtractFeatures(mtiae);
        }

    }
}