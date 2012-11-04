using System;
using System.Windows.Forms;
using ZXMAK2.Engine;


namespace ZXMAK2.Controls.Configuration
{
    public abstract class ConfigScreenControl : UserControl
    {
        public abstract void Apply();
    }
}
