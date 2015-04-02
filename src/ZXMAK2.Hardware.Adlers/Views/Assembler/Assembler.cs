using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using FastColoredTextBoxNS;
using System.Diagnostics;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Engine;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ZXMAK2.Hardware.Adlers.Views.CustomControls;
using ZXMAK2.Hardware.Adlers.Core;

namespace ZXMAK2.Hardware.Adlers.Views.AssemblerView
{
    public partial class Assembler : Form
    {
        private byte _tabSpace = 16; //how many characters on tab

        //assembler sources array
        private int _actualAssemblerNodeIndex = 0;
        private Dictionary<int, AssemblerSourceInfo> _assemblerSources = new Dictionary<int, AssemblerSourceInfo>();
        private readonly string _strDefaultAsmSourceCode = "org 40000\n\nret\n";

        //private IDebuggable m_spectrum;
        private FormCpu m_debugger;
       
        //colors
        private static AssemblerColorConfig _ColorConfig;
        //instance(this)
        private static Assembler m_instance = null;
        private Assembler(FormCpu spectrum)
        {
            m_debugger = spectrum;

            InitializeComponent();

            //Symbols list view
            listViewSymbols.View = View.Details;
            listViewSymbols.Columns.Add("Name      ", -2, HorizontalAlignment.Center);
            listViewSymbols.Columns.Add("Addr ", -2, HorizontalAlignment.Left);

            txtAsm.DoCaretVisible();
            txtAsm.IsChanged = false;
            txtAsm.ClearUndo();

            txtAsm.SelectionLength = 0;
            txtAsm.SelectionStart = txtAsm.Text.Length + 1;

            //register assembler source(noname.asm), will have Id = 0
            treeViewFiles.Nodes[0].Tag = (int)0;
            txtAsm.Text = _strDefaultAsmSourceCode; //must be here; will be used as source code for the new added source code
            AddNewSource(new AssemblerSourceInfo("noname.asm", false), _strDefaultAsmSourceCode);
            btnFormatCode_Click(null, null);

            //highlight colors
            _ColorConfig = new AssemblerColorConfig(this);

            this.KeyPreview = true;
            this.BringToFront();

            //print compiler version to form title
            double compilerVersion;
            if (Compiler.GetVersion(out compilerVersion) == 0)
                this.Text = "Assembler, " + String.Format("ver {0:0.##}", compilerVersion).Replace(',', '.');
        }

