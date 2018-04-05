using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Fingerprints.Medina2012;
using Fingerprints.Storage;

namespace Fingerprints.Run
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var dataFolder = "./data";

            var matcher = new Medina2012Matcher();

            var fs = new FileSystemStorage<MtripletsFeature>(dataFolder, "dat");

            foreach (var f in Directory.GetFiles(dataFolder,"*.tif"))
            {
                var candidateName = Path.GetFileNameWithoutExtension(f);

                if (!fs.ContainsCandidate(candidateName))
                   matcher.Store(fs, new Bitmap(f), candidateName);
            }


            Console.WriteLine($"Searching between {fs.CandidatesCount} candidates");

            var ts = Stopwatch.StartNew();

            var matches = matcher.Match(fs, new Bitmap(Path.Combine(dataFolder,"101_1.tif")), 0, (int) fs.CandidatesCount);

            ts.Stop();

            foreach (var m in matches)
            {
                Console.WriteLine($"{m.EntryId},");
            }
            
            Console.WriteLine($"Found in {ts.ElapsedMilliseconds} ms");
            Console.ReadLine();
        }
    }
}
