/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;

namespace Fingerprints.Computation.ImageProcessingTools
{
    public class ConvolutionFilter
    {
        protected int[,] Pixels;

        protected ConvolutionFilter()
        {
        }

        private int this[int row, int column] => Pixels[row, column];


        protected virtual int Height { private set; get; }


        protected virtual int Width { private set; get; }


        protected virtual int Factor { private set; get; }


        public ImageMatrix Apply(ImageMatrix img)
        {
            var newImg = new ImageMatrix(img.Width, img.Height);
            var dy = Height / 2;
            var dx = Width / 2;
            for (var row = 0; row < img.Height; row++)
            for (var col = 0; col < img.Width; col++)
            {
                var sum = 0;
                for (int yi = row - dy, yj = 0; yi <= row + dy; yi++, yj++)
                for (int xi = col - dx, xj = 0; xi <= col + dx; xi++, xj++)
                    if (yi >= 0 && yi < img.Height && xi >= 0 && xi < img.Width)
                        sum += img[yi, xi] * this[yj, xj];
                    else
                        sum += 255 * this[yj, xj];

                var value = 1.0 * sum / Factor;
                newImg[row, col] = Convert.ToInt32(Math.Round(value));
            }

            return newImg;
        }
    }
}