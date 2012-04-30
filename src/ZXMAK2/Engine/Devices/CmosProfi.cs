using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Engine.Devices
{
    public class CmosProfi : BusDeviceBase
    {
        #region IBusDevice Members

        public override string Name { get { return "CMOS PROFI"; } }
        public override string Description { get { return "PROFI CMOS device\nPort:\t#9F"; } }
        public override BusCategory Category { get { return BusCategory.Other; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_memory = bmgr.FindDevice(typeof(IMemoryDevice)) as IMemoryDevice;
            bmgr.SubscribeWRIO(0x009F, 0x009F, busWriteProfi);
            bmgr.SubscribeRDIO(0x009F, 0x009F, busReadProfi);
        }

        public override void BusConnect()
        {
            loadRam();
        }

        public override void BusDisconnect()
        {
            saveRam();
        }

        #endregion


        private byte[] m_ram = new byte[256];
        private byte m_pDFF7 = 0;
        private bool m_modified = false;


        #region Bus Handlers

        private void busWriteProfi(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            if ((m_memory.CMR0 & 0x10) != 0 && (m_memory.CMR1 & 0x20) != 0)
            {
                iorqge = true;
                if ((addr & 0x20) != 0)
                    m_pDFF7 = value;
                else
                    cmos_write(m_pDFF7, value);
            }
        }

        private void busReadProfi(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            if ((m_memory.CMR0 & 0x10) != 0 && (m_memory.CMR1 & 0x20) != 0)
            {
                if ((addr & 0x20) == 0)
                {
                    iorqge = false;
                    value = cmos_read(m_pDFF7);
                }
            }
        }

        #endregion

        private IMemoryDevice m_memory;
        private DateTime m_dt = DateTime.Now;
        private int m_second = 0;
        private bool m_uf = false;


        private void cmos_write(byte addr, byte value)
        {
            m_modified |= m_ram[addr] != value;
            m_ram[addr] = value;
        }

        private byte cmos_read(byte addr)
        {
            byte value = 0;

            m_dt = DateTime.Now;
            if (m_second != m_dt.Second)
            {
                m_uf = true;
                m_second = m_dt.Second;
            }
            switch (addr)
            {
                case 0: value = bcd((byte)m_dt.Second); break;
                case 2: value = bcd((byte)m_dt.Minute); break;
                case 4: value = bcd((byte)m_dt.Hour); break;
                case 6: value = bcd((byte)m_dt.DayOfWeek); break;   // new: 1+(((BYTE)st.wDayOfWeek+8-conf.cmos) % 7);
                case 7: value = bcd((byte)m_dt.Day); break;
                case 8: value = bcd((byte)m_dt.Month); break;
                case 9: value = bcd((byte)(m_dt.Year % 100)); break;
                case 10: value = (byte)(0x20 | (m_ram[10] & 0x0F)); break;
                case 11: value = (byte)(0x02 | (m_ram[11] & 0x04)); break;
                case 12:
                    value = (byte)(m_uf ? 0x10 : 0);
                    m_uf = false;
                    break;
                case 13: value = 0x80; break;
                default: value = m_ram[m_pDFF7]; break;
            }
            return value;
        }


        private void loadRam()
        {
            try
            {
                string folderName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string fileName = Path.Combine(folderName, "cmos");
                if (File.Exists(fileName))
                    using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        fs.Read(m_ram, 0, m_ram.Length);
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
            m_modified = false;
        }

        private void saveRam()
        {
            if (!m_modified)
                return;
            try
            {
                string folderName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string fileName = Path.Combine(folderName, "cmos");
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                    fs.Write(m_ram, 0, m_ram.Length);
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
            m_modified = false;
        }



        private static byte bcd(byte n)
        {
            return (byte)((n % 10) + 0x10 * ((n / 10) % 10));
        }
    }
}
