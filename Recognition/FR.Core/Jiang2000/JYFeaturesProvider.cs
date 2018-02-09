/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Jiang2000
{
    /// <summary>
    ///     Allows retrieving features of type <see cref="JYFeatures"/> from a <see cref="ResourceRepository"/>.
    /// </summary>
    /// <remarks>
    ///     This features are computed from a <see cref="Minutia"/> list and <see cref="SkeletonImage"/>. This way, in order to compute <see cref="JYFeatures"/>, you must set the properties <see cref="_mtiaListProvider"/> and <see cref="SkeletonImgProvider"/>.
    /// </remarks>
    public class JYFeaturesProvider
    {
        private readonly MinutiaListProvider _mtiaListProvider;
        private readonly JYFeatureExtractor featureExtractor;
        private readonly SkeletonImageProvider SkeletonImgProvider;

        public JYFeaturesProvider(MinutiaListProvider minutiaListProvider)
        {
            _mtiaListProvider = minutiaListProvider;
            SkeletonImgProvider = new SkeletonImageProvider { SkeletonImageExtractor = new Ratha1995SkeImgExtractor() };
            featureExtractor = new JYFeatureExtractor();
        }

        /// <summary>
        ///     Extracts <see cref="JYFeatures"/> from the specified fingerprint and <see cref="ResourceRepository"/>.
        /// </summary>
        /// <param name="fingerprint">The fingerprint which resource is being extracted.</param>
        /// <param name="repository">The object used to store and retrieve resources.</param>
        /// <exception cref="InvalidOperationException">Thrown when the minutia list provider is not assigned, the skeleton image provider is not assigned, the minutia list extractor is not assigned or the skeleton image extractor is not assigned.</exception>
        /// <returns>The extracted <see cref="JYFeatures"/>.</returns>
        protected JYFeatures Extract(string fingerprint, ResourceRepository repository)
        {
            try
            {
                var mtiae = _mtiaListProvider.GetResource(fingerprint, repository);
                var skeletonImg = SkeletonImgProvider.GetResource(fingerprint, repository);

                return featureExtractor.ExtractFeatures(mtiae, skeletonImg);
            }
            catch (Exception)
            {
                if (_mtiaListProvider == null)
                    throw new InvalidOperationException("Unable to extract JYFeatures: Unassigned minutia list provider!");
                if (SkeletonImgProvider == null)
                    throw new InvalidOperationException("Unable to extract JYFeatures: Unassigned skeleton image provider!");
                throw;
            }
        }
    }
}
