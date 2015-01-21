using System.Windows.Forms;

namespace ZXMAK2.Hardware.Adlers.Views
{
    class BitmapGrid : DataGridView
    {
        private byte INITIAL_GRID_WIDTH = 8*2, INITIAL_GRID_HEIGHT = 8*3;           // Grid Height and Width

        private bool isInitialised = false;

        public BitmapGrid()
        {
            Init();
        }

        private void Init()
        {
            ColumnCount = INITIAL_GRID_WIDTH;
            for (int countCount = 0; countCount < INITIAL_GRID_WIDTH/8; countCount++)
                for (int count = 0; count < 8; count++)
                {
                    int temp = (countCount * 8) + count;

                    Columns[temp].Name = (count + 1).ToString();
                    Columns[temp].Width = this.Size.Width / INITIAL_GRID_WIDTH;
                    Columns[temp].HeaderText = " ";
                }

            for (int count = 0; count < INITIAL_GRID_HEIGHT; count++)
            {
                DataGridViewRow Arow = new DataGridViewRow();
                Arow.HeaderCell.Value = count.ToString();
                Rows.Add(Arow);
            }

            // hide column and row headers
            ColumnHeadersVisible = false;
            RowHeadersVisible = false;

            this.AutoResizeColumns();
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.AutoResizeColumns();
            RowTemplate.Height = 8; // this.Size.Height / INITIAL_GRID_HEIGHT / 2;

            isInitialised = true; // from now is the detailed grid always initialised
        }

        public void ResizeHeight(int i_parentHeigth) //height of the component where this grid is located
        {
            RowTemplate.Height = i_parentHeigth/INITIAL_GRID_HEIGHT;
        }
    }
}
