using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXMAK2.Dependency;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Host.Presentation;
using ZXMAK2.Host.Presentation.Interfaces;

namespace ZXMAK2.Hardware.FuzixDebugger
{
    public class Debugger : BusDeviceBase, IJtagDevice
    {
        private IBusManager _busManager;
        private IViewHolder _viewHolder;

        public Debugger()
        {
            Category = BusDeviceCategory.Debugger;
            Name = "FUZIX debugger";
            Description = "Debugger for SDCC binaries specially targeted at FUZIX binaries";

            try
            {
                _viewHolder = new ViewHolder<IDebuggerAdlersView>("Fuzix Debugger");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

        }

        #region BusDeviceBase
        public override void BusInit(IBusManager bmgr)
        {
            if (_viewHolder != null)
                bmgr.AddCommandUi(_viewHolder.CommandOpen);

            _busManager = bmgr;
        }
        public override void BusConnect()
        {
        }

        public override void BusDisconnect()
        {
            if (_viewHolder != null)
                _viewHolder.Close();
        }
        #endregion

        #region IJtagDevice
        public void Attach(IDebuggable dbg)
        {
            if (_viewHolder != null && dbg != null)
            {
                _viewHolder.Arguments = new[]
                {
                    new Argument("debugTarget", dbg),
                    new Argument("bmgr", _busManager),
                };
            }
        }

        public void Detach()
        {
            if (_viewHolder != null)
            {
                _viewHolder.Close();
                _viewHolder.Arguments = null;
            }
        }
        #endregion
    }
}
