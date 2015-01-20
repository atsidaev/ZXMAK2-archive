using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Host.WinForms.HardwareViews.Adlers
{
    public partial class GraphicsEditor : Form
    {
        private static ushort ZX_SCREEN_WIDTH  = 512;
        private static ushort ZX_SCREEN_HEIGHT = 384;

        private static GraphicsEditor m_instance = null;
        private IDebuggable m_spectrum = null;

        public GraphicsEditor(ref IDebuggable spectrum)
        {
            m_spectrum = spectrum;

            InitializeComponent();

            comboDisplayType.SelectedIndex = 0;
            comboDisplayTypeWidth.SelectedIndex = 0;
            comboDisplayTypeHeight.SelectedIndex = 0;
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
                return;
            }
            else
                m_instance.Show();

            m_instance.setZXImage();
        }

        #region Display options
        /// <summary>
        /// Screen View type
        /// </summary>
        public void setZXScreenView()
        {
            if (m_spectrum == null)
                return;

            Bitmap bmpZXMonochromatic = new Bitmap(32*8, 24*8);
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

                            BitArray spriteBits = GraphicsEditor.getAttributePixels(blockByte);

                            // Cycle: fill 8 pixels for 1 attribute
                            for (int pixels = 7; pixels > -1; pixels--)
                            {
                                if (spriteBits[pixels])
                                    bmpZXMonochromatic.SetPixel((7 - pixels) + (attributes * 8), linesInSegment + eightLines + (segments * 64), Color.Black);
                                else
                                    bmpZXMonochromatic.SetPixel((7 - pixels) + (attributes * 8), linesInSegment + eightLines + (segments * 64), Color.White);
                            }
                        }
                    }
                }
            } // 3 segments of the ZX Screen

            //Size newSize = new Size((int)(pictureZXDisplay.Width), (int)(pictureZXDisplay.Height));
            pictureZXDisplay.Image = bmpZXMonochromatic;
            pictureZXDisplay.Width = ZX_SCREEN_WIDTH;
            pictureZXDisplay.Height = ZX_SCREEN_HEIGHT;
            pictureZXDisplay.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        /// <summary>
        /// Sprite View type
        /// </summary>
        public void setZXSpriteView()
        {
            byte   spriteWidth = Convert.ToByte(comboDisplayTypeWidth.SelectedItem);
            //byte spriteHeight;
            ushort screenPointer = (ushort)numericUpDownActualAddress.Value;

            if (m_spectrum == null)
                return;

            Bitmap bmpSpriteView = new Bitmap(spriteWidth, ZX_SCREEN_HEIGHT);

            for (int line = 0; line < ZX_SCREEN_HEIGHT; line++)
            {
                BitArray spriteBits = GraphicsEditor.getAttributePixels(m_spectrum.ReadMemory(screenPointer++));

                // Cycle: fill 8 pixels for 1 attribute
                for (int pixels = 7; pixels > -1; pixels--)
                {
                    if (spriteBits[pixels])
                        bmpSpriteView.SetPixel(pixels, line, Color.Black);
                    else
                        bmpSpriteView.SetPixel(pixels, line, Color.White);
                }
            }

            Image resizedImage = bmpSpriteView.GetThumbnailImage(spriteWidth * 3, (spriteWidth * 3 * bmpSpriteView.Height) /
                bmpSpriteView.Width, null, IntPtr.Zero);
            pictureZXDisplay.Image = resizedImage;
            pictureZXDisplay.SizeMode = PictureBoxSizeMode.AutoSize;
        }
        #endregion

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
            BitArray bitsOut = new BitArray(8); //define the size

            //setting a value
            for (byte x = 0; x < bitsOut.Count; x++)
            {
                bitsOut[x] = (((attribute >> x) & 0x01) == 0x01) ? true : false;
            }

            return bitsOut;
        }

        private void setZXImage()
        {
            bool bEnableCombos = true;
            if (comboDisplayType.SelectedIndex == 0) //screen view
                bEnableCombos = false;

            comboDisplayTypeWidth.Enabled = bEnableCombos;
            comboDisplayTypeHeight.Enabled = bEnableCombos;

            switch (comboDisplayType.SelectedIndex)
            {
                case 0: //Screen view
                    setZXScreenView();
                    break;
                case 1: //Sprite view
                    setZXSpriteView();
                    break;
                default:
                    break;
            }
        }

        #region GUI methods
        private void numericUpDownActualAddress_ValueChanged(object sender, System.EventArgs e)
        {
            setZXImage();
        }
        private void buttonClose_Click(object sender, System.EventArgs e)
        {
            this.Hide();
        }
        private void comboDisplayType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            setZXImage();
        }
        private void numericIncDecDelta_ValueChanged(object sender, EventArgs e)
        {
            numericUpDownActualAddress.Increment = numericIncDecDelta.Value;
        }
        //Refresh button
        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            setZXImage();
        }
        #endregion
    }
}
