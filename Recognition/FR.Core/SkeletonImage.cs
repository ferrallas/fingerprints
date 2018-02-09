/*
 * Created by: Andrés Eduardo Gutiérrez Rodríguez (andres@bioplantas.cu), 
 *             Milton García Borroto (milton.garcia@gmail.com) and
 *             Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)             
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PatternRecognition.FingerprintRecognition.Core
{
    [Serializable]
    public class SkeletonImage
    {
        #region public

        public SkeletonImage(byte[] img, int width, int height)
        {
            Width = width;
            Height = height;
            image = new byte[Height, Width];
            for (var i = 0; i < Height; i++)
            for (var j = 0; j < Width; j++)
                image[i, j] = img[Width * i + j];
        }


        public SkeletonImage(byte[,] img, int width, int height)
        {
            Width = width;
            Height = height;
            image = img;
        }


        public unsafe Bitmap ConvertToBitmap()
        {
            var bmp = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
            var cp = bmp.Palette;
            for (var i = 1; i < 256; i++)
                cp.Entries[i] = Color.FromArgb(i, i, i);
            cp.Entries[0] = Color.Black;
            bmp.Palette = cp;
            var bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, bmp.PixelFormat);

            var p = (byte*) (void*) bmData.Scan0;
            var b = 0;
            var nOffset = bmData.Stride - bmp.Width;
            for (var y = 0; y < bmp.Height; ++y)
            {
                Marshal.Copy(ConvertMatrixToArray(), b, (IntPtr) p, bmp.Width);
                p += bmp.Width + nOffset;
                b += bmp.Width;
            }
            bmp.UnlockBits(bmData);
            return bmp;
        }


        public byte RidgeCount(int x0, int y0, int x1, int y1)
        {
            byte count = 0;
            var points = Bresenham(x0, y0, x1, y1);
            var i = 0;
            while (i < points.Count)
            {
                var j = i;
                if (i == 0)
                {
                    while (i < points.Count && PixelEnviroment(points[i]) == 0)
                        i++;
                    j = i;
                    if (i >= points.Count)
                        break;
                }
                while (j < points.Count && PixelEnviroment(points[j]) == 255)
                    j++;
                i = j;
                if (i >= points.Count)
                    break;
                while (i < points.Count && PixelEnviroment(points[i]) == 0)
                    i++;
                if (i >= points.Count)
                    break;
                count++;
            }
            return count;
        }


        public byte this[int row, int col] => image[row, col];


        public int Width { get; }


        public int Height { get; }

        #endregion

        #region private

        private readonly byte[,] image;

        private byte[] ConvertMatrixToArray()
        {
            var img = new byte[Width * Height];
            for (var i = 0; i < Height; i++)
            for (var j = 0; j < Width; j++)
                img[Width * i + j] = image[i, j];
            return img;
        }

        private byte PixelEnviroment(Point p)
        {
            if (image[p.Y - 1, p.X - 1] == 0) return 0;
            if (image[p.Y - 1, p.X] == 0) return 0;
            if (image[p.Y - 1, p.X + 1] == 0) return 0;
            if (image[p.Y, p.X - 1] == 0) return 0;
            if (image[p.Y, p.X] == 0) return 0;
            if (image[p.Y, p.X + 1] == 0) return 0;
            if (image[p.Y + 1, p.X - 1] == 0) return 0;
            if (image[p.Y + 1, p.X] == 0) return 0;
            if (image[p.Y + 1, p.X + 1] == 0) return 0;

            return 255;
        }

        private List<Point> Bresenham(int x0, int y0, int x1, int y1)
        {
            var pixels = new List<Point>();
            int x, y, dx, dy, p, incE, incNE, stepx, stepy;
            dx = x1 - x0;
            dy = y1 - y0;
            if (dy < 0)
            {
                dy = -dy;
                stepy = -1;
            }
            else
            {
                stepy = 1;
            }
            if (dx < 0)
            {
                dx = -dx;
                stepx = -1;
            }
            else
            {
                stepx = 1;
            }
            x = x0;
            y = y0;
            pixels.Add(new Point(x, y));
            if (dx > dy)
            {
                p = 2 * dy - dx;
                incE = 2 * dy;
                incNE = 2 * (dy - dx);
                while (x != x1)
                {
                    x = x + stepx;
                    if (p < 0)
                    {
                        p = p + incE;
                    }
                    else
                    {
                        y = y + stepy;
                        p = p + incNE;
                    }
                    pixels.Add(new Point(x, y));
                }
            }
            else
            {
                p = 2 * dx - dy;
                incE = 2 * dx;
                incNE = 2 * (dx - dy);
                while (y != y1)
                {
                    y = y + stepy;
                    if (p < 0)
                    {
                        p = p + incE;
                    }
                    else
                    {
                        x = x + stepx;
                        p = p + incNE;
                    }
                    pixels.Add(new Point(x, y));
                }
            }
            return pixels;
        }

        #endregion
    }
}