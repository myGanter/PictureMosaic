using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Core.Models;
using System.Drawing.Imaging;

namespace Core.Expansions
{
    public static class BmpExpansions
    {
        public unsafe static HSVCluster CreateCluster(this Bitmap Bmp, int N) 
        {
            if (N < 1)
                throw new Exception("N < 1");

            var cols = new ColorHSV[N];
            int width = Bmp.Width, height = Bmp.Height;
            int w = width / N, h = height / N;
            var pxCount = w * h;
            BitmapData bd = Bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            try
            {
                byte* curpos;
                for (var yn = 0; yn < N; ++yn)
                {
                    for (var xn = 0; xn < N; ++xn)
                    {
                        int r = 0, g = 0, b = 0;

                        var maxY = (yn + 1) * h;
                        for (var y = yn * h; y < maxY; ++y)
                        {
                            curpos = ((byte*)bd.Scan0) + y * bd.Stride + xn * w;

                            var maxX = (xn + 1) * w;
                            for (var x = xn * w; x < maxX; ++x)
                            {
                                b += *(curpos++);
                                g += *(curpos++);
                                r += *(curpos++);
                            }
                        }

                        var col = Color.FromArgb(r / pxCount, g / pxCount, b / pxCount).ToHSVColor();
                        cols[yn * N + xn] = col;
                    }
                }
            }
            finally 
            {
                Bmp.UnlockBits(bd);
            }

            return new HSVCluster(cols);
        }
    }
}
