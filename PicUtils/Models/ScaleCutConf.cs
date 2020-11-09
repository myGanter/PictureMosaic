using Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PicUtils.Models
{
    class ScaleCutConf
    {
        [Arg("-ResPuth")]
        public string ResultPuth { get; set; }

        [Arg("-NW")]
        public int Width { get; set; }

        [Arg("-NH")]
        public int Height { get; set; }
    }
}
