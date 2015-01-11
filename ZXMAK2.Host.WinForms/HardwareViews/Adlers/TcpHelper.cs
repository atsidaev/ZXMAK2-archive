using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;
using ZXMAK2.Interfaces;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;

namespace ZXMAK2.Hardware.Adlers.UI
{
    public partial class TcpHelper : Form
    {
        private static WebClient m_client;

        public TcpHelper()
        {
            InitializeComponent();

            m_client = new WebClient();
            m_client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
        }

        private void checkBoxIsProxy_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = checkBoxIsProxy.Checked;

            this.textBoxProxyAdress.Enabled = isChecked;
            this.textBoxProxyPort.Enabled = isChecked;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            try
            {
                m_client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                m_client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                if(checkBoxIsProxy.Checked)
                {
                    WebProxy proxy = new WebProxy("http://" + this.textBoxProxyAdress.Text.Trim() + ":" + this.textBoxProxyPort.Text.Trim() + "/",true);
                    m_client.Proxy = proxy;
                }

                labelStatusText.Text = "Downloading...";
                buttonStart.Enabled = false;

                m_client.DownloadFileAsync(new Uri(@"http://download-codeplex.sec.s-msft.com/Download/Release?ProjectName=pasmo2&DownloadId=991511&FileTime=130631149681430000&Build=20959"), "Pasmo2.dll");
            }
            catch(Exception tcpException)
            {
                Locator.Resolve<IUserMessage>()
                    .Error("Error: \n" + tcpException.Message);
            }
        }


        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.progressBarDownloadStatus.Value = e.ProgressPercentage;
        }
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            labelStatusText.Text = "Completed!";
            buttonStart.Enabled = true;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (m_client.IsBusy)
                m_client.CancelAsync();

            labelStatusText.Text = "Canceled";
            buttonStart.Enabled = true;

            this.Hide();
        }
    }
}
