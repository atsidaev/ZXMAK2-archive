/* 
 *  Copyright 2008, 2015 Alex Makeev
 * 
 *  This file is part of ZXMAK2 (ZX Spectrum virtual machine).
 *
 *  ZXMAK2 is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  ZXMAK2 is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with ZXMAK2.  If not, see <http://www.gnu.org/licenses/>.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZXMAK2.Host.WinForms.Mdx.Renderers
{
    public abstract class RendererBase : IRenderer
    {
        #region Fields

        protected readonly AllocatorPresenter Allocator;

        #endregion Fields


        #region .ctor

        public RendererBase(AllocatorPresenter allocator)
        {
            Allocator = allocator;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            Unload();
        }

        #endregion .ctor


        #region IRenderer

        public bool IsVisible { get; set; }

        public void Load()
        {
            if (IsLoaded)
            {
                throw new InvalidOperationException("Attach already done!");
            }
            Allocator.ExecuteSynchronized(() =>
            {
                if (IsLoaded)
                {
                    throw new InvalidOperationException("Attach already done!");
                }
                IsLoaded = true;
                AttachSynchronized();
            });
        }

        public void Unload()
        {
            Allocator.ExecuteSynchronized(() =>
            {
                DetachSynchronized();
                IsLoaded = false;
            });
        }

        public void Render(int width, int height)
        {
            if (!IsLoaded || !IsVisible)
            {
                return;
            }
            Allocator.ExecuteSynchronized(() =>
            {
                if (!IsLoaded || !IsVisible)
                {
                    return;
                }
                RenderSynchronized(width, height);
            });
        }

        #endregion IRenderer


        #region Protected

        protected bool IsLoaded { get; private set; }

        protected virtual void AttachSynchronized()
        {
        }

        protected virtual void DetachSynchronized()
        {
        }

        protected virtual void RenderSynchronized(int width, int height)
        {
        }

        #endregion Protected
    }
}
