/*
 * Created by: Milton Garcia Borroto
 * Created: 1/5/2007
 * Comments by: Miguel Angel Medina Pérez (migue.cu@gmail.com)
 */

namespace PatternRecognition.FingerprintRecognition.Core
{
    public interface IDissimilarity<T>
    {
        double Compare(T source, T compareTo);
    }
}