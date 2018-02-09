/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Andrés Eduardo Gutiérrez Rodríguez (andres@bioplantas.cu)
 *             
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Tico2003
{
    [Serializable]
    public class Tico2003Features
    {
        internal List<OBMtiaDescriptor> Minutiae;

        internal Tico2003Features(List<Minutia> minutiae, OrientationImage dImg)
        {
            Minutiae = new List<OBMtiaDescriptor>(minutiae.Count);
            for (short i = 0; i < minutiae.Count; i++)
            {
                var mtiaDescriptor = new OBMtiaDescriptor(minutiae[i], dImg);
                Minutiae.Add(mtiaDescriptor);
            }
        }
    }
}