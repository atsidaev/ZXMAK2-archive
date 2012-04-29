using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Z80;

namespace ZXMAK2.Controls
{
	public partial class TapeForm : Form
	{
		private ITapeDevice _tape;
		
        public TapeForm(ITapeDevice tape)
		{
			_tape = tape;
			InitializeComponent();
			tape.TapeStateChanged += new EventHandler(OnTapeStateChanged);
			OnTapeStateChanged(null, null);
			OnTapeStateChanged(null, null);
		}

        private void TapeForm_FormClosed(object sender, FormClosedEventArgs e)
		{
            _tape.TapeStateChanged -= new EventHandler(OnTapeStateChanged);
		}


		private void toolButtonRewind_Click(object sender, EventArgs e)
		{
		    _tape.Rewind();
		}

		private void toolButtonPrev_Click(object sender, EventArgs e)
		{
		    _tape.CurrentBlock++;
		}

        private void toolButtonPlay_Click(object sender, EventArgs e)
        {
            if (_tape.IsPlay)
                _tape.Stop();
            else
                _tape.Play();
        }

		private void toolButtonNext_Click(object sender, EventArgs e)
		{
			_tape.CurrentBlock--;
		}

		private void OnTapeStateChanged(object sender, EventArgs args)
		{
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(OnTapeStateChanged), sender, args);
                return;
            }
            if (_tape.Blocks.Count <= 0)
			{
				btnRewind.Enabled =
				   btnPrev.Enabled =
				   btnPlay.Enabled =
				   btnNext.Enabled = false;
				blockList.SelectedIndex = -1;
			}
			else
			{
				btnNext.Enabled = btnPrev.Enabled = !_tape.IsPlay;
				btnRewind.Enabled = btnPlay.Enabled = true;
                if (_tape.IsPlay)
                    btnPlay.Image = global::ZXMAK2.Properties.Resources.StopIcon;
                else
                    btnPlay.Image = global::ZXMAK2.Properties.Resources.PlayIcon;
                if (checkContentDifferent(blockList.Items, _tape.Blocks))
                {
                    blockList.Items.Clear();
                    foreach (TapeBlock tb in _tape.Blocks)
                        blockList.Items.Add(tb.Description);
                }
				blockList.SelectedIndex = _tape.CurrentBlock;
			}
			blockList.Enabled = !_tape.IsPlay;
            btnTraps.Enabled = !_tape.IsPlay;
            btnTraps.Checked = _tape.TrapsAllowed;
		}

        private bool checkContentDifferent(ListBox.ObjectCollection itemList, List<TapeBlock> list)
        {
            if (itemList.Count != list.Count)
                return true;
            for (int i = 0; i < list.Count; i++)
                if ((string)itemList[i] != list[i].Description)
                    return true;
            return false;
        }

        private void timerProgress_Tick(object sender, EventArgs e)
		{
			toolProgressBar.Minimum = 0;

            int blockCount = _tape.Blocks.Count;
            int curBlock, position, maximum;
            do
            {
                curBlock = _tape.CurrentBlock;
                if (curBlock >= 0 && curBlock < blockCount)
                {
                    maximum = _tape.Blocks[curBlock].Periods.Count;
                    position = _tape.Position;
                }
                else
                {
                    maximum = 65535;
                    position = 0;
                }
            } while (position > maximum);

            toolProgressBar.Maximum = maximum;
            toolProgressBar.Value = position;
		}

		private void blockList_Click(object sender, EventArgs e)
		{
			if (!blockList.Enabled || _tape.IsPlay) return;
			_tape.CurrentBlock = blockList.SelectedIndex;
		}
		
        private void blockList_DoubleClick(object sender, EventArgs e)
		{
			if (!blockList.Enabled || _tape.IsPlay) return;
			_tape.CurrentBlock = blockList.SelectedIndex;
			_tape.Play();
		}

        private void btnTraps_Click(object sender, EventArgs e)
        {
            _tape.TrapsAllowed = btnTraps.Checked;
        }
	}
}