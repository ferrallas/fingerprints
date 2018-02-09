/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using PatternRecognition.FingerprintRecognition.Core.Ratha1995;

namespace PatternRecognition.FingerprintRecognition.Core.Jiang2000
{
    public class JyFeatureExtractor
    {
        public JyFeatures ExtractFeatures(Bitmap image)
        {
            var minutiae = MinutiaeExtractor.ExtractFeatures(image);
            var skeletonImg = SkeletonImageExtractor.ExtractFeatures(image);
            return ExtractFeatures(minutiae, skeletonImg);
        }


        public static JyFeatures ExtractFeatures(List<Minutia> minutiae, SkeletonImage skeletonImg)
        {
            var descriptorsList = new List<JyMtiaDescriptor>();

            if (minutiae.Count <= 3) return new JyFeatures(descriptorsList);

            var mtiaIdx = new Dictionary<Minutia, int>();
            for (var i = 0; i < minutiae.Count; i++)
                mtiaIdx.Add(minutiae[i], i);
            for (short idx = 0; idx < minutiae.Count; idx++)
            {
                var query = minutiae[idx];
                var nearest = GetNearest(minutiae, query);
                for (var i = 0; i < nearest.Length - 1; i++)
                for (var j = i + 1; j < nearest.Length; j++)
                {
                    var newMTriplet = new JyMtiaDescriptor(skeletonImg, minutiae, idx, nearest[i],
                        nearest[j]);
                    descriptorsList.Add(newMTriplet);
                }
            }
            descriptorsList.TrimExcess();
            return new JyFeatures(descriptorsList);
        }

        #region private

        private static short[] GetNearest(List<Minutia> minutiae, Minutia query)
        {
            var distances = new double[NeighborsCount];
            var nearestM = new short[NeighborsCount];
            for (var i = 0; i < distances.Length; i++)
                distances[i] = double.MaxValue;

            for (short i = 0; i < minutiae.Count; i++)
                if (minutiae[i] != query)
                {
                    var currentDistance = MtiaEuclideanDistance.Compare(query, minutiae[i]);
                    var maxIdx = 0;
                    for (var j = 1; j < NeighborsCount; j++)
                        if (distances[j] > distances[maxIdx])
                            maxIdx = j;
                    if (currentDistance < distances[maxIdx])
                    {
                        distances[maxIdx] = currentDistance;
                        nearestM[maxIdx] = i;
                    }
                }
            return nearestM;
        }

        private const byte NeighborsCount = 2;

        #endregion
    }
}