using System;
using System.Runtime.InteropServices;


namespace ZXMAK2.Host.WinForms.Tools
{
    internal sealed class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public unsafe static extern void CopyMemory(int* destination, int* source, int length);

        [DllImport("kernel32.dll", SetLastError = true)]
        public unsafe static extern void CopyMemory(uint* destination, uint* source, int length);

        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint uPeriod);

        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint uPeriod);

        
        public static uint TimeBeginPeriod(uint uPeriod)
        {
            try
            {
                var result = timeBeginPeriod(uPeriod);
                if (result != TIMERR_NOERROR)
                {
                    Logger.Debug("timeBeginPeriod({0}): error {1}", result);
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Debug("{0}: {1}", ex.GetType().Name, ex.Message);
            }
            return 0xFFFFFFFF;
        }

        public static uint TimeEndPeriod(uint uPeriod)
        {
            try
            {
                var result = timeEndPeriod(uPeriod);
                if (result != TIMERR_NOERROR)
                {
                    Logger.Debug("timeBeginPeriod({0}): error {1}", result);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("{0}: {1}", ex.GetType().Name, ex.Message);
            }
            return 0xFFFFFFFF;
        }

        public const uint TIMERR_NOERROR = 0;
        public const uint TIMERR_NOCANDO = 96+1;
    }
}
