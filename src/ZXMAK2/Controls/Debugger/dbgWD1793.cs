using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.Interfaces;


namespace ZXMAK2.Controls.Debugger
{
   public partial class dbgWD1793 : Form
   {
      private IBetaDiskDevice _betaDiskDevice;

      public dbgWD1793(IBetaDiskDevice debugTarget)
      {
          _betaDiskDevice = debugTarget;
         InitializeComponent();
      }

      protected override void OnShown(EventArgs e)
      {
         base.OnShown(e);
      }

      protected override void OnFormClosed(FormClosedEventArgs e)
      {
         base.OnFormClosed(e);
      }

      private void timerUpdate_Tick(object sender, EventArgs e)
      {
		  if (_betaDiskDevice != null)
			  label1.Text = _betaDiskDevice.DumpState();
		  else
			  label1.Text = "Beta Disk interface not found";
      }
   }
}