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
using ZXMAK2.Hardware.Adlers.Views.CustomControls;

namespace ZXMAK2.Hardware.Adlers.Views.AssemblerView
{
    public partial class Assembler : Form
    {
        [DllImport(@"Pasmo2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compile")]
        private unsafe static extern int compile(
                  /*char**/ [MarshalAs(UnmanagedType.LPStr)] string compileArg,   //e.g. --bin, --tap; terminated by NULL(0); generate symbol table: --<mode> <input> <output> <symbol_table_filename>
                  /*char**/ [MarshalAs(UnmanagedType.LPStr)] string inAssembler,
	              /*char**/ IntPtr compiledOut,
                  /*int* */ IntPtr codeSize,
                  /*int**/  IntPtr errFileLine,
                  /*char**/ IntPtr errFileName,
                  /*char**/ IntPtr errReason
                  );

        private byte _tabSpace = 16; //how many characters on tab

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
                this.richCompileMessages.AppendLog( "Nothing to compile...", LOG_LEVEL.Info, true/*log time*/);
                return;
            }

            if(validateCompile() == false)
                return;

            unsafe
            {
                //FixedBuffer fixedBuf = new FixedBuffer();

                string  asmToCompileOrFileName = String.Empty;
                byte[]  compiledOut = new byte[65536/*big as ROM+RAM - security reason*/];
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
                            string errStringText = String.Empty;
                            this.richCompileMessages.AppendLog("Compiling...\n", LOG_LEVEL.Error, true);

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
                                {
                                    string errMessageTrimmed = GetStringFromMemory(perrReason);
                                    ConvertRadix.RemoveFormattingChars(ref errMessageTrimmed, true/*trim start*/);
                                    errStringText += String.Format("Compile error on line {0}!\n    {1}", errFileLine, errMessageTrimmed.TrimStart());
                                }

                                LOG_INFO logInfo = new LOG_INFO();
                                logInfo.ErrorLine = errFileLine; logInfo.SetMessage( errStringText ); logInfo.Level = LOG_LEVEL.Error;
                                this.richCompileMessages.AppendLog(logInfo);
                            }
                            else
                            {
                                //we got a assembly
                                this.richCompileMessages.AppendLog(DateTime.Now.ToLongTimeString() + ": Compilation OK ! Now writing memory...", LOG_LEVEL.Info);

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
                                        try
                                        {
                                            memAdress = ConvertRadix.ConvertNumberWithPrefix(textMemAdress.Text);
                                        }
                                        catch(CommandParseException exc)
                                        {
                                            Locator.Resolve<IUserMessage>().Error(String.Format("Incorrect memory address!\n{0}", exc.Message));
                                            return;
                                        }
                                    }

