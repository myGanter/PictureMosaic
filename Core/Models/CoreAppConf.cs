using System;
using System.Collections.Generic;
using System.Text;
using Core.Attributes;

namespace Core.Models
{
    public class CoreAppConf
    {
        [Arg("-N")]
        public List<short> N;

        [Arg("-Na")]
        public int Q { get; set; }
    }
}
