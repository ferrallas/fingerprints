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
            _image = new byte[Height, Width];
            for (var i = 0; i < Height; i++)
            for (var j = 0; j < Width; j++)
                _image[i, j] = img[Width * i + j];
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

        private int Width { get; }


        private int Height { get; }

        #endregion

        #region private

        private readonly byte[,] _image;

        private byte PixelEnviroment(Point p)
        {
            if (_image[p.Y - 1, p.X - 1] == 0) return 0;
            if (_image[p.Y - 1, p.X] == 0) return 0;
            if (_image[p.Y - 1, p.X + 1] == 0) return 0;
            if (_image[p.Y, p.X - 1] == 0) return 0;
            if (_image[p.Y, p.X] == 0) return 0;
            if (_image[p.Y, p.X + 1] == 0) return 0;
            if (_image[p.Y + 1, p.X - 1] == 0) return 0;
            if (_image[p.Y + 1, p.X] == 0) return 0;
            if (_image[p.Y + 1, p.X + 1] == 0) return 0;

            return 255;
        }

        private static List<Point> Bresenham(int x0, int y0, int x1, int y1)
        {
            var pixels = new List<Point>();
            int p, incE, incNe, stepx, stepy;
            var dx = x1 - x0;
            var dy = y1 - y0;
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
            var x = x0;
            var y = y0;
            pixels.Add(new Point(x, y));
            if (dx > dy)
            {
                p = 2 * dy - dx;
                incE = 2 * dy;
                incNe = 2 * (dy - dx);
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
                        p = p + incNe;
                    }
                    pixels.Add(new Point(x, y));
                }
            }
            else
            {
                p = 2 * dx - dy;
                incE = 2 * dx;
                incNe = 2 * (dx - dy);
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
                        p = p + incNe;
                    }
                    pixels.Add(new Point(x, y));
                }
            }
            return pixels;
        }

        #endregion
    }
}