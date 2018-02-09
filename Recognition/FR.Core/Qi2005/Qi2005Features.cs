/*
 * Created by: Andrés Eduardo Gutiérrez Rodríguez (andres@bioplantas.cu)
 *             Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Qi2005
{
    [Serializable]
    public class Qi2005Features
    {
        internal List<GOwMtia> Minutiae;

        internal Qi2005Features(List<Minutia> minutiae, OrientationImage dImg)
        {
            Minutiae = new List<GOwMtia>(minutiae.Count);
            foreach (var mtia in minutiae)
                Minutiae.Add(new GOwMtia(mtia, dImg));
        }
    }
}