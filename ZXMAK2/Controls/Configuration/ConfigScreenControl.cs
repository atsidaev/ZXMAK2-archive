using System;
using ZXMAK2.Engine.Bus;
using System.Windows.Forms;


namespace ZXMAK2.Controls.Configuration
{
    public abstract class ConfigScreenControl : UserControl
    {
        public abstract void Apply();
    }
}
