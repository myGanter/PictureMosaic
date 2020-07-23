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
            //var h = new List<ColorHSV>() { new ColorHSV(1, 2, 3), new ColorHSV(4, 5, 6) };
            //var str = JsonConvert.SerializeObject(h);

            //Console.WriteLine(str);

            //var hh = JsonConvert.DeserializeObject<List<ColorHSV>>(str, new JsonHSVConverter());


            var h = new HSVCluster(new ColorHSV[2] { new ColorHSV(1, 2, 3), new ColorHSV(4, 5, 6) });
            var h1 = new HSVCluster(new ColorHSV[2] { new ColorHSV(1, 2, 3), new ColorHSV(4, 5, 6) });

            var c = h.GetHashCode();
            var c2 = h1.GetHashCode();

            var dic = new Dictionary<HSVCluster, int>();
            dic.Add(h, 1);
            dic.Add(h1, 1);

            Console.ReadKey();
        }
    }
}
