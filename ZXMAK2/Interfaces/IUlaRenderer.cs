﻿using System;
using System.IO;
using System.Drawing;

namespace ZXMAK2.Interfaces
{
    public interface IUlaRenderer
    {
        Size VideoSize { get; }
        int FrameLength { get; }
        int IntLength { get; }
        float PixelHeightRatio { get; }

        void UpdateBorder(int value);

        void UpdatePalette(int index, uint value);

        /// <summary>
        /// Free bus effect emulation (port #FF)
        /// </summary>
        void ReadFreeBus(int frameTact, ref byte value);

        /// <summary>
        /// Render screen portion
        /// </summary>
        /// <param name="bufPtr">Bitmap memory pointer</param>
        /// <param name="startTact">First rendering tact</param>
        /// <param name="endTact">Last rendering tact</param>
        /// <param name="ulaFetch">Internal ULA registers state</param>
        unsafe void Render(
            uint* bufPtr,
            int startTact,
            int endTact);

        /// <summary>
        /// Perform frame post rendering actions
        /// </summary>
        void Frame();

        /// <summary>
        /// Load screen data from the stream (SCR format)
        /// </summary>
        void LoadScreenData(Stream stream);

        /// <summary>
        /// Save screen data to the stream (SCR format)
        /// </summary>
        void SaveScreenData(Stream stream);

        /// <summary>
        /// Clone IUlaRenderer (include current state)
        /// </summary>
        IUlaRenderer Clone();
    }
}