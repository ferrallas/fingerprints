/*
 * Created by: Octavio Loyola González (octavioloyola@gmail.com)
 * Created: 
 * Comments by: 
 */

namespace PatternRecognition.FingerprintRecognition.Core.ImageProcessingTools.ConvolutionFilters
{
    public class GaussianBlur : ConvolutionFilter
    {
        public GaussianBlur()
        {
            pixels = new int[5, 5]
            {
                {0, 1, 2, 1, 0},
                {1, 2, 3, 2, 1},
                {2, 3, 4, 3, 2},
                {1, 2, 3, 2, 1},
                {0, 1, 2, 1, 0}
            };
        }


        public override int Height => 5;


        public override int Width => 5;


        public override int Factor => 40;
    }
}