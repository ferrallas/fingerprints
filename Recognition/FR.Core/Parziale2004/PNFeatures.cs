/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core.Parziale2004
{
    internal class MtiaeTripletPair
    {
        public MtiaTriplet queryMTp;
        public MtiaTriplet templateMTp;
    }


    [Serializable]
    public class PNFeatures
    {
        #region internal

        internal PNFeatures(List<MtiaTriplet> mtList, List<Minutia> mtiaList)
        {
            Minutiae = mtiaList;
            MTriplets = mtList;
        }

        internal List<MtiaeTripletPair> FindAllSimilar(MtiaTriplet queryMTp)
        {
            var result = new List<MtiaeTripletPair>();
            for (var j = 0; j < MTriplets.Count; j++)
            {
                var currMTp = MTriplets[j];
                if (queryMTp.Match(currMTp))
                    result.Add(new MtiaeTripletPair
                        {
                            queryMTp = queryMTp,
                            templateMTp = currMTp
                        }
                    );
            }
            if (result.Count > 0)
                return result;
            return null;
        }

        internal List<MtiaTriplet> MTriplets { get; }

        internal List<Minutia> Minutiae { get; }

        #endregion
    }
}