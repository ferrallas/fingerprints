/*
 * Created by: Milton García Borroto (milton.garcia@gmail.com)
 *             Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System.IO;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public class ResourceRepository
    {
        private string resourceBasePath;


        public ResourceRepository(string resourcePath)
        {
            ResourcePath = resourcePath;
        }


        private string ResourcePath
        {
            set
            {
                if (value.EndsWith(@"\"))
                    resourceBasePath = value;
                else
                    resourceBasePath = value + @"\";
            }
            get => resourceBasePath;
        }


        public string GetFullPath(string resourceName)
        {
            return ResourcePath + resourceName.Replace('/', '\\');
        }


        public byte[] RetrieveResource(string resourceName)
        {
            var FullPath = GetFullPath(resourceName);
            if (File.Exists(FullPath))
                return File.ReadAllBytes(FullPath);
            return null;
        }


        public object RetrieveObjectResource(string resourceName)
        {
            var FullPath = GetFullPath(resourceName);
            if (File.Exists(FullPath))
                return BinarySerializer.Deserialize(FullPath);
            return null;
        }


        public void StoreResource(string resourceName, byte[] resource)
        {
            var FullPath = GetFullPath(resourceName);
            Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
            File.WriteAllBytes(FullPath, resource);
        }


        public void StoreResource(string resourceName, object resource)
        {
            var FullPath = GetFullPath(resourceName);
            Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
            BinarySerializer.Serialize(resource, FullPath);
        }


        public bool ResourceExists(string resourceName)
        {
            var FullPath = GetFullPath(resourceName);
            return File.Exists(FullPath);
        }
    }
}