/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Andrés Eduardo Gutiérrez Rodríguez (andres@bioplantas.cu)
 *             
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using Fingerprints.Model;

namespace Fingerprints.Tico2003
{
    [Serializable]
    public class TicoFeatures
    {
        internal readonly List<MinutiaDescriptor> Minutiae;

        internal TicoFeatures(IReadOnlyList<Minutia> minutiae, OrientationImage dImg)
        {
            Minutiae = new List<MinutiaDescriptor>(minutiae.Count);
            for (short i = 0; i < minutiae.Count; i++)
            {
                var mtiaDescriptor = new MinutiaDescriptor(minutiae[i], dImg);
                Minutiae.Add(mtiaDescriptor);
            }
        }
    }
}