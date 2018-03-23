using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fingerprints.Run
{
    internal class FileSystemStorage : IStoreProvider
    {
        private readonly string _folder;
        private readonly string _extension;

        public long CandidatesCount => Directory.GetFiles(_folder, $"*.{_extension}").Length;

        IEnumerable<string> IStoreProvider.Candidates => Directory.GetFiles(_folder, $"*.{_extension}").Select(Path.GetFileNameWithoutExtension);

        public FileSystemStorage(string folder, string extension = ".dat")
        {
            _folder = folder;
            _extension = extension;
        }

        public void Add(Candidate candidate)
        {
            File.WriteAllBytes(Path.Combine(_folder, $"{candidate.EntryId}.{_extension}"),candidate.Feautures);
        }

        public bool ContainsCandidate(string candidate)
        {
            return File.Exists(Path.Combine(_folder, $"{candidate}.{_extension}"));
        }

        public byte[] Retrieve(string candidate)
        {
            return File.ReadAllBytes(Path.Combine(_folder,$"{candidate}.{_extension}"));
        }
    }
}