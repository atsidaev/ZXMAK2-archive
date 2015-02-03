﻿using System;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;

namespace ZXMAK2.Hardware.Adlers
{
    public class BreakpointAdlers : Breakpoint
    {
        public bool IsNeedWriteMemoryCheck { get; set; }
        public bool IsForceStop { get; set; }
        
        private object lockingObj = new object();

        private static bool debuggerStop = false;

        public BreakpointAdlers(BreakpointInfo info)
        {
            Label = info.BreakpointString;
            Check = checkInfo;
            Address = null;
            Info = info;
        }

        public BreakpointInfo Info { get; private set; }

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

            if (!Info.IsOn)
                return false;

            //ushort leftValue = 0;

            switch (Info.AccessType)
            {
                // e.g.: PC == #9C40
                case BreakPointConditionType.registryVsValue: //only value pair, e.g: BC, HL, DE, ...ToDo:
                // e.g.: (PC) == #AFC9 - instruction breakpoint; must be here because the registry change must be taking into account, not memory change
                case BreakPointConditionType.registryMemoryReferenceVsValue:
                    if (Info.CheckBreakpoint())
                    {
                        if (Info.IsMulticonditional)
                            return Info.CheckSecondCondition();
                        else
                            return true;
                    }
                    else
                        return false;
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

            if (!Info.IsOn)
                return false;

            ushort leftValue = 0;

            switch (Info.AccessType)
            {
                // e.g.: (#9C40) != #2222
                case BreakPointConditionType.memoryVsValue:
                    if (Info.CheckBreakpoint())
                    {
                        if( Info.IsMulticonditional )
                            return Info.CheckSecondCondition();
                        else
                            return true;
                    }
                    else
                        return false;
                default:
                    break;
            }

            //condition
            if (Info.IsConditionEquals) // is equal
                return leftValue == Info.RightValue;
            else
                return leftValue != Info.RightValue;
        }
    }
}
