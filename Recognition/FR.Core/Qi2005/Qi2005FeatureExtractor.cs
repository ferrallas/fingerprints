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
    public class Qi2005FeatureExtractor : FeatureExtractor<Qi2005Features>
    {
        public IFeatureExtractor<List<Minutia>> MtiaExtractor { set; get; }


        public IFeatureExtractor<OrientationImage> OrImgExtractor { set; get; }


        public override Qi2005Features ExtractFeatures(Bitmap image)
        {
            try
            {
                var mtiae = MtiaExtractor.ExtractFeatures(image);
                var dImg = OrImgExtractor.ExtractFeatures(image);

                return new Qi2005Features(mtiae, dImg);
            }
            catch (Exception e)
            {
                if (MtiaExtractor == null)
                    throw new InvalidOperationException(
                        "Can not extract Qi2005Features: Unassigned minutia list extractor!", e);
                if (OrImgExtractor == null)
                    throw new InvalidOperationException(
                        "Can not extract Qi2005Features: Unassigned orientation image extractor!", e);
                throw;
            }
        }


        public Qi2005Features ExtractFeatures(List<Minutia> mtiae, OrientationImage orImg)
        {
            return new Qi2005Features(mtiae, orImg);
        }
    }
}