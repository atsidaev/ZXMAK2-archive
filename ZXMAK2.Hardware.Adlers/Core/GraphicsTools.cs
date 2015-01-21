using System;
using System.Collections;

namespace ZXMAK2.Hardware.Adlers.Core
{
    static class GraphicsTools
    {
        public static UInt16 ZX_SCREEN_START= 16384;
        public static UInt16 MAX_X_PIXEL = Convert.ToUInt16(256); // X-coordinate maximum(pixel)
        public static UInt16 MAX_Y_PIXEL = Convert.ToUInt16(192); // Y-coordinate maximum(pixel)

        /// <summary>
        /// getAttributePixels
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns>BitArray</returns>
        public static BitArray getAttributePixels(byte attribute, bool i_mirrorImage)
        {
            BitArray bitsOut = new BitArray(8); //define the size

            if (i_mirrorImage) //mirror image ?
            {
                for (byte x = 0; x < bitsOut.Count; x++)
                {
                    bitsOut[x] = (((attribute >> x) & 0x01) == 0x01) ? true : false;
                }
            }
            else
            {
                //setting a value
                for (int x = 0; x < bitsOut.Length; x++) //mirror bits in array to be correctly displayed on screen(left to right)
                {
                    bitsOut[bitsOut.Length - 1 - x] = (((attribute >> x) & 0x01) == 0x01) ? true : false;
                }
            }

            return bitsOut;
        }

        /// <summary>
        /// getSegment
        /// </summary>
        /// <param name="y"></param>
        /// <returns>short</returns>
        public static short getSegment(int y)
        {
            if (y < 64) // segment #2
                return 1; //<0; 63>

            //<64; 127>
            if (y > 127) // segment #3
                return 3;

            return 2;
        }

        /// <summary>
        /// getScreenAdress
        /// </summary>
        /// <param name="xCoor"></param>
        /// <param name="yCoor"></param>
        /// <returns>ushort</returns>
        public static ushort getScreenAdress(int xCoor, int yCoor)
        {
            int yCoorLocal = yCoor;
            ushort sAdress = ZX_SCREEN_START;
            short sSegment = getSegment(yCoor); // in which screen segment we are ?
            if (sSegment == 3)
                yCoorLocal++; //mouse pointer correction

            sAdress += Convert.ToUInt16((sSegment - 1) * 2048);
            yCoorLocal -= (sSegment - 1) * 64;     // move it into the ground fictive segment
            sAdress += Convert.ToUInt16(((xCoor /*- 1*/) / 8));

            //add y coordinate value
            int attributeLineNumber = (yCoorLocal /*- 1*/) / 8;   // defines which line of attributes is it on the screen
            int lineInAttribute = (yCoorLocal /*- 1*/) % 8;   // defines which line is it in the attribute...from above

            sAdress += Convert.ToUInt16((attributeLineNumber * 32) + (lineInAttribute * MAX_X_PIXEL));

            return sAdress;
        }
    }
}
