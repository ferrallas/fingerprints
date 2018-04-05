using System.Threading.Tasks;
using ApiKey;
using Fingerprint.WebApplication.Models.ApiKeyModels;
using Fingerprint.WebApplication.Models.FingerPrintModel;
using Microsoft.AspNetCore.Mvc;
using Mongolino;

namespace Fingerprint.WebApplication.Controllers
{
    public class ApiController : Controller
    {
        static readonly Collection<FingerprintCandidate> _candidates = new Collection<FingerprintCandidate>("mongodb://localhost/fingerprints", "candidates");
        static readonly Collection<ApiKeyValue> _apiKeys = new Collection<ApiKeyValue>("mongodb://localhost/fingerprints", "apikeys");

        private readonly ApiKeyValidator _apiKeyValidator = new ApiKeyValidator(new ApiKeyValidatorOptions
        {
            KeySize = 4096,
            StoreApiKeyIntoDatabase = access =>
            {
                _apiKeys.Add(access);
                return access;
            },
            RetrievePrivateKey = File.

        });

        public async Task<IActionResult> Index()
        {
            return Json(new
            {
                Count = await _candidates.CountAsync()
            });
        }

        public async Task<IActionResult> Add(string apiKey)
        {
        }
    }
}