using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZXMAK2.Mvvm;

namespace ZXMAK2.Hardware.WinForms.General.Views
{
    public static class Converters
    {
        public static readonly IValueConverter RegPairToString = 
            new IntegerToStringConverter() { IsHex = true, DigitCount = 4, };
    }
}
