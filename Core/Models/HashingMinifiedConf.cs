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
    }
}
