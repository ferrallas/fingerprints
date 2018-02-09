/*
 * Created by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 * Created: 1/5/2007
 * Comments by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 */

using System.Collections.Generic;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public interface IMatcher
    {
        double Match(object query, object template);
    }


    public interface IMinutiaMatcher : IMatcher
    {
        double Match(object query, object template, out List<MinutiaPair> matchingMtiae);
    }
}