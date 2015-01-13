using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;


namespace ZXMAK2.Hardware.Circuits.Ata
{
    public class AtaPort
    {
        public AtaDevice[] Devices { get; private set; }

        
        public AtaPort()
        {
            Devices = new AtaDevice[2]
            {
                new AtaDevice(0x00),
                new AtaDevice(0x10),
            };
        }

        public void Open()
        {
            Devices[0].Open();
            Devices[1].Open();
        }

        public void Reset()
        {
            //Logger.Debug("AtaPort.reset");
            Devices[0].reset(RESET_TYPE.RESET_HARD);
            Devices[1].reset(RESET_TYPE.RESET_HARD);
        }

        public byte Read(AtaReg n_reg)
        {
            byte result = (byte)(Devices[0].read(n_reg) & Devices[1].read(n_reg));
            //Logger.Debug("AtaPort.read({0}) = 0x{1:X2}", n_reg, result);
            return result;
        }

        public UInt16 ReadData()
        {
            UInt16 result = (UInt16)(Devices[0].read_data() & Devices[1].read_data());
            //Logger.Debug("AtaPort.read_data() = 0x{0:X4}", result);
            return result;
        }

        public void Write(AtaReg n_reg, byte data)
        {
            //Logger.Debug("AtaPort.write({0}, 0x{1:X2})", n_reg, data);
            Devices[0].write(n_reg, data);
            Devices[1].write(n_reg, data);
        }

        public void WriteData(UInt16 data)
        {
            //Logger.Debug("AtaPort.write_data(0x{0:X4})", data);
            Devices[0].write_data(data);
            Devices[1].write_data(data);
        }

        public byte GetIntRq()
        {
            byte result = (byte)(Devices[0].read_intrq() & Devices[1].read_intrq());
            //Logger.Debug("AtaPort.read_intrq() = 0x{0:X2}", result);
            return result;
        }

        public bool LedIo
        {
            get { return Devices[0].LedIo || Devices[1].LedIo; }
            set { Devices[0].LedIo = Devices[1].LedIo = value; }
        }

        public bool LogIo
        {
            get { return Devices[0].LogIo || Devices[1].LogIo; }
            set { Devices[0].LogIo = Devices[1].LogIo = value; }
        }
    }
}
