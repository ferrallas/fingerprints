/*
 * Created by: Andrés Eduardo Gutiérrez Rodríguez (andres@bioplantas.cu)
 *             Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using Fingerprints.Model;

namespace Fingerprints.Qi2005
{
    [Serializable]
    public class QiFeatures
    {
        public List<QiMinutia> Minutiae { get; set; }

        public QiFeatures()
        {

        }

        public QiFeatures(IReadOnlyCollection<Minutia> minutiae, OrientationImage dImg)
        {
            Minutiae = new List<QiMinutia>(minutiae.Count);
            foreach (var mtia in minutiae)
                Minutiae.Add(new QiMinutia(mtia, dImg));
        }
    }
}