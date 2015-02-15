using System;
using System.Drawing;
using Microsoft.DirectX.Direct3D;


namespace ZXMAK2.Host.WinForms.Mdx
{
    public abstract class RenderObject : IDisposable
    {
        public void Dispose()
        {
            ReleaseResources();
        }
        
        
        public abstract void ReleaseResources();

        public abstract void Render(Device device, Size size);


        #region Helpers

        protected static int GetPotSize(int size)
        {
            // Create POT texture (e.g. 512x512) to render NPOT image (e.g. 320x240),
            // because NPOT textures is not supported on some videocards
            var potSize = 0;
            for (var power = 1; potSize < size; power++)
            {
                potSize = Pow(2, power);
            }
            return potSize;
        }

        private static int Pow(int value, int power)
        {
            var result = value;
            for (var i = 0; i < power; i++)
            {
                result *= value;
            }
            return result;
        }

        protected static void Dispose<T>(ref T disposable)
            where T : IDisposable
        {
            var value = disposable;
            disposable = default(T);
            if (value != null)
            {
                value.Dispose();
            }
        }

        #endregion Helpers
    }
}
