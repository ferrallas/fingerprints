/*
 * Created by: Andrés Eduardo Gutiérrez Rodríguez (andres@bioplantas.cu)
 *             Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace PatternRecognition.FingerprintRecognition.Core.Qi2005
{
    public class Qi2005FeatureExtractor
    {
        public static Qi2005Features ExtractFeatures(List<Minutia> mtiae, OrientationImage orImg)
        {
            return new Qi2005Features(mtiae, orImg);
        }
    }
}