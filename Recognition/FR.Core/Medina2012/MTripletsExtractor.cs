/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 *             Milton García Borroto (milton.garcia@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace PatternRecognition.FingerprintRecognition.Core.Medina2012
{
    public class MTripletsExtractor : FeatureExtractor<MtripletsFeature>
    {
        private readonly MtiaEuclideanDistance dist = new MtiaEuclideanDistance();

        public byte NeighborsCount { set; get; } = 4;


        public IFeatureExtractor<List<Minutia>> MtiaExtractor { set; get; }


        public override MtripletsFeature ExtractFeatures(Bitmap image)
        {
            try
            {
                var minutiae = MtiaExtractor.ExtractFeatures(image);
                return ExtractFeatures(minutiae);
            }
            catch (Exception)
            {
                if (MtiaExtractor == null)
                    throw new InvalidOperationException(
                        "Unable to extract MTriplets: Unassigned minutia list extractor!");
                throw;
            }
        }


        public MtripletsFeature ExtractFeatures(List<Minutia> minutiae)
        {
            var result = new List<MTriplet>();
            var triplets = new Dictionary<int, int>();

            var nearest = new short[minutiae.Count, NeighborsCount];
            var distance = new double[minutiae.Count, NeighborsCount];

            // Initializing distances
            for (var i = 0; i < minutiae.Count; i++)
            for (var j = 0; j < NeighborsCount; j++)
            {
                distance[i, j] = double.MaxValue;
                nearest[i, j] = -1;
            }

            // Computing m-triplets
            for (short i = 0; i < minutiae.Count; i++)
            {
                // Updating nearest minutiae
                UpdateNearest(minutiae, i, nearest, distance);

                // Building m-triplets
                for (var j = 0; j < NeighborsCount - 1; j++)
                for (var k = j + 1; k < NeighborsCount; k++)
                    if (nearest[i, j] != -1 && nearest[i, k] != -1)
                    {
                        if (i == nearest[i, j] || i == nearest[i, k] || nearest[i, j] == nearest[i, k])
                            throw new Exception("Wrong mtp");

                        var newMTriplet = new MTriplet(new[] {i, nearest[i, j], nearest[i, k]}, minutiae);
                        var newHash = newMTriplet.GetHashCode();
                        if (!triplets.ContainsKey(newHash))
                        {
                            triplets.Add(newHash, 0);
                            result.Add(newMTriplet);
                        }
                    }
            }
            result.TrimExcess();
            return new MtripletsFeature(result, minutiae);
        }

        private void UpdateNearest(List<Minutia> minutiae, int idx, short[,] nearest, double[,] distance)
        {
            for (var i = idx + 1; i < minutiae.Count; i++)
            {
                var dValue = dist.Compare(minutiae[idx], minutiae[i]);

                var maxIdx = 0;
                for (var j = 1; j < NeighborsCount; j++)
                    if (distance[idx, j] > distance[idx, maxIdx])
                        maxIdx = j;
                if (dValue < distance[idx, maxIdx])
                {
                    distance[idx, maxIdx] = dValue;
                    nearest[idx, maxIdx] = Convert.ToInt16(i);
                }

                maxIdx = 0;
                for (var j = 1; j < NeighborsCount; j++)
                    if (distance[i, j] > distance[i, maxIdx])
                        maxIdx = j;
                if (dValue < distance[i, maxIdx])
                {
                    distance[i, maxIdx] = dValue;
                    nearest[i, maxIdx] = Convert.ToInt16(idx);
                }
            }
        }
    }
}