using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Attributes
{
    public class ArgAttribute : Attribute
    {
        public string Name { get; set; }

        public ArgAttribute(string Name) 
        {
            this.Name = Name;
        }
    }
}
