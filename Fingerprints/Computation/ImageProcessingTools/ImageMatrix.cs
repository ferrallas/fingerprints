/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System.Drawing;
using System.Drawing.Imaging;

namespace Fingerprints.Computation.ImageProcessingTools
{
    public class ImageMatrix
    {
        private readonly int[,] _pixels;

        public ImageMatrix(Bitmap bmp)
        {
            var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;
            Height = bmp.Height;
            Width = bmp.Width;
            _pixels = new int[bmp.Height, bmp.Width];
            unsafe
            {
                var p = (byte*) (void*) scan0;

                var nOffset = stride - bmp.Width * 3;

                for (var y = 0; y < bmp.Height; ++y)
                {
                    for (var x = 0; x < bmp.Width; ++x)
                    {
                        var blue = p[0];
                        var green = p[1];
                        var red = p[2];

                        _pixels[y, x] = (byte) (.299 * red
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
            _pixels = new int[height, width];
        }


        public int this[int row, int column]
        {
            get => _pixels[row, column];
            set => _pixels[row, column] = value;
        }


        public int Height { get; }


        public int Width { get; }
    }
}