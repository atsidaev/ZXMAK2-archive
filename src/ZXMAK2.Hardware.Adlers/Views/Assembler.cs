using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using FastColoredTextBoxNS;
using System.Diagnostics;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ZXMAK2.Hardware.Adlers.Views
{
    public partial class Assembler : Form
    {
        [DllImport(@"Pasmo2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compile")]
        private unsafe static extern int compile( 
                  /*char**/ [MarshalAs(UnmanagedType.LPStr)] string compileArg,   //e.g. --bin, --tap; terminated by NULL(0)
                  /*char**/ [MarshalAs(UnmanagedType.LPStr)] string inAssembler,
	              /*char**/ IntPtr compiledOut,
                  /*int* */ IntPtr codeSize,
                  /*int**/  IntPtr errFileLine,
                  /*char**/ IntPtr errFileName,
                  /*char**/ IntPtr errReason
                  );

        #region Syntax highlightning styles
            Style CommentStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
            Style CommonInstructionStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
            Style JumpInstructionStyle = new TextStyle(Brushes.DarkViolet, null, FontStyle.Regular);
            Style StackInstructionStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
            Style CompilerInstructionStyle = new TextStyle(Brushes.SaddleBrown, null, FontStyle.Italic);
            Style RegistryStyle = new TextStyle(Brushes.DarkRed, null, FontStyle.Regular);
            Style NumbersStyle = new TextStyle(Brushes.DarkCyan, null, FontStyle.Regular);
        #endregion

        //private byte tabSpace = 16; //how many characters on tab

        //assembler sources array
        private int _actualAssemblerNode = 0;
        private Dictionary<int, AssemblerSourceInfo> _assemblerSources = new Dictionary<int, AssemblerSourceInfo>();

        private IDebuggable m_spectrum;

        private bool compileFromFile = false; //if loaded from file then "--binfile" compile parameter will be used
        
        //colors
        private static AssemblerColorConfig _ColorConfig;
        //instance(this)
        private static Assembler m_instance = null;
        private Assembler(ref IDebuggable spectrum)
        {
            m_spectrum = spectrum;

            InitializeComponent();

            txtAsm.DoCaretVisible();
            txtAsm.IsChanged = false;
            txtAsm.ClearUndo();

            txtAsm.SelectionLength = 0;
            txtAsm.SelectionStart = txtAsm.Text.Length + 1;

            //register assembler source(noname.asm), will have Id = 0
            treeViewFiles.Nodes[0].Tag = (int)0;
            _assemblerSources.Add(0, new AssemblerSourceInfo("noname.asm", false));

            //colors
            _ColorConfig = new AssemblerColorConfig(this);

            this.KeyPreview = true;
            this.BringToFront();
        }

        public static void Show(ref IDebuggable spectrum)
        {
            if (m_instance == null || m_instance.IsDisposed)
            {
                m_instance = new Assembler(ref spectrum);
                m_instance.LoadConfig();
                m_instance.ShowInTaskbar = true;
                m_instance.Show();
            }
            else
                m_instance.Show();

            m_instance.txtAsm.Focus();
        }

        public static Assembler GetInstance()
        {
            return m_instance;
        }

        /*internal unsafe struct FixedBuffer
        {
            public fixed byte compiledOut[0xFFFF + 1];
            public fixed byte errMessage[1024];
            public int     codeSize;
        }*/

        private void compileToZ80()
        {
            int retCode = -1;

            if (txtAsm.Text.Trim().Length < 2)
            {
                this.richCompileMessages.Text = DateTime.Now.ToLongTimeString() + ": Nothing to compile...\n===================\n" + this.richCompileMessages.Text;
                return;
            }

            if(validateCompile() == false)
                return;

            unsafe
            {
                //FixedBuffer fixedBuf = new FixedBuffer();

                string  asmToCompileOrFileName = String.Empty;
                byte[]  compiledOut = new byte[65536-16384 + 2/*memory start when --binfile is used*/];
                byte[]  errReason = new byte[1024];
                int     codeSize = 0;
                int     errFileLine = 0;
                byte[]  errFileName = new byte[512];

                string compileOption;
                if (compileFromFile /*|| (!checkMemory.Checked && IsStartAdressInCode())*/)
                {
                    asmToCompileOrFileName = _assemblerSources[_actualAssemblerNode].GetFileNameToSave();
                    compileOption = "--binfile";
                    //Set the current directory so that the compiler could find also include files(in the same dir as compiled source)
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(asmToCompileOrFileName));
                }
                else
                {
                    /*if( chckbxMemory.Checked )
                        asmToCompileOrFileName += "org " + textMemAdress.Text + "\n";*/

                    asmToCompileOrFileName += txtAsm.Text;
                    compileOption = "--bin";
                }

                fixed (byte* pcompiledOut = &compiledOut[0])
                {
                    fixed (byte* perrReason = &errReason[0])
                    {
                        fixed (byte* perrFileName = &errFileName[0])
                        {
                            string errStringText = DateTime.Now.ToLongTimeString() + ": Compiling...\n";
                            this.richCompileMessages.Text = errStringText + this.richCompileMessages.Text;

                            try
                            {
                                if (LoadLibrary(Path.Combine(Utils.GetAppFolder(), "Pasmo2.dll"))==IntPtr.Zero)
                                {
                                    Locator.Resolve<IUserMessage>()
                                        .Error("Cannot load Pasmo2.dll...\n\nTrying to download it again.");

                                    File.Delete(Path.Combine(Utils.GetAppFolder(), "Pasmo2.dll"));

                                    TcpHelper client = new TcpHelper();
                                    client.Show();

                                    return;
                                }
                                retCode = compile(compileOption, asmToCompileOrFileName, new IntPtr(pcompiledOut),
                                                  new IntPtr(&codeSize), new IntPtr(&errFileLine),
                                                  new IntPtr(perrFileName), new IntPtr(perrReason)
                                                  );
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                                Locator.Resolve<IUserMessage>().Error( "Technical error in compilation...\nSorry, compilation cannot be executed.");
                                return;
                            }
                            if (retCode != 0)
                            {
                                if (compileOption == "--binfile")
                                {
                                    errStringText += "Error on line " + errFileLine.ToString() + ", file: " + GetStringFromMemory(perrFileName);
                                    errStringText += "\n    ";
                                    errStringText += GetStringFromMemory(perrReason);
                                }
                                else
                                    errStringText += String.Format("Compile error on line {0}!\n    {1}", errFileLine, GetStringFromMemory(perrReason));

                                this.richCompileMessages.Text = errStringText + "\n===================\n" + this.richCompileMessages.Text;
                            }
                            else
                            {
                                //we got a assembly
                                this.richCompileMessages.Text = DateTime.Now.ToLongTimeString() + ": Compilation OK ! Now writing memory...";

                                //write to memory ?
                                //if (checkMemory.Checked)
                                if( codeSize > 0 )
                                {
                                    //get address where to write the code
                                    ushort memAdress = 0;
                                    ushort memArrayDelta = 2;
                                    if (compileOption != "--bin" || !chckbxMemory.Checked) //binary
                                        memAdress = (ushort)(compiledOut[0] + compiledOut[1] * 256);
                                    else
                                    {
                                        //--bin mode
                                        memAdress = ConvertRadix.ConvertNumberWithPrefix(textMemAdress.Text);
                                        //memArrayDelta = 0;
                                    }

                                    if (memAdress == 0 && this.chckbxMemory.Checked)
                                        memAdress = ConvertRadix.ConvertNumberWithPrefix(this.textMemAdress.Text);
                                    if (memAdress >= 0x4000) //RAM start
                                    {
                                        Stopwatch watch = new Stopwatch();
                                        watch.Start();
                                        m_spectrum.WriteMemory(memAdress, compiledOut, memArrayDelta, codeSize);
                                        watch.Stop();

                                        TimeSpan time = watch.Elapsed;
                                        this.richCompileMessages.Text += String.Format("\n    Memory written at start address: #{0:X04}({1})", memAdress, memAdress);
                                        this.richCompileMessages.Text += String.Format("\n    Written #{0:X04}({1}) bytes", codeSize, codeSize);
                                    }
                                    else
                                    {
                                        this.richCompileMessages.Text = "\n    Cannot write to ROM(address = " + memAdress.ToString() + "). Bail out." + this.richCompileMessages.Text;
                                        return;
                                    }
                                }
                                else
                                    this.richCompileMessages.Text = "\n    Nothing to write to memory !" + this.richCompileMessages.Text;
                            }   
                        }
                    }
                }
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string fileName);

        private bool validateCompile()
        {
            bool startAdressManual = chckbxMemory.Checked;
            bool startAdressInCode = this.IsStartAdressInCode();

            Directory.SetCurrentDirectory(Utils.GetAppFolder());

            if (!File.Exists(@"Pasmo2.dll"))
            {
                Locator.Resolve<IUserMessage>().Error(
                    "Pasmo2.dll not found in " + Utils.GetAppFolder() + "!\n\nThis file is needed for compilation\n" +
                    "into Z80 code." +
                    "\n\n" +
                    "Now going to try to get it from internet.\nPlease click OK."
                    );

                TcpHelper client = new TcpHelper();
                client.Show();

                return false;
            }

            if (startAdressInCode == false && startAdressManual == false)
            {
                //start adress for compilation not found
                Locator.Resolve<IUserMessage>()
                    .Warning(
                        "Compilation failed(missing start address)\n\n" +
                        "Either check the check box for memory address(Compile to -> Memory)\n" + 
                        "or define it using 'ORG' instruction in source code !\n\n" +
                        "Compilation is canceled.");
                return false;
            }
            if (startAdressInCode && startAdressManual)
            {
                //duplicate adress for compilation
                /*Locator.Resolve<IUserMessage>().Warning(
                    "Compilation failed(duplicity in start address)\n\n" +
                    "Either UNcheck the check box for memory address(Compile to -> Memory)\n" +
                    "or remove ALL 'ORG' instructions from the source code !\n\n" +
                    "Compilation is canceled.");
                return false;*/
                //org has higher priority
                this.checkMemory_CheckedChanged(null, null);
            }

            return true;
        }

        private bool IsStartAdressInCode()
        {
            foreach (string line in this.txtAsm.Lines)
            {
                string toCheck = line.Split(';')[0].Trim(); //remove comments
                Match match = Regex.Match(toCheck, @"\borg\b", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return true;
                }
            }
            return false;
        }

        static unsafe private string GetStringFromMemory(byte* i_pointer)
        {
            string retString = String.Empty;

            for (; ; )
            {
                char c = (char)*(i_pointer++);
                if (c == '\0')
                    break;

                retString += c;
            }

            return retString;
        }

        #region GUI methods
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void checkMemory_CheckedChanged(object sender, EventArgs e)
        {
            textMemAdress.Enabled = chckbxMemory.Checked;
        }

        private void assemblerForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                compileToZ80();
                return;
            }

            if( e.KeyCode == Keys.O && e.Control )
            {
                openFileStripButton_Click(null, null);
            }

            if (e.KeyCode == Keys.S && e.Control)
            {
                saveFileStripButton_Click(null, null);
            }
        }

        //Compile Button
        private void btnCompile_Click(object sender, EventArgs e)
        {
            compileToZ80();
        }
        private void compileToolStrip_Click(object sender, EventArgs e)
        {
            compileToZ80();
        }

        //Select font
        /*private void fonttoolStrip_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            fontDialog.Font = txtAsm.Font;

            DialogResult res = fontDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                Font font = fontDialog.Font;
                txtAsm.Font = font; //FastColoredTextBox supports only monospaced fonts !
            }
        }*/

        //text Color
        private void toolStripColors_Click(object sender, EventArgs e)
        {
            _ColorConfig.ShowDialog();
            //this.RefreshAssemblerCode();
        }
        //refresh assembler text when color styles changed
        public void RefreshAssemblerCode()
        {
            if (_ColorConfig != null)
            {
                Range range = txtAsm.VisibleRange;
                range.ClearStyle(CommentStyle);

                CommentStyle = _ColorConfig.CommentStyle;
                range.SetStyle(CommentStyle, @";.*$", RegexOptions.Multiline);
            }
        }

        //open file
        private void openFileStripButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog loadDialog = new OpenFileDialog())
            {
                loadDialog.InitialDirectory = ".";
                loadDialog.SupportMultiDottedExtensions = true;
                loadDialog.Title = "Load file...";
                loadDialog.Filter = "Assembler files (asm,txt)|*.asm;*.txt|All files (*.*)|*.*";
                loadDialog.DefaultExt = "";
                loadDialog.FileName = "";
                loadDialog.ShowReadOnly = false;
                loadDialog.CheckFileExists = true;
                if (loadDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                string fileText;
                if (LoadAsm(loadDialog.FileName, out fileText) == false)
                    return;

                TreeNode node = new TreeNode(Path.GetFileName(loadDialog.FileName));
                AssemblerSourceInfo sourceInfo = new AssemblerSourceInfo(loadDialog.FileName, true);
                node.ToolTipText = loadDialog.FileName;
                node.Checked = true;
                node.Tag = sourceInfo.Id = _actualAssemblerNode = SourceInfo_ActualMax();

                this.txtAsm.Text = sourceInfo.SourceCode = fileText;
                _assemblerSources.Add(sourceInfo.Id, sourceInfo);

                treeViewFiles.Nodes.Add(node);
                treeViewFiles.SelectedNode = node;

                compileFromFile = true;
            }
        }

        private void txtAsm_TextChanged(object sender, TextChangedEventArgs e)
        {
            //clear styles
            /*e.ChangedRange.ClearStyle(CommonInstructionStyle);
            e.ChangedRange.ClearStyle(JumpInstructionStyle);
            e.ChangedRange.ClearStyle(StackInstructionStyle);
            e.ChangedRange.ClearStyle(RegistryStyle);
            e.ChangedRange.ClearStyle(CompilerInstructionStyle);
            e.ChangedRange.ClearStyle(NumbersStyle);
            e.ChangedRange.ClearStyle(CommentStyle);*/
            txtAsm.ClearStylesBuffer();

            e.ChangedRange.SetStyle(CommentStyle, @";.*$", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(NumbersStyle, @"(?:\(|\n|,| )\d{1,5}\b|[^a-zA-Z](?:x|#|\$)[0-9A-Fa-f]{1,4}|%[0-1]{1,16}", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(CommonInstructionStyle, @"\bldir\b|\blddr\b|\bld\b|\bim\b|\badd\b|\bsub\b|\bdec\b|\bsbc\b|\bhalt\b|\bbit\b|" +
                @"\bset\b|xor|\binc(?! sp)\b|\bcp\b|\bcpl\b|\bei\b|\bdi\b|\band\b|\bor\b|\band\b" +
                @"|\brr\b|\bscf\b|\bccf\b|\bneg\b|\bsrl\b|exx|\bex\b|\brla\b|\brra\b|\brr\b|\bout\b|\bin\b|\bsla\b|\brl\b",
                RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(CompilerInstructionStyle, @"\bdefb\b|\bdefw\b|\bdefl\b|\bdefm\b|\bdefs\b|\bequ\b|\bmacro\b|\bendm\b|include|incbin|" +
                @"\bif\b|\bendif\b|\belse\b", 
                RegexOptions.Multiline | RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(StackInstructionStyle, @"\bpush\b|\bpop\b|\bdec sp\b|\binc sp\b", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(JumpInstructionStyle, @"\borg\b|\breti\b|\bretn\b|\bret\b|\bjp\b|\bjr\b|\bcall\b|\bdjnz\b", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(RegistryStyle, @"\bhl\b|\bbc\b|\bix\b|\biy\b|\bde\b|\bpc\b|\baf\b|\bsp\b", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        //Save button
        private void saveFileStripButton_Click(object sender, EventArgs e)
        {
            SaveAsm(_assemblerSources[_actualAssemblerNode].GetFileNameToSave());
            _assemblerSources[_actualAssemblerNode].SourceCode = txtAsm.Text;
        }

        //Refresh button
        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            string dummy;
            LoadAsm(_assemblerSources[_actualAssemblerNode].GetFileNameToSave(), out dummy);
        }

        //Form close
        private void Assembler_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        //Clear compilation log
        private void buttonClearAssemblerLog_Click(object sender, EventArgs e)
        {
            this.richCompileMessages.Clear();
        }

        //Sources treeview select
        private void treeViewFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _actualAssemblerNode = ConvertRadix.ParseUInt16(e.Node.Tag.ToString(), 10);
            this.txtAsm.Text = _assemblerSources[_actualAssemblerNode].SourceCode;
        }
        #endregion

        private bool LoadAsm(string i_fileName, out string o_strFileText)
        {
            o_strFileText = string.Empty;

            if (i_fileName == String.Empty || i_fileName == null)
                return false;

            try
            {
                FileInfo fileInfo = new FileInfo(i_fileName);
                int s_len = (int)fileInfo.Length;

                byte[] data = new byte[s_len];
                using (FileStream fs = new FileStream(i_fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    fs.Read(data, 0, data.Length);
                string asmCode = Encoding.UTF8.GetString(data, 0, data.Length);
                o_strFileText = asmCode;
                if (IsStartAdressInCode())
                    this.chckbxMemory.Checked = false;
                checkMemory_CheckedChanged(null, null);

                if (this.richCompileMessages.Text.Trim() != String.Empty)
                    this.richCompileMessages.Text += "\n\n";

                this.richCompileMessages.Text += "File " + i_fileName + " read successfully..";
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                this.richCompileMessages.Text += "\n\nFile " + i_fileName + " read ERROR!";
                return false;
            }
        }

        private bool SaveAsm(string i_fileName)
        {
            if (i_fileName == String.Empty)
                i_fileName = "code_save.asm";
            File.WriteAllText(Path.Combine(Utils.GetAppFolder(),i_fileName), this.txtAsm.Text);

            Locator.Resolve<IUserMessage>().Info("File " + i_fileName + " saved!");
            return true;
        }

        #region Source management(add/delete/save/refresh)
        class AssemblerSourceInfo
        {
            private int _id;
            public int Id{get { return this._id; } set{ if(value >= 0) this._id = value; }}

            public string SourceCode{ get; set; }

            private bool IsFile { get; set; }
            private bool IsSaved { get; set; }
            private string SourceName { get; set; } //empty when it is a file

            private string _fileName;

            public AssemblerSourceInfo(string i_sourceName, bool i_isFile)
            {
                SourceName = i_sourceName;
                IsFile = i_isFile;
                IsSaved = true;
                _fileName = SourceCode = string.Empty;
            }

            public string GetFileNameToSave()
            {
                if (_fileName == string.Empty || !IsFile)
                    return SourceName;
                else
                    return _fileName;
            }

            public void SetSourceNameOrFilename(string i_newName)
            {
                if (_fileName == string.Empty || !IsFile) //if it is not file
                    SourceName = i_newName;
                else
                    _fileName = i_newName;
            }
        }

        private int AddNewSource(AssemblerSourceInfo i_sourceCandidate)
        {
            i_sourceCandidate.Id = SourceInfo_ActualMax();
            _assemblerSources.Add(i_sourceCandidate.Id, i_sourceCandidate);

            return i_sourceCandidate.Id;
        }
        private void RemoveSource(int i_sourceIndex)
        {
            if( _assemblerSources != null && _assemblerSources.Count > 0 )
                _assemblerSources.Remove(i_sourceIndex);
            if (_assemblerSources.Count > 0)
            {
                _actualAssemblerNode = 0;
                treeViewFiles.SelectedNode = treeViewFiles.Nodes[0];
            }
        }
        private int SourceInfo_ActualMax()
        {
            return _assemblerSources.Max(p => p.Key) + 1;
        }

        //treeViewFiles: KeyUp
        private void treeViewFiles_KeyUp(object sender, KeyEventArgs e)
        {
            if( e.KeyCode == Keys.Delete && treeViewFiles.Nodes.Count > 0 )
            {
                var node = treeViewFiles.SelectedNode;
                if (node != null)
                {
                    RemoveSource((int)node.Tag);
                    treeViewFiles.Nodes.Remove(node);
                }
            }
        }
        //treeViewFiles: AfterLabelEdit
        private void treeViewFiles_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            TreeNode node = treeViewFiles.SelectedNode;
            if( node != null )
            {
                int index = (int)node.Tag;
                AssemblerSourceInfo sourceInfo;
                if( _assemblerSources != null && _assemblerSources.TryGetValue(index, out sourceInfo) )
                {
                    sourceInfo.SetSourceNameOrFilename(e.Label);
                }

                _actualAssemblerNode = index;
            }
        }
        #endregion

        #region Config
        public AssemblerColorConfig GetColors()
        {
            return _ColorConfig;
        }
        public void GetPartialConfig(ref XmlWriter io_writer)
        {
            if (m_instance == null)
                return;
            AssemblerColorConfig colors = GetInstance().GetColors();

            //Assembler root
            io_writer.WriteStartElement("Assembler");
            io_writer.WriteStartElement("Colors");
                //Colors->Comments
                io_writer.WriteStartElement("CommentStyle");
                io_writer.WriteAttributeString("TextColor", colors.CommentStyle.GetCSS());
                //io_writer.WriteElementString("Value", this.textBoxOpcode.Text);
                io_writer.WriteEndElement();

            io_writer.WriteEndElement(); //Colors end
            io_writer.WriteEndElement(); //Assembler end
        }
        public void LoadConfig()
        {
            if (!File.Exists(Path.Combine(Utils.GetAppFolder(), FormCpu.ConfigXmlFileName)))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(Utils.GetAppFolder(), FormCpu.ConfigXmlFileName));
            XmlNode node = xmlDoc.DocumentElement.SelectSingleNode("/Root/Assembler/Colors/CommentStyle");
            if (node != null)
            {
                string css = node.Attributes["TextColor"].InnerText;
                Color commentColor = ParseCss_GetColor(css);
                FontStyle fontStyle = new FontStyle();
                if (ParseCss_IsItalic(css))
                    fontStyle |= FontStyle.Italic;
                if (ParseCss_IsBold(css))
                    fontStyle |= FontStyle.Bold;
                if (ParseCss_IsUnderline(css))
                    fontStyle |= FontStyle.Underline;
                if (ParseCss_IsStrikeout(css))
                    fontStyle |= FontStyle.Strikeout;
                GetInstance().GetColors().ChangeCommentsStyle(new TextStyle(new SolidBrush(commentColor), null, fontStyle));
            }
        }
        private Color ParseCss_GetColor(string i_cssString)
        {
            Regex regex = new Regex(@"(?!color:)#[0-9a-fA-F]{6}");
            Match match = regex.Match(i_cssString);
            if (match.Success)
            {
                return ColorTranslator.FromHtml(match.Value);
            }
            return Color.Black;
        }
        private bool ParseCss_IsItalic(string i_cssString)
        {
            //;font-style:oblique;
            return i_cssString.Contains(";font-style:oblique;");
        }
        private bool ParseCss_IsBold(string i_cssString)
        {
            //;font-weight:bold;
            return i_cssString.Contains(";font-weight:bold;");
        }
        private bool ParseCss_IsUnderline(string i_cssString)
        {
            //;text-decoration:underline;
            return i_cssString.Contains(";text-decoration:underline;");
        }
        private bool ParseCss_IsStrikeout(string i_cssString)
        {
            //;text-decoration:line-through;
            return i_cssString.Contains(";text-decoration:line-through;");
        }
        #endregion
    }
}
