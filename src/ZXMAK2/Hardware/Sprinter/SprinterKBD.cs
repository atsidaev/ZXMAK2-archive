using System;
using System.Text;
using System.Collections.Generic;

using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;


namespace ZXMAK2.Hardware.Sprinter
{
    public class SprinterKeyboard : BusDeviceBase, IKeyboardDevice
    {
        private IKeyboardState m_keyboardState;
//        private long m_intState;
//        private Microsoft.DirectX.DirectInput.Device m_keyb;
        //private KeyboardDevice m_zxkbd;

        private Queue<byte> m_kbdbuf = new Queue<byte>();
        //private List<int> m_kbdhits = new List<int>();
        private KbdHits m_kbdhits = new KbdHits();

        #region ScanCodes
        private Key[] m_kbd_sc_num = {
                                         Key.Escape,
                                         Key.F1,
                                         Key.F2,
                                         Key.F3,
                                         Key.F4,
                                         Key.F5,
                                         Key.F6,
                                         Key.F7,
                                         Key.F8,
                                         Key.F9,
                                         Key.F10,
                                         Key.F11,
                                         Key.F12,
                                         Key.Delete,
                                         Key.Insert,
                                         Key.UpArrow,
                                         Key.DownArrow,
                                         Key.LeftArrow,
                                         Key.RightArrow,
                                         Key.Home,
                                         Key.End,
                                         Key.PageUp,
                                         Key.PageDown,
                                         Key.LeftAlt,
                                         Key.RightAlt,
                                         Key.Space,
                                         Key.LeftControl,
                                         Key.RightControl,
                                         Key.LeftShift,
                                         Key.RightShift,
                                         Key.NumPadEnter,
                                         Key.CapsLock,
                                         Key.Tab,
                                         Key.Grave,    //Должно быть `~ Equals?
                                         Key.D1,
                                         Key.D2,
                                         Key.D3,
                                         Key.D4,
                                         Key.D5,
                                         Key.D6,
                                         Key.D7,
                                         Key.D8,
                                         Key.D9,
                                         Key.D0,
                                         Key.Minus,//NumPadMinus
                                         Key.Equals,//NumPadPlus
                                         Key.BackSpace,

                                         Key.Q,
                                         Key.W,
                                         Key.E,
                                         Key.R,
                                         Key.T,

                                         Key.Y,
                                         Key.U,
                                         Key.I,
                                         Key.O,
                                         Key.P,

                                         Key.A,
                                         Key.S,
                                         Key.D,
                                         Key.F,
                                         Key.G,

                                         Key.H,
                                         Key.J,
                                         Key.K,
                                         Key.L,

                                         Key.Z,
                                         Key.X,
                                         Key.C,
                                         Key.V,
                                         Key.B,
                                         Key.N,
                                         Key.M,

                                         Key.Apostrophe,    //"'"
                                         Key.BackSlash,     //"\"

                                         Key.Return,        //Enter
                                         Key.Period,    //"."
//                                         Key.NumPadSlash,
                                         Key.SemiColon, //";"
                                         Key.Slash,      //"/"
                                         Key.Comma,      //","
                                         Key.LeftBracket,   //"["
                                         Key.RightBracket   //"]"
                                     };

