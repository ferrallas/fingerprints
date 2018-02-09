/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;

namespace PatternRecognition.FingerprintRecognition.Core.ImageProcessingTools
{
    public class ConvolutionFilter
    {
        public int this[int row, int column] => pixels[row, column];


        public virtual int Height { private set; get; }


        public virtual int Width { private set; get; }


        public virtual int Factor { private set; get; }


        public ImageMatrix Apply(ImageMatrix img)
        {
            var newImg = new ImageMatrix(img.Width, img.Height);
            var dy = Height / 2;
            var dx = Width / 2;
            int sum;
            double value;
            for (var row = 0; row < img.Height; row++)
            for (var col = 0; col < img.Width; col++)
            {
                sum = 0;
                for (int yi = row - dy, yj = 0; yi <= row + dy; yi++, yj++)
                for (int xi = col - dx, xj = 0; xi <= col + dx; xi++, xj++)
                    if (yi >= 0 && yi < img.Height && xi >= 0 && xi < img.Width)
                        sum += img[yi, xi] * this[yj, xj];
                    else
                        sum += 255 * this[yj, xj];

                value = 1.0 * sum / Factor;
                newImg[row, col] = Convert.ToInt32(Math.Round(value));
            }
            return newImg;
        }

        #region protected

        protected int[,] pixels;


        protected ConvolutionFilter()
        {
        }

        #endregion
    }
}