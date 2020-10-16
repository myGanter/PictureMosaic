using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace Core.Expansions
{
    public static class BitmapExpansions
    {
        public static Color GetAverageColor(this Bitmap Bmp)
        {
            return GetAverageColor(Bmp, 0, 0, Bmp.Width, Bmp.Height);
        }

        public static unsafe Color GetAverageColor(this Bitmap Bmp, int OffSetX, int OffSetY, int W, int H) 
        {
            BitmapData bd = Bmp.LockBits(new Rectangle(0, 0, Bmp.Width, Bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            try
            {
                byte* curpos;
                ulong r = 0, g = 0, b = 0;

                for (var y = 0; y < H; ++y)
                {
                    curpos = ((byte*)bd.Scan0) + y * bd.Stride + OffSetY * bd.Stride + OffSetX * 3;

                    for (var x = 0; x < W; ++x)
                    {
                        b += *(curpos++);
                        g += *(curpos++);
                        r += *(curpos++);
                    }
                }

                var pxCount = (uint)(W * H);

                return Color.FromArgb((int)(r / pxCount), (int)(g / pxCount), (int)(b / pxCount));
            }
            finally
            {
                Bmp.UnlockBits(bd);
            }
        }

        public static unsafe Bitmap ToGray(this Bitmap Bmp)
        {
            var res = new Bitmap(Bmp.Width, Bmp.Height, PixelFormat.Format24bppRgb);

            BitmapData bd = Bmp.LockBits(new Rectangle(0, 0, Bmp.Width, Bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData resd = res.LockBits(new Rectangle(0, 0, Bmp.Width, Bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            try
            {
                byte* curpos;
                byte* rescurpos;

                for (var y = 0; y < Bmp.Height; ++y)
                {
                    curpos = ((byte*)bd.Scan0) + y * bd.Stride;
                    rescurpos = ((byte*)resd.Scan0) + y * resd.Stride;

                    for (var x = 0; x < Bmp.Width; ++x)
                    {
                        int r = 0, g = 0, b = 0;

                        b += *(curpos++);
                        g += *(curpos++);
                        r += *(curpos++);

                        var av = (byte)((r + g + b) / 3);

                        *(rescurpos++) = av;
                        *(rescurpos++) = av;
                        *(rescurpos++) = av;
                    }
                }
            }
            finally
            {
                Bmp.UnlockBits(bd);
                res.UnlockBits(resd);
            }

            return res;
        }

        public static Bitmap ScaleCutBmpCenter(this Bitmap Bmp, int NewW, int NewH)
        {
            var w = Bmp.Width;
            var h = Bmp.Height;

            if (NewW < 1 || NewH < 1)
                return (Bitmap)Bmp.Clone();

            //скейлим картинку под нужный размер
            var picWCoof = w / (double)h;
            var picHCoof = h / (double)w;

            var newWCoof = NewW / (double)NewH;
            var newHCoof = NewH / (double)NewW;

            int newPicW = 0;
            int newPicH = 0;

            if (picWCoof <= newWCoof)
            {
                newPicW = NewW;
                newPicH = (int)Math.Round(NewW * picHCoof);
            }
            else if (picHCoof < newHCoof)
            {
                newPicW = (int)Math.Round(NewH * picWCoof);
                newPicH = NewH;        
            }

            w = newPicW;
            h = newPicH;

            using var resizeBmp = new Bitmap(Bmp, newPicW, newPicH);
            var sW = (w - NewW) / 2;
            var sH = (h - NewH) / 2;

            var res = resizeBmp.Clone(new Rectangle(sW, sH, NewW, NewH), resizeBmp.PixelFormat);

            return res;
        }

        public static Bitmap SetImgOpacity(this Bitmap ImgPic, float ImgOpac)
        {
            var bmpPic = new Bitmap(ImgPic.Width, ImgPic.Height);
            var gfxPic = Graphics.FromImage(bmpPic);
            var cmxPic = new ColorMatrix
            {
                Matrix33 = ImgOpac
            };
            var iaPic = new ImageAttributes();

            iaPic.SetColorMatrix(cmxPic, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            gfxPic.DrawImage(ImgPic, new Rectangle(0, 0, bmpPic.Width, bmpPic.Height), 0, 0, ImgPic.Width, ImgPic.Height, GraphicsUnit.Pixel, iaPic);
            gfxPic.Dispose();

            return bmpPic;
        }

        public static Bitmap ToFormat24bppRgb(this Bitmap Bmp) 
        {
            var nBmp = new Bitmap(Bmp.Width, Bmp.Height, PixelFormat.Format24bppRgb);
            using var g = Graphics.FromImage(nBmp);
            g.DrawImage(Bmp, new Rectangle(0, 0, nBmp.Width, nBmp.Height));

            return nBmp;
        }
    }
}
