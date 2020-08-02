using Core.Expansions;
using Core.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using Core.Attributes;

namespace Core.Services
{
    [Name("AverageHSV")]
    public class AverageHSVColorService : BaseClusterService
    {
        public readonly int N;

        public readonly short ClusterCountH;

        public readonly short ClusterCountS;

        public readonly short ClusterCountV;

        public AverageHSVColorService(int N, short ClusterCountH, short ClusterCountS, short ClusterCountV) 
        {
            if (N < 1)
                throw new Exception("N < 1");

            if (ClusterCountH < 1 || ClusterCountH > 360)
                throw new Exception("H < 1 or H > 360");

            if (ClusterCountS < 1 || ClusterCountS > 100)
                throw new Exception("S < 1 or S > 100");

            if (ClusterCountV < 1 || ClusterCountV > 100)
                throw new Exception("V < 1 or V > 100");

            this.N = N;
            this.ClusterCountH = ClusterCountH;
            this.ClusterCountS = ClusterCountS;
            this.ClusterCountV = ClusterCountV;
        }

        public static BaseClusterService CreateInstance()
        {
            var conf = AppConfigService.GetConfig<AverageHSVConf>();
            return new AverageHSVColorService(conf.ClusterCount, conf.ClusterLenH, conf.ClusterLenS, conf.ClusterLenV);
        }

        public static IClusterSerializer CreateSerializer() 
        {
            return new AverageHSVColorSerializer();
        }

        public override Cluster CreateCluster(Bitmap Bmp)
        {
            return CreateCluster(Bmp, 0, 0, Bmp.Width, Bmp.Height);
        }

        public unsafe override Cluster CreateCluster(Bitmap Bmp, int OffSetX, int OffSetY, int W, int H)
        {
            var cols = new ColorHSV[N * N];
            int width = W, height = H;
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
                            curpos = ((byte*)bd.Scan0) + y * bd.Stride + xn * w * 3 + OffSetY * bd.Stride + OffSetX * 3;

                            var maxX = (xn + 1) * w;
                            for (var x = xn * w; x < maxX; ++x)
                            {
                                b += *(curpos++);
                                g += *(curpos++);
                                r += *(curpos++);
                            }
                        }

                        var col = Color.FromArgb(Math.Abs(r / pxCount), Math.Abs(g / pxCount), Math.Abs(b / pxCount))
                            .ToHSVColor()
                            .FindCluster(ClusterCountH, ClusterCountS, ClusterCountV);
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
