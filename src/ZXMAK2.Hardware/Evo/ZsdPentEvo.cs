using System;

using ZXMAK2.Hardware.Circuits.SecureDigital;
using ZXMAK2.Host.Entities;
using ZXMAK2.Presentation.Entities;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Hardware.Evo
{
    public class ZsdPentEvo : BusDeviceBase
    {
        #region Fields

        private IMemoryDevice mem;
        private SdCard card;
        private byte buf;
        private bool card_cs;

        #endregion


        public bool SHADOW
        {
            get { return (mem == null) ? false : mem.SYSEN; }
        }


        #region IBusDevice

        public override string Name
        {
            get { return "SD PentEvo"; }
        }

        public override string Description
        {
            get { return "PentEvo SD Card\r\nWritten by ZEK"; }
        }

        public override BusDeviceCategory Category
        {
            get { return BusDeviceCategory.Disk; }
        }


        public override void BusInit(IBusManager bmgr)
        {
            mem = bmgr.FindDevice<IMemoryDevice>();
            bmgr.AddCommandUi(
                new CommandDelegate(
                    CommandUi_OnExecute, 
                    CommandUi_OnCanExecute, 
                    "Open SD Card image..."));

            bmgr.SubscribeReset(Reset);
            bmgr.SubscribeWrIo(0x00FF, 0x0057, WrXX57);
            bmgr.SubscribeRdIo(0x00FF, 0x0057, RdXX57);
            bmgr.SubscribeWrIo(0x00FF, 0x0077, WrXX77);
            bmgr.SubscribeRdIo(0x00FF, 0x0077, RdXX77);
        }

        public override void BusConnect()
        {
            card = new SdCard();
        }

        public override void BusDisconnect()
        {
            card.Close();
        }

        #endregion


        #region Bus handlers

        protected virtual void Reset()
        {
            card_cs = false;
            card.Reset();
            buf = 0xFF;
        }

        protected virtual void WrXX57(ushort addr, byte val, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;

                if (SHADOW)
                {
                    if ((addr & 0x8000) != 0)
                        card_cs = ((val & 0x02) != 0);
                    else
                        CardWr(val);
                }
                else
                    CardWr(val);
            }
        }

        protected virtual void RdXX57(ushort addr, ref byte val, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;

                if (SHADOW)
                {
                    if ((addr & 0x8000) != 0)
                        val = 0;
                    else
                        val = CardRd();
                }
                else
                    val = CardRd();
            }
        }

        protected virtual void WrXX77(ushort addr, byte val, ref bool iorqge)
        {
            if (iorqge && !SHADOW)
            {
                iorqge = false;
                card_cs = ((val & 0x02) != 0);
            }
        }

        protected virtual void RdXX77(ushort addr, ref byte val, ref bool iorqge)
        {
            if (iorqge && !SHADOW)
            {
                iorqge = false;
                val = 0;
            }
        }

        #endregion


        #region SD Card Emu

        protected void CardWr(byte val)
        {
            buf = card.Rd();
            card.Wr(val);
        }

        protected byte CardRd()
        {
            var tmp = buf;
            buf = card.Rd();
            card.Wr(0xff);
            return tmp;
        }

        #endregion


        #region CommandUi

        private bool CommandUi_OnCanExecute(Object arg)
        {
            var viewResolver = Locator.Resolve<IResolver>("View");
            return viewResolver.CheckAvailable<IOpenFileDialog>();
        }

        private void CommandUi_OnExecute(Object arg)
        {
            if (!CommandUi_OnCanExecute(arg))
            {
                return;
            }
            try
            {
                var viewResolver = Locator.Resolve<IResolver>("View");
                var dlg = viewResolver.TryResolve<IOpenFileDialog>();
                if (dlg == null)
                {
                    return;
                }
                dlg.CheckFileExists = true;
                //dlg.CheckPathExists = true;
                //dlg.DefaultExt = "img|ima";
                dlg.Filter = "Disk image file (*.img, *.ima)|*.img;*.ima";
                dlg.Multiselect = false;
                if (dlg.ShowDialog(arg) != DlgResult.OK)
                {
                    return;
                }
                card.Open(dlg.FileName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion CommandUi
    }
}
