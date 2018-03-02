using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PatternRecognition.FingerprintRecognition.Core;
using PatternRecognition.FingerprintRecognition.Core.Medina2012;

namespace PatternRecognition.FingerprintRecognition.Applications
{
    internal class FileSystemStorage : IStoreProvider<MtripletsFeature>
    {
        public IEnumerable<Candidate<MtripletsFeature>> Candidates => Directory.GetFiles(@"D:\IMPRONTE", "*.json")
            .Select(File.ReadAllText).Select(json =>
            {
                if(json == null)
                    Console.WriteLine();

                return JsonConvert.DeserializeObject<Candidate<MtripletsFeature>>(json);
            }).ToArray();

        public void Add(Candidate<MtripletsFeature> candidate)
        {
            File.WriteAllText(Path.Combine(@"D:\IMPRONTE", $"{candidate.EntryId}.json"), JsonConvert.SerializeObject(candidate));
        }
    }
}