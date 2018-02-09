/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: Thursday, December 20, 2007
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System.Drawing;

namespace PatternRecognition.FingerprintRecognition.Core
{

    public interface IFeatureExtractor<FeatureType> 
    {
        FeatureType ExtractFeatures(Bitmap image);
    }


    public abstract class FeatureExtractor<FeatureType> : IFeatureExtractor<FeatureType>
    {
        #region IFeatureExtractor<FeatureType> Members

        public abstract FeatureType ExtractFeatures(Bitmap image);

        #endregion
    }
}