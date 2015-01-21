using System.Drawing;
using System.Windows.Forms;

namespace ZXMAK2.Hardware.Adlers.Views
{
    class BitmapGrid : Panel
    {
        private byte X_BIT_COUNT = 8*2, Y_BIT_COUNT = 8*3;           // Grid Height and Width
        private byte[][] gridBits;

        private bool _isInitialised = false;

        public BitmapGrid()
        {
            gridBits = new byte[Y_BIT_COUNT][];
        }

        public void Init()
        {
            _isInitialised = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!_isInitialised)
                return;

            using (Graphics g = e.Graphics)
            {
                int bitWidth = this.Width / X_BIT_COUNT;
                int bitHeight = this.Height / Y_BIT_COUNT;
                Rectangle rect = ClientRectangle;
                rect.Size = new Size(bitWidth, bitHeight);

                int startX = 2; // grid margin
                int startY = 2; // grid margin
                this.Width = bitWidth * X_BIT_COUNT + 8;
                this.Height = bitHeight * Y_BIT_COUNT + 8;
                for (int counterY = 0; counterY < Y_BIT_COUNT; counterY++)
                    for (int counter = 0; counter < X_BIT_COUNT; counter++)
                    {
                        rect.Location = new Point(startX + (counter * bitWidth), startY+(counterY * bitHeight));
                        using (Brush brush = new SolidBrush(Color.Cyan))
                        {
                            g.FillRectangle(brush, rect);
                        }
                        using (Pen pen = new Pen(Color.Black))
                        {
                            g.DrawRectangle(pen, rect);
                        }
                    }
            }
        }
    }
}
