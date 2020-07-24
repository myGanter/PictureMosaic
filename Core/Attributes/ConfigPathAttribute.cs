using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Attributes
{
    public class ConfigPathAttribute : Attribute
    {
        public string Path { get; set; }

        public ConfigPathAttribute(string Path) 
        {
            this.Path = Path;
        }
    }
}
