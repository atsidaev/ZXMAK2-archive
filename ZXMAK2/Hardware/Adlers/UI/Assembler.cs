using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ZXMAK2.Interfaces;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using FastColoredTextBoxNS;
using System.Diagnostics;

namespace ZXMAK2.Hardware.Adlers.UI
{
    public partial class Assembler : Form
    {
        [DllImport(@"Pasmo.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int compile( 
                  /*char**/ [MarshalAs(UnmanagedType.LPStr)] string compileArg,   //e.g. --bin, --tap; terminated by NULL(0)
                  /*char**/ [MarshalAs(UnmanagedType.LPStr)] string inAssembler,
	              /*char**/ IntPtr compiledOut,
                  /*int* */ IntPtr codeSize,
                  /*int**/  IntPtr errFileLine,
                  /*char**/ IntPtr errFileName,
                  /*char**/ IntPtr errReason
                  );
        [DllImport(@"PasmoXP.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint="compile")]
        public unsafe static extern int compileXP(
            /*char**/ [MarshalAs(UnmanagedType.LPStr)] string compileArg,   //e.g. --bin, --tap; terminated by NULL(0)
            /*char**/ [MarshalAs(UnmanagedType.LPStr)] string inAssembler,
            /*char**/ IntPtr compiledOut,
            /*int* */ IntPtr codeSize,
            /*int**/  IntPtr errFileLine,
            /*char**/ IntPtr errFileName,
            /*char**/ IntPtr errReason
            );

        //text editor styles
        Style CommentStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        Style CommonInstructionStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        //Style JumpInstructionStyle = new TextStyle(new SolidBrush(Color.FromArgb(255, 43, 145, 175)), null, FontStyle.Regular);
        Style JumpInstructionStyle = new TextStyle(Brushes.DarkViolet, null, FontStyle.Regular);
        Style StackInstructionStyle = new TextStyle(Brushes.DarkCyan, null, FontStyle.Regular);
        Style CompilerInstructionStyle = new TextStyle(Brushes.SaddleBrown, null, FontStyle.Italic);

        private byte tabSpace = 16; //how many characters on tab

        private IDebuggable m_spectrum;

        private bool compileFromFile = false; //if loaded from file then --binfile compile parameter will be used
        private string actualLoadedFile = String.Empty;

        private static Assembler m_instance = null;
        private Assembler(ref IDebuggable spectrum)
        {
            m_spectrum = spectrum;

            InitializeComponent();

            //txtAsm.Selection.Start = Place.Empty;
            txtAsm.DoCaretVisible();
            txtAsm.IsChanged = false;
            txtAsm.ClearUndo();

            txtAsm.Text = new string(' ', tabSpace);
            txtAsm.SelectionLength = 0;
            txtAsm.SelectionStart = txtAsm.Text.Length + 1;

            this.KeyPreview = true;
        }

        public static void Show(ref IDebuggable spectrum)
        {
            if (m_instance == null)
            {
                m_instance = new Assembler(ref spectrum);
                m_instance.ShowInTaskbar = false;
                m_instance.Show();
            }
            else
                m_instance.Show();
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

            if (txtAsm.Text.Trim() == String.Empty)
            {
                this.richCompileMessages.Text = "Nothing to compile...";
                return;
            }

            unsafe
            {
                //FixedBuffer fixedBuf = new FixedBuffer();

                string  asmToCompileOrFileName;
                byte[]  compiledOut = new byte[65536-16384 + 2/*memory start when --binfile is used*/];
                byte[]  errReason = new byte[1024];
                int     codeSize;
                int     errFileLine;
                byte[]  errFileName = new byte[512];

                string compileOption;
                if (compileFromFile)
                {
                    asmToCompileOrFileName = actualLoadedFile;
                    compileOption = "--binfile";
                }
                else
                {
                    asmToCompileOrFileName = txtAsm.Text;
                    compileOption = "--bin";
                }

                fixed (byte* pcompiledOut = &compiledOut[0])
                {
                    fixed (byte* perrReason = &errReason[0])
                    {
                        fixed (byte* perrFileName = &errFileName[0])
                        {
                            try
                            {
                                retCode = compile(compileOption, asmToCompileOrFileName, new IntPtr(pcompiledOut),
                                                  new IntPtr(&codeSize), new IntPtr(&errFileLine),
                                                  new IntPtr(perrFileName), new IntPtr(perrReason)
                                                  );
                            }
                            catch(DllNotFoundException)
                            {
                                retCode = compileXP(compileOption, asmToCompileOrFileName, new IntPtr(pcompiledOut),
                                                    new IntPtr(&codeSize), new IntPtr(&errFileLine),
                                                    new IntPtr(perrFileName), new IntPtr(perrReason)
                                                    );
                            }
                            if (retCode != 0)
                            {
                                string errStringText = DateTime.Now.ToLongTimeString() + ": ";
                                errStringText += "Error on line " + errFileLine.ToString() + ", file: " + getString(perrFileName);
                                errStringText += "\n    ";
                                errStringText += getString(perrReason);

                                this.richCompileMessages.Text = errStringText;
                            }
                            else
                            {
                                //we got a assembly
                                this.richCompileMessages.Text = DateTime.Now.ToLongTimeString() + ": Compilation OK ! Now writing memory...";

                                //write to memory ?
                                if (checkMemory.Checked)
                                {
                                    //get address where to write the code
                                    ushort memAdress = 0;
                                    ushort memArrayDelta = 0;
                                    try
                                    {
                                        if (compileOption == "--binfile") //here the start address will be first 2 bytes from compiledOut
                                        {
                                            memAdress = (ushort)(compiledOut[0] + compiledOut[1] * 256);
                                            memArrayDelta = 2;
                                        }
                                        else
                                            memAdress = DebuggerManager.convertNumberWithPrefix(textMemAdress.Text);
                                    }
                                    catch (System.Exception)
                                    {
                                        this.richCompileMessages.Text += "\n    Incorrect memory address. No write memory to be processed...";
                                        return;
                                    }

                                    if (memAdress >= 0x4000) //RAM start
                                    {
                                        Stopwatch watch = new Stopwatch();
                                        watch.Start();
                                        m_spectrum.WriteMemory(memAdress, compiledOut, memArrayDelta, codeSize);
                                        watch.Stop();

                                        TimeSpan time = watch.Elapsed;
                                        this.richCompileMessages.Text += String.Format("\n    Memory written in {0:0.00000} seconds", time.TotalSeconds);
                                    }
                                    else
                                    {
                                        this.richCompileMessages.Text += "\n    Cannot write to ROM(address = " + memAdress.ToString() + "). Bail out.";
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        static unsafe private string getString(byte* i_pointer)
        {
            string retString = String.Empty;

            for (int counter = 0; ; counter++)
            {
                if (*(i_pointer + counter) == 0)
                    break;

                retString += (char)*(i_pointer + counter);
            }

            return retString;
        }

        private void checkMemory_CheckedChanged(object sender, EventArgs e)
        {
            if (checkMemory.Checked)
            {
                textMemAdress.Enabled = true;
            }
            else
            {
                textMemAdress.Enabled = false;
            }
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
        private void fonttoolStrip_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            fontDialog.Font = txtAsm.Font;

            DialogResult res = fontDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                Font font = fontDialog.Font;
                txtAsm.Font = font;
            }
        }

        //text Color
        private void colorToolStrip_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = txtAsm.ForeColor;
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                txtAsm.ForeColor = colorDialog.Color;
            }
        }

        //background Color
        private void backColortoolStrip_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = txtAsm.BackColor;
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                txtAsm.BackColor = colorDialog.Color;
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
                loadDialog.Filter = "Assembler files (asm,txt)|*.asm;*.txt";
                loadDialog.DefaultExt = "";
                loadDialog.FileName = "";
                loadDialog.ShowReadOnly = false;
                loadDialog.CheckFileExists = true;
                if (loadDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                FileInfo fileInfo = new FileInfo(loadDialog.FileName);
                int s_len = (int)fileInfo.Length;

                byte[] data = new byte[s_len];
                using (FileStream fs = new FileStream(loadDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    fs.Read(data, 0, data.Length);
                this.txtAsm.Text = Encoding.UTF8.GetString(data, 0, data.Length);

                TreeNode node = treeViewFiles.Nodes.Add(loadDialog.FileName);

                //node.ForeColor = Color.Red;

                //add file content to Dictionary
                //codeFileContent.Add(loadDialog.FileName, newFileContent);

                //add to TreeView Left Panel
                //tabToAddNewFile.Controls.Add(this);

                compileFromFile = true;
                actualLoadedFile = loadDialog.FileName;
                textMemAdress.Enabled = false;
            }
        }

        private void txtAsm_TextChanged(object sender, TextChangedEventArgs e)
        {
            //clear styles
            e.ChangedRange.ClearStyle(CommentStyle);
            e.ChangedRange.ClearStyle(JumpInstructionStyle);

            //comment highlighting
            e.ChangedRange.SetStyle(CommentStyle, @";.*$", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(CommonInstructionStyle, @"ldir|lddr|\bld\b|\bim\b|add|\bsub\b|\bdec\b|sbc|halt|\bbit\b|set|xor|\binc\b|cp|\bei\b|\bdi\b|\band\b|\bor\b|\band\b" +
                @"|\brr\b|scf|ccf|\bneg\b|srl|exx|\bex\b",
                RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(CompilerInstructionStyle, @"defb|include", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(StackInstructionStyle, @"push|pop|dec sp|inc sp", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            e.ChangedRange.SetStyle(JumpInstructionStyle, @"reti|retn|ret|jp|jr|call|djnz", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        } 
   }
}
