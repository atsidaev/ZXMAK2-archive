using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using UI = ZXMAK2.Controls.Debugger;
using ZXMAK2.Entities;
using ZXMAK2.MVP.Interfaces;
using ZXMAK2.MVP.WinForms;
using ZXMAK2.Hardware.General.UI;

namespace ZXMAK2.Hardware.General
{
    public class Debugger : BusDeviceBase, IJtagDevice
    {
        #region BusDeviceBase

        public override string Name { get { return "DEBUGGER"; } }
        public override string Description { get { return "Default Debugger"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Debugger; } }

        private IViewHolder m_viewHolder;


        public Debugger()
        {
            CreateViewHolder();
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

        
        #region IGuiExtension

        private void CreateViewHolder()
        {
            try
            {
                m_viewHolder = new ViewHolder<FormCpu>("Debugger");
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        #endregion IGuiExtension
    }
}
