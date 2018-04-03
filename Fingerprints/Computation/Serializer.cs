using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Fingerprints.Computation
{
    public static class Serializer
    {
        public static byte[] Serialize<T>(T obj)
        {
            var ms = new MemoryStream();
            using (var writer = new BsonDataWriter(ms))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, obj);
            }

            return ms.ToArray();
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            var ms = new MemoryStream(bytes);
            using (var writer = new BsonDataReader(ms))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(writer);
            }
        }
    }
}
