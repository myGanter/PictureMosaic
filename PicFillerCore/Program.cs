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
                
                if (conf.IsBatchProcessing)
                {
                    new MultiImgProcessorWorker().Run();
                }
                else
                {
                    new SingleImgProcessorWorker().Run();
                }                         
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.Message}" + (e.InnerException != null ? $"\nError: {e.InnerException?.Message}" : string.Empty));
            }
        }
    }
}
