using Core.Services;
using PicUtils.Models;
using PicUtils.Services;
using System;
using System.Linq;

namespace PicUtils
{
    class Program
    {
        private static void WriteHelp()
        {
            Console.WriteLine("TODO.\n");
        }

        private static void DrawAttr(string Attr)
        {
            Console.Write(Attr + (Attr.Length >= 15 ? "" : new string(' ', 15 - Attr.Length)));
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

                switch (conf.Util)
                {
                    case Utils.AvarageSize:
                        new AvarageSizeWorker().Run();
                        break;
                    case Utils.ScaleCutDataSet:
                        new ScaleCutDataSetWorker().Run();
                        break;
                    default:
                        throw new Exception("-U invalid");                        
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.Message}" + (e.InnerException != null ? $"\nError: {e.InnerException?.Message}" : string.Empty));
            }
        }
    }
}
