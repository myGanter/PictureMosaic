using System;
using System.Collections.Generic;
using System.Text;
using Core.Attributes;

namespace PicFillerCore.Models
{
    class Conf
    {
        [Arg("-Pic")]
        public string Pic { get; set; }

        [Arg("-JP")]
        public string JsonPath { get; set; }

        [Arg("-ThCout")]
        public sbyte ThreadCount { get; set; }

        [Arg("-ClusterW")]
        public int W { get; set; }

        [Arg("-ClusterH")]
        public int H { get; set; }

        [Arg("-ResW")]
        public int WR { get; set; }

        [Arg("-ResH")]
        public int HR { get; set; }

        [Arg("-UseNear")]
        public bool UseNearCluster { get; set; }
    }
}
