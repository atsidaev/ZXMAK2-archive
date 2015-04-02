using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ZXMAK2.Hardware.Adlers.Views.CustomControls
{
    public partial class ProgressBarCustom : Form
    {
        private bool _isStarted = false;
        private bool _isCanceled = false;

        BackgroundWorker _backgroundWorker = new BackgroundWorker()
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        Action _workerFunc;

        public ProgressBarCustom(string i_progressTitle, string i_progressSubstring = null)
        {
            InitializeComponent();

            this.Text = i_progressTitle;
            if (i_progressSubstring != null)
                labelProcessInfo.Text = i_progressSubstring;

            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += new DoWorkEventHandler(_backgroundWorker_DoWork);
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(_backgroundWorker_ProgressChanged);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_backgroundWorker_RunWorkerCompleted);
        }

        public void Init(Action workerFunc, int i_maxValue)
        {
            progressBar.Maximum = i_maxValue;
            _workerFunc = workerFunc;
        }

        public void Start()
        {
            if (_isStarted)
                _isCanceled = true;
            else
            {
                _isStarted = true;
                _isCanceled = false;
            }
            this.Show();
            _backgroundWorker.RunWorkerAsync();
        }
        public void IncCounter()
        {
            progressBar.Value++;
        }
        public void Cancel()
        {
            _isCanceled = true;
        }
        public bool IsCanceled()
        {
            return _isCanceled;
        }
        public bool IsFinished()
        {
            return progressBar.Value >= progressBar.Maximum;
        }

        public void Finish()
        {
            this.Hide();
            _isStarted = false;
            _backgroundWorker.CancelAsync();
        }

        #region GUI
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            _isCanceled = true;
            Finish();
        }
        #endregion GUI

        #region Background worker

        public BackgroundWorker GetBackgroundWorker()
        {
            return _backgroundWorker;
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _workerFunc();
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value++;
        }

        private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Finish();
        }
        
        #endregion Background worker
    }
}
