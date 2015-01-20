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
        public static UInt16 MAX_X_PIXEL = Convert.ToUInt16(256); // X-coordinate maximum(pixel)
        public static UInt16 MAX_Y_PIXEL = Convert.ToUInt16(192); // Y-coordinate maximum(pixel)

        private static GraphicsEditor m_instance = null;
        private IDebuggable m_spectrum = null;

        private bool isInitialised = false;

        public GraphicsEditor(ref IDebuggable spectrum)
        {
            m_spectrum = spectrum;

            InitializeComponent();
            isInitialised = true;

            comboDisplayType.SelectedIndex = 0;
            comboSpriteWidth.SelectedIndex = 0;
            comboSpriteHeight.SelectedIndex = 0;
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
                //return;
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

            pictureZXDisplay.Width = ZX_SCREEN_WIDTH;
            pictureZXDisplay.Height = ZX_SCREEN_HEIGHT;

            Bitmap bmpZXMonochromatic = new Bitmap(ZX_SCREEN_WIDTH/2, ZX_SCREEN_HEIGHT/2);
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
                            if (spriteBits == null)
                                return;

                            // Cycle: fill 8 pixels for 1 attribute
                            for (int pixels = 0; pixels < 8; pixels++)
                            {
                                if (spriteBits[pixels])
                                    bmpZXMonochromatic.SetPixel(pixels + (attributes * 8), linesInSegment + eightLines + (segments * 64), Color.Black);
                                else
                                    bmpZXMonochromatic.SetPixel(pixels + (attributes * 8), linesInSegment + eightLines + (segments * 64), Color.White);
                            }
                        }
                    }
                }
            } // 3 segments of the ZX Screen

            //Size newSize = new Size((int)(pictureZXDisplay.Width), (int)(pictureZXDisplay.Height));
            /*pictureZXDisplay.Image = bmpZXMonochromatic;
            pictureZXDisplay.Width = ZX_SCREEN_WIDTH;
            pictureZXDisplay.Height = ZX_SCREEN_HEIGHT;*/
            Image resizedImage = bmpZXMonochromatic.GetThumbnailImage(ZX_SCREEN_WIDTH, ZX_SCREEN_HEIGHT, null, IntPtr.Zero);
            pictureZXDisplay.Image = resizedImage;
            pictureZXDisplay.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private Image getSpriteViewImage()
        {
            byte spriteWidth = Convert.ToByte(comboSpriteWidth.SelectedItem);
            //byte spriteHeight;
            ushort screenPointer = (ushort)numericUpDownActualAddress.Value;

            if (m_spectrum == null)
                return null;

            Bitmap bmpSpriteView = new Bitmap(spriteWidth, ZX_SCREEN_HEIGHT);

            for (int line = 0; line < ZX_SCREEN_HEIGHT; line++)
            {
                for (int widthCounter = 0; widthCounter < (spriteWidth / 8); widthCounter++)
                {
                    BitArray spriteBits = GraphicsEditor.getAttributePixels(m_spectrum.ReadMemory(screenPointer++));
                    if (spriteBits == null)
                        return null;

                    // Cycle: fill 8 pixels for 1 attribute
                    for (int pixelBit = 7; pixelBit > -1; pixelBit--)
                    {
                        if (spriteBits[pixelBit])
                            bmpSpriteView.SetPixel(pixelBit + widthCounter * 8, line, Color.Black);
                        else
                            bmpSpriteView.SetPixel(pixelBit + widthCounter * 8, line, Color.White);
                    }
                }
            }

            byte spriteZoomFactor = Convert.ToByte(numericUpDownZoomFactor.Value);
            Image resizedImage = bmpSpriteView.GetThumbnailImage(spriteWidth * spriteZoomFactor, (spriteWidth * spriteZoomFactor * bmpSpriteView.Height) /
                bmpSpriteView.Width, null, IntPtr.Zero);
            return resizedImage;
        }

        /// <summary>
        /// Sprite View type
        /// </summary>
        public void setZXSpriteView()
        {
            pictureZXDisplay.Image = getSpriteViewImage();
            pictureZXDisplay.SizeMode = PictureBoxSizeMode.AutoSize;
        }

        /// <summary>
        /// JetPac style view
        /// the image is mirrored and turned 180degrees
        /// </summary>
        public void setZXJetpacView()
        {
            Image img = getSpriteViewImage();

            img.RotateFlip(RotateFlipType.Rotate180FlipY);
            img.RotateFlip(RotateFlipType.Rotate180FlipNone);
            pictureZXDisplay.Image = img;
            pictureZXDisplay.SizeMode = PictureBoxSizeMode.AutoSize;
        }
        #endregion

        /// <summary>
        /// getAttributePixels
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns>BitArray</returns>
        public static BitArray getAttributePixels(byte attribute)
        {
            if (m_instance == null)
                return null;

            BitArray bitsOut = new BitArray(8); //define the size

            if (m_instance.checkBoxMirror.Checked) //mirror image ?
            {
                for (byte x = 0; x < bitsOut.Count; x++)
                {
                    bitsOut[x] = (((attribute >> x) & 0x01) == 0x01) ? true : false;
                }
            }
            else
            {
                //setting a value
                for (int x = 0; x < bitsOut.Length; x++) //mirror bits in array to be correctly displayed on screen(left to right)
                {
                    bitsOut[bitsOut.Length - 1 - x] = (((attribute >> x) & 0x01) == 0x01) ? true : false;
                }
            }

            return bitsOut;
        }

        /// <summary>
        /// getSegment
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static short getSegment(int y)
        {
            if (y < 64) // segment #2
                return 1; //<0; 63>

            //<64; 127>
            if (y > 127) // segment #3
                return 3;

            return 2;
        }
        /// <summary>
        /// getScreenAdress
        /// </summary>
        /// <param name="xCoor"></param>
        /// <param name="yCoor"></param>
        /// <returns></returns>
        public static ushort getScreenAdress(int xCoor, int yCoor)
        {
            int yCoorLocal = yCoor;
            ushort sAdress = 16384;
            short sSegment = getSegment(yCoor); // in which screen segment we are ?

            sAdress += Convert.ToUInt16((sSegment - 1) * 2048);
            yCoorLocal -= (sSegment - 1) * 64;     // move it into the ground fictive segment
            sAdress += Convert.ToUInt16(((xCoor /*- 1*/) / 8));

            //add y coordinate value
            int attributeLineNumber = (yCoorLocal /*- 1*/) / 8;   // defines which line of attributes is it on the screen
            int lineInAttribute = (yCoorLocal /*- 1*/) % 8;   // defines which line is it in the attribute...from above

            sAdress += Convert.ToUInt16((attributeLineNumber * 32) + (lineInAttribute * MAX_X_PIXEL));

            return sAdress;
        }

        private void setZXImage()
        {
            if (!isInitialised)
                return;

            bool bEnableControls = true;
            if (comboDisplayType.SelectedIndex == 0) //screen view
                bEnableControls = false;

            comboSpriteWidth.Enabled = bEnableControls;
            //comboSpriteHeight.Enabled = bEnableControls;
            numericUpDownZoomFactor.Enabled = bEnableControls;
            groupBoxScreenInfo.Visible = !bEnableControls;

            switch (comboDisplayType.SelectedIndex)
            {
                case 0: //Screen view
                    setZXScreenView();
                    break;
                case 1: //Sprite view
                    setZXSpriteView();
                    break;
                case 4: //JetPac style
                    setZXJetpacView();
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
        private void numericUpDownZoomFactor_ValueChanged(object sender, EventArgs e)
        {
            setZXImage();
        }
        private void comboSpriteWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            setZXImage();
        }
        private void pictureZXDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X < 0 || e.Y < 0)
                return;

            int Xcoor = (e.X + 1) / 2;
            int Ycoor = (e.Y + 1) / 2;
            ushort screenAdress = getScreenAdress(Xcoor, Ycoor);

            textBoxScreenAddress.Text = screenAdress.ToString();

            textBoxXCoorYCoor.Text = String.Format("{0}; {1}", Xcoor, Ycoor);

            textBoxBytesAtAdress.Text =  String.Format("#{0:X2}", m_spectrum.ReadMemory(screenAdress));
            for( ushort memValue = (ushort)(screenAdress+1); memValue < screenAdress + 5; memValue++ )
            {
                textBoxBytesAtAdress.Text += "; " + String.Format("#{0:X2}", m_spectrum.ReadMemory(memValue));
            }
        }
        private void checkBoxMirror_CheckedChanged(object sender, EventArgs e)
        {
            setZXImage();
        }
        #endregion
    }
}
