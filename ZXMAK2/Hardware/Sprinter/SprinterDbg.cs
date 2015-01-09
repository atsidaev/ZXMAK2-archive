using System;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

using ZXMAK2.Controls;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.Sprinter.UI;
using ZXMAK2.Dependency;
using ZXMAK2.Presentation.Interfaces;
using ZXMAK2.MVP;
using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Hardware.Sprinter
{
    public class SprinterDebugger : BusDeviceBase, IJtagDevice
    {
        private IViewHolder m_viewHolder;


        public SprinterDebugger()
        {
            CreateViewHolder();
        }


        #region IJtagDevice

        public void Attach(IDebuggable dbg)
        {
            if (m_viewHolder != null && dbg != null)
            {
                m_viewHolder.Arguments = new [] { new Argument("debugTarget", dbg) };
            }
        }

        public void Detach()
        {
            if (m_viewHolder != null)
            {
                m_viewHolder.Close();
                m_viewHolder.Arguments = null;
            }
        }

        #endregion

        
        #region IBusDevice

        public override BusDeviceCategory Category { get { return BusDeviceCategory.Debugger; } }

        public override string Name
        {
            get { return "DEBUGGER SPRINTER"; }
        }

        public override string Description
        {
            get { return "Sprinter debugger"; }
        }

        public override void BusInit(IBusManager bmgr)
        {
            if (m_viewHolder != null)
            {
                bmgr.AddCommandUi(m_viewHolder.CommandOpen);
            }
        }

        public override void BusConnect()
        {
            
        }

        public override void BusDisconnect()
        {
            if (m_viewHolder != null)
            {
                m_viewHolder.Close();
            }
        }

        #endregion


        #region IGuiExtension

        private void CreateViewHolder()
        {
            try
            {
                var resolver = Locator.Resolve<IViewResolver>();
                m_viewHolder = new ViewHolder<IDebuggerSprinterView>(resolver, "Debugger");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion IGuiExtension
    }
}
