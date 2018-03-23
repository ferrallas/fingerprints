namespace PatternRecognition.FingerprintRecognition.Core
{
    public class MinutiaPair
    {
        public Minutia QueryMtia { set; get; }


        public Minutia TemplateMtia { set; get; }


        public double MatchingValue { set; get; }


        public override int GetHashCode()
        {
            unchecked
            {
                return (QueryMtia.GetHashCode() * 397) ^ TemplateMtia.GetHashCode();
            }
        }

        public override string ToString()
        {
            return GetHashCode().ToString();
        }


        public static bool operator ==(MinutiaPair mp1, MinutiaPair mp2)
        {
            if (!ReferenceEquals(null, mp1))
                return mp1.Equals(mp2);
            return ReferenceEquals(null, mp2);
        }


        public static bool operator !=(MinutiaPair mp1, MinutiaPair mp2)
        {
            if (!ReferenceEquals(null, mp1))
                return !mp1.Equals(mp2);
            return !ReferenceEquals(null, mp2);
        }


        private bool Equals(MinutiaPair obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.QueryMtia.Equals(QueryMtia) && obj.TemplateMtia.Equals(TemplateMtia);
        }


        public override bool Equals(object obj)
        {
            var mp = obj as MinutiaPair;
            return Equals(mp);
        }
    }
}