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
    public class Tico2003Features
    {
        internal readonly List<ObMtiaDescriptor> Minutiae;

        internal Tico2003Features(IReadOnlyList<Minutia> minutiae, OrientationImage dImg)
        {
            Minutiae = new List<ObMtiaDescriptor>(minutiae.Count);
            for (short i = 0; i < minutiae.Count; i++)
            {
                var mtiaDescriptor = new ObMtiaDescriptor(minutiae[i], dImg);
                Minutiae.Add(mtiaDescriptor);
            }
        }
    }
}