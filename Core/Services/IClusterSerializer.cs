using Core.Models;
using System;
using System.Collections.Generic;

namespace Core.Services
{
    public interface IClusterSerializer
    {
        public string Serialize(List<Tuple<Cluster, List<string>>> Data);

        public Tuple<BaseClusterService, Dictionary<Cluster, List<string>>> Deserialize(string Json);
    }
}
