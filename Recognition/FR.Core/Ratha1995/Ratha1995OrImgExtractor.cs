/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Drawing;
using PatternRecognition.FingerprintRecognition.Core.ImageProcessingTools;
using PatternRecognition.FingerprintRecognition.Core.ImageProcessingTools.ConvolutionFilters;

namespace PatternRecognition.FingerprintRecognition.Core.Ratha1995
{
    public class Ratha1995OrImgExtractor
    {
        private const byte BlockSize = 16;

        private readonly SobelHorizontalFilter _xFilter = new SobelHorizontalFilter();

        private readonly SobelVerticalFilter _yFilter = new SobelVerticalFilter();

        public OrientationImage ExtractFeatures(Bitmap image)
        {
            var matrix = new ImageMatrix(image);

            var gx = _yFilter.Apply(matrix);
            var gy = _xFilter.Apply(matrix);

            var width = Convert.ToByte(image.Width / BlockSize);
            var height = Convert.ToByte(image.Height / BlockSize);
            var oi = new OrientationImage(width, height, BlockSize);
            for (var row = 0; row < height; row++)
            for (var col = 0; col < width; col++)
            {
                int x, y;
                oi.GetPixelCoordFromBlock(row, col, out x, out y);
                var x0 = Math.Max(x - BlockSize / 2, 0);
                var x1 = Math.Min(image.Width - 1, x + BlockSize / 2);
                var y0 = Math.Max(y - BlockSize / 2, 0);
                var y1 = Math.Min(image.Height - 1, y + BlockSize / 2);

                int gxy = 0, gxx = 0, gyy = 0;
                for (var yi = y0; yi <= y1; yi++)
                for (var xi = x0; xi <= x1; xi++)
                {
                    gxy += gx[yi, xi] * gy[yi, xi];
                    gxx += gx[yi, xi] * gx[yi, xi];
                    gyy += gy[yi, xi] * gy[yi, xi];
                }

                if (gxx - gyy == 0 && gxy == 0)
                {
                    oi[row, col] = OrientationImage.Null;
                }
                else
                {
                    var angle = Angle.ToDegrees(Angle.ComputeAngle(gxx - gyy, 2 * gxy));
                    angle = angle / 2 + 90;
                    if (angle > 180)
                        angle = angle - 180;
                    oi[row, col] = Convert.ToByte(Math.Round(angle));
                }
            }

            RemoveBadBlocksVariance(oi, matrix);
            RemoveBadBlocks(oi);
            var smoothed = SmoothOrImg(oi);
            return smoothed;
        }

        #region private

        private static void RemoveBadBlocksVariance(OrientationImage oi, ImageMatrix matrix)
        {
            var maxLength = oi.WindowSize / 2;
            var varianceMatrix = new int[oi.Height, oi.Width];
            double max = 0;
            var min = double.MaxValue;
            for (var row = 0; row < oi.Height; row++)
            for (var col = 0; col < oi.Width; col++)
            {
                int x, y;
                oi.GetPixelCoordFromBlock(row, col, out x, out y);

                // Computing Average
                var sum = 0;
                var count = 0;
                for (var xi = x - maxLength; xi < x + maxLength; xi++)
                for (var yi = y - maxLength; yi < y + maxLength; yi++)
                    if (xi >= 0 && xi < matrix.Width && yi >= 0 && yi < matrix.Height)
                    {
                        sum += matrix[yi, xi];
                        count++;
                    }
                var avg = 1.0 * sum / count;

                // Computing Variance
                double sqrSum = 0;
                for (var xi = x - maxLength; xi < x + maxLength; xi++)
                for (var yi = y - maxLength; yi < y + maxLength; yi++)
                    if (xi >= 0 && xi < matrix.Width && yi >= 0 && yi < matrix.Height)
                    {
                        var diff = matrix[yi, xi] - avg;
                        sqrSum += diff * diff;
                    }
                varianceMatrix[row, col] = Convert.ToInt32(Math.Round(sqrSum / (count - 1)));

                // Computing de max variance
                if (varianceMatrix[row, col] > max)
                    max = varianceMatrix[row, col];
                if (varianceMatrix[row, col] < min)
                    min = varianceMatrix[row, col];
            }

            for (var row = 0; row < oi.Height; row++)
            for (var col = 0; col < oi.Width; col++)
                varianceMatrix[row, col] =
                    Convert.ToInt32(Math.Round(254.0 * (varianceMatrix[row, col] - min) / (max - min)));

            const int t = 15;
            for (var row = 0; row < oi.Height; row++)
            for (var col = 0; col < oi.Width; col++)
                if (!oi.IsNullBlock(row, col) && varianceMatrix[row, col] <= t)
                    oi[row, col] = OrientationImage.Null;
        }

