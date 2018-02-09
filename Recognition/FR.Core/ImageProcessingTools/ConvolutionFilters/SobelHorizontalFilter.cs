/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

namespace PatternRecognition.FingerprintRecognition.Core.ImageProcessingTools.ConvolutionFilters
{
    public class SobelHorizontalFilter : ConvolutionFilter
    {
        #region ConvolutionFilter Members

        public SobelHorizontalFilter()
        {
            Pixels = new int[3, 3]
            {
                {1, 2, 1},
                {0, 0, 0},
                {-1, -2, -1}
            };
        }


        protected override int Height => 3;


        protected override int Width => 3;


        protected override int Factor => 1;

        #endregion
    }
}