using Core.Attributes;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using Core.Expansions;
using System.Text;
using System.Drawing.Imaging;

namespace Core.Services
{
    [Name("HashingMinified")]
    public class HashingMinifiedClusterService : BaseClusterService
    {
        private readonly int CacheSize;

        public readonly short ClusterCountH;

        public readonly short ClusterCountS;

        public readonly short ClusterCountV;

        public HashingMinifiedClusterService(int CacheSize, short ClusterCountH, short ClusterCountS, short ClusterCountV) 
        {
            this.CacheSize = CacheSize;
            this.ClusterCountH = ClusterCountH;
            this.ClusterCountS = ClusterCountS;
            this.ClusterCountV = ClusterCountV;
        }

        public static BaseClusterService CreateInstance()
        {
            var conf = AppConfigService.GetConfig<HashingMinifiedConf>();
            if (conf.CacheSize < 1)
                throw new Exception("-CacheSize invalid");

            if (conf.ClusterLenH < 1 || conf.ClusterLenH > 360)
                throw new Exception("H < 1 or H > 360");

            if (conf.ClusterLenS < 1 || conf.ClusterLenS > 100)
                throw new Exception("S < 1 or S > 100");

            if (conf.ClusterLenV < 1 || conf.ClusterLenV > 100)
                throw new Exception("V < 1 or V > 100");

            return new HashingMinifiedClusterService(conf.CacheSize, conf.ClusterLenH, conf.ClusterLenS, conf.ClusterLenV);
        }

        public static IClusterSerializer CreateSerializer()
        {
            return new HashingMinifiedSerializer();
        }

        public unsafe override Cluster CreateCluster(Bitmap Bmp)
        {
            using var microPic = new Bitmap(Bmp, CacheSize, CacheSize);
            using var grayMicPic = microPic.ToGray();            
            var avCol = grayMicPic.GetAverageColor();
            int avi = avCol.R + avCol.G + avCol.B;
            BitmapData bd = grayMicPic.LockBits(new Rectangle(0, 0, grayMicPic.Width, grayMicPic.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var sb = new StringBuilder(CacheSize * CacheSize);
            var format = "X" + (CacheSize / 4 + (CacheSize % 4 > 0 ? 1 : 0)).ToString();

            try
            {
                byte* curpos;

                for (var y = 0; y < grayMicPic.Height; ++y)
                {
                    curpos = ((byte*)bd.Scan0) + y * bd.Stride;
                    int binaryHash = 0b0;

                    for (var x = 0; x < grayMicPic.Width; ++x)
                    {
                        int a = 0;

                        a += *(curpos++);
                        a += *(curpos++);
                        a += *(curpos++);

                        binaryHash <<= 0b1;

                        if (a > avi) 
                        {
                            binaryHash += 0b1;
                        }
                    }

                    sb.Append(binaryHash.ToString(format));
                }
            }
            finally 
            {
                grayMicPic.UnlockBits(bd);
            }

            var hash = sb.ToString();
            var bmpAvCol = Bmp.GetAverageColor();

            return new HashingMinifiedCluster(hash, bmpAvCol.ToHSVColor().FindCluster(ClusterCountH, ClusterCountS, ClusterCountV));
        }

        public override Cluster CreateCluster(Bitmap Bmp, int OffSetX, int OffSetY, int W, int H)
        {
            using var cutPic = Bmp.Clone(new Rectangle(OffSetX, OffSetY, W, H), PixelFormat.Format24bppRgb);
            return CreateCluster(cutPic);
        }
    }
}
