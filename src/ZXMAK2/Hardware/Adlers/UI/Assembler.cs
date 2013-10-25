using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ZXMAK2.Interfaces;

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

        private IDebuggable m_spectrum;

        private static Assembler m_instance = null;
        private Assembler(ref IDebuggable spectrum)
        {
            m_spectrum = spectrum;

            InitializeComponent();
        }

        public static void Show(ref IDebuggable spectrum)
        {
            if (m_instance == null)
            {
                m_instance = new Assembler(ref spectrum);
                m_instance.ShowInTaskbar = false;
                m_instance.ShowDialog();
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

        private void btnCompile_Click(object sender, EventArgs e)
        {
            int retCode = 0;

            unsafe
            {
                //FixedBuffer fixedBuf = new FixedBuffer();

                string  asmToCompile = txtAsm.Text;
                byte[]  compiledOut = new byte[65536];
                byte[]  errReason = new byte[1024];
                int     codeSize;
                int     errFileLine;
                byte[]  errFileName = new byte[512];

                fixed (byte* pcompiledOut = &compiledOut[0])
                {
                    fixed (byte* perrReason = &errReason[0])
                    {
                        fixed (byte* perrFileName = &errFileName[0])
                        {
                            retCode = compile("--bin", asmToCompile, new IntPtr(pcompiledOut),
                                              new IntPtr(&codeSize), new IntPtr(&errFileLine),
                                              new IntPtr(perrFileName), new IntPtr(perrReason)
                                              );
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
                                this.richCompileMessages.Text = DateTime.Now.ToLongTimeString() + ": Compilation OK !";

                                //write to memory ?
                                if (checkMemory.Checked)
                                {
                                    //get address where to write the code
                                    ushort memAdress = 0;
                                    try
                                    {
                                        memAdress = DebuggerManager.convertNumberWithPrefix(textMemAdress.Text);
                                    }
                                    catch (System.Exception)
                                    {
                                        this.richCompileMessages.Text += "\n    Incorrect memory address. No write memory to be processed...";
                                        return;
                                    }

                                    if (memAdress >= 0x4000)
                                    {
                                        for (ushort memPointer = 0; codeSize > 0; memPointer++)
                                        {
                                            m_spectrum.WriteMemory((ushort)(memPointer + memAdress), compiledOut[memPointer]);
                                            codeSize--;
                                        }
                                        this.richCompileMessages.Text += "\n    Memory written.";
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

        private void txtAsm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                e.Handled = true;
                return;
            }
        }

        private void txtAsm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Tab)
            {
                e.Handled = true;
                txtAsm.SelectedText = new string(' ', 4);
            }
        }

   }
}
