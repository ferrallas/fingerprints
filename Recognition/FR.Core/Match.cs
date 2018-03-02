namespace PatternRecognition.FingerprintRecognition.Core
{
    public class Match
    {
        public string EntryId { get; set; }

        public double Confidence { get; set; }

        public int MatchingPoints { get; set; }
    }
}
