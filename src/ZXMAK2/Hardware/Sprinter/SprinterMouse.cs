using System;
using System.Collections.Generic;

using ZXMAK2.Host.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.Sprinter
{
    public class SprinterMouse : BusDeviceBase, IMouseDevice
    {
        private IMouseState m_mouseState;
        private int m_mouseX;
        private int m_mouseY;
        private int m_mouseBtn;
        private bool m_swapbtns;

        private Queue<byte> m_msbuf = new Queue<byte>();

        public override void BusConnect()
        {
            m_swapbtns = true;
                 
        }

        public override void BusDisconnect()
        {
        }

        public override void BusInit(IBusManager bmgr)
        {
//            bmgr.SubscribeRDIO(0xffff, 0xfadf, new BusReadIoProc(this.readPortFADF));
  //          bmgr.SubscribeRDIO(0xffff, 0xfbdf, new BusReadIoProc(this.readPortFBDF));
    //        bmgr.SubscribeRDIO(0xffff, 0xffdf, new BusReadIoProc(this.readPortFFDF));
            bmgr.SubscribeRdIo(0x00ff, 0x001B, new BusReadIoProc(readMouseState));
            bmgr.SubscribeRdIo(0x00ff, 0x001A, new BusReadIoProc(readMouseData));
        }

        private void readMouseState(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;

                value = (byte)((m_msbuf.Count > 0) ? 1 : 0);
            }
        }

        private void readMouseData(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;

                //            value = (byte)((m_kbd_buff.Length > 0) ? 1 : 0);
                if (m_msbuf.Count > 0)
                {
                    value = m_msbuf.Dequeue();
                }
                else
                {
                    value = 0;
                }
            }
        }

        public IMouseState MouseState
        {
            get
            {
                return this.m_mouseState;
            }
            set
            {
//                m_mouseState = value;
                /*bool different = false;
                if (value != null)
                {
                    Logger.GetLogger().LogTrace(String.Format("Mouse X = {0}, Y = {1}, Btns = {2}", value.X, value.Y, value.Buttons));
                    if (m_mouseX != value.X)
                    {
//                        m_mouseX = value.X;
                        different = true;
                    }
                    if (m_mouseY!= value.Y)
                    {
                        
//                        m_mouseY = value.Y;
                        different = true;
                    }
                    if (m_mouseBtn != value.Buttons)
                    {
                        m_mouseBtn = value.Buttons;
                        different = true;
                    }
                }*/
/*                if (this.m_mouseState == null)
                {
                    this.m_mouseState = value;
                    different = true;
                }
                else
                {
                    if (this.)
                    if (!((this.m_mouseState.Buttons == value.Buttons) && (this.m_mouseState.Y == value.Y) && (this.m_mouseState.X == value.X)))
                    {
                        this.m_mouseState = value;
                        different = true;
                    }
                }*/
                if (!((m_mouseBtn== value.Buttons) && (m_mouseY == value.Y) && (m_mouseX == value.X)))
                //if (different)
                {
                    byte my;
                    my = (byte)(Math.Abs(m_mouseY - value.Y) / 2);
                    if ((m_mouseY - value.Y) > 0)
                    {
                        my ^= 0x7f;
                        my |= 128;
                    }
                    byte mx;
                    mx = (byte)(Math.Abs(m_mouseX - value.X) / 2);
                    if ((m_mouseX - value.X) > 0)
                    {
                        mx ^= 0x7f;
                        mx |= 128;
                    }
                    m_mouseX = value.X;
                    m_mouseY = value.Y;
                    if (m_swapbtns)
                    {
                        m_mouseBtn = ((value.Buttons & 0x01) << 1) | ((value.Buttons & 0x02) >> 1);
                    } else m_mouseBtn = value.Buttons;

                    m_mouseState = value;
                    byte b1 = (byte)(64 + ((m_mouseBtn & 3) << 4) + (((my) & 192) >> 4) + (((mx) & 192) >> 6));
                    m_msbuf.Enqueue(b1);
                    b1 = (byte)((mx) & 63);
                    m_msbuf.Enqueue(b1);
                    b1 = (byte)((my) & 63);
                    m_msbuf.Enqueue(b1);
                    //Logger.GetLogger().LogTrace(String.Format("Mouse event start, Buffer size = {0}", m_msbuf.Count));
                }
            }
        }

        public override BusDeviceCategory Category
        {
            get
            {
                return BusDeviceCategory.Mouse;
            }
        }

        public override string Name
        {
            get { return "MOUSE SPRINTER"; }
        }

        public override string Description
        {
            get { return "Standart Sprinter Mouse"; }
        }
    }
}
