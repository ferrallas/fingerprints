using Fingerprints.Medina2012;
using Mongolino;

namespace Fingerprint.WebApplication.Models.FingerPrintModel
{
    public class FingerprintCandidate : ICollectionItem
    {
        public string Id { get; set; }

        private MtripletsFeature Features { get; set; }
    }
}
