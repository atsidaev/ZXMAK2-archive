using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Interfaces
{
    public interface IJtagDevice //: BusDeviceBase
    {
        void Attach(IDebuggable dbg);
        void Detach();
    }

    //public class JtagTest : IBusDevice, IJtagDevice
    //{
    //    #region IBusDevice

    //    public string Name { get { return "JtagTest"; } }
    //    public string Description { get { return "Test JtagDevice"; } }
    //    public BusCategory Category { get { return BusCategory.Other; } }

    //    private int m_busOrder=0;
    //    public int BusOrder
    //    {
    //        get { return m_busOrder; }
    //        set { m_busOrder = value; }
    //    }

    //    public void BusInit(IBusManager bmgr)
    //    {
    //    }

    //    public void BusConnect()
    //    {
    //    }

    //    public void BusDisconnect()
    //    {
    //    }

    //    #endregion

    //    #region IJtagDevice

    //    public void Attach(IDebuggable dbg)
    //    {
    //        System.Windows.Forms.MessageBox.Show("Debugger Attach");
    //    }

    //    public void Detach()
    //    {
    //        System.Windows.Forms.MessageBox.Show("Debugger Detach");
    //    }

    //    #endregion
    //}
}
