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
        private static BaseClusterService ClusterBuilder { get; set; }

        private static Dictionary<Cluster, List<string>> Cache { get; set; }

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
                var wr = w * 10;
                var hr = h * 10;

                rPW = clWC * wr;
                rPH = clHC * hr;

                if (rPW > 22000 || rPH > 22000)
                {
                    if (rPW > rPH)
                    {
                        var cof = (double)rPH / rPW;
                        wr = 22000 / clWC;
                        hr = (int)Math.Round(wr * cof);
                    }
                    else
                    {
                        var cof = (double)rPW / rPH;
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

            using (var nPic = Pic.CutBmpToCenter(clWC * w, clHC * h))
            {
                Pic.Dispose();
                Pic = nPic.ToFormat24bppRgb();
            }

            InitMosaicService();

            for (var i = 0; i < clHC; ++i)
            {
                for (var j = 0; j < clWC; ++j)
                {
                    var cluster = ClusterBuilder.CreateCluster(Pic, j * w, i * h, w, h);

                    ClusterPos clP;
                    if (Cache.ContainsKey(cluster))
                    {
                        var pics = Cache[cluster];
                        var rndPic = pics[rnd.Next(pics.Count)];
                        clP = new ClusterPos(rndPic, j, i)
                        {
                            Col = cluster.GetAvColor(),
                            Conf = conf
                        };
                    }
                    else if (useNearCluster && Cache.Count > 0)
                    {
                        var bestCl = NearClusterSearcher.GetNearCluster(cluster);

                        var pics = Cache[bestCl];
                        var rndPic = pics[rnd.Next(pics.Count)];
                        clP = new ClusterPos(rndPic, j, i)
                        {
                            Col = cluster.GetAvColor(),
                            Conf = conf
                        };
                    }
                    else
                    {
                        clP = new ClusterPos(null, j, i)
                        {
                            Col = cluster.GetAvColor(),
                            Conf = conf
                        };
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

            if (Value.Pic == null || conf.PreDefaultColorRender)
            {
                MosaicService.FillRect(Value.Col, Value.OffSetW, Value.OffSetH);
            }

            if (Value.Pic != null)
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {Value.Pic}");
                
                using var fPic = new Bitmap(Value.Pic);
                using var sPic = fPic.CutBmpToCenter(fPic.Width / conf.WR * conf.WR, fPic.Height / conf.HR * conf.HR);
                using var rPic = new Bitmap(sPic, conf.WR, conf.HR);
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

            Console.WriteLine("PicFillerCore [-Pic] [-ThCout] [-JP] [-ClusterW] [-ClusterH] [-ResW] [-ResH] [-UseNear]");
            DrawAttr("-Pic");
            Console.WriteLine("Путь до картинки, которую нужно преобразовать. (string)");
            DrawAttr("-ThCout");
            Console.WriteLine("Количество потоков для обработки. (sbyte)");
            DrawAttr("-JP");
            Console.WriteLine("Путь до файла-образа. (string)");
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
            DrawAttr("-DfltColRndr");
            Console.WriteLine("Если True, сначала рендерит средний цвет кластера а затем накладывает картинку. (bool)");
            DrawAttr("-SegmOpac");
            Console.WriteLine("Устанавливает прозрачность кластера |от 0 до 1|. (float)");
        }

        static void Main(string[] args)
        {
            if (args != null && (args.Contains("-H") || args.Contains("-h")))
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

                if (!File.Exists(conf.JsonPath))
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
                    var jStr = File.ReadAllText(conf.JsonPath);
                    var algName = ClusterServiceBuilder.DetermineAlgName(jStr);
                    var ser = ClusterServiceBuilder.GetSerializer(algName);

                    var obj = ser.Deserialize(jStr);
                    ClusterBuilder = obj.Item1;
                    Cache = obj.Item2;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Json image invalid");
                    return;
                }

                if (conf.UseNearCluster)
                {
                    NearClusterSearcher = new ColorClusterSearcher
                    {
                        OldNewValChecker = (oldV, newV) => Cache[newV].Count > Cache[oldV].Count
                    };

                    foreach (var cl in Cache)
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
                MosaicService.Dispose();
                Pic.Dispose();
            }
        }
    }
}
