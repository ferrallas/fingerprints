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
        public List<JiangMinutiaDescriptor> Minutiae { get; }

        //for serialization purpose
        public JiangFeatures()
        {
        }

        public JiangFeatures(List<JiangMinutiaDescriptor> descriptorsList)
        {
            Minutiae = descriptorsList;
        }
    }
}