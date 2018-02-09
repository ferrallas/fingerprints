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


        public override int Height => 3;


        public override int Width => 3;


        public override int Factor => 1;

        #endregion
    }
}