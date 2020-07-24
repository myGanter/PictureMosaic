using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Core.Attributes;
using Newtonsoft.Json;
using System.IO;

namespace Core.Services
{
    public static class AppConfigService
    { 
        private static AppArgsService AppArgs;

        private static readonly Dictionary<Type, object> Cache;

        private static readonly Type ArgAttrType;

        private static readonly Type ConfPathAttrType;

        private static readonly object Locker;

        static AppConfigService() 
        {
            Cache = new Dictionary<Type, object>();
            ArgAttrType = typeof(ArgAttribute);
            ConfPathAttrType = typeof(ConfigPathAttribute);
            Locker = new object();
        }

        public static void InitArgs(string[] Args) 
        {
            AppArgs = new AppArgsService(Args);           
        }
        
        public static T GetConfig<T>() where T : class, new()
        {
            var type = typeof(T);
            var conf = new T();

            lock (Locker)
            {
                if (Cache.ContainsKey(type))
                    return (T)Cache[type];

                var confAttrO = type.GetCustomAttributes(false).Where(x => x.GetType() == ConfPathAttrType).FirstOrDefault();

                if (confAttrO is ConfigPathAttribute confAttr && File.Exists(confAttr.Path))
                {
                    var jsonStr = File.ReadAllText(confAttr.Path);
                    conf = JsonConvert.DeserializeObject<T>(jsonStr);
                }

                var typeFields = type
                    .GetFields()
                    .Where(x => x.GetCustomAttributes(false).Any(y => y.GetType() == ArgAttrType))
                    .Select(x => new { Field = x, Attr = (ArgAttribute)x.GetCustomAttributes(false).First(y => y.GetType() == ArgAttrType) })
                    .ToList();

                var typeProps = type
                    .GetProperties()
                    .Where(x => x.GetCustomAttributes(false).Any(y => y.GetType() == ArgAttrType))
                    .Select(x => new { Prop = x, Attr = (ArgAttribute)x.GetCustomAttributes(false).First(y => y.GetType() == ArgAttrType) })
                    .ToList();

                typeFields.ForEach(x =>
                {
                    var fT = x.Field.FieldType;
                    var argName = x.Attr.Name;

                    var args = AppArgs[argName];
                    if (args.Any())
                    {
                        try
                        {
                            x.Field.SetValue(conf, ConvertStrToType(fT, args));
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Attribute " + argName + " not a valid");
                        }
                    }
                });

                typeProps.ForEach(x =>
                {
                    var pT = x.Prop.PropertyType;
                    var argName = x.Attr.Name;

                    var args = AppArgs[argName];
                    if (args.Any())
                    {
                        try
                        {
                            x.Prop.SetValue(conf, ConvertStrToType(pT, args));
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Attribute " + argName + " not a valid");
                        }
                    }
                });

                Cache.Add(type, conf);
            }

            return conf;
        }

        private static object ConvertStrToType(Type ToT, List<string> Args) 
        {
            if (ToT.IsGenericType && ToT.GetGenericTypeDefinition() == typeof(List<>))             
            {
                var gT = ToT
                    .GetGenericArguments()[0];

                var lT = typeof(List<>);
                lT = lT.MakeGenericType(gT);
                var collection = Activator.CreateInstance(lT);
                lT = collection.GetType();                
                var method = lT.GetMethod("Add");

                Args.ForEach(x => 
                {
                    method.Invoke(collection, new object[1] { ConvertStrToType(gT, x) });
                });

                return collection;
            }
            else
                return ConvertStrToType(ToT, Args[0]);            
        }

        private static object ConvertStrToType(Type ToT, string Str) 
        {
            if (ToT == typeof(string))
                return Str;

            if (ToT == typeof(char))
                return Str[0];

            if (ToT == typeof(bool))
                return bool.Parse(Str);

            if (ToT == typeof(byte))
                return byte.Parse(Str);
            if (ToT == typeof(sbyte))
                return sbyte.Parse(Str);

            if (ToT == typeof(short))
                return short.Parse(Str);
            if (ToT == typeof(ushort))
                return ushort.Parse(Str);

            if (ToT == typeof(int))
                return int.Parse(Str);
            if (ToT == typeof(uint))
                return uint.Parse(Str);

            if (ToT == typeof(long))
                return long.Parse(Str);
            if (ToT == typeof(ulong))
                return ulong.Parse(Str);

            if (ToT == typeof(float))
                return float.Parse(Str);

            if (ToT == typeof(double))
                return double.Parse(Str);

            if (ToT == typeof(decimal))
                return decimal.Parse(Str);

            throw new Exception("Type not supported");
        }
    }
}