        private byte[][] m_kbd_scancodes = new byte[][] {
                                               new byte[]{0x76,0x00}, //Esc,        110
                                               new byte[]{0x05,0x00}, //F1,         112
                                               new byte[]{0x06,0x00}, //F2,         113
                                               new byte[]{0x04,0x00}, //F3,         114
                                               new byte[]{0x0c,0x00}, //F4,         115
                                               new byte[]{0x03,0x00}, //F5,         116
                                               new byte[]{0x0B,0x00}, //F6,         117
                                               new byte[]{0x83,0x00}, //F7,         118
                                               new byte[]{0x0A,0x00}, //F8,         119
                                               new byte[]{0x01,0x00}, //F9,         120
                                               new byte[]{0x09,0x00}, //F10,        121
                                               new byte[]{0x78,0x00}, //F11,        122
                                               new byte[]{0x07,0x00}, //F12,        123
                                               new byte[]{0xE0,0x71}, //Del,        76
                                               new byte[]{0xE0,0x70}, //Ins,        75
                                               new byte[]{0xE0,0x75}, //UpArrow,    83
                                               new byte[]{0xE0,0x72}, //DownArrow, 84
                                               new byte[]{0xE0,0x6B}, //LeftArrow, 79
                                               new byte[]{0xE0,0x74}, //RightArrow, 89
                                               new byte[]{0xE0,0x6c}, //Home, 80
                                               new byte[]{0xE0,0x69}, //End, 81
                                               new byte[]{0xE0,0x7D}, //PageUp, 85
                                               new byte[]{0xE0,0x7A}, //PageDn, 86
                                               new byte[]{0x11,0x00}, //LeftAlt, 60
                                               new byte[]{0xE0,0x11}, //RightAlt, 62
                                               new byte[]{0x29,0x00}, //Space, 61
                                               new byte[]{0x14,0x00}, //LeftCtrl, 58
                                               new byte[]{0xE0,0x14}, //RightCtrl, 64
                                               new byte[]{0x12,0x00}, //LeftShift, 44
                                               new byte[]{0x59,0x00}, //RightShift, 57
                                               new byte[]{0x5A,0x00}, //Enter, 43
                                               new byte[]{0x58,0x00}, //CapsLock, 30
                                               new byte[]{0x0D,0x00}, //Tab, 16
                                               new byte[]{0x0E,0x00}, //`~, 1
                                               new byte[]{0x16,0x00}, //1, 2
                                               new byte[]{0x1E,0x00}, //2, 3
                                               new byte[]{0x26,0x00}, //3, 4
                                               new byte[]{0x25,0x00}, //4, 5
                                               new byte[]{0x2e,0x00}, //5, 6
                                               new byte[]{0x36,0x00}, //6, 7
                                               new byte[]{0x3D,0x00}, //7, 8
                                               new byte[]{0x3E,0x00}, //8, 9
                                               new byte[]{0x46,0x00}, //9, 10
                                               new byte[]{0x45,0x00}, //0, 11
                                               new byte[]{0x4E,0x00}, //-, 12
                                               new byte[]{0x55,0x00}, //+, 13
                                               new byte[]{0x66,0x00}, //BackSpace, 15

                                               new byte[]{0x15,0x00}, //q, 17
                                               new byte[]{0x1D,0x00}, //w, 18
                                               new byte[]{0x24,0x00}, //e, 19
                                               new byte[]{0x2D,0x00}, //r, 20
                                               new byte[]{0x2C,0x00}, //t, 21

                                               new byte[]{0x35,0x00}, //y, 22
                                               new byte[]{0x3C,0x00}, //u, 23
                                               new byte[]{0x43,0x00}, //i, 24
                                               new byte[]{0x44,0x00}, //o, 25
                                               new byte[]{0x4D,0x00}, //p, 26

                                               new byte[]{0x1C,0x00}, //a, 31
                                               new byte[]{0x1B,0x00}, //s, 32
                                               new byte[]{0x23,0x00}, //d, 33
                                               new byte[]{0x2B,0x00}, //f, 34
                                               new byte[]{0x34,0x00}, //g, 35
                                               
                                               new byte[]{0x33,0x00}, //h, 36
                                               new byte[]{0x3B,0x00}, //j, 37
                                               new byte[]{0x42,0x00}, //k, 38
                                               new byte[]{0x4B,0x00}, //l, 39

                                               new byte[]{0x1A,0x00}, //z, 46
                                               new byte[]{0x22,0x00}, //x, 47
                                               new byte[]{0x21,0x00}, //c, 48
                                               new byte[]{0x2A,0x00}, //v, 49
                                               new byte[]{0x32,0x00}, //b, 50
                                               new byte[]{0x31,0x00}, //n, 51
                                               new byte[]{0x3A,0x00}, //m, 52

                                               new byte[]{0x52,0x00}, //apostrof, 41
                                               new byte[]{0x5D,0x00}, //BackSlash, 55
                                               new byte[]{0x5A,0x00}, //Return, 43
                                               new byte[]{0x49,0x00}, //Period, 54
//                                               new byte[]{0x5D,0x00}, //NumpadSlash, 95 "\"
                                               new byte[]{0x4C,0x00}, //SemiColon, 40 ";"
                                               new byte[]{0x4A,0x00}, //Slash, 104
                                               new byte[]{0x41,0x00}, //Comma, 53
                                               new byte[]{0x54,0x00}, //"[", 27
                                               new byte[]{0x5B,0x00} //"]", 28

                };

        #endregion

        //        private byte[][] m_kbd_buff = new byte[16][];

/*        public KeyboardSprinter()
        {
            for (int i = 0; i<m_kbd_buff.Length;i++)
                m_kbd_buff[i] = new byte[0x04];
        }*/

        public override void BusConnect()
        {
        }

        public override void BusDisconnect()
        {
        }

