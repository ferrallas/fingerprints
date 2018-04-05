using System;

namespace ApiKey
{
    public class ApiKeyValidatorOptions
    {
        public int KeySize;

        public Action<byte[]> StorePrivateKey;

        public Func<byte[]> RetrievePrivateKey;

        public Func<ApiKeyAccess, ApiKeyAccess> StoreApiKeyIntoDatabase;

        public Func<string, ApiKeyAccess> RetrieveApiKeyFromDatabase;

        public Func<ApiKeyAccess, byte[]> Serialization;

        public Func<byte[], ApiKeyAccess> Deserialization;
    }
}
