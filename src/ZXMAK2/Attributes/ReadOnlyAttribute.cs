using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
    public class ReadOnlyAttribute : Attribute
    {
        public bool IsReadOnly { get; private set; }

        public ReadOnlyAttribute(bool isReadOnly)
        {
            IsReadOnly = isReadOnly;
        }
    }
}
