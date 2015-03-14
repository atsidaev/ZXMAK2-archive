using System;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;

namespace ZXMAK2.Hardware.Adlers.Views
{
    public partial class TcpHelper : Form
    {
        private static WebClient m_client;

        public TcpHelper()
        {
            InitializeComponent();
            _proxyAddress = _proxyPort = string.Empty;

            m_client = new WebClient();
            m_client.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0)");
        }

        #region setters/getters
            private string _proxyAddress;
            public bool SetProxyAddress(string i_newAddress)
            {
                Match match = Regex.Match(i_newAddress, @"\b\d{2}\.\d{2}\.\d{2}\.\d{2}\b", RegexOptions.IgnoreCase);
                if (!match.Success)
                    return false;
                _proxyAddress = i_newAddress;

                return true;
            }
            private string _proxyPort;

        #endregion

        #region GUI
        private void checkBoxIsProxy_CheckedChanged(object sender, EventArgs e)
        {
            this.textBoxProxyPort.Enabled = this.textBoxProxyAdress.Enabled = checkBoxIsProxy.Checked;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            //download Pasmo2.dll
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

                m_client.DownloadFileAsync(new Uri(@"http://download-codeplex.sec.s-msft.com/Download/Release?ProjectName=pasmo2&DownloadId=1437196&FileTime=130703844002230000&Build=20959"), "Pasmo2.dll");
            }
            catch(Exception tcpException)
            {
                Logger.Error(tcpException);
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
        #endregion
    }
}
