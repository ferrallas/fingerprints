using System;
using System.Windows.Forms;

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
            Application.Run(new FeatureDisplayForm());
        }
    }
}
