using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZXMAK2.Hardware.Adlers.Core
{
    public class DebuggerTrace
    {
        public static byte[] ConditionalJumps = new byte[] { //JR`s
                                                             0x20, //JR NZ,d
                                                             0x28, //JR Z,d
                                                             0x30, //JR NC,d
                                                             0x38, //JR C,d
                                                             //JP`s
                                                             0xC2, //JP NZ,nn
                                                             0xCA, //JP Z,nn
                                                             0xD2, //JP NC,nn
                                                             0xDA, //JP C,nn
                                                             0xE2, //JP PO,nn
                                                             0xEA, //JP PE,nn
                                                             0xF2, //JP P,nn
                                                             0xFA, //JP M,nn
                                                             //CALL`s
                                                             0xC4, //CALL NZ,nn
                                                             0xCC, //CALL Z,nn
                                                             0xD4, //CALL NC,nn
                                                             0xDC, //CALL C,nn
                                                             0xE4, //CALL PO,nn
                                                             0xEC, //CALL PE,nn
                                                             0xF4, //CALL P,nn
                                                             0xFC  //CALL M,nn
                                                           };
        public static byte[] CommonJumps = new byte[] { 0x18, //JR d
                                                        0xC9, //RET
                                                        0xC3, //JP nn
                                                        0xCD, //CALL nn
                                                        0xE9, //JP (HL)
                                                      };
    }
}
