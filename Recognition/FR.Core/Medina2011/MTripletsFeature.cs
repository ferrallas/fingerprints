/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Milton García Borroto (milton.garcia@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2011
{
    internal class MtripletPair
    {
        public double matchingValue;
        public MTriplet queryMTp;
        public byte[] templateMtiaOrder;
        public MTriplet templateMTp;
    }


    [Serializable]
    public class MtripletsFeature
    {
        #region public

        internal MtripletsFeature(List<MTriplet> mtList, List<Minutia> mtiaList)
        {
            mtiaList.TrimExcess();
            Minutiae = mtiaList;

            mtList.TrimExcess();
            MTriplets = mtList;
        }

        internal List<MtripletPair> FindSimilarMTriplets(MTriplet queryMTp)
        {
            var result = new List<MtripletPair>();
            for (var j = 0; j < MTriplets.Count; j++)
            {
                var currMTp = MTriplets[j];
                byte[] currOrder;

                var currSim = queryMTp.Match(currMTp, out currOrder);

                if (currSim > 0)
                    result.Add(new MtripletPair
                        {
                            queryMTp = queryMTp,
                            templateMTp = currMTp,
                            matchingValue = currSim,
                            templateMtiaOrder = currOrder
                        }
                    );
            }
            if (result.Count > 0)
                return result;
            return null;
        }

        internal List<MTriplet> MTriplets { get; }

        public List<Minutia> Minutiae { get; }

        #endregion
    }
}