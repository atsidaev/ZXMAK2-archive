using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Plugins.Memory;
using ZXMAK2.Engine.Interfaces;

namespace Sprinter
{
    public class SprinterRTC : BusDeviceBase
    {

        IBusManager bus;
        bool sandbox;
        FileStream eepromFile;
        byte[] eeprom;
        byte addr;
        
        public SprinterRTC()
        {
            eeprom = new byte[256];
        }
        
        #region IBusDevice Members

        public override void BusConnect()
        {
            if (!sandbox)
            {
                using (eepromFile = File.Open(GetPath(), FileMode.OpenOrCreate))
                {
                    if (eepromFile.Length < 256)
                        eepromFile.Write(eeprom, 0, 256);
                    else
                        eepromFile.Read(eeprom, 0, 256);

                    eepromFile.Flush();
                    eepromFile.Close();
                }

            }
        }


        public override void BusDisconnect()
        {
            if (!sandbox)
            {
                using (eepromFile = File.Open(GetPath(), FileMode.OpenOrCreate))
                {
                    eepromFile.Write(eeprom, 0, 256);
                    eepromFile.Flush();
                    eepromFile.Close();
                }
            }
        }

        public override void BusInit(IBusManager bmgr)
        {
            sandbox = bmgr.IsSandbox;
            bus = bmgr;

            if (!sandbox)
            {
                bus.SubscribeRESET(Reset);
                bus.SubscribeWRIO(0xFFFF, 0xBFBD, CMOS_DWR);  //CMOS_DWR
                bus.SubscribeWRIO(0xFFFF, 0xDFBD, CMOS_AWR);  //CMOS_AWR
                bus.SubscribeRDIO(0xFFFF, 0xFFBD, CMOS_DRD);  //CMOS_DRD
            }
        }

        public override BusCategory Category { get { return BusCategory.Other; } }

        public override string Description { get { return "Sprinter RTC"; } }

        public override string Name { get { return "Sprinter RTC"; } }

        #endregion


        #region Bus

        void Reset()
        {
            addr = 0;
        }


        /// <summary>
        /// RTC control port
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="val"></param>
        /// <param name="iorqge"></param>


        /// <summary>
        /// RTC address port
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="val"></param>
        /// <param name="iorqge"></param>
        void CMOS_AWR(ushort addr, byte val, ref bool iorqge)
        {
                this.addr = val;
        }

        /// <summary>
        /// RTC write data port
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="val"></param>
        /// <param name="iorqge"></param>
        void CMOS_DWR(ushort addr, byte val, ref bool iorqge)
        {
                WrCMOS(val);
        }

        /// <summary>
        /// RTC read data port
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="val"></param>
        /// <param name="iorqge"></param>
        void CMOS_DRD(ushort addr, ref byte val, ref bool iorqge)
        {
                val = RdCMOS();
        }

        #endregion


        #region RTC emu

        DateTime dt = DateTime.Now;
        bool UF = false;

        byte RdCMOS()
        {
            var curDt = DateTime.Now;

            if (curDt.Subtract(dt).Seconds > 0 || curDt.Millisecond / 500 != dt.Millisecond/500)
            {
                dt = curDt;
                UF = true;
            }

            switch (addr)
            {
                case 0x00:
                    return BDC(dt.Second);
                case 0x02:
                    return BDC(dt.Minute);
                case 0x04:
                    return BDC(dt.Hour);
                case 0x06:
                    return (byte)(dt.DayOfWeek);
                case 0x07:
                    return BDC(dt.Day);
                case 0x08:
                    return BDC(dt.Month);
                case 0x09:
                    return BDC(dt.Year % 100);
                case 0x0A:
                    return 0x00;
                case 0x0B:
                    return 0x02;
                case 0x0C:
                    var res = (byte)(UF ? 0x1C : 0x0C);
                    UF = false;
                    return res;
                case 0x0D:
                    return 0x80;

                default:
                    return eeprom[addr];
            }
        }

        void WrCMOS(byte val)
        {
            if (addr < 0xF0)
                eeprom[addr] = val;
        }

        byte BDC(int val)
        {
            var res = val;

            if ((eeprom[11] & 4) == 0)
            {
                var rem = 0;
                res = Math.DivRem(val, 10, out rem);
                res = (res * 16 + rem);
            }

            return (byte)res;
        }

        #endregion

        string GetPath()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "eeprom.bin");
        }
    }
}
