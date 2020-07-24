using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Core.Services
{
    class AppArgsService
    {
        private readonly Dictionary<string, List<string>> Cache;

        private readonly string DefaultFirstArgName; 

        public List<string> DefaultFirstArgs => Cache[DefaultFirstArgName].Select(x => x).ToList();

        public List<string> this[string Arg] => Cache.ContainsKey(Arg) ? Cache[Arg].Select(x => x).ToList() : new List<string>();

        public bool ContainsArg(string Arg) => Cache.ContainsKey(Arg);

        public AppArgsService(string[] Args) 
        {
            if (Args == null)
                throw new Exception("Args is null");

            DefaultFirstArgName = "First args";
            Cache = new Dictionary<string, List<string>>();

            Init(Args);
        }

        private void Init(string[] Args) 
        {
            string cur = DefaultFirstArgName;
            Cache.Add(cur, new List<string>());

            foreach (var arg in Args) 
            {
                if (arg.StartsWith("-"))
                {
                    cur = arg;
                    Cache.Add(arg, new List<string>());
                }
                else 
                {
                    Cache[cur].Add(arg);
                }
            }
        }
    }
}
