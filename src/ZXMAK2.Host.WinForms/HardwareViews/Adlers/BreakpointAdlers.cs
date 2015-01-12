using System;
using ZXMAK2.Entities;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Host.WinForms.HardwareViews.Adlers
{
    public class BreakpointAdlers : Breakpoint
    {
        public bool IsNeedWriteMemoryCheck { get; set; }
        public bool IsForceStop { get; set; }
        
        private object lockingObj = new object();

        private static bool debuggerStop = false;

        public BreakpointAdlers(BreakpointInfo info)
        {
            Label = info.breakpointString;
            Check = checkInfo;
            Address = null;
            Info = info;
        }

        public BreakpointInfo Info { get; private set; }

        private static ushort getValuePair16bit(IntPtr pZ80Regs, int delta = 0)
        {
            unsafe
            {
                return (ushort)*((int*)pZ80Regs + delta);
            }
        }

        public bool checkInfo(IMachineState state)
        {
            var needWriteMemoryCheck = IsNeedWriteMemoryCheck;
            IsNeedWriteMemoryCheck = false; // reset flag for next cycle
            if (IsForceStop)
            {
                IsForceStop = false;    // reset stop flag for next cycle
                return true;            // return true to force stop
            }
            if (needWriteMemoryCheck && checkInfoMemory(state))
            {
                return true;
            }
            
            /*Stopwatch watch = new Stopwatch();
            watch.Start();*/
            lock (lockingObj)
            {
                if (debuggerStop)
                {
                    debuggerStop = false;
                    return true;
                }
            }

            if (!Info.isOn)
                return false;

            //ushort leftValue = 0;

            switch (Info.accessType)
            {
                // e.g.: PC == #9C40
                case BreakPointConditionType.registryVsValue: //only value pair, e.g: BC, HL, DE, ...ToDo:
                    return Info.checkBreakpoint();
                // e.g.: (PC) == #AFC9 - instruction breakpoint; must be here because the registry change must be taking into account, not memory change
                case BreakPointConditionType.registryMemoryReferenceVsValue:
                    return Info.checkBreakpoint();
                default:
                    return false;
            }
        }

        private bool checkInfoMemory(IMachineState state)
        {
            /*Stopwatch watch = new Stopwatch();
            watch.Start();*/
            lock (lockingObj)
            {
                if (debuggerStop)
                {
                    debuggerStop = false;
                    return true;
                }
            }

            if (!Info.isOn)
                return false;

            ushort leftValue = 0;

            switch (Info.accessType)
            {
                // e.g.: (#9C40) != #2222
                case BreakPointConditionType.memoryVsValue:
                    return Info.checkBreakpoint();
                default:
                    break;
            }

            //condition
            if (Info.conditionEquals) // is equal
                return leftValue == Info.rightValue;
            else
                return leftValue != Info.rightValue;
        }
    }
}
