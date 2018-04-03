/*
 * Created by: Miguel Angel Medina P�rez (miguel.medina.perez@gmail.com)
 *             Andr�s Eduardo Guti�rrez Rodr�guez (andres@bioplantas.cu)
 *             
 * Created: 
 * Comments by: Miguel Angel Medina P�rez (miguel.medina.perez@gmail.com)
 */

using System.Collections.Generic;
using System.Drawing;
using Fingerprints.Computation;
using Fingerprints.Model;

namespace Fingerprints.Tico2003
{
    public class Tico2003FeatureExtractor
    {
        public Tico2003Features ExtractFeatures(Bitmap image)
        {
            var mtiae = MinutiaeExtractor.ExtractFeatures(image);
            var dImg = ImageOrietantionExtractor.ExtractFeatures(image);

            return new Tico2003Features(mtiae, dImg);
        }


        public Tico2003Features ExtractFeatures(List<Minutia> mtiae, OrientationImage orImg)
        {
            return new Tico2003Features(mtiae, orImg);
        }
    }
}