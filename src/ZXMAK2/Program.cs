using System;
using System.Windows.Forms;
using ZXMAK2.Controls;


namespace ZXMAK2
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			LogAgent.Start();
            try
            {
                using (var form = new FormMain(args))
                {
                    Application.Run(form);
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                LogAgent.Error(ex);
                if (!ex.FileName.Contains("Microsoft.DirectX"))
                {
                    MessageBox.Show(
                        string.Format("{0}\n\n{1}", ex.GetType(), ex.Message),
                        "ZXMAK2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(
                        string.Format(
                        "{0}\n\n{1}\n\nPlease install DirectX End-User Runtime and try again!", ex.GetType(), ex.Message),
                        "ZXMAK2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                MessageBox.Show(
                    string.Format("{0}\n\n{1}", ex.GetType(), ex.Message),
                    "ZXMAK2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
			finally
			{
				LogAgent.Finish();
			}
		}
	}
}
