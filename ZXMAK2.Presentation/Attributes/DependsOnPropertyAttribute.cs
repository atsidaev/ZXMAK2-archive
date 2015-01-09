using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZXMAK2.Presentation.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class DependsOnPropertyAttribute : Attribute
    {
        public DependsOnPropertyAttribute(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }
            PropertyName = propertyName;
        }

        public string PropertyName { get; private set; }
    }
}
