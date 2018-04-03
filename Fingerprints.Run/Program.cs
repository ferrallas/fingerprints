using System;

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
            /*
            var fs = new FileSystemStorage(@"D:\IMPRONTE", "dat");

            foreach (var f in Directory.GetFiles(@"D:\IMPRONTE","*.tif"))
            {
                var candidateName = Path.GetFileNameWithoutExtension(f);

                if (!fs.ContainsCandidate(candidateName))
                   FingerPrintMatcher.Store(Algorithm.Medina2012, fs, new Bitmap(f), candidateName);
            }


            Console.WriteLine($"Searching between {fs.CandidatesCount} candidates");

            var ts = Stopwatch.StartNew();

            var matches = FingerPrintMatcher.Match(Algorithm.Medina2012, fs, new Bitmap(@"D:\IMPRONTE\101_1.tif"));

            ts.Stop();

            foreach (var m in matches)
            {
                Console.WriteLine($"{m.EntryId},");
            }
            
            Console.WriteLine($"Found in {ts.ElapsedMilliseconds} ms");*/
            Console.ReadLine();
        }
    }
}
