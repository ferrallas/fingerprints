/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System;
using System.Runtime.CompilerServices;
using Fingerprints.Model;

namespace Fingerprints.Computation
{
    public static class MtiaEuclideanDistance
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Compare(Minutia m0, Minutia m1)
        {
            double diff0 = m0.Y - m1.Y;
            double diff1 = m0.X - m1.X;
            return Math.Sqrt(diff0 * diff0 + diff1 * diff1);
        }
    }
}