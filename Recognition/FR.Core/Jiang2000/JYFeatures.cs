/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Jiang2000
{

    /// <summary>
    ///     The features used by <see cref="JY"/> to match fingerprints.
    /// </summary>
    /// <remarks>
    ///     The constructor of this class is internal. You must use <see cref="JYFeatureExtractor"/> in order to extract these features from fingerprints.
    /// </remarks>
    [Serializable]
    public class JYFeatures
    {
        internal List<JYMtiaDescriptor> Minutiae { get; private set; }

        internal JYFeatures(List<JYMtiaDescriptor> descriptorsList)
        {
            Minutiae = descriptorsList;
        }
    }
}