        public static void Show(FormCpu i_formCpu)
        {
            if (m_instance == null || m_instance.IsDisposed)
            {
                m_instance = new Assembler(i_formCpu);
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

        private void compileToZ80()
        {
            int retCode = -1;

            if (txtAsm.Text.Trim().Length < 2)
            {
                this.richCompileMessages.AppendLog( "Nothing to compile...", LOG_LEVEL.Info, true/*log time*/);
                return;
            }

            if(ValidateCompile() == false)
                return;

            unsafe
            {
                //FixedBuffer fixedBuf = new FixedBuffer();

                string  asmToCompileOrFileName = String.Empty;
                COMPILED_INFO compiled = new COMPILED_INFO();

                string compileOption;
                //if (compileFromFile /*|| (!checkMemory.Checked && IsStartAdressInCode())*/)
                //{
                //    asmToCompileOrFileName = _assemblerSources[_actualAssemblerNode].GetFileNameToSave();
                //    compileOption = "--binfile";
                //    //Set the current directory so that the compiler could find also include files(in the same dir as compiled source)
                //    Directory.SetCurrentDirectory(Path.GetDirectoryName(asmToCompileOrFileName));
                //}
                //else
                {
                    /*if( chckbxMemory.Checked )
                        asmToCompileOrFileName += "org " + textMemAdress.Text + "\n";*/
                    if (GetActualSourceInfo().IsFile())
                    {
                        asmToCompileOrFileName = GetActualSourceInfo().GetFileNameToSave();
                        compileOption = "--binfile";
                    }
                    else
                    {
                        asmToCompileOrFileName += txtAsm.Text;
                        compileOption = "--bin";
                    }
                }

                //fixed (byte* pcompiledOut = &compiledOut[0])
                {
                    /*fixed (*/
                    //char* perrReason = compiled.czErrMessage;
                    {
                        //fixed (byte* perrFileName = &errFileName[0])
                        {
                            string errStringText = String.Empty;
                            this.richCompileMessages.AppendLog("Compiling...\n", LOG_LEVEL.Info, true);

                            try
                            {
                                retCode = Compiler.DoCompile(compileOption, asmToCompileOrFileName, ref compiled);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                                Locator.Resolve<IUserMessage>().Error( "Technical error in compilation...\nSorry, compilation cannot be executed.\n\nDetail:\n" + ex.Message);
                                return;
                            }
                            if (retCode != 0)
                            {
                                if (compileOption == "--binfile")
                                {
                                    errStringText += "Error on line " + compiled.iErrFileLine.ToString() + ", file: " + Compiler.GetStringFromMemory(compiled.czErrFileName);
                                    errStringText += "\n    ";
                                    errStringText += Compiler.GetStringFromMemory(compiled.czErrMessage);
                                }
                                else
                                {
                                    string errMessageTrimmed = Compiler.GetStringFromMemory(compiled.czErrMessage);
                                    ConvertRadix.RemoveFormattingChars(ref errMessageTrimmed, true/*trim start*/);
                                    errStringText += String.Format("Compile error on line {0}!\n    {1}", compiled.iErrFileLine, errMessageTrimmed.TrimStart());
                                }

                                LOG_INFO logInfo = new LOG_INFO();
                                logInfo.ErrorLine = compiled.iErrFileLine; logInfo.SetMessage(errStringText); logInfo.Level = LOG_LEVEL.Error;
                                this.richCompileMessages.AppendLog(logInfo);
                            }
                            else
                            {
                                //we got a assembly
                                this.richCompileMessages.AppendLog(DateTime.Now.ToLongTimeString() + ": Compilation OK ! Now writing memory...", LOG_LEVEL.Info);

                                //write to memory ?
                                //if (checkMemory.Checked)
                                if ( compiled.iCompiledSize > 0)
                                {
                                    //get address where to write the code
                                    ushort memAdress = (ushort)(compiled.czCompiled[0] + compiled.czCompiled[1] * 256);

                                    /*if (memAdress == 0 && this.chckbxMemory.Checked)
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
                                    }*/
                                    if (memAdress >= 0x4000) //RAM start
                                    {
                                        Stopwatch watch = new Stopwatch();
                                        watch.Start();
                                        if ((memAdress + compiled.iCompiledSize) > 0xFFFF)
                                            compiled.iCompiledSize = 0xFFFF - memAdress; //prevent memory overload
                                        byte[] memBytesCompiled = ConvertRadix.PointerToManagedType(compiled.czCompiled + 2/*omit memory address*/, compiled.iCompiledSize);
                                        if( memBytesCompiled != null )
                                            m_debugger.GetVMKernel().WriteMemory(memAdress, memBytesCompiled, 0, compiled.iCompiledSize);
                                        watch.Stop();

                                        TimeSpan time = watch.Elapsed;
                                        string compileInfo = String.Format("\n    Memory written starting at address: #{0:X04}({1})", memAdress, memAdress);
                                        compileInfo += String.Format("\n    Written #{0:X04}({1}) bytes", compiled.iCompiledSize, compiled.iCompiledSize);

                                        this.richCompileMessages.AppendLog(compileInfo, LOG_LEVEL.Info);
                                    }
                                    else
                                    {
                                        this.richCompileMessages.AppendLog("Cannot write to ROM(address = " + memAdress.ToString() + "). Bail out.", LOG_LEVEL.Error);
                                        return;
                                    }

                                    //Symbols
                                    this.listViewSymbols.Items.Clear();
                                    if (new IntPtr(compiled.arrSourceSymbols) != IntPtr.Zero)
                                    {
                                        Dictionary<string, ushort> symbolsParsed = Compiler.ParseSymbols(Compiler.GetStringFromMemory(compiled.arrSourceSymbols));
                                        if (symbolsParsed != null && symbolsParsed.Keys.Count > 0)
                                        {
                                            foreach (var item in symbolsParsed)
                                            {
                                                ListViewItem itemToAdd = new ListViewItem(new[] { item.Key, String.Format("#{0:X2}", item.Value) });
                                                itemToAdd.Tag = String.Format("#{0:X2}", item.Value); //will be parsed in ListViewCustom
                                                //itemToAdd.Tag = tag;
                                                this.listViewSymbols.Items.Add(itemToAdd);
                                            }
                                        }
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

        private bool ValidateCompile()
        {
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

            if (Compiler.IsStartAdressInCode(txtAsm.Lines) == false)
            {
                //start adress for compilation not found
                Locator.Resolve<IUserMessage>().Error("Compile validation failed...\n\nMissing start address(ORG instruction).");
                return false;
            }

            return true;
        }

        #region GUI methods
        private bool _eventsDisabled = false;

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
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

                //new source code
                AssemblerSourceInfo sourceInfo = new AssemblerSourceInfo(loadDialog.FileName, true);
                sourceInfo.SetIsFile();
                sourceInfo.SetIsSaved(true);
                sourceInfo.SetSourceNameOrFilename(loadDialog.FileName);
                sourceInfo.SetSourceCode(fileText);
                _actualAssemblerNodeIndex = AddNewSource(sourceInfo);
                //new node
                TreeNode node = new TreeNode(Path.GetFileName(loadDialog.FileName));

                //add subitems(includes)
                List<string> includes = Compiler.ParseIncludes(loadDialog.FileName);
                if( includes != null )
                    foreach (string include in includes)
                    {
                        AssemblerSourceInfo includeInfo = new AssemblerSourceInfo(include, true);
                        includeInfo.SetIsFile();
                        includeInfo.SetIsSaved(true);
                        includeInfo.SetSourceNameOrFilename(include);
                        string includeFileContent;
                        bool retCode = FileTools.ReadFile(include, out includeFileContent);
                        if( retCode )
                            includeInfo.SetSourceCode(includeFileContent);

                        TreeNode incNode = new TreeNode(Path.GetFileName(include));
                        incNode.ToolTipText = include;
                        incNode.Checked = false;
                        incNode.Tag = AddNewSource(includeInfo);
                        node.Nodes.Add(incNode);
                    }
                node.ToolTipText = loadDialog.FileName;
                node.Checked = true;
                node.Tag = _actualAssemblerNodeIndex;
                treeViewFiles.Nodes.Add(node);
                //setting GUI
                txtAsm.Text = sourceInfo.GetSourceCode();
                _eventsDisabled = true;
                treeViewFiles.SelectedNode = node;
                _eventsDisabled = false;
            }
        }

        private void txtAsm_TextChanged(object sender, TextChangedEventArgs e)
        {
            AssemblerConfig.RefreshControlStyles(txtAsm, e);
        }

        //Save button
        private void saveFileStripButton_Click(object sender, EventArgs e)
        {
            AssemblerSourceInfo sourceInfo = _assemblerSources[_actualAssemblerNodeIndex];
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
                    TreeNode node = treeViewFiles.Nodes[_actualAssemblerNodeIndex];
                    node.Text = sourceInfo.GetDisplayName();
                }
                else
                {
                    Locator.Resolve<IUserMessage>().Warning("File NOT saved!");
                    return;
                }
            }

            SaveAsm(sourceInfo.GetFileNameToSave());
            sourceInfo.SetSourceCode( txtAsm.Text );
        }

        //Refresh button
        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            AssemblerSourceInfo info;
            _assemblerSources.TryGetValue(_actualAssemblerNodeIndex, out info);

            if (info != null)
            {
                string dummy;
                if (info.IsFile())
                    if(LoadAsm(info.GetFileNameToSave(), out dummy))
                    {
                        info.SetSourceCode(dummy);
                        info.SetIsSaved(true);
                        txtAsm.Text = dummy;
                    }

                RefreshFileList();
            }
        }
        //Refresh file list treenode
        private void RefreshFileList()
        {
            _eventsDisabled = true;
            if (_assemblerSources.Count == treeViewFiles.Nodes.Count)
            {
                foreach (TreeNode node in treeViewFiles.Nodes)
                {
                    node.Text = _assemblerSources[(int)node.Tag].GetDisplayName();
                }
            }
            _eventsDisabled = false;
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

        //Sources treeview before node changed
        private void treeViewFiles_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (_eventsDisabled)
                return;
            if (GetActualSourceInfo() != null)
            {
                AssemblerSourceInfo info = GetActualSourceInfo();
                if (info.IsSourceEqual(txtAsm.Text) == false)
                {
                    info.SetIsSaved(false);
                }
                info.SetSourceCode( txtAsm.Text );
            }
            if (treeViewFiles.SelectedNode != null)
                treeViewFiles.SelectedNode.Text = GetActualSourceInfo().GetDisplayName();
        }
        //Sources treeview after node changed
        private void treeViewFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_eventsDisabled)
                return;

            if (e.Node.Tag != null)
            {
                _actualAssemblerNodeIndex = ConvertRadix.ParseUInt16(e.Node.Tag.ToString(), 10);
                this.txtAsm.Text = _assemblerSources[_actualAssemblerNodeIndex].GetSourceCode();
            }
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

        //Context menu - Symbols(compiled by Pasmo2.dll)
        private void listViewSymbols_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                ctxmenuSymbols.Show(listViewSymbols, e.Location);
        }
        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewSymbols.SelectedItems.Count >= 1)
            {
                ListView.SelectedListViewItemCollection items = listViewSymbols.SelectedItems;
                foreach (ListViewItem item in items)
                    m_debugger.InsertCodeNote("; " + item.Text, ConvertRadix.ConvertNumberWithPrefix(item.Tag.ToString()));
            }
        }
        private void commentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewSymbols.SelectedItems.Count >= 1)
            {
                ListView.SelectedListViewItemCollection items = listViewSymbols.SelectedItems;
                foreach(ListViewItem item in items )
                    m_debugger.InsertCodeComment(item.Text, ConvertRadix.ConvertNumberWithPrefix(item.Tag.ToString()));
            }
        }

