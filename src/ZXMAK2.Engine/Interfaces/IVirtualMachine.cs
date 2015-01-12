using System;


namespace ZXMAK2.Engine.Interfaces
{
    public interface IVirtualMachine : IDisposable
    {
        bool IsRunning { get; }
        IBus Bus { get; }
        
        void DoRun();
        void DoStop();

        void DoReset();
        void DoNmi();

        void SaveConfig();
    }
}