        public override void BusInit(IBusManager bmgr)
        {

//            bmgr.SubscribeRDIO(1, 0, new BusReadIoProc(this.readPortFE));
//            m_zxkbd = bmgr.FindDevice(typeof(KeyboardDevice)) as KeyboardDevice;
//            if (m_zxkbd == null) throw new ApplicationException("Standart ZX Keyboard Device not found");
            //InitializeKeyboard();
            bmgr.SubscribeEndFrame(new BusFrameEventHandler(ScanKeys));
            bmgr.SubscribeRDIO(0x00ff, 0x0019, new BusReadIoProc(readKbdState));
            bmgr.SubscribeRDIO(0x00ff, 0x0018, new BusReadIoProc(readKbdData));
        }

        private void readKbdState(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;

                value = (byte)((m_kbdbuf.Count > 0) ? 1 : 0);
            }
        }

        private void readKbdData(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;

                //            value = (byte)((m_kbd_buff.Length > 0) ? 1 : 0);
                if (m_kbdbuf.Count > 0)
                {
                    value = m_kbdbuf.Dequeue();
                }
                else
                {
                    value = 0;
                }
            }
        }

        public override BusDeviceCategory Category
        {
            get
            {
                return BusDeviceCategory.Keyboard;
            }
        }

        public IKeyboardState KeyboardState
        {
            get
            {
                return this.m_keyboardState;
            }
            set
            {
                this.m_keyboardState = value;

//                this.m_intState = scanState(this.m_keyboardState);
            }
        }

/*        public void InitializeKeyboard()
        {
            m_keyb = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Keyboard);
            m_keyb.SetCooperativeLevel(null , CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
            m_keyb.Acquire();
        }*/

        private void ScanKeys()
        {
            if (m_keyboardState != null)
            {
                for (int num = 0; num < m_kbdhits.Count;num++ )
                {
                    if (!m_keyboardState[m_kbd_sc_num[m_kbdhits.Items[num].Code]])
                    {
                        if (m_kbd_scancodes[m_kbdhits.Items[num].Code][0] == 0xE0)
                        {
                            m_kbdbuf.Enqueue(m_kbd_scancodes[m_kbdhits.Items[num].Code][0]);
                            m_kbdbuf.Enqueue(0xF0);
                            m_kbdbuf.Enqueue(m_kbd_scancodes[m_kbdhits.Items[num].Code][1]);
                        }
                        else
                        {
                            m_kbdbuf.Enqueue(0xF0);
                            m_kbdbuf.Enqueue(m_kbd_scancodes[m_kbdhits.Items[num].Code][0]);
                        }
                        m_kbdhits.Remove(num);
                    }
                    else
                    {
                        m_kbdhits.Items[num].Frames += 1;
                        if ((m_kbdhits.Items[num].Frames == 15) && (m_kbdhits.Items[num].First))
                        {
                            m_kbdbuf.Enqueue(m_kbd_scancodes[m_kbdhits.Items[num].Code][0]);
                            if (m_kbd_scancodes[m_kbdhits.Items[num].Code][1] != 0) m_kbdbuf.Enqueue(m_kbd_scancodes[m_kbdhits.Items[num].Code][1]);
                            m_kbdhits.Items[num].Frames = 0;
                        }
                        else
                        {
                            if ((m_kbdhits.Items[num].Frames == 3) && (!m_kbdhits.Items[num].First))
                            {
                                m_kbdbuf.Enqueue(m_kbd_scancodes[m_kbdhits.Items[num].Code][0]);
                                if (m_kbd_scancodes[m_kbdhits.Items[num].Code][1] != 0) m_kbdbuf.Enqueue(m_kbd_scancodes[m_kbdhits.Items[num].Code][1]);
                                m_kbdhits.Items[num].Frames = 0;
                            }
                        }
                    }
                }
                for (int num = 0; num < m_kbd_sc_num.Length; num++)
                {
                    if (m_keyboardState[m_kbd_sc_num[num]])
                    {
                        if (!m_kbdhits.Contains(num))
                        {
                            m_kbdhits.Add(num);
                            m_kbdbuf.Enqueue(m_kbd_scancodes[num][0]);
                            if (m_kbd_scancodes[num][1] != 0) m_kbdbuf.Enqueue(m_kbd_scancodes[num][1]);
                        }
                        //                    m_kbdhits.Enqueue(num);
                    }

                }
            }
            
        }

        public override string Description
        {
            get
            {
                return "Sprinter AT Keyboard";
            }
        }

        public override string Name
        {
            get
            {
                return "Sprinter AT Keyboard";
            }
        }

    }
}
