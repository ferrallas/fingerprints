using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
            var candidate = ImageProvider.GetResource(File.ReadAllBytes(@"D:\IMPRONTE\101_1.tif"));

            var qFeatures = M3Gl.Extract(candidate);

            var ss = Directory.GetFiles(@"D:\IMPRONTE", "*.tif")
                .Select(File.ReadAllBytes)
                .ToArray();


            foreach (var tFeatures in ss)
            {

                var feat = M3Gl.Extract(ImageProvider.GetResource(File.ReadAllBytes(@"D:\IMPRONTE\101_1.tif")));
                var score = M3Gl.Match(qFeatures, feat, out List<MinutiaPair> matchingMtiae);

                if (Math.Abs(score) < double.Epsilon || matchingMtiae == null)
                    continue;

                if(matchingMtiae.Count > 10) 
                    Console.WriteLine($"{tFeatures} confidence:{score*100}% {matchingMtiae.Count}");
            }
        }
    }
}
