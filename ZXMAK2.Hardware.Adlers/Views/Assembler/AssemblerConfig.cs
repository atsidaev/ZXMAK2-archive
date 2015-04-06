using FastColoredTextBoxNS;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;

namespace ZXMAK2.Hardware.Adlers.Views.AssemblerView
{
    class AssemblerConfig
    {
        //private string uriProxyAddress;
        //private string uriProxyPort;

        public AssemblerConfig()
        {
            InitSyntaxHighlightningStyles();
        }

        #region Syntax highlightning
            //comments
            public static Style  styleComment = new TextStyle(Brushes.Green, null, FontStyle.Italic);
            public static string regexComment = @";.*$";
            //common instructions
            public static Style  styleCommonInstruction = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
            public static string regexCommonInstruction = @"\bldir\b|\blddr\b|\bld\b|\bim[ ]+\b|\badd\b|\bsub\b|[ ]dec[ ]|\bsbc\b|\bhalt\b|\bbit\b|" +
                                                          @"\bset\b|xor|[ ]inc[ ]|\bcp\b|\bcpl\b|\bei\b|\bdi\b|\band\b|\bor\b|\band\b" +
                                                          @"|\brr\b|\bscf\b|\bccf\b|\bneg\b|\bsrl\b|exx|\bex\b|\brla\b|\brra\b|\brr\b|\bout\b|\bin\b|\bsla\b|\brl\b|\brrca\b" + 
                                                          @"|\brlca\b";
            //jumps
            public static Style  styleJumpInstruction = new TextStyle(Brushes.DarkViolet, null, FontStyle.Regular);
            public static string regexJumpInstruction = @"\breti\b|\bretn\b|\bret\b|\bjp\b|\bjr\b|\bcall\b|\bdjnz\b";
            //stack
            public static Style  styleStackInstruction = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
            public static string regexStackInstruction = @"\bpush\b|\bpop\b|\bdec sp\b|\binc sp\b";
            //registry
            public static Style  styleRegistry = new TextStyle(Brushes.DarkRed, null, FontStyle.Regular);
            public static string regexRegistry = @"\bhl\b|(?<=[ ,(])BC(?=[ ,)\n;\r])|\bix\b|\biy\b|\bde\b|\bpc\b|\bsp\b|[\( ,]\b(IR|HL)\b|(?<=[ ,(])AF(?=[ ,)\n;\r])";
            //compiler directives
            public static Style  styleCompilerDirectives = new TextStyle(Brushes.SaddleBrown, null, FontStyle.Italic);
            public static string regexCompilerDirectives = @"\borg\b|\bdefb\b|\bdefw\b|\bdefl\b|\bdefm\b|\bdefs\b|\bequ\b|\bmacro\b|\bendm\b|include|incbin|" +
                                                           @"\bif\b|\bendif\b|\belse\b";
            //numbers
            public static Style  styleNumbers = new TextStyle(Brushes.DarkCyan, null, FontStyle.Regular);
            public static string regexNumbers = @"(?:\(|\n|,| |\+|\-|\*|\/)\d{1,5}\b|[^a-zA-Z](?:x|#|\$)[0-9A-Fa-f]{1,4}|%[0-1]{1,16}";
        
        private void InitSyntaxHighlightningStyles()
        {
            styleComment = new TextStyle(Brushes.Green, null, FontStyle.Italic);
            styleCommonInstruction = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
            styleJumpInstruction = new TextStyle(Brushes.DarkViolet, null, FontStyle.Regular);
            styleStackInstruction = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
            styleCompilerDirectives = new TextStyle(Brushes.SaddleBrown, null, FontStyle.Italic);
            styleRegistry = new TextStyle(Brushes.DarkRed, null, FontStyle.Regular);
            styleNumbers = new TextStyle(Brushes.DarkCyan, null, FontStyle.Regular);
        }
        public static void RefreshControlStyles( FastColoredTextBox fctxbBox, TextChangedEventArgs e)
        {
            Range range = fctxbBox.Range;
            range = (e == null ? fctxbBox.VisibleRange : e.ChangedRange);

            range.ClearStyle(AssemblerConfig.styleCompilerDirectives);
            range.ClearStyle(AssemblerConfig.styleComment);
            range.ClearStyle(AssemblerConfig.styleNumbers);
            range.ClearStyle(AssemblerConfig.styleCommonInstruction);
            range.ClearStyle(AssemblerConfig.styleJumpInstruction);
            range.ClearStyle(AssemblerConfig.styleRegistry);
            range.ClearStyle(AssemblerConfig.styleStackInstruction);
            fctxbBox.ClearStylesBuffer();

            //keep style order! highest priority has first style added.
            range.SetStyle(AssemblerConfig.styleComment, AssemblerConfig.regexComment, RegexOptions.Multiline);
            range.SetStyle(AssemblerConfig.styleRegistry, AssemblerConfig.regexRegistry, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            range.SetStyle(AssemblerConfig.styleNumbers, AssemblerConfig.regexNumbers, RegexOptions.Multiline);
            range.SetStyle(AssemblerConfig.styleCompilerDirectives, AssemblerConfig.regexCompilerDirectives, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            range.SetStyle(AssemblerConfig.styleCommonInstruction, AssemblerConfig.regexCommonInstruction, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            range.SetStyle(AssemblerConfig.styleJumpInstruction, AssemblerConfig.regexJumpInstruction, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            range.SetStyle(AssemblerConfig.styleStackInstruction, AssemblerConfig.regexStackInstruction, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }
        #endregion
    }
}