                                    if (memAdress == 0 && this.chckbxMemory.Checked)
                                    {
                                        try
                                        {
                                            memAdress = ConvertRadix.ConvertNumberWithPrefix(textMemAdress.Text);
                                        }
                                        catch(CommandParseException exc)
                                        {
                                            Locator.Resolve<IUserMessage>().Error(String.Format("Incorrect memory address!\n{0}", exc.Message));
                                            return;
                                        }
                                    }
                                    if (memAdress >= 0x4000) //RAM start
                                    {
                                        Stopwatch watch = new Stopwatch();
                                        watch.Start();
                                        if ((memAdress + codeSize) > 0xFFFF)
                                            codeSize = 0xFFFF - memAdress; //prevent memory overload
                                        m_spectrum.WriteMemory(memAdress, compiledOut, memArrayDelta, codeSize);
                                        watch.Stop();

                                        TimeSpan time = watch.Elapsed;
                                        string compileInfo = String.Format("\n    Memory written at start address: #{0:X04}({1})", memAdress, memAdress);
                                        compileInfo += String.Format("\n    Written #{0:X04}({1}) bytes", codeSize, codeSize);

                                        this.richCompileMessages.AppendLog(compileInfo, LOG_LEVEL.Info);
                                    }
                                    else
                                    {
                                        this.richCompileMessages.AppendLog("Cannot write to ROM(address = " + memAdress.ToString() + "). Bail out.", LOG_LEVEL.Error);
                                        return;
                                    }
                                }
                                else
                                    this.richCompileMessages.AppendLog("Nothing to write to memory !", LOG_LEVEL.Info);
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
                txtAsm.ClearStylesBuffer();

                Range range = txtAsm.VisibleRange;
                range.ClearStyle(AssemblerConfig.styleComment);

                //AssemblerConfig.styleComment = _ColorConfig.CommentStyle;
                range.SetStyle(AssemblerConfig.styleComment, AssemblerConfig.regexComment, RegexOptions.Multiline);

                //AssemblerConfig.styleCompilerDirectives = _ColorConfig.CompilerDirectivesStyle;
                range.SetStyle(AssemblerConfig.styleCompilerDirectives, AssemblerConfig.regexCompilerDirectives, RegexOptions.Multiline | RegexOptions.IgnoreCase);
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
                node.Tag = sourceInfo.Id = _actualAssemblerNode = SourceInfo_GetNextId();

                this.txtAsm.Text = sourceInfo.SourceCode = fileText;
                _assemblerSources.Add(sourceInfo.Id, sourceInfo);

                treeViewFiles.Nodes.Add(node);
                treeViewFiles.SelectedNode = node;

                compileFromFile = true;
            }
        }

        private void txtAsm_TextChanged(object sender, TextChangedEventArgs e)
        {
            AssemblerConfig.RefreshControlStyles(txtAsm);
        }

        //Save button
        private void saveFileStripButton_Click(object sender, EventArgs e)
        {
            AssemblerSourceInfo sourceInfo = _assemblerSources[_actualAssemblerNode];
            //save as if not a file
            if (!sourceInfo.IsFile())
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = "Save assembler code";
                DialogResult dialogResult = saveDialog.ShowDialog();
                if (saveDialog.FileName != String.Empty && dialogResult == DialogResult.OK)
                {
                    sourceInfo.SetIsFile();
                    sourceInfo.SetSourceNameOrFilename(saveDialog.FileName);

                    //change name of actual source
                    TreeNode node = treeViewFiles.Nodes[_actualAssemblerNode];
                    node.Text = sourceInfo.GetDisplayName();
                }
                else
                {
                    Locator.Resolve<IUserMessage>().Warning("File NOT saved!");
                    return;
                }
            }

            SaveAsm(sourceInfo.GetFileNameToSave());
            sourceInfo.SourceCode = txtAsm.Text;
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
            this.richCompileMessages.ClearLog();
        }

        //Sources treeview select
        private void treeViewFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _actualAssemblerNode = ConvertRadix.ParseUInt16(e.Node.Tag.ToString(), 10);
            this.txtAsm.Text = _assemblerSources[_actualAssemblerNode].SourceCode;
        }

        //Z80 source code(Libs)
        private void toolCodeLibrary_Click(object sender, EventArgs e)
        {
            Z80AsmResources resources = new Z80AsmResources(ref this.txtAsm);
            resources.ShowDialog();
        }

        //Context menu - Assembler commands(visual)
        private void txtAsm_MouseClick(object sender, MouseEventArgs e)
        {
            if( e.Button == System.Windows.Forms.MouseButtons.Right )
                ctxMenuAssemblerCommands.Show(txtAsm, e.Location);
        }

        //Format code
        private void btnFormatCode_Click(object sender, EventArgs e)
        {
            //ToDo: we need a list of all assembler commands here; Regex rgx = new Regex(@"\b(?!push|djnz)\b[ ]+\b", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] opcodes = new string[] { "ld", "org", "push", "ex", "call", "inc", "pop", "sla", "ldir", "djnz", "ret", "add", "adc", "and", "sub", "xor", "jr", "jp", "exx",
                                              "dec", "srl", "scf", "ccf", "di", "ei", "im", "or", "cpl", "out", "in", "cp", "reti", "retn", "rra", "rla", "sbc", "rst",
                                              "rlca", "rrc", "res", "set", "bit", "halt", "cpd", "cpdr", "cpi", "cpir", "cpl", "daa", "equ", "rrca" };
            string[] strAsmLines = txtAsm.Lines.ToArray<string>();

            string codeFormatted = String.Empty;
            foreach(string line in strAsmLines)
            {
                bool isNewLine = true; bool isInComment = false; bool addNewlineAtLineEnd = true;

                string[] lineSplitted = Regex.Split(line, @"\s+", RegexOptions.IgnoreCase);
                foreach (string strToken in lineSplitted)
                {
                    if (strToken == String.Empty)
                    {
                        if (lineSplitted.Length == 1)
                        {
                            codeFormatted += "\n";
                            addNewlineAtLineEnd = false;
                        }
                        continue;
                    }
                    if (strToken.StartsWith(";"))
                    {
                        if (!isInComment)
                        {
                            isInComment = true;
                            codeFormatted += strToken + " ";
                            continue;
                        }
                        isInComment = false;
                    }
                    if (isInComment)
                    {
                        codeFormatted += strToken + " ";
                        continue;
                    }

                    if (opcodes.Contains(strToken.ToLower()))
                    {
                        if (isNewLine)
                            codeFormatted += new String(' ', _tabSpace);
                        codeFormatted += strToken + new String(' ', 6 - strToken.Length);
                    }
                    else
                    {
                        //label, compiler directive, ...
                        int spacesAfter = 1;
                        if (_tabSpace > strToken.Length && isNewLine)
                            spacesAfter = _tabSpace - strToken.Length;
                        codeFormatted += strToken + new String(' ', spacesAfter);
                    }

                    isNewLine = false;
                }
                codeFormatted = codeFormatted.TrimEnd(' ');
                if (addNewlineAtLineEnd)
                    codeFormatted += "\n";
                isInComment = false;
            }
            txtAsm.Text = codeFormatted;
        }

        private void richCompileMessages_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LOG_INFO logInfo = richCompileMessages.GetCurrentMessage();
            if (logInfo != null && logInfo.ErrorLine != -1 && logInfo.ErrorLine-1 < txtAsm.LinesCount)
            {
                Range range = new Range(txtAsm, logInfo.ErrorLine-1);
                txtAsm.DoRangeVisible(range);
            }
            //Locator.Resolve<IUserMessage>().Info(String.Format("Current error line: {0}", logInfo.ErrorLine));
        }
        #endregion GUI

        #region File operations
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
        #endregion File operations

        #region Source management(add/delete/save/refresh)
            class AssemblerSourceInfo
        {
            private int _id;
            public int Id{get { return this._id; } set{ if(value >= 0) this._id = value; }}

            public string SourceCode{ get; set; }

            private  bool _isFile { get; set; }
            private bool IsSaved { get; set; }
            private string SourceName { get; set; } //empty when it is a file

            private string _fileName;

            public AssemblerSourceInfo(string i_sourceName, bool i_isFile)
            {
                SourceName = i_sourceName;
                _isFile = i_isFile;
                IsSaved = true;
                _fileName = SourceCode = string.Empty;
            }

            public string GetFileNameToSave()
            {
                if (_fileName == string.Empty || !_isFile)
                    return SourceName;
                else
                    return _fileName;
            }
            public string GetSourceName()
            {
                return SourceName;
            }
            public string GetDisplayName()
            {
                if (SourceName.IndexOf(Path.DirectorySeparatorChar) != -1)
                {
                    string[] splitted = SourceName.Split(Path.DirectorySeparatorChar);
                    return splitted[splitted.Length-1];
                }
                return SourceName;
            }
            public bool IsFile()
            {
                return _isFile;
            }
            public void SetIsFile()
            {
                _isFile = true;
            }

            public void SetSourceNameOrFilename(string i_newName)
            {
                if (!_isFile) //if it is not file
                    SourceName = i_newName;
                else
                {
                    int lastIndex = _fileName.LastIndexOf(Path.DirectorySeparatorChar);
                    //if it is a file then we remember the file path
                    if (lastIndex != -1)
                    {
                        _fileName = _fileName.Substring(0, lastIndex) + Path.DirectorySeparatorChar + i_newName;
                        SourceName = _fileName.Substring(0, lastIndex);
                    }
                    else
                        SourceName = _fileName = i_newName;
                }
            }
        }

        private int AddNewSource(AssemblerSourceInfo i_sourceCandidate)
        {
            i_sourceCandidate.Id = SourceInfo_GetNextId();
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
        private int SourceInfo_GetNextId()
        {
            return _assemblerSources.Max(p => p.Key) + 1;
        }
        private string GetNewSourceName()
        {
            for (int counter = 1; counter < int.MaxValue; counter++ )
            {
                string sourceNameCandidate = String.Format("noname{0}.asm", counter);
                if( _assemblerSources.Values.FirstOrDefault( p => p.GetSourceName() == sourceNameCandidate ) == null )
                    return sourceNameCandidate;
            }
            
            return string.Empty;
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

        //Create new source code
        private void toolStripNewSource_Click(object sender, EventArgs e)
        {
            string newSourceName = GetNewSourceName();
            
            //register node + source
            AssemblerSourceInfo newSource = new AssemblerSourceInfo(newSourceName, false);
            TreeNode node = new TreeNode();
            node.Text = newSource.GetSourceName();
            node.Tag = AddNewSource(newSource);
            treeViewFiles.Nodes.Add(node);
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
                    io_writer.WriteAttributeString("TextColor", AssemblerConfig.styleComment.GetCSS());
                    io_writer.WriteEndElement();  //Colors->Comments end

                    //Colors->CompilerDirectivesStyle
                    io_writer.WriteStartElement("CompilerDirectivesStyle");
                    io_writer.WriteAttributeString("TextColor", AssemblerConfig.styleCompilerDirectives.GetCSS());
                    io_writer.WriteEndElement();  //Colors->Compiler directives end

                    //Colors->JumpsStyle
                    io_writer.WriteStartElement("JumpsStyle");
                    io_writer.WriteAttributeString("TextColor", AssemblerConfig.styleJumpInstruction.GetCSS());
                    io_writer.WriteEndElement();  //Colors->Jumps end

            io_writer.WriteEndElement(); //Colors end
            io_writer.WriteEndElement(); //Assembler end
        }
        public void LoadConfig()
        {
            if (!File.Exists(Path.Combine(Utils.GetAppFolder(), FormCpu.ConfigXmlFileName)))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(Utils.GetAppFolder(), FormCpu.ConfigXmlFileName));

            //comments
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
                GetInstance().GetColors().ChangeSyntaxStyle(new TextStyle(new SolidBrush(commentColor), null, fontStyle), 0);
            }

            //compiler directive style
            node = xmlDoc.DocumentElement.SelectSingleNode("/Root/Assembler/Colors/CompilerDirectivesStyle");
            if (node != null)
            {
                string css = node.Attributes["TextColor"].InnerText;
                Color compilerDirectivesColor = ParseCss_GetColor(css);
                FontStyle fontStyle = new FontStyle();
                if (ParseCss_IsItalic(css))
                    fontStyle |= FontStyle.Italic;
                if (ParseCss_IsBold(css))
                    fontStyle |= FontStyle.Bold;
                if (ParseCss_IsUnderline(css))
                    fontStyle |= FontStyle.Underline;
                if (ParseCss_IsStrikeout(css))
                    fontStyle |= FontStyle.Strikeout;
                GetInstance().GetColors().ChangeSyntaxStyle(new TextStyle(new SolidBrush(compilerDirectivesColor), null, fontStyle), 1);
            }

            //jumps style
            node = xmlDoc.DocumentElement.SelectSingleNode("/Root/Assembler/Colors/JumpsStyle");
            if (node != null)
            {
                string css = node.Attributes["TextColor"].InnerText;
                Color jumpsStyleColor = ParseCss_GetColor(css);
                FontStyle fontStyle = new FontStyle();
                if (ParseCss_IsItalic(css))
                    fontStyle |= FontStyle.Italic;
                if (ParseCss_IsBold(css))
                    fontStyle |= FontStyle.Bold;
                if (ParseCss_IsUnderline(css))
                    fontStyle |= FontStyle.Underline;
                if (ParseCss_IsStrikeout(css))
                    fontStyle |= FontStyle.Strikeout;
                GetInstance().GetColors().ChangeSyntaxStyle(new TextStyle(new SolidBrush(jumpsStyleColor), null, fontStyle), 2);
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
