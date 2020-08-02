using Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Services
{
    public class AverageHSVColorSerializer : IClusterSerializer
    {
        public Tuple<BaseClusterService, Dictionary<Cluster, List<string>>> Deserialize(string Json)
        {
            var obj = JsonConvert.DeserializeObject<AverageHSVColorDeSerializeContainer>(Json, new JsonHSVConverter());

            var shsvConf = obj.HSVConf;

            var hsvConf = AppConfigService.GetConfig<AverageHSVConf>();

            hsvConf.ClusterCount = shsvConf.ClusterCount;
            hsvConf.ClusterLenH = shsvConf.ClusterLenH;
            hsvConf.ClusterLenS = shsvConf.ClusterLenS;
            hsvConf.ClusterLenV = shsvConf.ClusterLenV;

            var clusterBuilder = ClusterServiceBuilder.GetBuilder(obj.CoreConf.Clustering);

            var cache = obj.Data.ToDictionary(v => (Cluster)v.Item1, v => v.Item2);

            return Tuple.Create(clusterBuilder, cache);
        }

        public string Serialize(List<Tuple<Cluster, List<string>>> Data)
        {
            var serCont = new AverageHSVColorSerializeContainer() 
            {
                Data = Data,
                HSVConf = AppConfigService.GetConfig<AverageHSVConf>(),
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
