using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Hardware.Adlers.Core;

namespace ZXMAK2.Hardware.Adlers.Views
{
    public partial class GraphicsEditor : Form
    {
        private static ushort ZX_SCREEN_WIDTH  = 512;
        private static ushort ZX_SCREEN_HEIGHT = 384;

        private static GraphicsEditor m_instance = null;
        private IDebuggable _spectrum = null;

        private bool _isInitialised = false;

        public GraphicsEditor(ref IDebuggable spectrum)
        {
            _spectrum = spectrum;

            InitializeComponent();
            comboDisplayType.SelectedIndex = 0;
            comboSpriteWidth.SelectedIndex = 0;
            comboSpriteHeight.SelectedIndex = 0;

            bitmapGridSpriteView.Init(_spectrum, Convert.ToByte(comboSpriteWidth.SelectedItem), 24);

            _isInitialised = true;
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
            if (_spectrum == null || m_instance == null)
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
                            byte blockByte = _spectrum.ReadMemory(screenPointer++);

                            BitArray spriteBits = GraphicsTools.getAttributePixels(blockByte, m_instance.checkBoxMirror.Checked);
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

            if (_spectrum == null || m_instance == null)
                return null;

            Bitmap bmpSpriteView = new Bitmap(spriteWidth, ZX_SCREEN_HEIGHT);

            for (int line = 0; line < ZX_SCREEN_HEIGHT; line++)
            {
                for (int widthCounter = 0; widthCounter < (spriteWidth / 8); widthCounter++)
                {
                    BitArray spriteBits = GraphicsTools.getAttributePixels(_spectrum.ReadMemory(screenPointer++), m_instance.checkBoxMirror.Checked);
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

        private Image getTileViewImage()
        {
            byte spriteWidth = Convert.ToByte(comboSpriteWidth.SelectedItem);
            //byte spriteHeight;
            ushort screenPointer = (ushort)numericUpDownActualAddress.Value;

            if (_spectrum == null || m_instance == null)
                return null;

            Bitmap bmpSpriteView = new Bitmap(spriteWidth, ZX_SCREEN_HEIGHT);

            for (int YPointerShift = 0; YPointerShift < ZX_SCREEN_HEIGHT; YPointerShift += 8)
            {
                for (int XPointerShift = 0; XPointerShift < spriteWidth; XPointerShift += 8)
                {
                    //draw one tile
                    for (int line = 0; line < 8; line++)
                    {
                        BitArray spriteBits = GraphicsTools.getAttributePixels(_spectrum.ReadMemory(screenPointer++), m_instance.checkBoxMirror.Checked);
                        if (spriteBits == null)
                            return null;

                        // Cycle: fill 8 pixels for 1 attribute
                        for (int pixelBit = 7; pixelBit > -1; pixelBit--)
                        {
                            if (spriteBits[pixelBit])
                                bmpSpriteView.SetPixel(pixelBit + XPointerShift, line + YPointerShift, Color.Black);
                            else
                                bmpSpriteView.SetPixel(pixelBit + XPointerShift, line + YPointerShift, Color.White);
                        }
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
        /// Tile view; 8 lines per sprite
        /// Used in ISChess(Cyrus Chess) for instance
        /// </summary>
        public void setTileView()
        {
            pictureZXDisplay.Image = getTileViewImage();
            pictureZXDisplay.SizeMode = PictureBoxSizeMode.AutoSize;
        }

        /// <summary>
        /// JetPac style view
        /// the image is mirrored and turned 180degrees
        /// </summary>
        public void setZXJetpacView()
        {
            comboSpriteWidth.SelectedIndex = 1;

            Image img = getSpriteViewImage();

            img.RotateFlip(RotateFlipType.Rotate180FlipY);
            img.RotateFlip(RotateFlipType.Rotate180FlipNone);
            pictureZXDisplay.Image = img;
            pictureZXDisplay.SizeMode = PictureBoxSizeMode.AutoSize;
        }
        #endregion

        private bool isSpriteViewType()
        {
            if (comboDisplayType.SelectedIndex == 0) //screen view
                return false;

            return true;
        }

        private void setZXImage()
        {
            if (!_isInitialised)
                return;

            bool bIsSpriteViewType = isSpriteViewType();
            comboSpriteWidth.Enabled = bIsSpriteViewType;
            //comboSpriteHeight.Enabled = bEnableControls;
            numericUpDownZoomFactor.Enabled = bIsSpriteViewType;
            groupBoxScreenInfo.Visible = !bIsSpriteViewType;
            groupBoxSpriteDetails.Visible = bIsSpriteViewType;
            pictureZoomedArea.Visible = !bIsSpriteViewType;

            switch (comboDisplayType.SelectedIndex)
            {
                case 0: //Screen view
                    setZXScreenView();
                    break;
                case 1: //Sprite view
                    setZXSpriteView();
                    groupBoxSpriteDetails.Enabled = true;
                    bitmapGridSpriteView.setBitmapBits(_spectrum, Convert.ToUInt16(numericUpDownActualAddress.Value));
                    bitmapGridSpriteView.Draw(null);
                    break;
                case 3: //Tile view
                    groupBoxSpriteDetails.Enabled = true;
                    setTileView();
                    break;
                case 4: //JetPac style
                    groupBoxSpriteDetails.Enabled = false;
                    setZXJetpacView();
                    break;
                default:
                    break;
            }

            if (bIsSpriteViewType)
            {
                Point locationSpriteDetails = new Point();
                locationSpriteDetails.X = pictureZXDisplay.Location.X + pictureZXDisplay.Size.Width + 10;
                locationSpriteDetails.Y = groupBoxSpriteDetails.Location.Y;
                groupBoxSpriteDetails.Location = locationSpriteDetails;
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
            if (_isInitialised)
            {
                bitmapGridSpriteView.setGridWidth(Convert.ToByte(comboSpriteWidth.SelectedItem));
                bitmapGridSpriteView.setBitmapBits(_spectrum, Convert.ToUInt16(numericUpDownActualAddress.Value));
            }
            setZXImage();
        }
        private void pictureZXDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X < 0 || e.Y < 0)
                return;

            string numberFormat = this.hexNumbersToolStripMenuItem.Checked ? "#{0:X2}" : "{0}";

            int Xcoor = (e.X + 1) / 2;
            int Ycoor = (e.Y + 1) / 2;

            if( !isSpriteViewType() )
            {
                ushort screenAdress = GraphicsTools.getScreenAdress(Xcoor, Ycoor);
                textBoxScreenAddress.Text = String.Format(numberFormat, screenAdress);

                if (this.hexNumbersToolStripMenuItem.Checked)
                {
                    textBoxXCoorYCoor.Text = String.Format("#{0:X2}; #{1:X2}", Xcoor, Ycoor);
                    textBoxBytesAtAdress.Text = String.Format("#{0:X2}", _spectrum.ReadMemory(screenAdress));
                }
                else
                {
                    textBoxXCoorYCoor.Text = String.Format("{0}; {1}", Xcoor, Ycoor);
                    textBoxBytesAtAdress.Text = String.Format("{0}", _spectrum.ReadMemory(screenAdress));
                }

                for (ushort memValue = (ushort)(screenAdress + 1); memValue < screenAdress + 5; memValue++)
                {
                    textBoxBytesAtAdress.Text += "; " + String.Format(numberFormat, _spectrum.ReadMemory(memValue));
                }
            }
            else if (comboDisplayType.SelectedIndex == 1) //only Sprite View for now...ToDo other view types, e.g. Jetpac type
            {
                int zoomFactor = (int)numericUpDownZoomFactor.Value;
                ushort addressPointer = Convert.ToUInt16( numericUpDownActualAddress.Value );
                int addressUnderCursorBase = addressPointer + (Convert.ToByte(comboSpriteWidth.SelectedItem) / 8)*(Ycoor) + (Xcoor/8); //ToDo: zooming will crash this !
                ushort addressUnderCursor = Convert.ToUInt16(addressUnderCursorBase > 0xFFFF ? addressUnderCursorBase-0xFFFF: addressUnderCursorBase);

                //Sprite address
                textBoxSpriteAddress.Text = String.Format("#{0:X2}({1})", addressUnderCursor, addressUnderCursor);

                //Bytes at address
                textBoxSpriteBytes.Text = String.Format("#{0:X2}", _spectrum.ReadMemory(addressUnderCursor));
                for (ushort memValue = (ushort)(addressUnderCursor + 1); memValue < addressUnderCursor + 5; memValue++)
                {
                    textBoxSpriteBytes.Text += "; " + String.Format("#{0:X2}", _spectrum.ReadMemory(memValue));
                }
            }
        }
        private void checkBoxMirror_CheckedChanged(object sender, EventArgs e)
        {
            setZXImage();
        }
        private void hexNumbersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            numericUpDownActualAddress.Hexadecimal = hexNumbersToolStripMenuItem.Checked;
            labelMemoryAddress.Text = String.Format("Memory address({0}):", hexNumbersToolStripMenuItem.Checked ? "hex" : "dec");
        }
        private void bitmapGridSpriteView_MouseUp(object sender, MouseEventArgs e)
        {
            int clickedPixel = bitmapGridSpriteView.getClickedPixel(e);
            int temp = (int)numericUpDownActualAddress.Value + clickedPixel / 8;
            if (temp > 0xFFFF)
                temp -= 0xFFFF;
            UInt16 bitToToggleAddress = Convert.ToUInt16(temp);
            if (bitToToggleAddress < 0x4000)
                return; //cannot change ROM
            byte memValue = _spectrum.ReadMemory(bitToToggleAddress);

            memValue = (byte)GraphicsTools.ToggleBit(memValue, clickedPixel % 8);
            _spectrum.WriteMemory(Convert.ToUInt16(bitToToggleAddress), memValue);

            setZXImage(); //refresh
        }
        #endregion
    }
}
