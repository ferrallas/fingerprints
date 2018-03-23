/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;

namespace Fingerprints.Medina2012
{
    [Serializable]
    public class MtripletsFeature
    {
        public List<MTriplet> MTriplets { get; set; }

        public List<Minutia> Minutiae { get; set; }

        public MtripletsFeature() { }

        internal MtripletsFeature(List<MTriplet> mtList, List<Minutia> mtiaList)
        {
            mtiaList.TrimExcess();
            Minutiae = mtiaList;

            mtList.TrimExcess();
            MTriplets = mtList;
            MTriplets.Sort(new MtComparer());

            var hashTable = new Dictionary<int, List<MTriplet>>();
            foreach (var mtp in MTriplets)
            {
                var alphaCodes = GetAlphaCodes(mtp);
                var hash = MergeHashes(alphaCodes[0], alphaCodes[1], alphaCodes[2]);
                if (!hashTable.ContainsKey(hash))
                    hashTable.Add(hash, new List<MTriplet>());
                hashTable[hash].Add(mtp);
            }
        }

        internal List<MtripletPair> FindNoRotateAllSimilar(MTriplet queryMTp)
        {
            // Indexing by MaxDistance
            const double dThr = MTriplet.DistanceThreshold;
            var d = queryMTp.MaxDistance - dThr;
            var iniIdx = BinarySearch(MTriplets, d);
            d = queryMTp.MaxDistance + dThr;

            var result = new List<MtripletPair>();
            for (var j = iniIdx; j < MTriplets.Count && MTriplets[j].MaxDistance <= d; j++)
            {
                var currMTp = MTriplets[j];
                var currSim = queryMTp.NoRotateMatch(currMTp, out var currOrder);
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

        #region private

        private static int BinarySearch(IList<MTriplet> mtps, double value)
        {
            var low = 0;
            var high = mtps.Count - 1;
            while (low < high)
            {
                var mid = (low + high) / 2;
                if (mtps[mid].MaxDistance > value)
                    high = mid - 1;
                else if (mtps[mid].MaxDistance < value)
                    low = mid + 1;
                else
                    return mid; // found
            }
            return low; // not found
        }

        private class MtComparer : Comparer<MTriplet>
        {
            public override int Compare(MTriplet x, MTriplet y)
            {
                return Math.Sign(x.MaxDistance - y.MaxDistance);
            }
        }

        #endregion

        #region Generating hashes

        private static int DiscretizeAngle(double d)
        {
            var code = Convert.ToInt32(d * 180 / Math.PI / 45);
            return code != 8 ? code : 0;
        }

        private static int[] GetAlphaCodes(MTriplet mtp)
        {
            var alpha = new int[3];
            for (var i = 0; i < 3; i++)
            {
                int j;
                if (i == 2)
                    j = 0;
                else
                    j = i + 1;

                var qMtiai = mtp[i];
                var qMtiaj = mtp[j];
                double x = qMtiai.X - qMtiaj.X;
                double y = qMtiai.Y - qMtiaj.Y;
                var angleij = Angle.ComputeAngle(x, y);
                var qAlpha = Angle.Difference2Pi(qMtiai.Angle, angleij);

                alpha[i] = DiscretizeAngle(qAlpha);
            }
            return alpha;
        }

        private static int MergeHashes(int a0, int a1, int a2)
        {
            var block2 = a2;
            // Storing d1 in the next 4 bits.
            var block1 = a1 << 4;
            // Storing d0 in the next 4 bits.
            var block0 = a0 << 8;
            var hash = block0 | block1 | block2;
            return hash;
        }

        #endregion
    }
}