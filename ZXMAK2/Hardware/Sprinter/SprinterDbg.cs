using System;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

using ZXMAK2.Controls;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.MVP.Interfaces;
using ZXMAK2.MVP.WinForms;
using ZXMAK2.Hardware.Sprinter.UI;

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
                m_viewHolder.Arguments = new object[] { dbg };
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
                m_viewHolder = new ViewHolder<DebugForm>("Debugger");
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        #endregion IGuiExtension
    }
}
