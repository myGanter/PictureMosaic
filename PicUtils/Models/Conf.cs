using Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PicUtils.Models
{
    class Conf
    {
        [Arg("-U")]
        public Utils? Util { get; set; }
    }
}
