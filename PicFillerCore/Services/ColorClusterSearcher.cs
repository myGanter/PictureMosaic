using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PicFillerCore.Services
{
    class ColorClusterSearcher
    {
        public Func<Cluster, Cluster, bool> OldNewValChecker { get; set; }

        private readonly Cluster[,,] Cache;

        public ColorClusterSearcher() 
        {
            Cache = new Cluster[256, 256, 256];
            OldNewValChecker = (o, n) => true;
        }

        public void AddCluster(Cluster Obj) 
        {
            var col = Obj.GetAvColor();

            if (Cache[col.R, col.G, col.B] == null || OldNewValChecker(Cache[col.R, col.G, col.B], Obj))
                Cache[col.R, col.G, col.B] = Obj;
        }

        public Cluster GetNearCluster(Cluster Obj) 
        {
            var col = Obj.GetAvColor();
            var offSet = 1;
            var key = true;

            while (key) 
            {
                key = false;

                var cluster = AroundContourPlaneRG(offSet, col.R, col.G, col.B, ref key);
                if (cluster != null)
                    return cluster;

                offSet++;
            }

            return null;
        }

        private int GetNonNullCount() 
        {
            var i = 0;
            for (var r = 0; r < 256; ++r)
                for (var g = 0; g < 256; ++g)
                    for (var b = 0; b < 256; ++b)
                        if (Cache[r, g, b] != null) 
                            i++;

            return i;
        }

        public Cluster AroundContourPlaneRG(int OffSet, int R, int G, int B, ref bool Key) 
        {
            var b = B - OffSet;
            if (b > -1)
            {
                Key = true;

                for (var rr = R - OffSet; rr <= R + OffSet; ++rr) 
                {
                    if (rr > -1 && rr < 256)
                    {
                        for (var gg = G - OffSet; gg <= G + OffSet; ++gg) 
                        {
                            if (gg > -1 && gg < 256 && Cache[rr, gg, b] != null) 
                            {
                                return Cache[rr, gg, b];
                            }
                        }
                    }
                }            
            }

            b = B + OffSet;
            if (b < 256)
            {
                Key = true;

                for (var rr = R - OffSet; rr <= R + OffSet; ++rr)
                {
                    if (rr > -1 && rr < 256)
                    {
                        for (var gg = G - OffSet; gg <= G + OffSet; ++gg)
                        {
                            if (gg > -1 && gg < 256 && Cache[rr, gg, b] != null)
                            {
                                return Cache[rr, gg, b];
                            }
                        }
                    }
                }
            }

            var r = R - OffSet;
            if (r > -1) 
            {
                Key = true;

                for (var bb = B - OffSet; bb <= B + OffSet; ++bb) 
                {
                    if (bb > -1 && bb < 256) 
                    {
                        for (var gg = G - OffSet; gg <= G + OffSet; ++gg) 
                        {
                            if (gg > -1 && gg < 256 && Cache[r, gg, bb] != null)
                            {
                                return Cache[r, gg, bb];
                            }
                        }
                    }
                }
            }

            r = R + OffSet;
            if (r < 256) 
            {
                Key = true;

                for (var bb = B - OffSet; bb <= B + OffSet; ++bb)
                {
                    if (bb > -1 && bb < 256)
                    {
                        for (var gg = G - OffSet; gg <= G + OffSet; ++gg)
                        {
                            if (gg > -1 && gg < 256 && Cache[r, gg, bb] != null)
                            {
                                return Cache[r, gg, bb];
                            }
                        }
                    }
                }
            }

            var g = G - OffSet;
            if (g > -1) 
            {
                Key = true;

                for (var rr = R - OffSet; rr <= R + OffSet; ++rr)
                {
                    if (rr > -1 && rr < 256)
                    {
                        for (var bb = B - OffSet; bb <= B + OffSet; ++bb)
                        {
                            if (bb > -1 && bb < 256 && Cache[rr, g, bb] != null)
                            {
                                return Cache[rr, g, bb];
                            }
                        }
                    }
                }
            }

            g = G + OffSet;
            if (g < 256) 
            {
                Key = true;

                for (var rr = R - OffSet; rr <= R + OffSet; ++rr)
                {
                    if (rr > -1 && rr < 256)
                    {
                        for (var bb = B - OffSet; bb <= B + OffSet; ++bb)
                        {
                            if (bb > -1 && bb < 256 && Cache[rr, g, bb] != null)
                            {
                                return Cache[rr, g, bb];
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
