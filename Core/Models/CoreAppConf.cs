using Core.Attributes;
using System.Collections.Generic;

namespace Core.Models
{
    public class CoreAppConf
    {
        [Arg("-Alg")]
        public string Clustering { get; set; }

        [Arg("-P")]
        public List<string> Paths { get; set; }

        [Arg("-ThCout")]
        public sbyte ThreadCount { get; set; }
    }
}
