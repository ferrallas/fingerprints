/*
 * Created by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 * Created: June 8, 2010
 * Comments by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 */

using System;
using System.IO;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public static class SkeletonImageSerializer
    {
        public static void Serialize(string fileName, SkeletonImage skImg)
        {
            var byteArr = ToByteArray(skImg);
            File.WriteAllBytes(fileName, byteArr);
        }


        public static SkeletonImage Deserialize(string fileName)
        {
            var byteArr = File.ReadAllBytes(fileName);
            return FromByteArray(byteArr);
        }


        public static SkeletonImage FromByteArray(byte[] bytes)
        {
            int width = bytes[0];
            width |= bytes[1] << 8;
            int height = bytes[2];
            height |= bytes[3] << 8;

            var counter = 0;
            var cursor = 4;
            var imageData = new byte[height, width];
            for (var i = 0; i < height; i++)
            for (var j = 0; j < width; j++)
            {
                var value = 1 & (bytes[cursor] >> counter);
                imageData[i, j] = (byte) (value == 1 ? 255 : 0);
                if (counter == 7)
                {
                    counter = 0;
                    cursor++;
                }
                else
                {
                    counter++;
                }
            }

            return new SkeletonImage(imageData, width, height);
        }


        public static byte[] ToByteArray(SkeletonImage skImg)
        {
            var length = (int) Math.Ceiling(skImg.Width * skImg.Height / 8.0);
            var raw = new byte[length + 4];

            raw[0] = (byte) (255 & skImg.Width);
            raw[1] = (byte) (255 & (skImg.Width >> 8));
            raw[2] = (byte) (255 & skImg.Height);
            raw[3] = (byte) (255 & (skImg.Height >> 8));

            var counter = 0;
            var cursor = 4;
            var currValue = 0;
            for (var i = 0; i < skImg.Height; i++)
            for (var j = 0; j < skImg.Width; j++)
            {
                currValue |= (skImg[i, j] == 255 ? 1 : 0) << counter;
                if (counter == 7)
                {
                    raw[cursor++] = (byte) currValue;
                    currValue = 0;
                    counter = 0;
                }
                else
                {
                    counter++;
                }
            }

            return raw;
        }
    }
}