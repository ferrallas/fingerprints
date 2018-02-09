/*
 * Created by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 * Created: 
 * Comments by: Miguel Angel Medina Pérez (miguel.medina.perez@gmail.com)
 */

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PatternRecognition.FingerprintRecognition.Core
{
    public class FingerprintImageProvider
    {
        public Bitmap GetResource(string fingerprint, ResourceRepository repository)
        {
            byte[] rawImage = null;
            foreach (var ext in new[] {"tif", "bmp", "jpg"})
            {
                var resourceName = $"{fingerprint}.{ext}";
                rawImage = repository.RetrieveResource(resourceName);
                if (rawImage != null)
                    break;
            }
            if (rawImage == null)
                return null;
            var srcBitmap = Image.FromStream(new MemoryStream(rawImage)) as Bitmap;
            if (srcBitmap == null)
                return null;
            Bitmap returnBitmap;
            using (srcBitmap)
            {
                PixelFormat pixelFormat;
                switch (srcBitmap.PixelFormat)
                {
                    case PixelFormat.Format8bppIndexed:
                    case PixelFormat.Indexed:
                    case PixelFormat.Format4bppIndexed:
                    case PixelFormat.Format1bppIndexed:
                        pixelFormat = PixelFormat.Format24bppRgb;
                        break;
                    default:
                        pixelFormat = srcBitmap.PixelFormat;
                        break;
                }
                returnBitmap = new Bitmap(srcBitmap.Width, srcBitmap.Height, pixelFormat);
                returnBitmap.SetResolution(srcBitmap.HorizontalResolution, srcBitmap.VerticalResolution);
                var g = Graphics.FromImage(returnBitmap);
                g.DrawImage(srcBitmap, 0, 0);
            }
            return returnBitmap;
        }
    }
}