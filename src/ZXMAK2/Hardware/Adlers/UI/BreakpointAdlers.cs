using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Entities;
using ZXMAK2.Interfaces;
using System.Diagnostics;

namespace ZXMAK2.Hardware.Adlers.UI
{
    public class BreakpointAdlers : Breakpoint
    {
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

        #region Breakpoint Check Methods
        private static ushort getValuePair16bit(IntPtr pZ80Regs, int delta = 0)
        {
            unsafe
            {
                return (ushort)*((int*)pZ80Regs + delta);
            }
        }
        public static void checkWriteMem(ushort addr, byte value)
        {
            /*if (addr == 40000)
                debuggerStop = true;*/
            return;
        }
        #endregion


        private bool checkInfo(IMachineState state)
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
                // e.g.: PC == #9C40
                case BreakPointConditionType.registryVsValue: //only value pair, e.g: BC, HL, DE, ...ToDo:
                    unsafe
                    {
                        fixed (ushort* pRegs = &(state.CPU.regs.AF))
                        {
                            leftValue = getValuePair16bit(new IntPtr(pRegs + Info.leftRegistryArrayIndex));
                        }
                    }
                    break;
                // e.g.: (#9C40) != #2222
                case BreakPointConditionType.memoryVsValue:
                    leftValue = state.ReadMemory(Info.leftValue);
                    break;
                // e.g.: (PC) == #D1 - instruction breakpoint
                case BreakPointConditionType.registryMemoryReferenceVsValue:
                    unsafe
                    {
                        fixed (ushort* pRegs = &(state.CPU.regs.AF))
                        {
                            if (Info.rightValue <= 0xFF)
                            {
                                leftValue = state.ReadMemory(getValuePair16bit(new IntPtr(pRegs)));
                            }
                            else
                            {
                                //TimeSpan time = watch.Elapsed;
                                leftValue = state.ReadMemory16bit(getValuePair16bit(new IntPtr(pRegs + Info.leftRegistryArrayIndex)));
                                /*watch.Stop();
                                TimeSpan time = watch.Elapsed;
                                time = time;*/
                            }
                        }
                    }
                    break;
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
