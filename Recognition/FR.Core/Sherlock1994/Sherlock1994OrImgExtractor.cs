/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Drawing;
using PatternRecognition.FingerprintRecognition.Core.ImageProcessingTools;

namespace PatternRecognition.FingerprintRecognition.Core.Sherlock1994
{
    public class Sherlock1994OrImgExtractor : FeatureExtractor<OrientationImage>
    {
        public byte BlockSize { get; set; } = 16;


        public override OrientationImage ExtractFeatures(Bitmap image)
        {
            var matrix = new ImageMatrix(image);

            var width = Convert.ToByte(image.Width / BlockSize);
            var height = Convert.ToByte(image.Height / BlockSize);
            var oi = new OrientationImage(width, height, BlockSize);
            for (var row = 0; row < height; row++)
            for (var col = 0; col < width; col++)
            {
                int x, y;
                oi.GetPixelCoordFromBlock(row, col, out x, out y);

                double maxVariation = 0;
                double bestAngle = 0;
                for (var i = 0; i < 16; i++)
                {
                    var currAngle = i * Math.PI / 16;
                    double currVariation = GetVariation(currAngle, y, x, matrix);
                    if (currVariation > maxVariation)
                    {
                        maxVariation = currVariation;
                        bestAngle = currAngle;
                    }
                }

                if (maxVariation != 0)
                {
                    var angle = Angle.ToDegrees(bestAngle);
                    oi[row, col] = Convert.ToByte(Math.Round(angle));
                }
                else
                {
                    oi[row, col] = OrientationImage.Null;
                }
            }

            RemoveBadBlocksVariance(oi, matrix);
            RemoveBadBlocks(oi);
            var smoothed = SmoothOrImg(oi);
            return smoothed;
        }

        #region private

        private int GetVariation(double angle, int y, int x, ImageMatrix matrix)
        {
            var orthogonalAngle = angle + Math.PI / 2;

            var maxLength = BlockSize / 2;
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

            var variation = 0;
            for (var i = 0; i < projection.Length - 1; i++)
                variation += Math.Abs(projection[i + 1] - projection[i]);

            return variation;
        }

        private void RemoveBadBlocksVariance(OrientationImage oi, ImageMatrix matrix)
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
            double xSum, ySum, xAvg, yAvg, angle;
            int count;
            byte wSize = 3;
            for (var row = 0; row < img.Height; row++)
            for (var col = 0; col < img.Width; col++)
                if (!img.IsNullBlock(row, col))
                {
                    xSum = ySum = count = 0;
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
                        xAvg = xSum / count;
                        yAvg = ySum / count;
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