using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class HashingMinifiedSerializeContainer
    {
        public CoreAppConf CoreConf { get; set; }

        public HashingMinifiedConf HashMinConf { get; set; }

        public List<Tuple<Cluster, List<string>>> Data { get; set; }
    }

    public class HashingMinifiedDeSerializeContainer
    {
        public CoreAppConf CoreConf { get; set; }

        public HashingMinifiedConf HashMinConf { get; set; }

        public List<Tuple<HashingMinifiedCluster, List<string>>> Data { get; set; }
    }
}
