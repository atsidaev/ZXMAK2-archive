using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using ZXMAK2.Engine;
using ZXMAK2.Hardware.General;


namespace ZXMAK2.Controls.Configuration
{
    public partial class CtlSettingsBetaDisk : ConfigScreenControl
    {
        private BusManager m_bmgr;
        private BetaDiskInterface m_device;

        public CtlSettingsBetaDisk()
        {
            InitializeComponent();
        }

        public void Init(BusManager bmgr, BetaDiskInterface device)
        {
            m_bmgr = bmgr;
            m_device = device;
            chkNoDelay.Checked = m_device.NoDelay;
            chkLogIO.Checked = m_device.LogIO;
            initDrive(m_device.FDD[0], chkPresentA, txtPathA, chkProtectA);
            initDrive(m_device.FDD[1], chkPresentB, txtPathB, chkProtectB);
            initDrive(m_device.FDD[2], chkPresentC, txtPathC, chkProtectC);
            initDrive(m_device.FDD[3], chkPresentD, txtPathD, chkProtectD);
        }

        public override void Apply()
        {
            m_device.NoDelay = chkNoDelay.Checked;
            m_device.LogIO = chkLogIO.Checked;
            applyDrive(m_device.FDD[0], chkPresentA, txtPathA, chkProtectA);
            applyDrive(m_device.FDD[1], chkPresentB, txtPathB, chkProtectB);
            applyDrive(m_device.FDD[2], chkPresentC, txtPathC, chkProtectC);
            applyDrive(m_device.FDD[3], chkPresentD, txtPathD, chkProtectD);
        }

        private void initDrive(DiskImage diskImage, CheckBox chkPresent, TextBox txtPath, CheckBox chkProtect)
        {
            chkPresent.Checked = diskImage.Present;
            txtPath.Text = diskImage.FileName;
            txtPath.SelectionStart = txtPath.Text.Length;
            chkProtect.Checked = diskImage.IsWP;
            updateEnabled();
        }

        private void applyDrive(DiskImage diskImage, CheckBox chkPresent, TextBox txtPath, CheckBox chkProtect)
        {
            var fileName = txtPath.Text;
            if (fileName != string.Empty)
            {
                if (!File.Exists(Path.GetFullPath(fileName)) &&
                    chkPresent.Checked)
                {
                    throw new FileNotFoundException(
                        string.Format(
                            "File not found: \"{0}\"",
                            fileName));
                }
                fileName = Path.GetFullPath(fileName);
            }
            diskImage.Present = chkPresent.Checked;
            diskImage.FileName = fileName;
            diskImage.IsWP = chkProtect.Checked;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var drive = sender == btnBrowseD ? 3 :
                sender == btnBrowseC ? 2 :
                sender == btnBrowseB ? 1 : 0;
            var pathTxt = new[] { txtPathA, txtPathB, txtPathC, txtPathD };
            var wpChk = new[] { chkProtectA, chkProtectB, chkProtectC, chkProtectD };

            var loadDialog = new OpenFileDialog();
            loadDialog.InitialDirectory = ".";
            loadDialog.SupportMultiDottedExtensions = true;
            loadDialog.Title = "Open...";
            loadDialog.Filter = m_device.FDD[0].SerializeManager.GetOpenExtFilter();
            loadDialog.DefaultExt = ""; //m_betaDisk.BetaDisk.FDD[drive].Serializer.GetDefaultExtension();
            loadDialog.FileName = "";
            loadDialog.ShowReadOnly = true;
            loadDialog.ReadOnlyChecked = true;
            loadDialog.CheckFileExists = true;
            loadDialog.FileOk += new CancelEventHandler(loadDialog_FileOk);
            if (loadDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            pathTxt[drive].Text = loadDialog.FileName;
            pathTxt[drive].SelectionStart = pathTxt[drive].Text.Length;
            wpChk[drive].Checked = loadDialog.ReadOnlyChecked;
        }

        private void loadDialog_FileOk(object sender, CancelEventArgs e)
        {
            OpenFileDialog loadDialog = sender as OpenFileDialog;
            if (loadDialog == null) return;
            e.Cancel = !m_device.FDD[0].SerializeManager.CheckCanOpenFileName(loadDialog.FileName);
        }

        private void chkPresent_CheckedChanged(object sender, EventArgs e)
        {
            updateEnabled();
        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {
            updateEnabled();
        }

        private void updateEnabled()
        {
            setEnabled(txtPathA, chkProtectA, btnBrowseA, chkPresentA);
            setEnabled(txtPathB, chkProtectB, btnBrowseB, chkPresentB);
            setEnabled(txtPathC, chkProtectC, btnBrowseC, chkPresentC);
            setEnabled(txtPathD, chkProtectD, btnBrowseD, chkPresentD);
        }

        private void setEnabled(
            TextBox txtPath,
            CheckBox chkProtect,
            Button btnBrowse,
            CheckBox chkPresent)
        {
            var isZip = txtPath.Text != string.Empty &&
                string.Compare(Path.GetExtension(txtPath.Text), ".ZIP", true) == 0;
            txtPath.Enabled = chkPresent.Checked;
            chkProtect.Enabled = chkPresent.Checked && !isZip;
            if (isZip)
            {
                chkProtect.Checked = true;
            }
            btnBrowse.Enabled = chkPresent.Checked;
        }
    }
}
