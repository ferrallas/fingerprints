/*
 * Created by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 */

using System;
using System.IO;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public static class OrientationImageSerializer
    {
        public static byte[] ToByteArray(OrientationImage orImg)
        {
            var bytes = new byte[orImg.Width * orImg.Height + 3];
            bytes[0] = orImg.WindowSize;
            bytes[1] = orImg.Height;
            bytes[2] = orImg.Width;
            var k = 3;
            for (var i = 0; i < orImg.Height; i++)
            for (var j = 0; j < orImg.Width; j++)
                if (orImg.IsNullBlock(i, j))
                    bytes[k++] = 255;
                else
                    bytes[k++] = Convert.ToByte(Math.Round(orImg.AngleInRadians(i, j) * 180 / Math.PI));
            return bytes;
        }


        public static OrientationImage FromByteArray(byte[] bytes)
        {
            var height = bytes[1];
            var width = bytes[2];
            var orientations = new byte[height, width];
            for (var i = 0; i < height; i++)
            for (var j = 0; j < width; j++)
                orientations[i, j] = Convert.ToByte(bytes[i * width + j + 3]);

            var orImg = new OrientationImage(width, height, orientations, bytes[0]);
            return orImg;
        }


        public static void Serialize(string fileName, OrientationImage orImg)
        {
            var byteArr = ToByteArray(orImg);
            File.WriteAllBytes(fileName, byteArr);
        }


        public static OrientationImage Deserialize(string fileName)
        {
            var byteArr = File.ReadAllBytes(fileName);
            return FromByteArray(byteArr);
        }
    }
}