using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection.Emit;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Hardware.Adlers;


namespace ZXMAK2.Hardware.Adlers.Views
{
    #region Debugger enums/structs, ...
    // enum BreakPointConditionType
    // e.g.: 1.) memoryVsValue = left is memory reference, right is number(#9C40, %1100, 6755, ...)
    //       2.) valueVsRegister = left is value, right is register value
    //
    public enum BreakPointConditionType 
    { 
        memoryVsValue, 
        valueVsRegister, 
        registryVsValue, 
        registryMemoryReferenceVsValue, 
        memoryRead 
    };

    //Information about extended breakpoint
    public class BreakpointInfo
    {
        public BreakPointConditionType AccessType { get; set; }

        //condition in string, e.g.: "pc", "(#9C40)"
        public string LeftCondition { get; set; }
        public string RightCondition { get; set; }

        //value of condition(if relevant), valid for whole values or memory access
        public ushort LeftValue { get; set; }
        public ushort RightValue { get; set; }

        public int LeftRegistryArrayIndex { get; set; }

        //condition type
        public string ConditionTypeSign { get; set; }
        public bool IsConditionEquals { get; set; } // true - if values must be equal

        //is active
        public bool IsOn { get; set; }

        //original breakpoint string(raw data get from dbg command line)
        public string BreakpointString { get; set; }

        //value mask - e.g.: for F registry => 0xFF, for A registry => 0xFF00; for AF => isMasked = false
        public bool Is8Bit { get; set; }

        public Func<bool> CheckBreakpoint { get; set; }

        public void SetBreakpointCheckMethod(Func<bool> checkBreakpoint)
        {
            /*switch (i_brkAccessType)
            {
                case BreakPointConditionType.registryVsValue:
                    checkBreakpoint = (checkBreakpointDelegate<bool>)i_emittedCode.CreateDelegate(typeof(checkBreakpointDelegate<bool>), z80Registers);
                    break;
                case BreakPointConditionType.memoryVsValue:
                    checkBreakpoint = (checkBreakpointDelegate<bool>)i_emittedCode.CreateDelegate(typeof(checkBreakpointDelegate<bool>), (VirtualMachine)i_spectrum);
                    break;
            }*/
            CheckBreakpoint = checkBreakpoint;
        }

        public BreakpointInfo()
        {
            IsConditionEquals = false;
            LeftCondition = string.Empty;
            RightCondition = string.Empty;
        }
    }
    #endregion

    public partial class DebuggerManager
    {
        public static string[] Regs16Bit = new string[] { "AF", "BC", "DE", "HL", "IX", "IY", "SP", "IR", "PC", "AF'", "BC'", "DE'", "HL'" };
        public static char[]   Regs8Bit  = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'H', 'L' };

        public enum CommandType 
        { 
            memoryOrRegistryManipulation, 
            breakpointManipulation, 
            gotoAdress, 
            removeBreakpoint, 
            enableBreakpoint,
            disableBreakpoint, 
            loadBreakpointsListFromFile, 
            saveBreakpointsListToFile, 
            showAssembler, 
            showGraphicsEditor, 
            Unidentified
        };

        public enum BreakPointAccessType 
        { 
            memoryAccess, 
            memoryWrite, 
            memoryChange, 
            memoryRead, 
            registryValue, 
            All, 
            Undefined 
        };

        public enum CharType { Number = 0, Letter, Other };

        //debugger commands list
        public static string DbgKeywordLD = "ld"; // memory/registers modification(=ld as in assembler)
        public static string DbgKeywordBREAK = "br"; // set breakpoint
        public static string DbgKeywordDissassemble = "ds"; // dasmPanel - goto adress(disassembly panel), (=disassembly)
        public static string DbgRemoveBreakpoint = "del"; // remove breakpoint, e.g.: del 1 - will delete breakpoint nr. 1
        public static string DbgEnableBreakpoint = "on"; // enables breakpoint
        public static string DbgDisableBreakpoint = "off"; // disables breakpoint
        public static string DbgLoadBreakpointsListFromFile = "loadbrs"; // loads breakpoints from file
        public static string DbgSaveBreakpointsListFromFile = "savebrs"; // save actual breakpoints list into file
        public static string DbgOpenAssembler = "asm"; // opens Assembler Form
        public static string DbgOpenGraphicsEditor = "ge"; // opens Graphics editor

        private static readonly char[] DebugDelimitersOther = new char[] { '(', '=', ')', '!' };

