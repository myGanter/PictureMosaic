using Core.Expansions;
using Core.Models;
using Core.Parallel;
using Core.Services;
using PicFillerCore.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PicFillerCore.Services
{
    class SingleImgProcessorWorker : IAppWorker<ClusterPos>
    {
        protected Bitmap Pic { get; set; }

        protected MosaicCollectorService MosaicService { get; set; }

        protected List<Dictionary<Cluster, List<string>>> Caches { get; set; }

        protected List<BaseClusterService> ClusterBuilders { get; set; }

        protected ColorClusterSearcher NearClusterSearcher { get; set; }

        protected Conf Config { get; set; }

        private void IsValidAndInit()
        {
            Config = AppConfigService.GetConfig<Conf>();
            if (!File.Exists(Config.Pic))
            {
                throw new Exception("-Pic invalid");
            }

            Pic = new Bitmap(Config.Pic);

            if (Config.JsonPath.Any(x => !File.Exists(x)))
            {
                throw new Exception("-JP invalid");
            }

            if (Config.ThreadCount < 1 || Config.ThreadCount > 16)
            {
                throw new Exception("-ThCout invalid");
            }

            var clw = Config.W;
            var clh = Config.H;
            if (clw < 1 || clh < 1 || clw > Pic.Width || clh > Pic.Height)
            {
                throw new Exception("-ClusterW or -ClusterH invalid");
            }

            if (ClusterBuilders == null)
            {
                ClusterBuilders = new List<BaseClusterService>();
                Caches = new List<Dictionary<Cluster, List<string>>>();

                foreach (var jsonP in Config.JsonPath)
                {
                    var jStr = File.ReadAllText(jsonP);
                    var algName = ClusterServiceBuilder.DetermineAlgName(jStr);
                    var ser = ClusterServiceBuilder.GetSerializer(algName);

                    var obj = ser.Deserialize(jStr);
                    ClusterBuilders.Add(obj.Item1);
                    Caches.Add(obj.Item2);
                }

                if (Config.UseNearCluster)
                {
                    var maxCount = Caches.Max(x => x.Count);
                    var cache = Caches.First(x => x.Count == maxCount);

                    NearClusterSearcher = new ColorClusterSearcher
                    {
                        OldNewValChecker = (oldV, newV) => cache[newV].Count > cache[oldV].Count
                    };

                    foreach (var cl in cache)
                        NearClusterSearcher.AddCluster(cl.Key);
                }
            }
        }

        public void Frame(ClusterPos Value)
        {
            var conf = Value.Conf;

            if (conf.DefaultRender != DefaultRenderEnum.RenderOriginPicture && (Value.Pic == null || conf.DefaultRender == DefaultRenderEnum.PreDefaultColorRender))
            {
                MosaicService.FillRect(Value.Col, Value.OffSetW, Value.OffSetH);
            }

            if (Value.Pic != null)
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {Value.Pic}");

                using var fPic = new Bitmap(Value.Pic);
                using var rPic = conf.UseScaleCut ? fPic.ScaleCutBmpCenter(conf.WR, conf.HR) : new Bitmap(fPic, conf.WR, conf.HR);
                if (conf.SegmentOpacity == null || conf.SegmentOpacity.Value < 0)
                {
                    MosaicService.DrawImg(rPic, Value.OffSetW, Value.OffSetH);
                }
                else
                {
                    using var roppic = rPic.SetImgOpacity(conf.SegmentOpacity.Value);
                    MosaicService.DrawImg(roppic, Value.OffSetW, Value.OffSetH);
                }
            }
        }

        public virtual void Run()
        {
            try
            {
                IsValidAndInit();                

                var fTC = new FrameTaskController<ClusterPos>(Config.ThreadCount, TaskCreator, Frame);
                fTC.Run();

                SavePic();

                PrintOk();
            }
            finally
            {
                if (MosaicService != null)
                    MosaicService.Dispose();
                if (Pic != null)
                    Pic.Dispose();
            }            
        }

        public virtual async Task RunAsync()
        {
            try
            {
                IsValidAndInit();

                var fTC = new FrameTaskController<ClusterPos>(Config.ThreadCount, TaskCreator, Frame);
                await fTC.RunAsync();

                SavePic();

                PrintOk();
            }
            finally
            {
                if (MosaicService != null)
                    MosaicService.Dispose();
                if (Pic != null)
                    Pic.Dispose();
            }
        }

        private void PrintOk()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{Config.Pic} processing completed");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void InitMosaicService()
        {
            int picW = Pic.Width, picH = Pic.Height;
            int w = Config.W, h = Config.H;
            int clWC = picW / w, clHC = picH / h;
            int rPW = clWC * Config.WR, rPH = clHC * Config.HR;

            if (Config.HR < 1 || Config.WR < 1 || rPW > 22000 || rPH > 22000)
            {
                var wr = w * 15;
                var hr = h * 15;

                rPW = clWC * wr;
                rPH = clHC * hr;

                if (rPW > 22000 || rPH > 22000)
                {
                    if (rPW > rPH)
                    {
                        var cof = (double)h / w;
                        wr = 22000 / clWC;
                        hr = (int)Math.Round(wr * cof);
                    }
                    else
                    {
                        var cof = (double)w / h;
                        hr = 22000 / clHC;
                        wr = (int)Math.Round(hr * cof);
                    }

                    rPW = clWC * wr;
                    rPH = clHC * hr;
                }

                Config.WR = wr;
                Config.HR = hr;
            }

            Console.WriteLine($"Mosaic segment W:{Config.WR} H:{Config.HR}");
            MosaicService = new MosaicCollectorService(rPW, rPH, Config.WR, Config.HR);
        }

        public void TaskCreator(Action<ClusterPos> Adder)
        {
            var rnd = new Random();

            var useNearCluster = Config.UseNearCluster;

            int picW = Pic.Width, picH = Pic.Height;
            int w = Config.W, h = Config.H;
            int clWC = picW / w, clHC = picH / h;

            using (var nPic = Pic.ScaleCutBmpCenter(clWC * w, clHC * h))
            {
                Pic.Dispose();
                Pic = nPic.ToFormat24bppRgb();
            }

            InitMosaicService();

            if (Config.DefaultRender == DefaultRenderEnum.RenderOriginPicture)
            {
                MosaicService.FillImg(Pic);
            }

            var maxCount = Caches.Max(x => x.Count);
            var maxCountCache = Caches.First(x => x.Count == maxCount);
            var maxCacheIndex = Caches.IndexOf(maxCountCache);

            for (var i = 0; i < clHC; ++i)
            {
                for (var j = 0; j < clWC; ++j)
                {
                    ClusterPos clP = null;
                    Cluster firstCluster = null;

                    for (var bi = 0; bi < ClusterBuilders.Count; ++bi)
                    {
                        var clB = ClusterBuilders[bi];
                        var cluster = clB.CreateCluster(Pic, j * w, i * h, w, h);

                        if (bi == maxCacheIndex)
                            firstCluster = cluster;

                        var cache = Caches[bi];

                        if (cache.ContainsKey(cluster))
                        {
                            var pics = cache[cluster];
                            var rndPic = pics[rnd.Next(pics.Count)];
                            clP = new ClusterPos(rndPic, j, i)
                            {
                                Col = cluster.GetAvColor(),
                                Conf = Config
                            };

                            break;
                        }
                    }

                    if (clP == null)
                    {
                        if (useNearCluster && maxCountCache.Count > 0)
                        {
                            var bestCl = NearClusterSearcher.GetNearCluster(firstCluster);

                            var pics = maxCountCache[bestCl];
                            var rndPic = pics[rnd.Next(pics.Count)];
                            clP = new ClusterPos(rndPic, j, i)
                            {
                                Col = firstCluster.GetAvColor(),
                                Conf = Config
                            };
                        }
                        else
                        {
                            clP = new ClusterPos(null, j, i)
                            {
                                Col = firstCluster.GetAvColor(),
                                Conf = Config
                            };
                        }
                    }

                    Adder(clP);
                }
            }

            Console.WriteLine(new string('_', Console.WindowWidth));
        }

        protected virtual void SavePic()
        {
            MosaicService.GetBmp().Save("res.bmp");
        }
    }
}
