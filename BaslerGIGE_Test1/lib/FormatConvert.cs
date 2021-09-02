using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using HalconDotNet;
using System.Drawing.Imaging;

namespace BaslerGIGE_Test1.lib
{
    class FormatConvert
    {
        public static void BitmapToHObject(Bitmap bmp, out HObject image)
        {
            try
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData srcBmData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                System.IntPtr srcPtr = srcBmData.Scan0;
                HOperatorSet.GenImageInterleaved(out image, srcPtr, "bgrx", bmp.Width, bmp.Height, -1, "byte", 0, 0, 0, 0, -1, 0);
                bmp.UnlockBits(srcBmData);
            }
            catch (Exception ex)
            {
                image = null;

            }

        }

    }
}
