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
        private readonly FingerprintImageProvider _imageProvider = new FingerprintImageProvider();
        private readonly IFeatureExtractor<List<Minutia>> _minutiaListExtractor;

        public MinutiaListProvider(IFeatureExtractor<List<Minutia>> extractor)
        {
            _minutiaListExtractor = extractor;
        }

        public List<Minutia> Extract(string fingerprintLabel, ResourceRepository repository)
        {
            var image = _imageProvider.GetResource(fingerprintLabel, repository);
            if (image == null)
                throw new ArgumentOutOfRangeException(fingerprintLabel,
                    "Unable to extract minutia list: Invalid fingerprint!");
            if (_minutiaListExtractor == null)
                throw new InvalidOperationException(
                    "Unable to extract minutia list: Unassigned minutia list extractor!");
            return _minutiaListExtractor.ExtractFeatures(image);
        }
    }
}