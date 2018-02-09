/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;

namespace PatternRecognition.FingerprintRecognition.Core
{
    [Serializable]
    public class OrientationImage
    {
        public const byte Null = 255;

        #region private fields

        private readonly byte[,] orientations;

        #endregion


        public OrientationImage(byte width, byte height, byte[,] orientations, byte wSize)
        {
            Width = width;
            Height = height;
            WindowSize = wSize;
            this.orientations = orientations;
        }


        public OrientationImage(byte width, byte height, byte wSize)
        {
            Width = width;
            Height = height;
            WindowSize = wSize;
            orientations = new byte[height, width];
        }


        public byte this[int row, int col]
        {
            get => orientations[row, col];
            set => orientations[row, col] = value;
        }


        public byte Width { set; get; }


        public byte Height { set; get; }


        public byte WindowSize { set; get; }


        public double AngleInRadians(int row, int col)
        {
            return IsNullBlock(row, col) ? double.NaN : orientations[row, col] * Math.PI / 180;
        }


        public bool IsNullBlock(int row, int col)
        {
            return orientations[row, col] == Null;
        }


        public void GetBlockCoordFromPixel(double x, double y, out int row, out int col)
        {
            row = Convert.ToInt16(Math.Round((y - 1.0 * WindowSize / 2) / (1.0 * WindowSize)));
            col = Convert.ToInt16(Math.Round((x - 1.0 * WindowSize / 2) / (1.0 * WindowSize)));
        }


        public void GetPixelCoordFromBlock(int row, int col, out double x, out double y)
        {
            x = col * WindowSize + 1.0 * WindowSize / 2;
            y = row * WindowSize + 1.0 * WindowSize / 2;
        }


        public void GetPixelCoordFromBlock(int row, int col, out int x, out int y)
        {
            x = col * WindowSize + WindowSize / 2;
            y = row * WindowSize + WindowSize / 2;
        }


        public void GetBlockCoordFromIdx(int blockIdx, out int row, out int col)
        {
            row = Math.DivRem(blockIdx, Width, out col);
        }


        public int GetBlockIdxFromCoord(int row, int col)
        {
            return row * Width + col;
        }
    }
}