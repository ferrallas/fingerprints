using System;
using System.Security.Cryptography;

namespace ApiKey
{
    public class ApiKeyValidator
    {
        private readonly ApiKeyValidatorOptions _options;
        private readonly RSACryptoServiceProvider _csp;

        public ApiKeyValidator(ApiKeyValidatorOptions options)
        {
            _options = options ?? throw new NullReferenceException(nameof(options));

            if (_options.RetrievePrivateKey == null) throw new NullReferenceException(nameof(options.RetrievePrivateKey));
            if (_options.StoreApiKeyIntoDatabase == null) throw new NullReferenceException(nameof(options.StoreApiKeyIntoDatabase));
            if (_options.RetrieveApiKeyFromDatabase == null) throw new NullReferenceException(nameof(options.RetrieveApiKeyFromDatabase));
            if (_options.StorePrivateKey == null) throw new NullReferenceException(nameof(options.StorePrivateKey));
            if (_options.Deserialization == null) throw new NullReferenceException(nameof(options.Deserialization));
            if (_options.Serialization == null) throw new NullReferenceException(nameof(options.Serialization));

            var key = options.RetrievePrivateKey();

            if (key == null)
            {
                _csp = new RSACryptoServiceProvider(options.KeySize);
                options.StorePrivateKey(_csp.ExportCspBlob(true));
            }
            else
            {
                _csp = new RSACryptoServiceProvider();
                _csp.ImportCspBlob(key);
            }
        }

        public string Generate(string user)
        {
            var key = _options.StoreApiKeyIntoDatabase(new ApiKeyAccess
            {
                CreatedAt = DateTime.Now,
                ServerSecret = Guid.NewGuid().ToString("N"),
                Use = 0,
                User = user
            });

            if(string.IsNullOrWhiteSpace(key.Id))
                throw new Exception("Api key must have an Id after being saved into the database");

            var serialized = _options.Serialization(key);
            return Convert.ToBase64String(_csp.Encrypt(serialized, false));
        }

        public bool Validate(string apiKey, out string user)
        {
            ApiKeyAccess access;

            try
            {
                access = _options.Deserialization(_csp.Decrypt(Convert.FromBase64String(apiKey), false));
            }
            catch (Exception)
            {
                user = null;
                return false;
            }

            if (access == null || string.IsNullOrWhiteSpace(access.Id))
            {
                user = null;
                return false;
            }

            var retrieved = _options.RetrieveApiKeyFromDatabase(access.Id);

            if (retrieved == null)
            {
                user = null;
                return false;
            }

            user = retrieved.User;

            retrieved.Use = retrieved.Use + 1;
            _options.StoreApiKeyIntoDatabase(retrieved);

            return true;
        }
    }
}