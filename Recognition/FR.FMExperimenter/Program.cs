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
            var candidate = File.ReadAllBytes(@"D:\IMPRONTE\101_1.tif");

            var matcher = new M3gl();
            var provider = new MtpsFeatureProvider();

            var qFeatures = provider.Extract(candidate);

            foreach (var path in Directory.GetFiles(@"D:\IMPRONTE","*.tif"))
            {
                var tFeatures = provider.Extract(File.ReadAllBytes(path));

                var score = matcher.Match(qFeatures, tFeatures, out List<MinutiaPair> matchingMtiae);

                if (Math.Abs(score) < double.Epsilon || matchingMtiae == null)
                    continue;

                if(matchingMtiae.Count > 10) 
                    Console.WriteLine($"{Path.GetFileName(path)} confidence:{score*100}% {matchingMtiae.Count}");
            }
        }
    }
}
