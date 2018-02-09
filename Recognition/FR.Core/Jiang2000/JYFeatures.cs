/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Jiang2000
{
    [Serializable]
    public class JyFeatures
    {
        internal JyFeatures(List<JyMtiaDescriptor> descriptorsList)
        {
            Minutiae = descriptorsList;
        }

        internal List<JyMtiaDescriptor> Minutiae { get; }
    }
}