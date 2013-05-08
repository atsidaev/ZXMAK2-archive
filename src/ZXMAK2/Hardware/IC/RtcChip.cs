using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ZXMAK2.Hardware.IC
{
    public class RtcChip
    {
        private byte m_addr = 0;
        private byte[] m_ram = new byte[256];
        private DateTime m_dateTime = DateTime.Now;
        private bool m_uf = false;


        public void Load(string fileName)
        {
            try
            {
                for (var i = 0; i < m_ram.Length; i++)
                {
                    m_ram[i] = 0x00;
                }
                if (File.Exists(fileName))
                {
                    using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        fs.Read(m_ram, 0, m_ram.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        public void Save(string fileName)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    fs.Write(m_ram, 0, m_ram.Length);
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        public void Reset()
        {
            m_addr = 0;
        }

        public void WriteAddr(byte value)
        {
            m_addr = value;
        }

        public void ReadAddr(ref byte value)
        {
        }

        public void WriteData(byte value)
        {
            if (m_addr < 0xF0)
            {
                m_ram[m_addr] = value;
            }
        }

        public void ReadData(ref byte value)
        {
            var curDt = DateTime.Now;

            if (curDt.Subtract(m_dateTime).Seconds > 0 || 
                curDt.Millisecond / 500 != m_dateTime.Millisecond / 500)
            {
                m_dateTime = curDt;
                m_uf = true;
            }
            switch (m_addr)
            {
                case 0x00:
                    value = Bdc(m_dateTime.Second);
                    break;
                case 0x02:
                    value = Bdc(m_dateTime.Minute);
                    break;
                case 0x04:
                    value = Bdc(m_dateTime.Hour);
                    break;
                case 0x06:
                    value = (byte)(m_dateTime.DayOfWeek);
                    break;
                case 0x07:
                    value = Bdc(m_dateTime.Day);
                    break;
                case 0x08:
                    value = Bdc(m_dateTime.Month);
                    break;
                case 0x09:
                    value = Bdc(m_dateTime.Year % 100);
                    break;
                case 0x0A:
                    value = 0x00;
                    break;
                case 0x0B:
                    value = 0x02;
                    break;
                case 0x0C:
                    value = (byte)(m_uf ? 0x1C : 0x0C);
                    m_uf = false;
                    break;
                case 0x0D:
                    value = 0x80;
                    break;
                default:
                    value = m_ram[m_addr];
                    break;
            }
        }

        private byte Bdc(int val)
        {
            int res = val;
            if ((m_ram[11] & 4) == 0)
            {
                int rem = 0;
                res = Math.DivRem(val, 10, out rem);
                res = (res * 16 + rem);
            }
            return (byte)res;
        }
    }
}
