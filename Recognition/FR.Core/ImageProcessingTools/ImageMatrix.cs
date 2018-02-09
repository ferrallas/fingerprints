/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System.Drawing;
using System.Drawing.Imaging;

namespace PatternRecognition.FingerprintRecognition.Core.ImageProcessingTools
{
    public class ImageMatrix
    {
        #region private

        private readonly int[,] pixels;

        #endregion

        #region public

        public ImageMatrix(Bitmap bmp)
        {
            var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;
            Height = bmp.Height;
            Width = bmp.Width;
            pixels = new int[bmp.Height, bmp.Width];
            unsafe
            {
                var p = (byte*) (void*) scan0;

                var nOffset = stride - bmp.Width * 3;

                byte red, green, blue;

                for (var y = 0; y < bmp.Height; ++y)
                {
                    for (var x = 0; x < bmp.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];

                        pixels[y, x] = (byte) (.299 * red
                                               + .587 * green
                                               + .114 * blue);

                        p += 3;
                    }
                    p += nOffset;
                }
            }

            bmp.UnlockBits(bmData);
        }


        public ImageMatrix(int width, int height)
        {
            Height = height;
            Width = width;
            pixels = new int[height, width];
        }


        public int this[int row, int column]
        {
            get => pixels[row, column];
            set => pixels[row, column] = value;
        }


        public int Height { get; }


        public int Width { get; }


        public Bitmap ToBitmap()
        {
            var bmp = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;

            unsafe
            {
                var p = (byte*) (void*) scan0;
                var nOffset = stride - bmp.Width * 3;
                for (var y = 0; y < Height; ++y)
                {
                    for (var x = 0; x < Width; ++x)
                    {
                        p[0] = p[1] = p[2] = (byte) (pixels[y, x] < 0 ? 0 : pixels[y, x] > 255 ? 255 : pixels[y, x]);
                        p += 3;
                    }
                    p += nOffset;
                }
            }

            bmp.UnlockBits(bmData);
            return bmp;
        }

        #endregion
    }
}