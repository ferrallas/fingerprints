using System.Collections.Generic;

namespace Fingerprints
{
    public interface IStoreProvider<TFeature>
    {
        bool ContainsCandidate(string candidate);

        long CandidatesCount { get; }

        IEnumerable<string> GetCandidates(int skip, int take);

        void Add(Candidate<TFeature> candidate);

        TFeature Retrieve(string candidate);
    }
}