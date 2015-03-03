using System;


namespace ZXMAK2.Hardware.Adlers
{
    public static class ConvertRadix
    {
        private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        
        public static ushort ParseUInt16(string text, int radix)
        {
            if (radix < 2 || radix > Alphabet.Length)
            {
                throw new ArgumentOutOfRangeException("radix");
            }
            text = text.Trim().ToUpperInvariant();

            var pos = 0;
            var result = 0;

            // Use lookup table to parse string
            while (pos < text.Length && !char.IsWhiteSpace(text[pos]))
            {
                var digit = text.Substring(pos, 1);
                var i = Alphabet.IndexOf(digit);
                if (i >= 0 && i < radix)
                {
                    result *= radix;
                    result += i;
                    pos++;
                    if (result > ushort.MaxValue)
                    {
                        // Overflow
                        throw new CommandParseException(
                            string.Format("Value out of range: {0}", text));
                    }
                }
                else
                {
                    // Invalid character encountered
                    throw new CommandParseException(
                        string.Format("Invalid character encountered: {0}", text));
                }
            }
            // Return true if any characters processed
            if (pos < 1)
            {
                throw new CommandParseException("Missing value");
            }
            return (ushort)result;
        }

        ////////////////////////////////////////////////////////////////////
        //
        // Method: convertNumberWithPrefix()
        //
        public static UInt16 ConvertNumberWithPrefix(string input) //Prefix: % - binary, # - hexadecimal
        {
            if (input == null || input.Trim() == String.Empty)
            {
                throw new CommandParseException("ConvertNumberWithPrefix: Empty or null value to be converted => error");
            }
            string inputTrimmed = input.Trim();

            // % - binary
            if (inputTrimmed[0] == '%')
            {
                var number = inputTrimmed.Substring(1, inputTrimmed.Length - 1);
                return ConvertRadix.ParseUInt16(number, 2);
            }

            // '#' or 'x' - hexadecimal
            if (inputTrimmed[0] == '#' || inputTrimmed[0] == 'x')
            {
                var number = inputTrimmed.Substring(1, inputTrimmed.Length - 1);
                return ConvertRadix.ParseUInt16(number, 16);
            }
            return ConvertRadix.ParseUInt16(inputTrimmed, 10);
        }
    }
}
