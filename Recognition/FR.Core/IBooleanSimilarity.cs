/*
 * Created by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 * Created: Thursday, October 25, 2007
 * Comments by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 */

namespace PatternRecognition.FingerprintRecognition.Core
{
    public interface IBooleanSimilarity<T>
    {
        bool Compare(T source, T compareTo);
    }
}