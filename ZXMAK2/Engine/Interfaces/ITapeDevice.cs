using System;
using System.Collections.Generic;


namespace ZXMAK2.Engine.Interfaces
{
    public interface ITapeDevice //: BusDeviceBase
    {
        void Play();
        void Stop();
        void Rewind();
        void Reset();       // loaded new image
        bool IsPlay { get; }
        int TactsPerSecond { get; }
        List<TapeBlock> Blocks { get; }
        event EventHandler TapeStateChanged;
        int CurrentBlock { get; set; }
        int Position { get; }
        bool TrapsAllowed { get; set; }
    }

    public enum TapeCommand
    {
        NONE,
        STOP_THE_TAPE,
        STOP_THE_TAPE_48K,
        BEGIN_GROUP,
        END_GROUP,
        SHOW_MESSAGE,
    }

    public class TapeBlock
    {
        public string Description;
        public int TzxId = -1;
        public List<int> Periods = new List<int>();
		public byte[] TapData = null;
		public TapeCommand Command = TapeCommand.NONE;
    }
}
