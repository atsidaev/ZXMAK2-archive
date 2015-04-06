using System.Windows.Forms;

namespace ZXMAK2.Hardware.Adlers.Views.AssemblerView
{
    public partial class Settings : Form
    {
        //instance
        private static Settings m_instance = null;

        private Settings()
        {
            InitializeComponent();
        }

        public static void ShowForm()
        {
            if (m_instance == null || m_instance.IsDisposed)
            {
                m_instance = new Settings();
                //m_instance.LoadConfig();
                m_instance.ShowInTaskbar = true;
                m_instance.ShowDialog();
            }
            else
                m_instance.Show();
        }
        public static Settings GetInstance()
        {
            return m_instance;
        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            m_instance.Hide();
        }
    }
}
