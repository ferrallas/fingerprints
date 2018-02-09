/*
 * Created by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public static class MinutiaListSerializer
    {
        public static byte[] ToByteArray(List<Minutia> minutiaList)
        {
            var bytes = new byte[minutiaList.Count * 4];
            var k = 0;
            for (var i = 0; i < minutiaList.Count; i++)
            {
                var currArr = MtiaToByteArray(minutiaList[i]);
                for (var j = 0; j < 4; j++)
                    bytes[k++] = currArr[j];
            }
            return bytes;
        }


        public static List<Minutia> FromByteArray(byte[] bytes)
        {
            if (bytes.Length % 4 != 0)
                throw new ArgumentOutOfRangeException(nameof(bytes), bytes,
                    "Invalid bytes count: A correct bytes count can be divided by 4.");
            var mtiaCount = bytes.Length / 4;
            var mtiae = new List<Minutia>(mtiaCount);
            for (var i = 0; i < mtiaCount; i++)
            {
                var mtiaBytes = new byte[4];
                for (var k = 0; k < 4; k++)
                    mtiaBytes[k] = bytes[i * 4 + k];

                mtiae.Add(MtiaFromByteArray(mtiaBytes));
            }
            return mtiae;
        }


        public static void Serialize(string fileName, List<Minutia> minutiae)
        {
            var byteArr = ToByteArray(minutiae);
            File.WriteAllBytes(fileName, byteArr);
        }


        public static List<Minutia> Deserialize(string fileName)
        {
            var byteArr = File.ReadAllBytes(fileName);
            return FromByteArray(byteArr);
        }

        #region private

        private static Minutia MtiaFromByteArray(byte[] bytes)
        {
            var mtia = new Minutia();
            var info = (bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0];

            mtia.MinutiaType = (MinutiaType) (3 & info);
            info >>= 2;
            mtia.Angle = 2 * Math.PI * (255 & info) / 255;
            info >>= 8;
            mtia.Y = Convert.ToInt16(info & 2047);
            info >>= 11;
            mtia.X = Convert.ToInt16(info & 2047);

            return mtia;
        }

        private static byte[] MtiaToByteArray(Minutia mtia)
        {
            var bytes = new byte[4];
            // Storing value X in the left most 11 bits.
            var blockX = (2047 & mtia.X) << 21;
            // Storing value Y in the next 11 bits.
            var blockY = (2047 & mtia.Y) << 10;
            // Storing value Angle in the next 8 bits.
            var blockAngle = Convert.ToByte(Math.Round(mtia.Angle * 255 / (2 * Math.PI))) << 2;
            // Storing value MinutiaType in the last 2 bits.
            var blockType = (int) mtia.MinutiaType;
            // Merging all data
            var info = blockX | blockY | blockAngle | blockType;

            bytes[0] = Convert.ToByte(255 & info);
            info >>= 8;
            bytes[1] = Convert.ToByte(255 & info);
            info >>= 8;
            bytes[2] = Convert.ToByte(255 & info);
            info >>= 8;
            bytes[3] = Convert.ToByte(255 & info);

            return bytes;
        }

        #endregion
    }
}