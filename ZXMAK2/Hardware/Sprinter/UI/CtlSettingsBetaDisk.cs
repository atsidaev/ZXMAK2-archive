using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using ZXMAK2.Engine;
using ZXMAK2.Controls.Configuration;


namespace ZXMAK2.Hardware.Sprinter.UI
{
    public partial class CtlSettingsBetaDisk : ConfigScreenControl
    {
        private BusManager m_bmgr;
        private SprinterBDI m_device;
        
        public CtlSettingsBetaDisk()
        {
            InitializeComponent();
        }

        public void Init(BusManager bmgr, SprinterBDI device)
        {
            m_bmgr = bmgr;
            m_device = device;
            chkNoDelay.Checked = m_device.NoDelay;
            chkLogIO.Checked = m_device.LogIO;
            initDrive(m_device.FDD[0], chkPresentA, txtPathA, chkProtectA);
            initDrive(m_device.FDD[1], chkPresentB, txtPathB, chkProtectB);
//            initDrive(m_device.FDD[2], chkPresentC, txtPathC, chkProtectC);
//            initDrive(m_device.FDD[3], chkPresentD, txtPathD, chkProtectD);
        }

        public override void Apply()
        {
            m_device.NoDelay = chkNoDelay.Checked;
            m_device.LogIO = chkLogIO.Checked;
            applyDrive(m_device.FDD[0], chkPresentA, txtPathA, chkProtectA);
            applyDrive(m_device.FDD[1], chkPresentB, txtPathB, chkProtectB);
//            applyDrive(m_device.FDD[2], chkPresentC, txtPathC, chkProtectC);
  //          applyDrive(m_device.FDD[3], chkPresentD, txtPathD, chkProtectD);
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
            string fileName = txtPath.Text;
            if (fileName != string.Empty)
            {
				if (!File.Exists(Path.GetFullPath(fileName)) && chkPresent.Checked)
                    throw new FileNotFoundException(string.Format("File not found: \"{0}\"", fileName));
                fileName = Path.GetFullPath(fileName);
            }
            
            diskImage.Present = chkPresent.Checked;
            diskImage.FileName = fileName;
            diskImage.IsWP = chkProtect.Checked;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            int drive = sender == btnBrowseB ? 1 : 0;
            TextBox[] pathTxt = new TextBox[] { txtPathA, txtPathB};
            CheckBox[] wpChk = new CheckBox[] { chkProtectA, chkProtectB};

            OpenFileDialog loadDialog = new OpenFileDialog();
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
            if (loadDialog.ShowDialog() != DialogResult.OK) return;

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

        private void updateEnabled()
        {
            setEnabled(txtPathA, chkProtectA, btnBrowseA, chkPresentA);
            setEnabled(txtPathB, chkProtectB, btnBrowseB, chkPresentB);
//            setEnabled(txtPathC, chkProtectC, btnBrowseC, chkPresentC);
  //          setEnabled(txtPathD, chkProtectD, btnBrowseD, chkPresentD);
        }

        private void setEnabled(TextBox txtPath, CheckBox chkProtect, Button btnBrowse, CheckBox chkPresent)
        {
            txtPath.Enabled = chkPresent.Checked;
            chkProtect.Enabled = chkPresent.Checked;
            btnBrowse.Enabled = chkPresent.Checked;
        }
    }
}