        private void RemoveBadBlocks(OrientationImage oi)
        {
            var neighborsMatrix = new int[oi.Height, oi.Width];
            for (var row0 = 0; row0 < oi.Height; row0++)
            for (var col0 = 0; col0 < oi.Width; col0++)
                if (oi[row0, col0] != OrientationImage.Null)
                {
                    var lowRow = Math.Max(0, row0 - 1);
                    var lowCol = Math.Max(0, col0 - 1);
                    var highRow = Math.Min(row0 + 1, oi.Height - 1);
                    var highCol = Math.Min(col0 + 1, oi.Width - 1);
                    for (var row1 = lowRow; row1 <= highRow; row1++)
                    for (var col1 = lowCol; col1 <= highCol; col1++)
                        if (oi[row1, col1] != OrientationImage.Null)
                            neighborsMatrix[row0, col0]++;
                }

            for (var row0 = 0; row0 < oi.Height; row0++)
            for (var col0 = 0; col0 < oi.Width; col0++)
                if (oi[row0, col0] != OrientationImage.Null)
                {
                    var shouldRemove = true;
                    var lowRow = Math.Max(0, row0 - 1);
                    var lowCol = Math.Max(0, col0 - 1);
                    var highRow = Math.Min(row0 + 1, oi.Height - 1);
                    var highCol = Math.Min(col0 + 1, oi.Width - 1);
                    for (var row1 = lowRow; row1 <= highRow && shouldRemove; row1++)
                    for (var col1 = lowCol; col1 <= highCol && shouldRemove; col1++)
                        if (neighborsMatrix[row1, col1] == 9)
                            shouldRemove = false;
                    if (shouldRemove)
                        oi[row0, col0] = OrientationImage.Null;
                }
        }


        private OrientationImage SmoothOrImg(OrientationImage img)
        {
            var smoothed = new OrientationImage(img.Width, img.Height, img.WindowSize);
            const byte wSize = 3;
            for (var row = 0; row < img.Height; row++)
            for (var col = 0; col < img.Width; col++)
                if (!img.IsNullBlock(row, col))
                {
                    int count;
                    double ySum;
                    var xSum = ySum = count = 0;
                    double angle;
                    for (var y = row - wSize / 2; y <= row + wSize / 2; y++)
                    for (var x = col - wSize / 2; x <= col + wSize / 2; x++)
                        if (y >= 0 && y < img.Height && x >= 0 && x < img.Width && !img.IsNullBlock(y, x))
                        {
                            angle = img.AngleInRadians(y, x);
                            xSum += Math.Cos(2 * angle);
                            ySum += Math.Sin(2 * angle);
                            count++;
                        }
                    if (count == 0 || xSum == 0 && ySum == 0)
                    {
                        smoothed[row, col] = OrientationImage.Null;
                    }
                    else
                    {
                        var xAvg = xSum / count;
                        var yAvg = ySum / count;
                        angle = Angle.ToDegrees(Angle.ComputeAngle(xAvg, yAvg)) / 2;

                        smoothed[row, col] = Convert.ToByte(Math.Round(angle));
                    }
                }
                else
                {
                    smoothed[row, col] = OrientationImage.Null;
                }

            return smoothed;
        }

        #endregion
    }
}