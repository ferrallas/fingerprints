using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using PatternRecognition.FingerprintRecognition.Core;
using PatternRecognition.FingerprintRecognition.Core.Medina2012;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            var mtiaListProvider = new MinutiaListProvider(new Ratha1995MinutiaeExtractor());
            var orImgProvider = new OrientationImageProvider(new Ratha1995OrImgExtractor());
            var matcher = new M3gl();

            var provider = new MtpsFeatureProvider(mtiaListProvider);
            var repository = new ResourceRepository(@"D:\IMPRONTE");

            var qFeatures = provider.Extract(Path.GetFileNameWithoutExtension("101_1.tif"), repository);

            foreach (var path in Directory.GetFiles(@"D:\IMPRONTE","*.tif"))
            {
                var shortFileName = Path.GetFileNameWithoutExtension(path);
                var tFeatures = provider.Extract(shortFileName, repository);

                var score = matcher.Match(qFeatures, tFeatures, out List<MinutiaPair> matchingMtiae);

                if (Math.Abs(score) < double.Epsilon || matchingMtiae == null) continue;

                if(matchingMtiae.Count > 10) 
                    Console.WriteLine($"{shortFileName} confidence:{score*100}% {matchingMtiae.Count}");
            }
        }
    }
}
