/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: Thursday, December 20, 2007
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Drawing;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public interface IFeatureDisplay
    {
        void Show(object features, Graphics g);
    }


    public interface IFeatureDisplay<TFeatureType> : IFeatureDisplay
    {
        void Show(TFeatureType features, Graphics g);
    }


    public abstract class FeatureDisplay<TFeatureType> : IFeatureDisplay<TFeatureType>
    {
        #region IFeatureDisplay<FeatureType> Members

        public abstract void Show(TFeatureType features, Graphics g);

        #endregion

        #region IFeatureDisplay Members

        public void Show(object features, Graphics g)
        {
            if (features.GetType() != typeof(TFeatureType))
            {
                var msg = "Unable to display features: Invalid features type!";
                throw new ArgumentOutOfRangeException(nameof(features), features, msg);
            }
            Show((TFeatureType) features, g);
        }

        #endregion
    }
}