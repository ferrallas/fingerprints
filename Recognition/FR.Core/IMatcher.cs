/*
 * Created by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 * Created: 1/5/2007
 * Comments by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core
{
    /// <summary>
    ///     Represents a non-generic fingerprint matching algorithm.
    /// </summary>
    /// <remarks>
    ///     A fingerprint matching algorithm compares fingerprint features and returns a matching score. The higher returned value, the greater is the fingerprints similarity.
    /// </remarks>
    public interface IMatcher
    {
        /// <summary>
        ///     Matches the specified fingerprint features.
        /// </summary>
        /// <param name="query">
        ///     The query fingerprint features.
        /// </param>
        /// <param name="template">
        ///     The template fingerprint features.
        /// </param>
        /// <returns>
        ///     The fingerprint similarity value.
        /// </returns>
        double Match(object query, object template);
    }

    /// <summary>
    ///     Represents a minutia matching algorithm. 
    /// </summary>
    /// <remarks>
    ///     A minutia matching algorithm compares fingerprints based on minutia features. 
    /// </remarks>
    public interface IMinutiaMatcher : IMatcher
    {
        /// <summary>
        ///     Matches the specified fingerprint features and returns the matching minutiae.
        /// </summary>
        /// <param name="query">
        ///     The query fingerprint features.
        /// </param>
        /// <param name="template">
        ///     The template fingerprint features.
        /// </param>
        /// <param name="matchingMtiae">
        ///     The matching minutiae..
        /// </param>
        /// <returns>
        ///     The fingerprint similarity value.
        /// </returns>
        double Match(object query, object template, out List<MinutiaPair> matchingMtiae);
    }
}