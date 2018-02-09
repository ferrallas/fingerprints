/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */


namespace PatternRecognition.FingerprintRecognition.Core.Jiang2000
{
    public partial class Jy
    {
        private class JyTriplet
        {
            public MinutiaPair MainMinutia { set; get; }
            public MinutiaPair NearestMtia { set; get; }
            public MinutiaPair FarthestMtia { set; get; }
            public double MatchingValue { set; get; }
        }
    }
}