using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PicFillerCore.Services
{
    class MosaicCollectorService : IDisposable
    {
        private readonly object Locker;

        private readonly Bitmap Pic;

        private readonly int ClW;

        private readonly int ClH;

        private readonly Graphics G;

        public MosaicCollectorService(int PicW, int PicH, int ClW, int ClH) 
        {
            Locker = new object();
            Pic = new Bitmap(PicW, PicH);
            G = Graphics.FromImage(Pic);
            this.ClW = ClW;
            this.ClH = ClH;
        }

        public Bitmap GetBmp() => Pic;

        public void DrawImg(Bitmap Bmp, int X, int Y) 
        {
            lock (Locker)
                G.DrawImage(Bmp, X * ClW, Y * ClH);
        }

        public void FillRect(Color Col, int X, int Y) 
        {
            lock (Locker)
                using (var b = new SolidBrush(Col))
                    G.FillRectangle(b, X * ClW, Y * ClH, ClW, ClH);
        }

        public void Dispose() 
        {
            G.Dispose();
            Pic.Dispose();
        }
    }
}
