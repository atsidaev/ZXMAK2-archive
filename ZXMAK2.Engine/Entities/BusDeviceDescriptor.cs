using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Interfaces;

namespace ZXMAK2.Entities
{
    public class BusDeviceDescriptor
    {
        public BusDeviceDescriptor(
            Type type,
            BusDeviceCategory category,
            string name,
            string description)
        {
            Type = type;
            Category = category;
            Name = name;
            Description = description;
        }

        public Type Type { get; private set; }
        public BusDeviceCategory Category { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
