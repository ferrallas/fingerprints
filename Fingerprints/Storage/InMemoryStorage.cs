using System.Collections.Generic;
using System.Linq;

namespace Fingerprints.Storage
{
    public class InMemoryStorage<TFeature> : IStoreProvider<TFeature> where TFeature:class
    {
        private readonly List<Candidate<TFeature>> _features = new List<Candidate<TFeature>>();

        public long CandidatesCount => _features.Count;

        public void Add(Candidate<TFeature> candidate) => _features.Add(candidate);

        public bool ContainsCandidate(string candidate) => _features.Any(x=> x.EntryId == candidate);

        public IEnumerable<string> GetCandidates(int skip, int take) =>
            _features.Skip(skip).Take(take).Select(x => x.EntryId);

        public TFeature Retrieve(string candidate) => _features.FirstOrDefault(x => x.EntryId == candidate)?.Feautures;
    }
}
