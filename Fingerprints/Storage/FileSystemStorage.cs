using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fingerprints.Storage
{
    internal class FileSystemStorage<TFeature> : IStoreProvider<TFeature>
    {
        private readonly string _folder;
        private readonly string _extension;

        public long CandidatesCount => Directory.GetFiles(_folder, $"*.{_extension}").Length;

        public FileSystemStorage(string folder, string extension = ".dat")
        {
            _folder = folder;
            _extension = extension;
        }

        public IEnumerable<string> GetCandidates(int skip, int take)
        {
            return Directory.GetFiles(_folder, $"*.{_extension}")
                .Select(Path.GetFileNameWithoutExtension)
                .Skip(skip)
                .Take(take);
        }

        public void Add(Candidate<TFeature> candidate)
        {
            File.WriteAllBytes(Path.Combine(_folder, $"{candidate.EntryId}.{_extension}"), BsonSerializer.Serialize(candidate.Feautures));
        }

        public bool ContainsCandidate(string candidate)
        {
            return File.Exists(Path.Combine(_folder, $"{candidate}.{_extension}"));
        }

        public TFeature Retrieve(string candidate)
        {
            return BsonSerializer.Deserialize<TFeature>(File.ReadAllBytes(Path.Combine(_folder,$"{candidate}.{_extension}")));
        }
    }
}