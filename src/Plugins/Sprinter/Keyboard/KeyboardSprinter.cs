using System;
using System.Collections.Generic;
using System.Text;
//using Microsoft.DirectX.DirectInput;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Devices;

namespace Plugins.Keyboard
{
    class KeyboardSprinter :  IKeyboardDevice, IBusDevice
    {
        private int m_busOrder;
        private IKeyboardState m_keyboardState;
        private long m_intState;
//        private Microsoft.DirectX.DirectInput.Device m_keyb;
        private KeyboardDevice m_zxkbd;

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
                                         Key.Delete
                                     };

        private byte[][] m_kbd_scancodes = new byte[][] {
                                               new byte[]{0x76,0x00}, //Esc
                                               new byte[]{0x05,0x00}, //F1
                                               new byte[]{0x06,0x00}, //F2
                                               new byte[]{0x04,0x00}, //F3
                                               new byte[]{0x0c,0x00}, //F4
                                               new byte[]{0x03,0x00}, //F5
                                               new byte[]{0x0B,0x00}, //F6
                                               new byte[]{0x83,0x00}, //F7
                                               new byte[]{0x0A,0x00}, //F8
                                               new byte[]{0x01,0x00}, //F9
                                               new byte[]{0x09,0x00}, //F10
                                               new byte[]{0xE0,0x71} //Del
                                            };

        private byte[][] m_kbd_buff = new byte[16][];

        public KeyboardSprinter()
        {
            for (int i = 0; i<m_kbd_buff.Length;i++)
                m_kbd_buff[i] = new byte[0x04];
        }

        public void BusConnect()
        {
        }

        public void BusDisconnect()
        {
        }

        public void BusInit(IBusManager bmgr)
        {

//            bmgr.SubscribeRDIO(1, 0, new BusReadIoProc(this.readPortFE));
            m_zxkbd = bmgr.FindDevice(typeof(KeyboardDevice)) as KeyboardDevice;
            if (m_zxkbd == null) throw new ApplicationException("Standart ZX Keyboard Device not found");
            //InitializeKeyboard();
            bmgr.SubscribeEndFrame(new BusFrameEventHandler(ScanKeys));
        }

        public int BusOrder
        {
            get
            {
                return this.m_busOrder;
            }
            set
            {
                this.m_busOrder = value;
            }
        }

        public BusCategory Category
        {
            get
            {
                return BusCategory.Keyboard;
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
            for (int num = 0;num<m_kbd_sc_num.Length;num++)
            {                
                if (m_keyboardState[m_kbd_sc_num[num]]){

                }

            }
            
        }

        public string Description
        {
            get
            {
                return "Sprinter AT Keyboard";
            }
        }

        public string Name
        {
            get
            {
                return "Sprinter AT Keyboard";
            }
        }

    }
}
