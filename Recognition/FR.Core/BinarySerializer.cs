/*
 * Created by: Milton García Borroto (milton.garcia@gmail.com) 
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public static class BinarySerializer
    {
        public static void Serialize(object obj, string FileName)
        {
            var formatter = new BinaryFormatter();
            Stream stream = new FileStream(FileName, FileMode.Create);
            formatter.Serialize(stream, obj);
            stream.Close();
        }


        public static byte[] Serialize(object obj)
        {
            var stream = new MemoryStream();
            Serialize(obj, stream);
            return stream.ToArray();
        }


        public static void Serialize(object obj, Stream stream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
        }


        public static object Deserialize(Stream stream)
        {
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }


        public static object Deserialize(string fileName)
        {
            var formatter = new BinaryFormatter();
            Stream stream = new FileStream(fileName, FileMode.Open);
            var Result = formatter.Deserialize(stream);
            stream.Close();
            return Result;
        }


        public static object Deserialize(byte[] data)
        {
            var stream = new MemoryStream(data);
            return Deserialize(stream);
        }
    }
}