using Core.Attributes;
using System.Collections.Generic;

namespace PicClusterizator.Models
{
    class Conf
    {
        [Arg("-P")]
        public List<string> Paths { get; set; }

        [Arg("-JP")]
        public string JsonFileName { get; set; }

        [Arg("-ThCout")]
        public sbyte ThreadCount { get; set; } 
    }
}
