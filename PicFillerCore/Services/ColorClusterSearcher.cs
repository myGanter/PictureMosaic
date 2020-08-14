using Core.Models;
using System;
using Core.Expansions;
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

            var accurateCl = Cache[col.R, col.G, col.B];
            if (accurateCl != null)
                return accurateCl;

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

        public int GetNonNullCount() 
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
            var bestDist = double.MaxValue;
            Cluster bestCl = null;

            var b = B - OffSet;
            if (b > -1)
            {
                Key = true;
                double minDist = double.MaxValue;
                Cluster cl = null;

                for (var rr = R - OffSet; rr <= R + OffSet; ++rr) 
                {
                    if (rr > -1 && rr < 256)
                    {
                        for (var gg = G - OffSet; gg <= G + OffSet; ++gg) 
                        {
                            if (gg > -1 && gg < 256 && Cache[rr, gg, b] != null) 
                            {
                                var ncl = Cache[rr, gg, b];
                                var nDist = ncl.CaclDistance(R, G, B);
                                if (nDist < minDist)
                                {
                                    cl = ncl;
                                    minDist = nDist;
                                }
                            }
                        }
                    }
                }

                if (cl != null && bestDist > minDist) 
                {
                    bestDist = minDist;
                    bestCl = cl;
                }
            }

            b = B + OffSet;
            if (b < 256)
            {
                Key = true;
                double minDist = double.MaxValue;
                Cluster cl = null;

                for (var rr = R - OffSet; rr <= R + OffSet; ++rr)
                {
                    if (rr > -1 && rr < 256)
                    {
                        for (var gg = G - OffSet; gg <= G + OffSet; ++gg)
                        {
                            if (gg > -1 && gg < 256 && Cache[rr, gg, b] != null)
                            {
                                var ncl = Cache[rr, gg, b];
                                var nDist = ncl.CaclDistance(R, G, B);
                                if (nDist < minDist)
                                {
                                    cl = ncl;
                                    minDist = nDist;
                                }
                            }
                        }
                    }
                }

                if (cl != null && bestDist > minDist)
                {
                    bestDist = minDist;
                    bestCl = cl;
                }
            }

            var r = R - OffSet;
            if (r > -1) 
            {
                Key = true;
                double minDist = double.MaxValue;
                Cluster cl = null;

                for (var bb = B - OffSet; bb <= B + OffSet; ++bb) 
                {
                    if (bb > -1 && bb < 256) 
                    {
                        for (var gg = G - OffSet; gg <= G + OffSet; ++gg) 
                        {
                            if (gg > -1 && gg < 256 && Cache[r, gg, bb] != null)
                            {
                                var ncl = Cache[r, gg, bb];
                                var nDist = ncl.CaclDistance(R, G, B);
                                if (nDist < minDist)
                                {
                                    cl = ncl;
                                    minDist = nDist;
                                }
                            }
                        }
                    }
                }

                if (cl != null && bestDist > minDist)
                {
                    bestDist = minDist;
                    bestCl = cl;
                }
            }

            r = R + OffSet;
            if (r < 256) 
            {
                Key = true;
                double minDist = double.MaxValue;
                Cluster cl = null;

                for (var bb = B - OffSet; bb <= B + OffSet; ++bb)
                {
                    if (bb > -1 && bb < 256)
                    {
                        for (var gg = G - OffSet; gg <= G + OffSet; ++gg)
                        {
                            if (gg > -1 && gg < 256 && Cache[r, gg, bb] != null)
                            {
                                var ncl = Cache[r, gg, bb];
                                var nDist = ncl.CaclDistance(R, G, B);
                                if (nDist < minDist)
                                {
                                    cl = ncl;
                                    minDist = nDist;
                                }
                            }
                        }
                    }
                }

                if (cl != null && bestDist > minDist)
                {
                    bestDist = minDist;
                    bestCl = cl;
                }
            }

            var g = G - OffSet;
            if (g > -1) 
            {
                Key = true;
                double minDist = double.MaxValue;
                Cluster cl = null;

                for (var rr = R - OffSet; rr <= R + OffSet; ++rr)
                {
                    if (rr > -1 && rr < 256)
                    {
                        for (var bb = B - OffSet; bb <= B + OffSet; ++bb)
                        {
                            if (bb > -1 && bb < 256 && Cache[rr, g, bb] != null)
                            {
                                var ncl = Cache[rr, g, bb];
                                var nDist = ncl.CaclDistance(R, G, B);
                                if (nDist < minDist)
                                {
                                    cl = ncl;
                                    minDist = nDist;
                                }
                            }
                        }
                    }
                }

                if (cl != null && bestDist > minDist)
                {
                    bestDist = minDist;
                    bestCl = cl;
                }
            }

            g = G + OffSet;
            if (g < 256) 
            {
                Key = true;
                double minDist = double.MaxValue;
                Cluster cl = null;

                for (var rr = R - OffSet; rr <= R + OffSet; ++rr)
                {
                    if (rr > -1 && rr < 256)
                    {
                        for (var bb = B - OffSet; bb <= B + OffSet; ++bb)
                        {
                            if (bb > -1 && bb < 256 && Cache[rr, g, bb] != null)
                            {
                                var ncl = Cache[rr, g, bb];
                                var nDist = ncl.CaclDistance(R, G, B);
                                if (nDist < minDist)
                                {
                                    cl = ncl;
                                    minDist = nDist;
                                }
                            }
                        }
                    }
                }

                if (cl != null && bestDist > minDist)
                {   
                    bestCl = cl;
                }
            }

            return bestCl;
        }
    }
}
