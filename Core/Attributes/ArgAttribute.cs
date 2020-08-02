﻿using System;

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
