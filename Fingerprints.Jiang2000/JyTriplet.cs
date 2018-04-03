/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */


using Fingerprints.Model;

namespace Fingerprints.Jiang2000
{
    internal class JyTriplet
    {
        public MinutiaPair MainMinutia { set; get; }
        public MinutiaPair NearestMtia { set; get; }
        public MinutiaPair FarthestMtia { set; get; }
        public double MatchingValue { set; get; }
    }
}