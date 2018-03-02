/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

namespace PatternRecognition.FingerprintRecognition.Core
{
    public class Candidate<T>
    {
        public string EntryId { get; set; }

        public T Feauture { get; set; }
    }
}