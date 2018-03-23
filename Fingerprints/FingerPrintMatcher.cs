using System;
using System.Collections.Generic;
using System.Drawing;
using Fingerprints.Medina2012;

namespace Fingerprints
{
    public static class FingerPrintMatcher
    {
        public static void Store(Algorithm algorithm, IStoreProvider storage, Bitmap bitmap, string subjectId)
        {
            switch (algorithm)
            {
                case Algorithm.Medina2012:
                    Medina2012Matcher.Store(storage,bitmap,subjectId);
                    break;
                case Algorithm.Medina2011:
                    Medina2011.Medina2011Matcher.Store(storage, bitmap, subjectId);
                    break;
                case Algorithm.Jiang2000:
                    Jiang2000.Jiang2000Matcher.Store(storage,bitmap,subjectId);
                    break;
                case Algorithm.Parziale2004:
                    Parziale2004.Parziale2004Matcher.Store(storage,bitmap,subjectId);
                    break;
                case Algorithm.Qi2005:
                    Qi2005.Qi2005Matcher.Store(storage,bitmap,subjectId);
                    break;
                case Algorithm.Tico2003:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }
        }

        public static IEnumerable<Match> Match(Algorithm algorithm, IStoreProvider storage, Bitmap bitmap)
        {
            switch (algorithm)
            {
                case Algorithm.Medina2012:
                    return Medina2012Matcher.Match(storage, bitmap);
                case Algorithm.Medina2011:
                    return Medina2011.Medina2011Matcher.Match(storage, bitmap);
                case Algorithm.Jiang2000:
                    return Jiang2000.Jiang2000Matcher.Match(storage, bitmap);
                case Algorithm.Parziale2004:
                    return Parziale2004.Parziale2004Matcher.Match(storage, bitmap);
                case Algorithm.Qi2005:
                    return Qi2005.Qi2005Matcher.Match(storage, bitmap);
                case Algorithm.Tico2003:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }
            throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
        }
    }
}
