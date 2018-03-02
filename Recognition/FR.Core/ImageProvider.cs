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
    public static class ImageProvider
    {
        public static Bitmap AdaptImage(byte[] rawImage)
        {
            return AdaptImage(Image.FromStream(new MemoryStream(rawImage)) as Bitmap);
        }

        public static Bitmap AdaptImage(Bitmap srcBitmap)
        {
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