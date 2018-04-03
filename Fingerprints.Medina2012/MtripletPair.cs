using System;

namespace Fingerprints.Medina2012
{
    [Serializable]
    public class MtripletPair
    {
        public double MatchingValue;
        public MTriplet QueryMTp;
        public byte[] TemplateMtiaOrder;
        public MTriplet TemplateMTp;
    }
}