using System;
using System.Linq;
using ZXMAK2.Hardware.General;


namespace ZXMAK2.Hardware.Evo
{
    public class AYCHRV : AY8910
    {
        public AYCHRV()
        {
            UpdateIRB += OnUpdateIrb;
        }

        public override string Name
        {
            get { return "AY8910-CHRV"; }
        }

        private void OnUpdateIrb(AY8910 sender, AyPortState state)
        {
            if (!state.DirOut)
            {
                state.InState = 0xFE; /*always ready??? Alone Coder us0.36.6*/
            }
        }
    }
}
