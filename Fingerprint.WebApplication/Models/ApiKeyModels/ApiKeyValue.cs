using ApiKey;
using Mongolino;

namespace Fingerprint.WebApplication.Models.ApiKeyModels
{
    public class ApiKeyValue: ICollectionItem
    {
        public ApiKeyAccess Access { get; set; }

        public string Id
        {
            get => Access.Id;
            set => Access.Id = value;
        }

        public static implicit operator ApiKeyAccess(ApiKeyValue value) => value.Access;

        public static implicit operator ApiKeyValue(ApiKeyAccess value) => new ApiKeyValue { Access = value };
    }
}
