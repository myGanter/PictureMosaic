using System;
using System.Drawing;
using Core.Services;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using PicFillerCore.Models;
using System.IO;
using Core.Models;
using Core.Expansions;
using Core.Parallel;
using PicFillerCore.Services;
using System.Threading;

namespace PicFillerCore
{
    class Program
    {
        private static List<BaseClusterService> ClusterBuilders { get; set; }

        private static List<Dictionary<Cluster, List<string>>> Caches { get; set; }

        private static ColorClusterSearcher NearClusterSearcher { get; set; }

        private static Bitmap Pic { get; set; }

        private static MosaicCollectorService MosaicService { get; set; }

        private static void InitMosaicService()
        {
            var conf = AppConfigService.GetConfig<Conf>();
            int picW = Pic.Width, picH = Pic.Height;
            int w = conf.W, h = conf.H;
            int clWC = picW / w, clHC = picH / h;
            int rPW = clWC * conf.WR, rPH = clHC * conf.HR;

            if (conf.HR < 1 || conf.WR < 1 || rPW > 22000 || rPH > 22000)
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

                conf.WR = wr;
                conf.HR = hr;
            }

            Console.WriteLine($"Mosaic segment W:{conf.WR} H:{conf.HR}");
            MosaicService = new MosaicCollectorService(rPW, rPH, conf.WR, conf.HR);
        }

        private static void TaskCreator(Action<ClusterPos> Adder)
        {
            var rnd = new Random();
            var conf = AppConfigService.GetConfig<Conf>();

            var useNearCluster = conf.UseNearCluster;

            int picW = Pic.Width, picH = Pic.Height;
            int w = conf.W, h = conf.H;
            int clWC = picW / w, clHC = picH / h;

            using (var nPic = Pic.ScaleCutBmpCenter(clWC * w, clHC * h))
            {
                Pic.Dispose();
                Pic = nPic.ToFormat24bppRgb();
            }

            InitMosaicService();

            if (conf.DefaultRender == DefaultRenderEnum.RenderOriginPicture)
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
                                Conf = conf
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
                                Conf = conf
                            };
                        }
                        else
                        {
                            clP = new ClusterPos(null, j, i)
                            {
                                Col = firstCluster.GetAvColor(),
                                Conf = conf
                            };
                        }
                    }

                    Adder(clP);
                }
            }

            NearClusterSearcher = null;
            Console.WriteLine(new string('_', Console.WindowWidth));
        }

        private static void Frame(ClusterPos Value)
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

        private static void DrawAttr(string Attr)
        {
            Console.Write(Attr + (Attr.Length >= 15 ? "" : new string(' ', 15 - Attr.Length)));
        }

        private static void WriteHelp()
        {
            Console.WriteLine("Преобразовывает изображение в мозаику, состоящую из других изображений.\n");

            Console.WriteLine("PicFillerCore -Pic -ThCout -JP -ClusterW -ClusterH [-ResW] [-ResH] [-UseNear] [-DfltColRndr] [-SegmOpac]");
            DrawAttr("-Pic");
            Console.WriteLine("Путь до картинки, которую нужно преобразовать. (string)");
            DrawAttr("-ThCout");
            Console.WriteLine("Количество потоков для обработки. (sbyte)");
            DrawAttr("-JP:path1 [path2] [path3]");
            Console.WriteLine("\nПуть до файла-образа. (string string string)");
            DrawAttr("-ClusterW");
            Console.WriteLine("Ширина обрабатываемой области оригинальной картинки. (int)");
            DrawAttr("-ClusterH");
            Console.WriteLine("Высота обрабатываемой области оригинальной картинки. (int)");
            DrawAttr("-ResW");
            Console.WriteLine("Ширина одного элемента мозаики. (int)");
            DrawAttr("-ResH");
            Console.WriteLine("Высота одного элемента мозаики. (int)");
            DrawAttr("-UseNear");
            Console.WriteLine("Если True, использовать ближайший кластер, в случае не нахождения точного. (bool)");
            DrawAttr("-DefaultRender");
            Console.WriteLine("\nЕсли 'RenderOriginPicture', рендерит на задний фон оригинальную картинку.");
            Console.WriteLine("Если 'PreDefaultColorRender', сначала рендерит средний цвет кластера а затем накладывает картинку. (enum)");
            DrawAttr("-SegmOpac");
            Console.WriteLine("Устанавливает прозрачность кластера |от 0 до 1|. (float)");
            DrawAttr("-UseScaleCut");
            Console.WriteLine("Если True, масштабировать изображение из дата сета при изменении размера. (bool)");
        }

        static void Main(string[] args)
        {
            if (args == null || !args.Any() || args.Contains("-H") || args.Contains("-h"))
            {
                WriteHelp();
                return;
            }

            try
            {
                AppConfigService.InitArgs(args);

                var conf = AppConfigService.GetConfig<Conf>();
                if (!File.Exists(conf.Pic))
                {
                    Console.WriteLine("-Pic invalid");
                    return;
                }

                Pic = new Bitmap(conf.Pic);

                if (conf.JsonPath.Any(x => !File.Exists(x)))
                {
                    Console.WriteLine("-JP invalid");
                    return;
                }

                if (conf.ThreadCount < 1 || conf.ThreadCount > 16)
                {
                    Console.WriteLine("-ThCout invalid");
                    return;
                }

                var clw = conf.W;
                var clh = conf.H;
                if (clw < 1 || clh < 1 || clw > Pic.Width || clh > Pic.Height)
                {
                    Console.WriteLine("-ClusterW or -ClusterH invalid");
                    return;
                }

                try
                {
                    ClusterBuilders = new List<BaseClusterService>();
                    Caches = new List<Dictionary<Cluster, List<string>>>();

                    foreach (var jsonP in conf.JsonPath) 
                    {
                        var jStr = File.ReadAllText(jsonP);
                        var algName = ClusterServiceBuilder.DetermineAlgName(jStr);
                        var ser = ClusterServiceBuilder.GetSerializer(algName);

                        var obj = ser.Deserialize(jStr);
                        ClusterBuilders.Add(obj.Item1);
                        Caches.Add(obj.Item2);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Json image invalid");
                    return;
                }

                if (conf.UseNearCluster)
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

                var fTC = new FrameTaskController<ClusterPos>(conf.ThreadCount, TaskCreator, Frame);
                fTC.Run();
                
                using var resImg = MosaicService.GetBmp();
                resImg.Save("res.bmp");                             
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
            finally 
            {
                if (MosaicService != null)
                    MosaicService.Dispose();
                if (Pic != null)
                    Pic.Dispose();
            }
        }
    }
}
