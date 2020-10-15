using System;
using Core.Models;
using System.Collections.Generic;
using Core.Services;
using PicClusterizator.Models;
using System.IO;
using Core.Parallel;
using System.Drawing;
using System.Threading;
using System.Linq;
using Core.Expansions;

namespace PicClusterizator
{
    class Program
    {
        private static BaseClusterService ClusterBuilder { get; set; }

        private static void TaskCreator(Action<string> Adder) 
        {
            var conf = AppConfigService.GetConfig<Conf>();
            var allowedExt = new string[2] { ".jpg", ".png" };

            var stack = new Stack<string>();
            foreach (var p in conf.Paths)
                stack.Push(p);

            while (stack.Count > 0)
            {
                var dir = stack.Pop();

                var files = Directory
                    .GetFiles(dir)
                    .Where(x => allowedExt.Any(y => x.ToLower().EndsWith(y)));

                foreach (var i in files)
                    Adder(i);

                foreach (var i in Directory.GetDirectories(dir))
                    stack.Push(i);                
            }

            Console.WriteLine(new string('_', Console.WindowWidth));
        }

        private static void Frame(string Value) 
        {
            var cache = ThreadingInstanceController<Dictionary<Cluster, List<string>>>.GetInstance();

            if (!File.Exists(Value))
                return;

            Cluster cluster = null;
            try 
            {
                using var bmp = new Bitmap(Value);
                if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    using var nBmp = bmp.ToFormat24bppRgb();
                    cluster = ClusterBuilder.CreateCluster(nBmp);
                }
                else
                    cluster = ClusterBuilder.CreateCluster(bmp);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Pic: {Value} Exception: {e.Message}");
                return;
            }

            if (!cache.ContainsKey(cluster))
                cache.Add(cluster, new List<string>());

            cache[cluster].Add(Value);

            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {Value}");
        }

        private static void DrawAttr(string Attr) 
        {
            Console.Write(Attr + (Attr.Length >= 19 ? "" : new string(' ', 19 - Attr.Length)));
        }

        private static void WriteHelp() 
        {
            Console.WriteLine("Создаёт кластеризованный образ .jpg & .png картинок.\n");

            Console.WriteLine("PicClusterizator -Alg:args -ThCout -JP -P");
            DrawAttr("-Alg");
            Console.WriteLine("Устанавливает алгоритм кластеризации. В соответствии с алгоритмом, нужно передать дополнительные аргументы. (string)");
            DrawAttr("-ThCout");
            Console.WriteLine("Количество потоков для обработки. (sbyte)");
            DrawAttr("-JP");
            Console.WriteLine("Выходной файл образа. (string)");
            DrawAttr("-P:path1 [+path2] [+path3]");
            Console.WriteLine("\nНачальные директории обработки. (string string string ...)\n");

            Console.WriteLine("Алгоритмы кластеризации");
            DrawAttr("AverageHSV");
            Console.WriteLine("В тупую находит среднее значение цвета.");
            DrawAttr("\t-ClCout");
            Console.WriteLine("Количество областей |sqrt n|. (int)");
            DrawAttr("\t-ClLenH");
            Console.WriteLine("Размер дельты Hue |H|. (short)");
            DrawAttr("\t-ClLenS");
            Console.WriteLine("Размер дельты Saturation |S|. (short)");
            DrawAttr("\t-ClLenV");
            Console.WriteLine("Размер дельты Value |V|. (short)");

            DrawAttr("HashingMinified");
            Console.WriteLine("Создает комбинированный хеш изображения из его среднего цвета и монохромной версии.");
            DrawAttr("\t-CacheSize");
            Console.WriteLine("Размер монохромного изображения для хеша |sqrt n|. (sbyte)");
            DrawAttr("\t-ClLenH");
            Console.WriteLine("Размер дельты Hue |H|. (short)");
            DrawAttr("\t-ClLenS");
            Console.WriteLine("Размер дельты Saturation |S|. (short)");
            DrawAttr("\t-ClLenV");
            Console.WriteLine("Размер дельты Value |V|. (short)");
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
                if (conf.Paths == null || conf.Paths.Any(x => !Directory.Exists(x)))
                {
                    Console.WriteLine("-P invalid");
                    return;
                }

                if (conf.ThreadCount < 1 || conf.ThreadCount > 16)
                {
                    Console.WriteLine("-ThCout invalid");
                    return;
                }

                if (File.Exists(conf.JsonFileName))
                {
                    File.Delete(conf.JsonFileName);
                }
                File.Create(conf.JsonFileName).Dispose();

                var coreConf = AppConfigService.GetConfig<CoreAppConf>();
                if (!ClusterServiceBuilder.GetNames().Contains(coreConf.Clustering)) 
                {
                    Console.WriteLine("-Alg invalid");
                    return;
                }

                try
                {
                    ClusterBuilder = ClusterServiceBuilder.GetBuilder(coreConf.Clustering);
                }
                catch (Exception e) 
                {
                    Console.WriteLine($"The {coreConf.Clustering} arguments are not valid");
                    return;
                }                

                ThreadingInstanceController<Dictionary<Cluster, List<string>>>.SetFactoryMethod(() => new Dictionary<Cluster, List<string>>());
                var fTC = new FrameTaskController<string>(conf.ThreadCount, TaskCreator, Frame);
                fTC.Run();

                var caches = ThreadingInstanceController<Dictionary<Cluster, List<string>>>.GetInstances(fTC.GetFrameThreadIds());

                var cache = caches[0];
                for (var i = 1; i < caches.Count; ++i) 
                {
                    var cache2 = caches[i];
                    foreach (var cl in cache2) 
                    {
                        if (cache.ContainsKey(cl.Key))
                            cache[cl.Key].AddRange(cl.Value);
                        else
                            cache.Add(cl.Key, cl.Value);
                    }
                }

                var serializer = ClusterServiceBuilder.GetSerializer(coreConf.Clustering);

                var jStr = serializer.Serialize(cache.Select(x => Tuple.Create(x.Key, x.Value)).ToList());

                File.WriteAllText(conf.JsonFileName, jStr);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }
    }
}
