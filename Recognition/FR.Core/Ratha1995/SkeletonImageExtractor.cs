/*
 * Created by: Octavio Loyola González (octavioloyola@gmail.com) and Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Drawing;
using PatternRecognition.FingerprintRecognition.Core.ImageProcessingTools;
using PatternRecognition.FingerprintRecognition.Core.ImageProcessingTools.ConvolutionFilters;

namespace PatternRecognition.FingerprintRecognition.Core.Ratha1995
{
    public static class SkeletonImageExtractor
    {
        #region public

        public static SkeletonImage ExtractFeatures(Bitmap image)
        {
            var ratha1995OrImgExtractor = new Ratha1995OrImgExtractor();
            var orientationImage = ratha1995OrImgExtractor.ExtractFeatures(image);
            var skeletonImage = ExtractSkeletonImage(image, orientationImage);

            var img = new byte[skeletonImage.Width * skeletonImage.Height];
            for (var i = 0; i < skeletonImage.Height; i++)
            for (var j = 0; j < skeletonImage.Width; j++)
                img[skeletonImage.Width * i + j] = (byte) skeletonImage[i, j];

            return new SkeletonImage(img, skeletonImage.Width, skeletonImage.Height);
        }


        public static ImageMatrix ExtractSkeletonImage(Bitmap image, OrientationImage orientationImage)
        {
            var matrix = new ImageMatrix(image);

            var gb = new GaussianBlur();
            matrix = gb.Apply(matrix);

            matrix = GetBinaryImage(matrix, orientationImage);
            ApplyThinning(matrix, orientationImage);
            RemoveSpikes(matrix, orientationImage);
            FixBifurcations(matrix, orientationImage);
            RemoveSmallSegments(matrix, orientationImage);

            return matrix;
        }

        #endregion

        #region private

        private static ImageMatrix GetBinaryImage(ImageMatrix matrix, OrientationImage orientationImage)
        {
            int[] filter = {1, 2, 5, 7, 5, 2, 1};
            var newImg = new ImageMatrix(matrix.Width, matrix.Height);
            for (var i = 0; i < matrix.Width; i++)
            for (var j = 0; j < matrix.Height; j++)
                newImg[j, i] = 255;
            for (var row = 0; row < orientationImage.Height; row++)
            for (var col = 0; col < orientationImage.Width; col++)
                if (!orientationImage.IsNullBlock(row, col))
                {
                    int x, y;
                    orientationImage.GetPixelCoordFromBlock(row, col, out x, out y);

                    var maxLength = orientationImage.WindowSize / 2;
                    for (var xi = x - maxLength; xi < x + maxLength; xi++)
                    for (var yi = y - maxLength; yi < y + maxLength; yi++)
                    {
                        var projection = GetProjection(orientationImage, row, col, xi, yi, matrix);

                        var smoothed = new int[orientationImage.WindowSize + 1];
                        const int n = 7;
                        for (var j = 0; j < projection.Length; j++)
                        {
                            var idx = 0;
                            int sum = 0, count = 0;
                            for (var k = j - n / 2; k <= j + n / 2; k++, idx++)
                                if (k >= 0 && k < projection.Length)
                                {
                                    sum += projection[k] * filter[idx];
                                    count++;
                                }
                            smoothed[j] = sum / count;
                        }

                        var center = smoothed.Length / 2;
                        int left;
                        for (left = center - 1; smoothed[left] == smoothed[center] && left > 0; left--) ;

                        int rigth;
                        for (rigth = center + 1;
                            smoothed[rigth] == smoothed[center] && rigth < smoothed.Length - 1;
                            rigth++) ;

                        if (xi >= 0 && xi < matrix.Width && yi >= 0 && yi < matrix.Height)
                            newImg[yi, xi] = 255;

                        if (xi > 0 && xi < matrix.Width - 1 && yi > 0 && yi < matrix.Height - 1 &&
                            !(left == 255 && rigth == smoothed.Length - 1))
                            if (smoothed[center] < smoothed[left] && smoothed[center] < smoothed[rigth])
                                newImg[yi, xi] = 0;
                            else if (rigth - left == 2 &&
                                     (smoothed[left] < smoothed[left - 1] &&
                                      smoothed[left] < smoothed[center] ||
                                      smoothed[rigth] < smoothed[rigth + 1] &&
                                      smoothed[rigth] < smoothed[center] ||
                                      smoothed[center] < smoothed[left - 1] &&
                                      smoothed[center] < smoothed[rigth + 1] ||
                                      smoothed[center] < smoothed[left - 1] &&
                                      smoothed[center] < smoothed[rigth] ||
                                      smoothed[center] < smoothed[left] &&
                                      smoothed[center] < smoothed[rigth + 1]))
                                newImg[yi, xi] = 0;
                    }
                }
            return newImg;
        }

        private static int[] GetProjection(OrientationImage oi, int row, int col, int x, int y, ImageMatrix matrix)
        {
            var angle = oi.AngleInRadians(row, col);
            var orthogonalAngle = oi.AngleInRadians(row, col) + Math.PI / 2;

            var maxLength = oi.WindowSize / 2;
            var projection = new int[2 * maxLength + 1];
            var outlayerCount = new int[2 * maxLength + 1];
            var outlayerFound = false;
            var totalSum = 0;
            var validPointsCount = 0;
            for (int li = -maxLength, i = 0; li <= maxLength; li++, i++)
            {
                var xi = Convert.ToInt32(x - li * Math.Cos(orthogonalAngle));
                var yi = Convert.ToInt32(y - li * Math.Sin(orthogonalAngle));

                var ySum = 0;
                for (var lj = -maxLength; lj <= maxLength; lj++)
                {
                    var xj = Convert.ToInt32(xi - lj * Math.Cos(angle));
                    var yj = Convert.ToInt32(yi - lj * Math.Sin(angle));
                    if (xj >= 0 && yj >= 0 && xj < matrix.Width && yj < matrix.Height)
                    {
                        ySum += matrix[yj, xj];
                        validPointsCount++;
                    }
                    else
                    {
                        outlayerCount[i]++;
                        outlayerFound = true;
                    }
                }
                projection[i] = ySum;
                totalSum += ySum;
            }

            if (outlayerFound)
            {
                var avg = totalSum / validPointsCount;
                for (var i = 0; i < projection.Length; i++)
                    projection[i] += avg * outlayerCount[i];
            }

            return projection;
        }

        private static void ApplyThinning(ImageMatrix matrix, OrientationImage orientationImage)
        {
            var changed = true;
            while (changed)
            {
                changed = false;
                for (var row = 0; row < orientationImage.Height; row++)
                for (var col = 0; col < orientationImage.Width; col++)
                    if (!orientationImage.IsNullBlock(row, col))
                    {
                        int x, y;
                        orientationImage.GetPixelCoordFromBlock(row, col, out x, out y);

                        var maxLength = orientationImage.WindowSize / 2;
                        for (var xi = x - maxLength; xi < x + maxLength; xi++)
                        for (var yi = y - maxLength; yi < y + maxLength; yi++)
                            if (xi > 0 && xi < matrix.Width - 1 && yi > 0 && yi < matrix.Height - 1)
                            {
                                var tl = matrix[yi - 1, xi - 1];
                                var tc = matrix[yi - 1, xi];
                                var tr = matrix[yi - 1, xi + 1];

                                var le = matrix[yi, xi - 1];
                                var ce = matrix[yi, xi];
                                var ri = matrix[yi, xi + 1];

                                var bl = matrix[yi + 1, xi - 1];
                                var bc = matrix[yi + 1, xi];
                                var br = matrix[yi + 1, xi + 1];

                                if (IsVl(tl, tc, tr, le, ce, ri, bl, bc, br) ||
                                    IsVr(tl, tc, tr, le, ce, ri, bl, bc, br) ||
                                    IsHt(tl, tc, tr, le, ce, ri, bl, bc, br) ||
                                    IsHb(tl, tc, tr, le, ce, ri, bl, bc, br))
                                {
                                    matrix[yi, xi] = 255;
                                    changed = true;
                                }
                            }
                            else
                            {
                                matrix[yi, xi] = 255;
                            }
                    }
            }
        }

        private static void RemoveSpikes(ImageMatrix matrix, OrientationImage orientationImage)
        {
            for (var row = 0; row < orientationImage.Height; row++)
            for (var col = 0; col < orientationImage.Width; col++)
                if (!orientationImage.IsNullBlock(row, col))
                {
                    var cos = new double[3];
                    var sin = new double[3];

                    var orthogonalAngle = orientationImage.AngleInRadians(row, col) + Math.PI / 2;
                    cos[0] = Math.Cos(orthogonalAngle);
                    sin[0] = Math.Sin(orthogonalAngle);

                    var orthogonalAngle1 = orthogonalAngle + Math.PI / 12;
                    cos[1] = Math.Cos(orthogonalAngle1);
                    sin[1] = Math.Sin(orthogonalAngle1);

                    var orthogonalAngle2 = orthogonalAngle - Math.PI / 12;
                    cos[2] = Math.Cos(orthogonalAngle2);
                    sin[2] = Math.Sin(orthogonalAngle2);

                    int x, y;
                    orientationImage.GetPixelCoordFromBlock(row, col, out x, out y);

                    var maxLength = orientationImage.WindowSize / 2;
                    for (var xi = x - maxLength; xi < x + maxLength; xi++)
                    for (var yi = y - maxLength; yi < y + maxLength; yi++)
                    {
                        var xj = xi;
                        var yj = yi;
                        var spikeFound = true;
                        while (spikeFound)
                        {
                            spikeFound = false;
                            if (xj > 0 && xj < matrix.Width - 1 && yj > 0 && yj < matrix.Height - 1)
                            {
                                var tl = matrix[yj - 1, xj - 1];
                                var tc = matrix[yj - 1, xj];
                                var tr = matrix[yj - 1, xj + 1];

                                var le = matrix[yj, xj - 1];
                                var ce = matrix[yj, xj];
                                var ri = matrix[yj, xj + 1];

                                var bl = matrix[yj + 1, xj - 1];
                                var bc = matrix[yj + 1, xj];
                                var br = matrix[yj + 1, xj + 1];

                                if (CouldBeSpike(tl, tc, tr, le, ce, ri, bl, bc, br))
                                    for (var i = 0; i < sin.Length && !spikeFound; i++)
                                    {
                                        var xk = Convert.ToInt32(Math.Round(xj - cos[i]));
                                        var yk = Convert.ToInt32(Math.Round(yj - sin[i]));
                                        if (matrix[yk, xk] == 0)
                                        {
                                            matrix[yj, xj] = 255;
                                            xj = xk;
                                            yj = yk;
                                            spikeFound = true;
                                        }
                                        else
                                        {
                                            xk = Convert.ToInt32(Math.Round(xj + cos[i]));
                                            yk = Convert.ToInt32(Math.Round(yj + sin[i]));
                                            if (matrix[yk, xk] == 0)
                                            {
                                                matrix[yj, xj] = 255;
                                                xj = xk;
                                                yj = yk;
                                                spikeFound = true;
                                            }
                                        }
                                    }
                            }
                        }
                    }
                }
        }

        private static void FixBifurcations(ImageMatrix matrix, OrientationImage orientationImage)
        {
            var changed = true;
            while (changed)
            {
                changed = false;
                for (var row = 0; row < orientationImage.Height; row++)
                for (var col = 0; col < orientationImage.Width; col++)
                    if (!orientationImage.IsNullBlock(row, col))
                    {
                        int x, y;
                        orientationImage.GetPixelCoordFromBlock(row, col, out x, out y);

                        var maxLength = orientationImage.WindowSize / 2;
                        for (var xi = x - maxLength; xi < x + maxLength; xi++)
                        for (var yi = y - maxLength; yi < y + maxLength; yi++)
                            if (xi > 0 && xi < matrix.Width - 1 && yi > 0 && yi < matrix.Height - 1)
                            {
                                var tl = matrix[yi - 1, xi - 1];
                                var tc = matrix[yi - 1, xi];
                                var tr = matrix[yi - 1, xi + 1];

                                var le = matrix[yi, xi - 1];
                                var ce = matrix[yi, xi];
                                var ri = matrix[yi, xi + 1];

                                var bl = matrix[yi + 1, xi - 1];
                                var bc = matrix[yi + 1, xi];
                                var br = matrix[yi + 1, xi + 1];

                                if (IsCorner(tl, tc, tr, le, ce, ri, bl, bc, br))
                                {
                                    matrix[yi, xi] = 255;
                                    changed = true;
                                }
                            }
                            else
                            {
                                matrix[yi, xi] = 255;
                            }
                    }
            }
        }

        private static void RemoveSmallSegments(ImageMatrix matrix, OrientationImage orientationImage)
        {
            for (var row = 0; row < orientationImage.Height; row++)
            for (var col = 0; col < orientationImage.Width; col++)
                if (!orientationImage.IsNullBlock(row, col))
                {
                    int x, y;
                    orientationImage.GetPixelCoordFromBlock(row, col, out x, out y);

                    var maxLength = orientationImage.WindowSize / 2;
                    for (var xi = x - maxLength; xi < x + maxLength; xi++)
                    for (var yi = y - maxLength; yi < y + maxLength; yi++)
                    {
                        const int pThreshold = 10;
                        var xArr = new int[pThreshold + 1];
                        var yArr = new int[pThreshold + 1];

                        int x0, y0;
                        if (IsEnd(matrix, xi, yi, out x0, out y0))
                        {
                            xArr[0] = xi;
                            yArr[0] = yi;

                            xArr[1] = x0;
                            yArr[1] = y0;

                            var endFound = false;
                            var bifurcationFound = false;
                            var pCount = 1;
                            for (var i = 1; i < pThreshold && !endFound && !bifurcationFound; i++)
                            {
                                if (IsEnd(matrix, xArr[i], yArr[i], out xArr[i + 1], out yArr[i + 1]))
                                    endFound = true;
                                else if (!IsContinous(matrix, xArr[i - 1], yArr[i - 1], xArr[i], yArr[i],
                                    out xArr[i + 1], out yArr[i + 1]))
                                    bifurcationFound = true;

                                pCount++;
                            }
                            if (endFound || bifurcationFound && pCount <= pThreshold)
                                for (var i = 0; i < pCount; i++)
                                    matrix[yArr[i], xArr[i]] = 255;
                        }
                    }
                }
        }

        private static bool IsVr(int tl, int tc, int tr, int le, int ce, int ri, int bl, int bc, int br)
        {
            if (tl == 0 && tc == 255 && tr == 255 &&
                le == 0 && ce == 0 && ri == 255 &&
                bl == 0 && bc == 255 && br == 255
            )
                return true;
            if (tl == 0 && tc == 255 && tr == 255 &&
                le == 0 && ce == 0 && ri == 255 &&
                bl == 0 && bc == 0 && br == 0
            )
                return true;
            if (tl == 255 && tc == 255 && tr == 255 &&
                le == 0 && ce == 0 && ri == 255 &&
                bl == 0 && bc == 0 && br == 255
            )
                return true;
            if (tl == 0 && tc == 255 && tr == 255 &&
                le == 0 && ce == 0 && ri == 255 &&
                bl == 0 && bc == 0 && br == 255
            )
                return true;
            if (tl == 0 && tc == 0 && tr == 255 &&
                le == 0 && ce == 0 && ri == 255 &&
                bl == 0 && bc == 255 && br == 255
            )
                return true;
            if (tl == 0 && tc == 0 && tr == 255 &&
                le == 0 && ce == 0 && ri == 255 &&
                bl == 0 && bc == 0 && br == 255
            )
                return true;

            return false;
        }

        private static bool IsVl(int tl, int tc, int tr, int le, int ce, int ri, int bl, int bc, int br)
        {
            if (tl == 255 && tc == 255 && tr == 0 &&
                le == 255 && ce == 0 && ri == 0 &&
                bl == 255 && bc == 255 && br == 0
            )
                return true;
            if (tl == 0 && tc == 0 && tr == 0 &&
                le == 255 && ce == 0 && ri == 0 &&
                bl == 255 && bc == 255 && br == 0
            )
                return true;
            if (tl == 255 && tc == 0 && tr == 0 &&
                le == 255 && ce == 0 && ri == 0 &&
                bl == 255 && bc == 255 && br == 255
            )
                return true;
            if (tl == 255 && tc == 0 && tr == 0 &&
                le == 255 && ce == 0 && ri == 0 &&
                bl == 255 && bc == 255 && br == 0
            )
                return true;
            if (tl == 255 && tc == 255 && tr == 0 &&
                le == 255 && ce == 0 && ri == 0 &&
                bl == 255 && bc == 0 && br == 0
            )
                return true;
            if (tl == 255 && tc == 0 && tr == 0 &&
                le == 255 && ce == 0 && ri == 0 &&
                bl == 255 && bc == 0 && br == 0
            )
                return true;

            return false;
        }

        private static bool IsHb(int tl, int tc, int tr, int le, int ce, int ri, int bl, int bc, int br)
        {
            if (tl == 0 && tc == 0 && tr == 0 &&
                le == 255 && ce == 0 && ri == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
                return true;
            if (tl == 0 && tc == 0 && tr == 0 &&
                le == 0 && ce == 0 && ri == 255 &&
                bl == 0 && bc == 255 && br == 255
            )
                return true;
            if (tl == 0 && tc == 0 && tr == 255 &&
                le == 0 && ce == 0 && ri == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
                return true;
            if (tl == 0 && tc == 0 && tr == 0 &&
                le == 0 && ce == 0 && ri == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
                return true;
            if (tl == 0 && tc == 0 && tr == 0 &&
                le == 255 && ce == 0 && ri == 0 &&
                bl == 255 && bc == 255 && br == 255
            )
                return true;
            if (tl == 0 && tc == 0 && tr == 0 &&
                le == 0 && ce == 0 && ri == 0 &&
                bl == 255 && bc == 255 && br == 255
            )
                return true;
            return false;
        }

        private static bool IsHt(int tl, int tc, int tr, int le, int ce, int ri, int bl, int bc, int br)
        {
            if (tl == 255 && tc == 255 && tr == 255 &&
                le == 255 && ce == 0 && ri == 255 &&
                bl == 0 && bc == 0 && br == 0
            )
                return true;
            if (tl == 255 && tc == 255 && tr == 0 &&
                le == 255 && ce == 0 && ri == 0 &&
                bl == 0 && bc == 0 && br == 0
            )
                return true;
            if (tl == 255 && tc == 255 && tr == 255 &&
                le == 255 && ce == 0 && ri == 0 &&
                bl == 255 && bc == 0 && br == 0
            )
                return true;
            if (tl == 255 && tc == 255 && tr == 255 &&
                le == 255 && ce == 0 && ri == 0 &&
                bl == 0 && bc == 0 && br == 0
            )
                return true;
            if (tl == 255 && tc == 255 && tr == 255 &&
                le == 0 && ce == 0 && ri == 255 &&
                bl == 0 && bc == 0 && br == 0
            )
                return true;
            if (tl == 255 && tc == 255 && tr == 255 &&
                le == 0 && ce == 0 && ri == 0 &&
                bl == 0 && bc == 0 && br == 0
            )
                return true;
            return false;
        }

        private static bool CouldBeSpike(int tl, int tc, int tr, int le, int ce, int ri, int bl, int bc, int br)
        {
            if (tl == 255 && tc == 255 && tr == 255 &&
                le == 255 && ce == 0 && ri == 255 &&
                bc == 0
            )
                return true;
            if (tl == 255 && tc == 255 && tr == 255 &&
                le == 255 && ce == 0 &&
                bl == 255 && br == 0
            )
                return true;
            if (tl == 255 && tc == 255 &&
                le == 255 && ce == 0 && ri == 0 &&
                bl == 255 && bc == 255
            )
                return true;
            if (tl == 255 && tr == 0 &&
                le == 255 && ce == 0 &&
                bl == 255 && bc == 255 && br == 255
            )
                return true;
            if (tc == 0 &&
                le == 255 && ce == 0 && ri == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
                return true;
            if (tl == 0 && tr == 255 &&
                ce == 0 && ri == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
                return true;
            if (tc == 255 && tr == 255 &&
                le == 0 && ce == 0 && ri == 255 &&
                bc == 255 && br == 255
            )
                return true;
            if (tl == 255 && tc == 255 && tr == 255 &&
                ce == 0 && ri == 255 &&
                bl == 0 && br == 255
            )
                return true;

            return false;
        }

        private static bool IsCorner(int tl, int tc, int tr, int le, int ce, int ri, int bl, int bc, int br)
        {
            if (tl == 255 && tc == 255 && //tr == 255 &&
                le == 255 && ce == 0 && ri == 0 &&
                /*bl == 255 &&*/ bc == 0 /*&& br == 255*/
            )
                return true;

            if ( /*tl == 255 &&*/ tc == 0 && //tr == 255 &&
                                  le == 255 && ce == 0 && ri == 0 &&
                                  bl == 255 && bc == 255 //&& br == 255
            )
                return true;

            if ( /*tl == 255 &&*/ tc == 0 && //tr == 255 &&
                                  le == 0 && ce == 0 && ri == 255 &&
                                  /*bl == 255 &&*/ bc == 255 && br == 255
            )
                return true;

            if ( /*tl == 255 &&*/ tc == 255 && tr == 255 &&
                                  le == 0 && ce == 0 && ri == 255 &&
                                  /*bl == 255 &&*/ bc == 0 //&& br == 255
            )
                return true;

            return false;
        }

        private static bool IsEnd(ImageMatrix matrix, int x, int y, out int x1, out int y1)
        {
            x1 = -1;
            y1 = -1;

            var tl = x > 0 && y > 0 ? matrix[y - 1, x - 1] : 255;
            var tc = y > 0 ? matrix[y - 1, x] : 255;
            var tr = x < matrix.Width - 1 && y > 0 ? matrix[y - 1, x + 1] : 255;
            var cl = x > 0 ? matrix[y, x - 1] : 255;
            var ce = matrix[y, x];
            var cr = x < matrix.Width - 1 ? matrix[y, x + 1] : 255;
            var bl = x > 0 && y < matrix.Height - 1 ? matrix[y + 1, x - 1] : 255;
            var bc = y < matrix.Height - 1 ? matrix[y + 1, x] : 255;
            var br = x < matrix.Width - 1 && y < matrix.Height - 1 ? matrix[y + 1, x + 1] : 255;

            if (tl == 255 && tc == 255 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
            {
                x1 = x;
                y1 = y;
                return true;
            }
            if (tl == 0 && tc == 255 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
            {
                x1 = x - 1;
                y1 = y - 1;
                return true;
            }
            if (tl == 255 && tc == 0 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
            {
                x1 = x;
                y1 = y - 1;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 0 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
            {
                x1 = x + 1;
                y1 = y - 1;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 0 &&
                bl == 255 && bc == 255 && br == 255
            )
            {
                x1 = x + 1;
                y1 = y;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 255 && br == 0
            )
            {
                x1 = x + 1;
                y1 = y + 1;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 0 && br == 255
            )
            {
                x1 = x;
                y1 = y + 1;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 0 && bc == 255 && br == 255
            )
            {
                x1 = x - 1;
                y1 = y + 1;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 255 &&
                cl == 0 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
            {
                x1 = x - 1;
                y1 = y;
                return true;
            }
            return false;
        }

        private static bool IsContinous(ImageMatrix matrix, int x0, int y0, int x, int y, out int x1, out int y1)
        {
            x1 = -1;
            y1 = -1;
            var isBlack = false;
            if (matrix[y0, x0] == 0)
            {
                matrix[y0, x0] = 255;
                isBlack = true;
            }

            var tl = x > 0 && y > 0 ? matrix[y - 1, x - 1] : 255;
            var tc = y > 0 ? matrix[y - 1, x] : 255;
            var tr = x < matrix.Width - 1 && y > 0 ? matrix[y - 1, x + 1] : 255;
            var cl = x > 0 ? matrix[y, x - 1] : 255;
            var ce = matrix[y, x];
            var cr = x < matrix.Width - 1 ? matrix[y, x + 1] : 255;
            var bl = x > 0 && y < matrix.Height - 1 ? matrix[y + 1, x - 1] : 255;
            var bc = y < matrix.Height - 1 ? matrix[y + 1, x] : 255;
            var br = x < matrix.Width - 1 && y < matrix.Height - 1 ? matrix[y + 1, x + 1] : 255;

            if (tl == 0 && tc == 255 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
            {
                x1 = x - 1;
                y1 = y - 1;
                if (isBlack)
                    matrix[y0, x0] = 0;
                return true;
            }
            if (tl == 255 && tc == 0 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
            {
                x1 = x;
                y1 = y - 1;
                if (isBlack)
                    matrix[y0, x0] = 0;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 0 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
            {
                x1 = x + 1;
                y1 = y - 1;
                if (isBlack)
                    matrix[y0, x0] = 0;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 0 &&
                bl == 255 && bc == 255 && br == 255
            )
            {
                x1 = x + 1;
                y1 = y;
                if (isBlack)
                    matrix[y0, x0] = 0;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 255 && br == 0
            )
            {
                x1 = x + 1;
                y1 = y + 1;
                if (isBlack)
                    matrix[y0, x0] = 0;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 0 && br == 255
            )
            {
                x1 = x;
                y1 = y + 1;
                if (isBlack)
                    matrix[y0, x0] = 0;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 255 &&
                cl == 255 && ce == 0 && cr == 255 &&
                bl == 0 && bc == 255 && br == 255
            )
            {
                x1 = x - 1;
                y1 = y + 1;
                if (isBlack)
                    matrix[y0, x0] = 0;
                return true;
            }
            if (tl == 255 && tc == 255 && tr == 255 &&
                cl == 0 && ce == 0 && cr == 255 &&
                bl == 255 && bc == 255 && br == 255
            )
            {
                x1 = x - 1;
                y1 = y;
                if (isBlack)
                    matrix[y0, x0] = 0;
                return true;
            }
            return false;
        }

        #endregion
    }
}