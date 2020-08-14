using Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class HashingMinifiedConf
    {
        [Arg("-CacheSize")]
        public sbyte CacheSize { get; set; }

        [Arg("-ClLenH")]
        public short ClusterLenH { get; set; }

        [Arg("-ClLenS")]
        public short ClusterLenS { get; set; }

        [Arg("-ClLenV")]
        public short ClusterLenV { get; set; }
    }
}
