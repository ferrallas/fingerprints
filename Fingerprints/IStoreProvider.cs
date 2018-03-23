using System.Collections.Generic;

namespace Fingerprints
{
    public interface IStoreProvider
    {
        bool ContainsCandidate(string candidate);

        long CandidatesCount { get; }

        IEnumerable<string> Candidates { get; }

        void Add(Candidate candidate);

        byte[] Retrieve(string candidate);
    }
}