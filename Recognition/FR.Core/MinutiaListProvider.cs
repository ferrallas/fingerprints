/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core
{
    /// <summary>
    ///     Allows retrieving minutia list from a <see cref="ResourceRepository" />.
    /// </summary>
    public class MinutiaListProvider : IResourceProvider<List<Minutia>>
    {
        public MinutiaListProvider(IFeatureExtractor<List<Minutia>> extractor)
        {
            _minutiaListExtractor = extractor;
        }

        /// <summary>
        ///     Used to extract minutia list in case that the resource have not being saved.
        /// </summary>
        private IFeatureExtractor<List<Minutia>> _minutiaListExtractor;

        /// <summary>
        ///     Gets minutia list from the specified fingerprint and <see cref="ResourceRepository" />.
        /// </summary>
        /// <param name="fingerprint">The fingerprint which minutia list is being retrieved.</param>
        /// <param name="repository">The object used to store and retrieve resources.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the fingerprint is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the minutia list extractor is not assigned.</exception>
        /// <returns>The retrieved minutia list.</returns>
        object IResourceProvider.GetResource(string fingerprint, ResourceRepository repository)
        {
            return GetResource(fingerprint, repository);
        }

        /// <summary>
        ///     Gets minutia list from the specified fingerprint and <see cref="ResourceRepository" />.
        /// </summary>
        /// <param name="fingerprint">The fingerprint which minutia list is being retrieved.</param>
        /// <param name="repository">The object used to store and retrieve resources.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the fingerprint is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the minutia list extractor is not assigned.</exception>
        /// <returns>The retrieved minutia list.</returns>
        public List<Minutia> GetResource(string fingerprint, ResourceRepository repository)
        {
            var isPersistent = IsResourcePersistent();
            var resourceName =
                $"{fingerprint}.{GetSignature()}";
            if (isPersistent && repository.ResourceExists(resourceName))
                return MinutiaListSerializer.FromByteArray(repository.RetrieveResource(resourceName));

            var resource = Extract(fingerprint, repository);
            if (resource == null)
                return null;

            if (isPersistent)
                repository.StoreResource(resourceName, MinutiaListSerializer.ToByteArray(resource));
            return resource;
        }

        /// <summary>
        ///     Gets the signature of the <see cref="MinutiaListProvider" />.
        /// </summary>
        /// <returns>
        ///     It returns a string formed by the name of the property <see cref="_minutiaListExtractor" /> concatenated with
        ///     ".mta".
        /// </returns>
        public string GetSignature()
        {
            return $"{_minutiaListExtractor.GetType().Name}.mta";
        }

        /// <summary>
        ///     Determines whether the provided minutia list is persistent.
        /// </summary>
        /// <returns>Always returns true.</returns>
        public bool IsResourcePersistent()
        {
            return true;
        }

        #region private

        private List<Minutia> Extract(string fingerprintLabel, ResourceRepository repository)
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

        private readonly FingerprintImageProvider _imageProvider = new FingerprintImageProvider();

        #endregion
    }
}