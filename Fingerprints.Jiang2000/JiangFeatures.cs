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
    public class JiangFeatures
    {
        internal JiangFeatures(List<JiangMinutiaDescriptor> descriptorsList)
        {
            Minutiae = descriptorsList;
        }

        internal List<JiangMinutiaDescriptor> Minutiae { get; }
    }
}