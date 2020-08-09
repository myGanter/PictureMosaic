using Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Services
{
    public class HashingMinifiedSerializer : IClusterSerializer
    {
        public Tuple<BaseClusterService, Dictionary<Cluster, List<string>>> Deserialize(string Json)
        {
            var obj = JsonConvert.DeserializeObject<HashingMinifiedDeSerializeContainer>(Json, new JsonHSVConverter());

            var conf = AppConfigService.GetConfig<HashingMinifiedConf>();

            conf.CacheSize = obj.HashMinConf.CacheSize;

            var clusterBuilder = ClusterServiceBuilder.GetBuilder(obj.CoreConf.Clustering);
            var cache = obj.Data.ToDictionary(v => (Cluster)v.Item1, v => v.Item2);

            return Tuple.Create(clusterBuilder, cache);
        }

        public string Serialize(List<Tuple<Cluster, List<string>>> Data)
        {
            var serCont = new HashingMinifiedSerializeContainer()
            {
                Data = Data,
                HashMinConf = AppConfigService.GetConfig<HashingMinifiedConf>(),
                CoreConf = AppConfigService.GetConfig<CoreAppConf>()
            };

            var jStr = JsonConvert.SerializeObject(serCont, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        ProcessDictionaryKeys = true
                    }
                },
                Formatting = Formatting.Indented
            });

            return jStr;
        }
    }
}
