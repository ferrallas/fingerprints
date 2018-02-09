/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Andrés Eduardo Gutiérrez Rodríguez (andres@bioplantas.cu)
 *             
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System.Collections.Generic;
using System.Drawing;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Tico2003
{
    public class Tico2003FeatureExtractor
    {
        readonly Ratha1995OrImgExtractor _orImgExtractor  = new Ratha1995OrImgExtractor();


        public Tico2003Features ExtractFeatures(Bitmap image)
        {
            var mtiae = MinutiaeExtractor.ExtractFeatures(image);
            var dImg = _orImgExtractor.ExtractFeatures(image);

            return new Tico2003Features(mtiae, dImg);
        }


        public Tico2003Features ExtractFeatures(List<Minutia> mtiae, OrientationImage orImg)
        {
            return new Tico2003Features(mtiae, orImg);
        }
    }
}