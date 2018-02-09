/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace PatternRecognition.FingerprintRecognition.Core.Jiang2000
{
    public class JYFeatureExtractor : FeatureExtractor<JYFeatures>
    {
        public IFeatureExtractor<List<Minutia>> MtiaExtractor { set; get; }


        public IFeatureExtractor<SkeletonImage> SkeletonImgExtractor { set; get; }


        public override JYFeatures ExtractFeatures(Bitmap image)
        {
            try
            {
                var minutiae = MtiaExtractor.ExtractFeatures(image);
                var skeletonImg = SkeletonImgExtractor.ExtractFeatures(image);
                return ExtractFeatures(minutiae, skeletonImg);
            }
            catch (Exception e)
            {
                if (MtiaExtractor == null)
                    throw new InvalidOperationException(
                        "Unable to extract JYFeatures: Unassigned minutia list extractor!", e);
                if (SkeletonImgExtractor == null)
                    throw new InvalidOperationException(
                        "Unable to extract JYFeatures: Unassigned skeleton image extractor!", e);
                throw;
            }
        }


        public static JYFeatures ExtractFeatures(List<Minutia> minutiae, SkeletonImage skeletonImg)
        {
            var descriptorsList = new List<JYMtiaDescriptor>();

            if (minutiae.Count <= 3) return new JYFeatures(descriptorsList);

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
                    var newMTriplet = new JYMtiaDescriptor(skeletonImg, minutiae, idx, nearest[i],
                        nearest[j]);
                    descriptorsList.Add(newMTriplet);
                }
            }
            descriptorsList.TrimExcess();
            return new JYFeatures(descriptorsList);
        }

        #region private

        private static short[] GetNearest(List<Minutia> minutiae, Minutia query)
        {
            var distances = new double[neighborsCount];
            var nearestM = new short[neighborsCount];
            for (var i = 0; i < distances.Length; i++)
                distances[i] = double.MaxValue;
            var dist = new MtiaEuclideanDistance();
            for (short i = 0; i < minutiae.Count; i++)
                if (minutiae[i] != query)
                {
                    var CurrentDistance = dist.Compare(query, minutiae[i]);
                    var MaxIdx = 0;
                    for (var j = 1; j < neighborsCount; j++)
                        if (distances[j] > distances[MaxIdx])
                            MaxIdx = j;
                    if (CurrentDistance < distances[MaxIdx])
                    {
                        distances[MaxIdx] = CurrentDistance;
                        nearestM[MaxIdx] = i;
                    }
                }
            return nearestM;
        }

        private const byte neighborsCount = 2;

        #endregion
    }
}