using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Core.Attributes;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Core.Models;

namespace Core.Services
{
    public static class ClusterServiceBuilder
    {
        private static readonly Dictionary<string, Tuple<Func<BaseClusterService>, Func<IClusterSerializer>>> Cache;

        static ClusterServiceBuilder() 
        {
            Cache = new Dictionary<string, Tuple<Func<BaseClusterService>, Func<IClusterSerializer>>>();
            InitCache();
        }

        public static bool Contains(string Name) => Cache.ContainsKey(Name);

        public static string DetermineAlgName(string Json) 
        {
            var obj = JsonConvert.DeserializeObject<CoreAppConfContainer>(Json);
            var coreConf = AppConfigService.GetConfig<CoreAppConf>();

            coreConf.Clustering = obj.CoreConf.Clustering;

            return coreConf.Clustering;
        }

        public static BaseClusterService GetBuilder(string Name) => Cache[Name].Item1();

        public static IClusterSerializer GetSerializer(string Name) => Cache[Name].Item2();

        public static List<string> GetNames() => Cache.Select(x => x.Key).ToList();

        private static void InitCache() 
        {
            var clServType = typeof(BaseClusterService);
            var nameAttrType = typeof(NameAttribute);

            var fabrics = Assembly
                .GetCallingAssembly()
                .GetTypes()
                .Where(x => x.BaseType == clServType)
                .Select(x => new { ((NameAttribute)x.GetCustomAttribute(nameAttrType))?.Name, Method = x.GetMethod("CreateInstance", BindingFlags.Public | BindingFlags.Static), Method2 = x.GetMethod("CreateSerializer", BindingFlags.Public | BindingFlags.Static) })
                .Where(x => x.Name != null && x.Method != null)
                .ToList();

            foreach (var i in fabrics) 
            {
                var methodCall = Expression.Call(null, i.Method, null);
                var expressionlambda = Expression.Lambda<Func<BaseClusterService>>(methodCall, null);
                var res = expressionlambda.Compile();

                var methodCall2 = Expression.Call(null, i.Method2, null);
                var expressionlambda2 = Expression.Lambda<Func<IClusterSerializer>>(methodCall2, null);
                var res2 = expressionlambda2.Compile();

                Cache.Add(i.Name, Tuple.Create(res, res2));
            }
        }
    }
}
