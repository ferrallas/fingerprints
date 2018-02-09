/*
 * Created by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 * Created: 1/5/2007
 * Comments by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 */

namespace PatternRecognition.FingerprintRecognition.Core
{
    public interface ISimilarity<T>
    {
        double Compare(T source, T compareTo);
    }
}