        // Main method - returns string list with items entered in debug command line, e.g. : 
        //
        // 0. item: br
        // 1. item: (PC)
        // 2. item: ==
        // 3. item: #9C40
        // 
        // Must be working: ld hl,  #4000; br (pc)==#4000; br (pc)   ==#af; br a<#FE; ld ( 16384  ), 255; ...
        public static List<string> ParseCommand(string dbgCommand)
        {
            try
            {
                var pattern = @"(\s+|,|==|!=|<|>)";
                var dbgParsed = new List<string>();
                dbgCommand = TrimCommand(dbgCommand);
                foreach (string result in Regex.Split(dbgCommand, pattern)) 
                {
                    if (!String.IsNullOrEmpty(result) && result.Trim() != "" && result != ",")
                        dbgParsed.Add(result);
                }
                return dbgParsed;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        private static string TrimCommand(string strIn)
        {
            var strOut = strIn;

            //trim all whitespaces after '(' until next character
            while (strOut.Contains("( "))
                strOut = strOut.Replace("( ", "(");
            //trim all whitespaces before ')'
            while (strOut.Contains(" )"))
                strOut = strOut.Replace(" )", ")");

            return strOut;
        }

        public static void HasDigitsAndLettersInString(string s, ref bool hasLetters, ref bool hasDigits)
        {
            bool parsingDigits = false;

            foreach (char c in s)
            {
                if (Char.IsLetter(c) && !parsingDigits) //parsingDigits - do not consider [A-Fa-f] as letter in case we`re parsing number
                {
                    hasLetters = true;
                    parsingDigits = false;
                    continue;
                }

                if (Char.IsDigit(c) || c == '%' || c == '#' || c == '(' || c == ')')  // % - binary number, # - hex number; '(' and ')' are also digits
                {
                    hasDigits = true;
                    parsingDigits = true;
                    continue;
                }
            }
        }

        public static void HasOtherCharsInString(string s, char[] searchingChars, ref bool hasOtherChars)
        {
            for (byte listCounter = 0; listCounter < searchingChars.Length; listCounter++)
            {
                if (s.IndexOf(searchingChars[listCounter]) >= 0)
                {
                    hasOtherChars = true;
                    return;
                }
            }
        }

        public static CharType getCharType(char inputChar)
        {
            if (Char.IsDigit(inputChar) || inputChar == '%' || inputChar == '#') // % - binary number, # - hex number
                return CharType.Number;

            if (Char.IsLetter(inputChar))
                return CharType.Letter;

            foreach (char c in DebugDelimitersOther)
            {
                if (c == inputChar)
                    return CharType.Other;
            }

            throw new CommandParseException("Incorrect character found: " + inputChar);
        }

        //Method will resolve whether command entered is memory modification or breakpoint setting
        public static CommandType getDbgCommandType(List<string> command)
        {
            if (command[0].ToUpper() == DbgKeywordLD.ToString().ToUpper())
            {
                return CommandType.memoryOrRegistryManipulation;
            }

            if (command[0].ToUpper() == DbgKeywordBREAK.ToString().ToUpper())
            {
                return CommandType.breakpointManipulation;
            }

            if (command[0].ToUpper() == DbgKeywordDissassemble.ToString().ToUpper())
            {
                return CommandType.gotoAdress;
            }

            if (command[0].ToUpper() == DbgEnableBreakpoint.ToString().ToUpper())
            {
                return CommandType.enableBreakpoint;
            }

            if (command[0].ToUpper() == DbgDisableBreakpoint.ToString().ToUpper())
            {
                return CommandType.disableBreakpoint;
            }

            if (command[0].ToUpper() == DbgRemoveBreakpoint.ToString().ToUpper())
            {
                return CommandType.removeBreakpoint;
            }

            if (command[0].ToUpper() == DbgLoadBreakpointsListFromFile.ToString().ToUpper())
            {
                return CommandType.loadBreakpointsListFromFile;
            }

            if (command[0].ToUpper() == DbgSaveBreakpointsListFromFile.ToString().ToUpper())
            {
                return CommandType.saveBreakpointsListToFile;
            }

            if (command[0].ToUpper() == DbgOpenAssembler.ToString().ToUpper())
            {
                return CommandType.showAssembler;
            }

            if (command[0].ToUpper() == DbgOpenGraphicsEditor.ToString().ToUpper())
            {
                return CommandType.showGraphicsEditor;
            }

            return CommandType.Unidentified;
        }

        ////////////////////////////////////////////////////////////////////
        //
        // Method: convertNumberWithPrefix()
        //
        public static UInt16 convertNumberWithPrefix(string input) //Prefix: % - binary, # - hexadecimal
        {
            var inputTrimmed = input.Trim();
            // % - binary
            if (inputTrimmed[0] == '%')
            {
                var number = inputTrimmed.Substring(1, inputTrimmed.Length - 1);
                return ConvertRadix.ParseUInt16(number, 2);
            }

            // # - hexadecimal
            if (inputTrimmed[0] == '#')
            {
                var number = inputTrimmed.Substring(1, inputTrimmed.Length - 1);
                return ConvertRadix.ParseUInt16(number, 16);
            }
            return ConvertRadix.ParseUInt16(inputTrimmed, 10);
        }

        public static bool isRegistry(string input)
        {
            try
            {
                string registry = input.ToUpper().Trim();

                //not available in .NET Framework 2.0 - so must coding :-)
                /*if (Regs16Bit.ToArray().Contains<string>(registry))
                    return true;
                if (Regs8Bit.Contains<char>(Convert.ToChar(registry)))
                    return true;*/

                for (byte counter = 0; counter < Regs16Bit.Length; counter++)
                {
                    if (Regs16Bit[counter] == registry)
                        return true;
                }

                //now only low(8bit) registry are allowed, such as A, B, C, L, D, ...
                if (registry.Length > 1)
                    return false;

                for (byte counter = 0; counter < Regs8Bit.Length; counter++)
                {
                    if ( Regs8Bit[counter].ToString() == registry)
                        return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return false;
        }

        public static bool IsHex(char c)
        {
            return (new Regex("[A-Fa-f0-9]").IsMatch(c.ToString()));
        }

        public static bool isRegistryMemoryReference(string registryMemoryReference)
        {
            string registry = getRegistryFromReference(registryMemoryReference);
            if (isRegistry(registry))
                return true;

            return false;
        }

        public static string getRegistryFromReference( string registryMemoryRef )
        {
            if (registryMemoryRef.Length < 4 || !registryMemoryRef.StartsWith("(") || !registryMemoryRef.EndsWith(")")) // (PC), (DE), (hl), ...
                return String.Empty;

            return registryMemoryRef.Substring(1, registryMemoryRef.Length - 2);
        }

        public static bool isMemoryReference(string input)
        {
            if (input.StartsWith("(") && input.EndsWith(")"))
                return true;

            return false;
        }

        public static UInt16 getReferencedMemoryPointer(string input)
        {
            if (!isMemoryReference(input))
                throw new CommandParseException("Incorrect memory reference: " + input);

            return convertNumberWithPrefix(input.Substring(1, input.Length - 2));
        }

        public static BreakPointAccessType getBreakpointType( List<string> breakpoint )
        {
            try
            {
                var left  = breakpoint[1];
                var right = breakpoint[3];

                if (isMemoryReference(left) || isMemoryReference(right))
                {
                    return BreakPointAccessType.memoryChange;
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex);
            }
            return BreakPointAccessType.Undefined;
        }

        public static ushort getRegistryValueByName(CpuRegs regs, string registryName)
        {
            registryName = registryName.ToUpperInvariant();

            switch (registryName)
            {
                case "PC":
                    return regs.PC;
                case "IR":
                    return regs.IR;
                case "SP":
                    return regs.SP;
                case "AF":
                    return regs.AF;
                case "A":
                    return (ushort)(regs.AF >> 8);
                case "HL":
                    return regs.HL;
                case "DE":
                    return regs.DE;
                case "BC":
                    return regs.BC;
                case "IX":
                    return regs.IX;
                case "IY":
                    return regs.IY;
                case "AF'":
                    return regs._AF;
                case "HL'":
                    return regs._HL;
                case "DE'":
                    return regs._DE;
                case "BC'":
                    return regs._BC;
                case "MW (Memptr Word)":
                    return regs.MW;
                default:
                    throw new CommandParseException("Bad registry name: " + registryName);
            }
        }

        public static int getRegistryArrayIndex(string registry)
        {
            switch (registry.ToUpperInvariant())
            {
                case "AF":
                case "F":
                    return 0;
                case "A":
                case "BC":
                    return 1;
                case "C":
                case "DE":
                    return 2;
                case "B":
                case "HL":
                    return 3;
                case "E":
                    return 4;
                case "D":
                    return 5;
                case "L":
                case "IX":
                    return 6;
                case "H":
                case "IY":
                    return 7;
                case "SP":
                    return 12;
                case "PC":
                    return 11;
            }
               
            return -1;
        }

        public static void EmitCondition(ILGenerator ilGenerator, string condition)
        {
            switch (condition)
            {
                case "==":
                    ilGenerator.Emit(OpCodes.Ceq);
                    break;
                case "!=":
                    ilGenerator.Emit(OpCodes.Ceq);
                    ilGenerator.Emit(OpCodes.Ldc_I4, 0);
                    ilGenerator.Emit(OpCodes.Ceq);
                    break;
                case ">":
                    ilGenerator.Emit(OpCodes.Cgt);
                    break;
                case "<":
                    ilGenerator.Emit(OpCodes.Clt);
                    break;
                default:
                    throw new CommandParseException(
                        string.Format("Unknown condition: {0}", condition));
            }
        }
    }
}
