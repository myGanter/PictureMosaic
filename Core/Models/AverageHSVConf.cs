using Core.Attributes;

namespace Core.Models
{
    public class AverageHSVConf
    {
        [Arg("-ClCout")]
        public int ClusterCount { get; set; }

        [Arg("-ClLenH")]
        public short ClusterLenH { get; set; }

        [Arg("-ClLenS")]
        public short ClusterLenS { get; set; }

        [Arg("-ClLenV")]
        public short ClusterLenV { get; set; } 
    }
}
