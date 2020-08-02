using System;

namespace Core.Attributes
{
    public class NameAttribute : Attribute
    {
        public string Name { get; set; }

        public NameAttribute(string Name) 
        {
            this.Name = Name;
        }
    }
}
