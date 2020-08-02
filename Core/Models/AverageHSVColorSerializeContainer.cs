using System;
using System.Collections.Generic;

namespace Core.Models
{
    public class AverageHSVColorSerializeContainer
    {
        public CoreAppConf CoreConf { get; set; }

        public AverageHSVConf HSVConf { get; set; }

        public List<Tuple<Cluster, List<string>>> Data { get; set; }
    }

    public class AverageHSVColorDeSerializeContainer
    {
        public CoreAppConf CoreConf { get; set; }

        public AverageHSVConf HSVConf { get; set; }

        public List<Tuple<HSVCluster, List<string>>> Data { get; set; }
    }
}
