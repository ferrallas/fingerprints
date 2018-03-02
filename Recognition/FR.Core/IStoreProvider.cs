
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public interface IStoreProvider<T>
    {
        IEnumerable<Candidate<T>> Candidates { get; }
        void Add(Candidate<T> candidate);
    }
}