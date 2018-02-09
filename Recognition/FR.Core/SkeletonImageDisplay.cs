/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System.Drawing;

namespace PatternRecognition.FingerprintRecognition.Core
{
    
    
    
    public class SkeletonImageDisplay : FeatureDisplay<SkeletonImage>
    {
        
        
        
        
        
        
        
        
        
        public override void Show(SkeletonImage skImg, Graphics g)
        {
            Image img = skImg.ConvertToBitmap();
            g.DrawImage(img, 0, 0);
        }
    }
}
