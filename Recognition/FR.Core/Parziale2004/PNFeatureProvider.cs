/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;

namespace PatternRecognition.FingerprintRecognition.Core.Parziale2004
{
    public class PNFeatureProvider
    {
        private readonly PNFeatureExtractor mTripletsCalculator = new PNFeatureExtractor();


        public MinutiaListProvider MtiaListProvider { get; set; }


        public  PNFeatures Extract(string fingerprint, ResourceRepository repository)
        {
            try
            {
                var mtiae = MtiaListProvider.Extract(fingerprint, repository);

                //using (StreamWriter sw = new StreamWriter("d:\\Points.txt"))
                //{
                //    foreach (var minutia in mtiae)
                //        sw.WriteLine("(" + minutia.X + "," + minutia.Y + ")");
                //    sw.Close();
                //}


                return mTripletsCalculator.ExtractFeatures(mtiae);
            }
            catch (Exception)
            {
                if (MtiaListProvider == null)
                    throw new InvalidOperationException(
                        "Unable to extract PNFeatures: Unassigned minutia list provider!");
                throw;
            }
        }
    }
}