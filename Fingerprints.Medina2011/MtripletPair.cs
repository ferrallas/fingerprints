using System;

namespace Fingerprints.Medina2011
{
    [Serializable]
    public class MtripletPair
    {
        public double MatchingValue { get; set; }
        public MTriplet QueryMTp { get; set; }
        public byte[] TemplateMtiaOrder { get; set; }
        public MTriplet TemplateMTp { get; set; }
    }
}