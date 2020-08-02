using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PicFillerCore.Expansions
{
    static class PicExpansions
    {
        public static Bitmap CutBmpToCenter(this Bitmap Bmp, int NewW, int NewH) 
        {
            var w = Bmp.Width;
            var h = Bmp.Height;

            if (NewW > w || NewW < 1 || NewH > h || NewH < 1)
                return (Bitmap)Bmp.Clone();

            var sW = (w - NewW) / 2;
            var sH = (h - NewH) / 2;

            return Bmp.Clone(new Rectangle(sW, sH, NewW, NewH), Bmp.PixelFormat);
        }
    }
}
