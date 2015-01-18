using System;
using System.Linq;
using ZXMAK2.Hardware.General;


namespace ZXMAK2.Hardware.Evo
{
    public class AYCHRV : AY8910
    {
        public AYCHRV()
        {
            Name = "AY8910-CHRV";
            Description = "AY8910 with #FE value on IRB input (required for PentEvo)";
            UpdateIRB += OnUpdateIrb;
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
