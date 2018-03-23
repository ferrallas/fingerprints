/*
 * Created by: Octavio Loyola González (octavioloyola@gmail.com)
 * Created: 
 * Comments by: 
 */

namespace Fingerprints.Computation.ImageProcessingTools.ConvolutionFilters
{
    public class GaussianBlur : ConvolutionFilter
    {
        public GaussianBlur()
        {
            Pixels = new[,]
            {
                {0, 1, 2, 1, 0},
                {1, 2, 3, 2, 1},
                {2, 3, 4, 3, 2},
                {1, 2, 3, 2, 1},
                {0, 1, 2, 1, 0}
            };
        }


        protected override int Height => 5;


        protected override int Width => 5;


        protected override int Factor => 40;
    }
}