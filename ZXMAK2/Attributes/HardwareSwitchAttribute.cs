using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HardwareSwitchAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Description { get; set; }


        public HardwareSwitchAttribute(string name)
        {
            Name = name;
        }
    }
}
