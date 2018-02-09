/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Andrés Eduardo Gutiérrez Rodríguez (andres@bioplantas.cu)
 *             
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace PatternRecognition.FingerprintRecognition.Core.Tico2003
{
    public class Tico2003FeatureExtractor : FeatureExtractor<Tico2003Features>
    {
        public IFeatureExtractor<List<Minutia>> MtiaExtractor { set; get; }


        public IFeatureExtractor<OrientationImage> OrImgExtractor { set; get; }


        public override Tico2003Features ExtractFeatures(Bitmap image)
        {
            try
            {
                var mtiae = MtiaExtractor.ExtractFeatures(image);
                var dImg = OrImgExtractor.ExtractFeatures(image);

                return new Tico2003Features(mtiae, dImg);
            }
            catch (Exception e)
            {
                if (MtiaExtractor == null)
                    throw new InvalidOperationException(
                        "Can not extract Tico2003Features: Unassigned minutia list extractor!", e);
                if (OrImgExtractor == null)
                    throw new InvalidOperationException(
                        "Can not extract Tico2003Features: Unassigned orientation image extractor!", e);
                throw;
            }
        }


        public Tico2003Features ExtractFeatures(List<Minutia> mtiae, OrientationImage orImg)
        {
            return new Tico2003Features(mtiae, orImg);
        }
    }
}