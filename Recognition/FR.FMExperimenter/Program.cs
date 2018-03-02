using System;
using System.Drawing;
using System.IO;
using System.Linq;
using PatternRecognition.FingerprintRecognition.Core;
using PatternRecognition.FingerprintRecognition.Core.Medina2012;

namespace PatternRecognition.FingerprintRecognition.Applications
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var fs = new FileSystemStorage();

            foreach (var f in Directory.GetFiles(@"D:\IMPRONTE","*tif"))
            {
                Medina2012Matcher.Store(fs, new Bitmap(f), Path.GetFileNameWithoutExtension(f));
            }

            var matches = Medina2012Matcher.Match(fs, new Bitmap(@"D:\IMPRONTE\101_1.tif"));

            foreach (var m in matches)
            {
                Console.WriteLine($"{m.EntryId},");
            }
        }
    }
}
