using System;
using Core.Models;
using Core.Expansions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft;
using Core.Services;

namespace PicClusterizator
{
    class Program
    {
        static void Main(string[] args)
        {
            AppConfigService.InitArgs(args);
            var conf = AppConfigService.GetConfig<CoreAppConf>();
            Console.WriteLine(conf.N[1]); Console.WriteLine(conf.Q);
            Console.ReadKey();
        }
    }
}
