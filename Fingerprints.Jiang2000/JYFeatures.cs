/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace Fingerprints.Jiang2000
{
    [Serializable]
    internal class JyFeatures
    {
        internal JyFeatures(List<JyMtiaDescriptor> descriptorsList)
        {
            Minutiae = descriptorsList;
        }

        internal List<JyMtiaDescriptor> Minutiae { get; }
    }
}