        //Format code
        private void btnFormatCode_Click(object sender, EventArgs e)
        {
            ProgressBarCustom progress = new ProgressBarCustom("Format code", "Formatting code....");
            progress.Init(() => FormatCode(() => progress.GetBackgroundWorker().ReportProgress(0, null)), txtAsm.Lines.Count);
            progress.Start();
        }

        private void FormatCode(Action actionIncCounter)
        {
            //ToDo: we need a list of all assembler commands here, but cannot use AssemblerConfig regex patterns
            string[] opcodes = new string[] { "ld", "org", "push", "ex", "call", "inc", "pop", "sla", "ldir", "djnz", "ret", "add", "adc", "and", "sub", "xor", "jr", "jp", "exx",
                                              "dec", "srl", "scf", "ccf", "di", "ei", "im", "or", "cpl", "out", "in", "cp", "reti", "retn", "rra", "rla", "sbc", "rst",
                                              "rlca", "rrc", "res", "set", "bit", "halt", "cpd", "cpdr", "cpi", "cpir", "cpl", "daa", "rrca", "rr"};
            string[] strAsmLines = txtAsm.Lines.ToArray<string>();
            Range actLineSave = new Range(txtAsm, txtAsm.Selection.Start, txtAsm.Selection.End);

            //step 1: add whitespace after each ',' and '('
            for (int lineCounter = 0; lineCounter < strAsmLines.Length; lineCounter++)
            {
                //while (Regex.IsMatch(strAsmLines[lineCounter], @"[,\(]\S+"))
                {
                    int index = strAsmLines[lineCounter].IndexOf(",");
                    while (index >= 0)
                    {
                        if (index < strAsmLines[lineCounter].Length - 1)
                        {
                            if (strAsmLines[lineCounter][index + 1] != ' ')
                                strAsmLines[lineCounter] = strAsmLines[lineCounter].Substring(0, index) + ", " + strAsmLines[lineCounter].Substring(index + 1, strAsmLines[lineCounter].Length - index - 1);
                        }
                        index = strAsmLines[lineCounter].IndexOf(",", index + 1);
                    }

                    //whitespace after each bracket "("
                    index = strAsmLines[lineCounter].IndexOf("(");
                    while (index >= 0)
                    {
                        if (index > 0)
                        {
                            if (strAsmLines[lineCounter][index - 1] != ' ')
                                strAsmLines[lineCounter] = strAsmLines[lineCounter].Substring(0, index) + " (" + strAsmLines[lineCounter].Substring(index + 1, strAsmLines[lineCounter].Length - index - 1);
                        }
                        index = strAsmLines[lineCounter].IndexOf("(", index + 1);
                    }
                }
            }

            string codeFormatted = String.Empty;
            foreach (string line in strAsmLines)
            {
                bool isNewLine = true; bool isInComment = false; bool addNewlineAtLineEnd = true; bool bIsInCompilerDirective = false;

                string[] lineSplitted = Regex.Split(line, @"\s+", RegexOptions.IgnoreCase);
                foreach (string token in lineSplitted)
                {
                    //compiler directives
                    if (Regex.IsMatch(token, AssemblerConfig.regexCompilerDirectives))
                    {
                        codeFormatted += token + new String(' ', Math.Max(8 - token.Length, 1)); //"include" has 7 chars
                        bIsInCompilerDirective = true;
                        continue;
                    }
                    if (bIsInCompilerDirective)
                    {
                        if (token == String.Empty) //newline?
                            bIsInCompilerDirective = false;
                        else
                            codeFormatted += token + " ";
                        continue;
                    }

                    if (token == String.Empty)
                    {
                        if (lineSplitted.Length == 1)
                        {
                            codeFormatted += "\n";
                            addNewlineAtLineEnd = false;
                        }
                        continue;
                    }
                    if (token.StartsWith(";"))
                    {
                        if (!isInComment)
                        {
                            isInComment = true;
                            codeFormatted += token + " ";
                            continue;
                        }
                        isInComment = false;
                    }
                    if (isInComment)
                    {
                        codeFormatted += token + " ";
                        continue;
                    }

                    if (opcodes.Contains(token.ToLower()))
                    {
                        if (isNewLine)
                        {
                            codeFormatted += new String(' ', _tabSpace);
                            bIsInCompilerDirective = false;
                        }
                        codeFormatted += token + new String(' ', Math.Max(6 - token.Length, 1));
                    }
                    else
                    {
                        //label, compiler directive, ...
                        int spacesAfter = 1;
                        if ((_tabSpace > token.Length && isNewLine))
                            spacesAfter = _tabSpace - token.Length;

                        codeFormatted += token + new String(' ', spacesAfter);
                    }

                    isNewLine = false;
                }
                codeFormatted = codeFormatted.TrimEnd(' ');
                if (addNewlineAtLineEnd)
                    codeFormatted += "\n";
                isInComment = false;

                //increase progress bar counter;
                actionIncCounter();
            }

            txtAsm.Text = codeFormatted.TrimEnd('\n') + '\n';
            txtAsm.Selection = actLineSave;
            txtAsm.DoSelectionVisible();
        }

