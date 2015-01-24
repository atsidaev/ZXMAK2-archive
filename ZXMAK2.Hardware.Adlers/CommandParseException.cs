using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZXMAK2.Hardware.Adlers
{
    public class CommandParseException : Exception
    {
        public CommandParseException(string message)
            : base(message)
        {
        }

        public CommandParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
