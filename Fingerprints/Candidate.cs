
namespace Fingerprints
{
    public class Candidate<TFeature>
    {
        public string EntryId { get; set; }

        public TFeature Feautures { get; set; }
    }
}