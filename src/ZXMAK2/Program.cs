using System;
using System.IO;
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
				using (FormMain form = new FormMain())
				{
					form.Show();
					form.InitWnd();
					if (args.Length > 0 && File.Exists(args[0]))
						form.StartupImage = Path.GetFullPath(args[0]);
					Application.Run(form);
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
