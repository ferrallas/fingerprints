/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;

namespace PatternRecognition.FingerprintRecognition.Core.ImageProcessingTools
{
    /// <summary>
    ///     Represents a convolution filter.
    /// </summary>
    public class ConvolutionFilter
    {
        /// <summary>
        ///     Gets or sets the value of a pixel in the filter.
        /// </summary>
        /// <param name="row">The row of the specified pixel.</param>
        /// <param name="column">The column of the specified pixel.</param>
        /// <returns>The value of the filter in the specified pixel.</returns>
        public int this[int row, int column] => pixels[row, column];

        /// <summary>
        ///     Gets the height of the filter.
        /// </summary>
        public virtual int Height { private set; get; }

        /// <summary>
        ///     Gets the width of the filter.
        /// </summary>
        public virtual int Width { private set; get; }

        /// <summary>
        ///     A factor to divide the value before assigning to the pixel.
        /// </summary>
        public virtual int Factor { private set; get; }

        /// <summary>
        ///     Applies the current convolution filter to the specified <see cref="ImageMatrix"/>.
        /// </summary>
        /// <param name="img">
        ///     The <see cref="ImageMatrix"/> where the convolution filter will be applied.
        /// </param>
        /// <returns>
        ///     A new <see cref="ImageMatrix"/> resulting from applying the current filter to the specified <see cref="ImageMatrix"/>.
        /// </returns>
        public ImageMatrix Apply(ImageMatrix img)
        {
            ImageMatrix newImg = new ImageMatrix(img.Width, img.Height);
            int dy = Height / 2;
            int dx = Width / 2;
            int sum;
            double value;
            for (int row = 0; row < img.Height; row++)
                for (int col = 0; col < img.Width; col++)
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

        /// <summary>
        ///     The matrix of the filter.
        /// </summary>
        protected int[,] pixels;

        /// <summary>
        ///     A base constructor to be used in concrete classes.
        /// </summary>
        protected ConvolutionFilter()
        {

        }

        #endregion
    }
}