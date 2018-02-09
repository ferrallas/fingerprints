/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public class MinutiaListProvider
    {
        private readonly IFeatureExtractor<List<Minutia>> _minutiaListExtractor;

        public MinutiaListProvider(IFeatureExtractor<List<Minutia>> extractor)
        {
            _minutiaListExtractor = extractor;
        }

        public List<Minutia> Extract(byte[] imageRaw)
        {
            var image = ImageProvider.GetResource(imageRaw);
            return _minutiaListExtractor.ExtractFeatures(image);
        }
    }
}