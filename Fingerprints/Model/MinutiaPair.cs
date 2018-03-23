namespace Fingerprints.Model
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
            return mp1?.Equals(mp2) ?? mp2 is null;
        }


        public static bool operator !=(MinutiaPair mp1, MinutiaPair mp2)
        {
            return !mp1?.Equals(mp2) ?? !(mp2 is null);
        }


        private bool Equals(MinutiaPair obj)
        {
            return !(obj is null) && (obj.QueryMtia.Equals(QueryMtia) && obj.TemplateMtia.Equals(TemplateMtia));
        }


        public override bool Equals(object obj)
        {
            var mp = obj as MinutiaPair;
            return Equals(mp);
        }
    }
}