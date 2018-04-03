/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Milton García Borroto (milton.garcia@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using Fingerprints.Model;

namespace Fingerprints.Medina2011
{
    [Serializable]
    public class MtripletsFeature
    {
        public List<MTriplet> MTriplets { get; set; }

        public List<Minutia> Minutiae { get; set; }

        //for serialization purpose
        public MtripletsFeature()
        {
        }

        public MtripletsFeature(List<MTriplet> mtList, List<Minutia> mtiaList)
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

                var currSim = queryMTp.Match(currMTp, out var currOrder);

                if (currSim > 0)
                    result.Add(new MtripletPair
                        {
                            QueryMTp = queryMTp,
                            TemplateMTp = currMTp,
                            MatchingValue = currSim,
                            TemplateMtiaOrder = currOrder
                        }
                    );
            }
            return result.Count > 0 ? result : null;
        }
    }
}