        private void richCompileMessages_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LOG_INFO logInfo = richCompileMessages.GetCurrentMessage();
            if (logInfo != null && logInfo.ErrorLine != -1 && logInfo.ErrorLine-1 < txtAsm.LinesCount)
            {
                int lineIndex = logInfo.ErrorLine - 1;
                txtAsm.Selection = new Range(txtAsm, lineIndex);
                //txtAsm.SelectionColor = Color.DarkRed;
                txtAsm.DoSelectionVisible();
            }
            //Locator.Resolve<IUserMessage>().Info(String.Format("Current error line: {0}", logInfo.ErrorLine));
        }

        private TreeNode GetActualNode()
        {
            if (treeViewFiles.SelectedNode != null)
                return treeViewFiles.SelectedNode;
            else
                return treeViewFiles.Nodes[0];
        }
        private AssemblerSourceInfo GetActualSourceInfo()
        {
            //TreeNode nodeAct = GetActualNode();
            AssemblerSourceInfo info;
            _assemblerSources.TryGetValue(_actualAssemblerNodeIndex, out info);
            return info;
        }

        private void listViewSymbols_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewSymbols.SelectedItems.Count == 1)
            {
                ListView.SelectedListViewItemCollection items = listViewSymbols.SelectedItems;

                ListViewItem lvItem = items[0];
                string label = lvItem.Text;

                int labelLineIndex = -1;
                for( int counter = 0; counter < txtAsm.Lines.Count; counter++ )
                {
                    string actLine = txtAsm.Lines[counter];
                    string[] lineTokens = Regex.Split(actLine, @"\s+");
                    if( lineTokens.Length > 0 && lineTokens != null )
                    {
                        if (lineTokens[0] == label || lineTokens[0] == label + ":")
                        {
                            labelLineIndex = counter;
                            txtAsm.Focus();
                            break;
                        }
                    }
                }
                if (labelLineIndex >= 0)
                {
                    txtAsm.Selection = new Range(txtAsm, labelLineIndex);
                    txtAsm.DoSelectionVisible();
                }
            }
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
                    bool retCode = FileTools.ReadFile(i_fileName, out o_strFileText);
                    if (!retCode)
                        return retCode;

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

                //Locator.Resolve<IUserMessage>().Info("File " + i_fileName + " saved!");
                this.richCompileMessages.AppendInfo("File " + i_fileName + " saved!");

                AssemblerSourceInfo info;
                _assemblerSources.TryGetValue(_actualAssemblerNodeIndex, out info);
                if (info != null)
                {
                    _assemblerSources[_actualAssemblerNodeIndex].SetIsSaved(true);
                    GetActualNode().Text = _assemblerSources[_actualAssemblerNodeIndex].GetDisplayName();
                }
                return true;
            }
        #endregion File operations

        #region Source management(add/delete/save/refresh)
        class AssemblerSourceInfo
        {
            private int _id;
            public int Id{get { return this._id; } set{ if(value >= 0) this._id = value; }}

            private string _sourceCode;
            public string GetSourceCode()
            {
                return _sourceCode;
            }
            public void SetSourceCode(string i_newSourceCode)
            {
                if (i_newSourceCode != null)
                    _sourceCode = i_newSourceCode;
                else
                    _sourceCode = String.Empty;
            }

            private  bool _isFile { get; set; }
            private bool IsSaved { get; set; }
            private string _sourceName { get; set; } //empty when it is a file

            private string _fileName;

            public AssemblerSourceInfo(string i_sourceName, bool i_isFile)
            {
                _sourceName = i_sourceName;
                _isFile = i_isFile;
                IsSaved = true;
                _fileName = _sourceCode = string.Empty;
            }

            public string GetFileNameToSave()
            {
                if (_fileName == string.Empty || !_isFile)
                    return _sourceName;
                else
                    return _fileName;
            }
            public string GetSourceName()
            {
                return _sourceName;
            }
            public string GetDisplayName()
            {
                string fileName = _sourceName;

                if (_sourceName.IndexOf(Path.DirectorySeparatorChar) != -1)
                {
                    string[] splitted = _sourceName.Split(Path.DirectorySeparatorChar);
                    fileName = splitted[splitted.Length-1];
                }
                if (!IsSaved)
                    return "*" + fileName;
                else
                    return fileName;
            }
            public bool IsFile()
            {
                return _isFile;
            }
            public void SetIsFile()
            {
                _isFile = true;
            }
            public void SetIsSaved(bool i_isSaved)
            {
                IsSaved = i_isSaved;
            }
            public bool IsSourceEqual(string i_sourceActual)
            {
                return i_sourceActual == this._sourceCode;
            }
            public void SetSourceNameOrFilename(string i_newName)
            {
                if (!_isFile) //if it is not file
                    _sourceName = i_newName;
                else
                {
                    int lastIndex = _fileName.LastIndexOf(Path.DirectorySeparatorChar);
                    //if it is a file then we remember the file path
                    if (lastIndex != -1)
                    {
                        _fileName = _fileName.Substring(0, lastIndex) + Path.DirectorySeparatorChar + i_newName;
                        _sourceName = i_newName;
                    }
                    else
                        _sourceName = _fileName = i_newName;
                }
            }
            public string GetFileDirectory()
            {
                if (IsFile() && _fileName.Length > 1)
                    return _fileName.Substring(0, _fileName.LastIndexOf(Path.DirectorySeparatorChar)) + Path.DirectorySeparatorChar;
                else
                    return String.Empty;
            }
            public void RenameFile(string i_newName)
            {
                if (IsFile())
                {
                    string newFileWithFullPath = GetFileDirectory() + i_newName;
                    if (File.Exists(_fileName))
                        File.Move(_fileName, newFileWithFullPath);
                    else
                        SetIsSaved(false);
                    _sourceName = i_newName;
                    _fileName = newFileWithFullPath;
                }
            }
        }

        private int AddNewSource(AssemblerSourceInfo i_sourceCandidate, string i_sourceCode = null)
        {
            i_sourceCandidate.Id = SourceInfo_GetNextId();
            if (i_sourceCandidate.Id == 0) //there were none
                i_sourceCandidate.SetSourceCode( txtAsm.Text );
            if (i_sourceCode!=null)
                i_sourceCandidate.SetSourceCode(_strDefaultAsmSourceCode);
            _assemblerSources.Add(i_sourceCandidate.Id, i_sourceCandidate);

            return i_sourceCandidate.Id;
        }
        private void RemoveSource(int i_sourceIndex)
        {
            if( _assemblerSources != null && _assemblerSources.Count > 0 )
                _assemblerSources.Remove(i_sourceIndex);
            if (_assemblerSources.Count > 0)
            {
                _actualAssemblerNodeIndex = 0;
                this.txtAsm.Text = _assemblerSources.FirstOrDefault().Value.GetSourceCode(); //setting to first in list
                treeViewFiles.SelectedNode = treeViewFiles.Nodes[0];
            }
            else
                this.txtAsm.Text = String.Empty;
        }
        private int SourceInfo_GetNextId()
        {
            if (_assemblerSources.Count == 0)
                return 0;
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
                if ( e.Label == null || e.Label == node.Text)
                    return;

                int index = (int)node.Tag;
                AssemblerSourceInfo sourceInfo;
                if( _assemblerSources != null && _assemblerSources.TryGetValue(index, out sourceInfo) )
                {
                    sourceInfo.RenameFile(e.Label);
                }

                _actualAssemblerNodeIndex = index;
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
