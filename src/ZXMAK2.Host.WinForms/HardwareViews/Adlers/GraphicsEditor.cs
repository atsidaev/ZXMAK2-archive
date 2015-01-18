using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Host.WinForms.HardwareViews.Adlers
{
    public partial class GraphicsEditor : Form
    {
        private static GraphicsEditor m_instance = null;
        private IDebuggable m_spectrum = null;

        public GraphicsEditor(ref IDebuggable spectrum)
        {
            m_spectrum = spectrum;

            InitializeComponent();

            comboDisplayType.SelectedIndex = 0;
        }

        public static GraphicsEditor getInstance()
        {
            return m_instance;
        }

        public static void Show(ref IDebuggable spectrum)
        {
            if (m_instance == null || m_instance.IsDisposed)
            {
                m_instance = new GraphicsEditor(ref spectrum);
                m_instance.ShowInTaskbar = true;
                m_instance.Show();
            }
            else
                m_instance.Show();

            m_instance.MakeZXBitmap();
        }

        public void MakeZXBitmap()
        {
            if (m_spectrum == null)
                return;

            Bitmap bmpZXMonochromatic = new Bitmap(256, 194);
            ushort screenPointer = (ushort)numericUpDownActualAddress.Value;

            //Screen View
            for (int segments = 0; segments < 3; segments++)
            {
                for (int eightLines = 0; eightLines < 8; eightLines++)
                {
                    //Cycle: Fill 8 lines in one segment
                    for (int linesInSegment = 0; linesInSegment < 64; linesInSegment += 8)
                    {
                        // Cycle: all attributes in 1 line
                        for (int attributes = 0; attributes < 32; attributes++)
                        {
                            byte blockByte = m_spectrum.ReadMemory(screenPointer++);

                            BitArray arrayOfBits = GraphicsEditor.getAttributePixels(blockByte);

                            // Cycle: fill 8 pixels for 1 attribute
                            for (int pixels = 7; pixels > -1; pixels--)
                            {
                                if (arrayOfBits[pixels])
                                    bmpZXMonochromatic.SetPixel((7 - pixels) + (attributes * 8), linesInSegment + eightLines + (segments * 64), Color.Black);
                                else
                                    bmpZXMonochromatic.SetPixel((7 - pixels) + (attributes * 8), linesInSegment + eightLines + (segments * 64), Color.White);
                            }
                        }
                    }
                }
            } // 3 segments of the ZX Screen

            //Size newSize = new Size((int)(pictureZXDisplay.Width * 7), (int)(pictureZXDisplay.Height * 7));
            pictureZXDisplay.Image = bmpZXMonochromatic; // new Bitmap(bmpZXMonochromatic, newSize);
        }

        /************************************************************
         *                                                          *
         *  Returns 8 pixels of 1 attribute given as input          *
         *                                                          *
         *  Input:   1 Byte = 1 line of attribute                   *
         *                                                          *
         *  Warning: Allowed only values {0 && 1} in return array   *
         *                                                          *
         ************************************************************/
        public static BitArray getAttributePixels(byte attribute)
        {
            BitArray myBits = new BitArray(8); //define the size

            //setting a value
            for (byte x = 0; x < myBits.Count; x++)
            {
                myBits[x] = (((attribute >> x) & 0x01) == 0x01) ? true : false;
            }

            return myBits;
        }



        public static Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            try
            {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage((Image)b))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }
                return b;
            }
            catch { }

            return null;
        }




        #region Events
        private void numericUpDownActualAddress_ValueChanged(object sender, System.EventArgs e)
        {
            MakeZXBitmap();
        }
        #endregion
    }
}
