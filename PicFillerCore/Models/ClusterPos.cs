using Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PicFillerCore.Models
{
    class ClusterPos
    {
        public string Pic { get; set; }

        public Color? Col { get; set; }

        public int OffSetW { get; set; }

        public int OffSetH { get; set; }

        public ClusterPos(string Pic, int OffSetW, int OffSetH) 
        {
            this.Pic = Pic;
            this.OffSetH = OffSetH;
            this.OffSetW = OffSetW;
        }
    }
}
