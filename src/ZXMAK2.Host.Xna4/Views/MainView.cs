using System;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ZXMAK2.Engine;
using ZXMAK2.Host.Xna4.Properties;
using ZXMAK2.Host.Xna4.Xna;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Presentation.Interfaces;
using ZXMAK2.Host.Presentation.Interfaces;


namespace ZXMAK2.Host.Xna4.Views
{
    public unsafe class MainView : Game, IMainView, IHostVideo
    {
        #region Fields

        private readonly GraphicsDeviceManager m_deviceManager;
        private readonly object m_syncTexture = new object();
        private readonly AutoResetEvent m_frameEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent m_cancelEvent = new AutoResetEvent(false);
        private Texture2D[] m_texture = new Texture2D[2];
        private SpriteBatch m_sprite;
        private SpriteFont m_font;
        private readonly FpsMonitor m_fpsRender = new FpsMonitor();
        private readonly FpsMonitor m_fpsUpdate = new FpsMonitor();
        private int m_textureIndex;

        private string m_title;
        private XnaHost m_host;
        private int[] m_translateBuffer;
        private int m_debugFrameStart;
        private IVideoData m_videoData;

        private ICommand CommandViewFullScreen { get; set; }
        private ICommand CommandVmWarmReset { get; set; }

        #endregion Fields


        public MainView()
        {
            m_deviceManager = new GraphicsDeviceManager(this);
        }

        public string KeyboardMapFile { get; set; }


        #region IMainView

        public string Title
        {
            get { return m_title; }
            set 
            { 
                m_title = value; 
                if (string.IsNullOrEmpty(m_title))
                {
                    Window.Title = "ZXMAK2-XNA4";
                }
                else
                {
                    Window.Title = string.Format(
                        "[{0}] - ZXMAK2-XNA4", 
                        m_title); 
                }
            }
        }

        public bool IsFullScreen
        {
            get { return m_deviceManager.IsFullScreen; }
            set { m_deviceManager.IsFullScreen = value; }
        }

        public IHost Host
        {
            get { return m_host; }
        }

        public ICommandManager CommandManager
        {
            get { return null; }
        }

        public event EventHandler ViewOpened;
        public event EventHandler ViewClosed;
        public event EventHandler RequestFrame;

        public void Bind(IMainPresenter presenter)
        {
            presenter.CommandViewSyncVBlank.Execute(false);
            CommandViewFullScreen = presenter.CommandViewFullScreen;
            CommandVmWarmReset = presenter.CommandVmWarmReset;
        }

        public void Close()
        {
            Exit();
        }

        public void ShowHelp(object obj)
        {
        }

        #endregion IMainView

        
        #region IHostVideo

        public void WaitFrame()
        {
            m_cancelEvent.Reset();
            WaitHandle.WaitAny(new[] { m_frameEvent, m_cancelEvent });
        }

        public void CancelWait()
        {
            m_cancelEvent.Set();
        }

        public void PushFrame(IVideoFrame frame, bool isRequested)
        {
            if (!isRequested)
            {
                m_fpsUpdate.Frame();
            }
            m_debugFrameStart = frame.StartTact;
            m_videoData = frame.VideoData;
            
            var videoLen = m_videoData.Size.Width * m_videoData.Size.Height;
            
            // we need to translate bgra colors to rgba
            // because brga color support was removed from XNA4
            if (m_translateBuffer == null ||
                m_translateBuffer.Length < videoLen)
            {
                m_translateBuffer = new int[videoLen];
            }
            fixed (int* pBuffer = m_videoData.Buffer)
            {
                Marshal.Copy(
                    (IntPtr)pBuffer,
                    m_translateBuffer,
                    0,
                    videoLen);
            }
            fixed (int* pBuffer = m_translateBuffer)
            {
                var puBuffer = (uint*)pBuffer;
                // bgra -> rgba
                for (var i = 0; i < videoLen; i++)
                {
                    puBuffer[i] = 
                        (puBuffer[i] & 0x000000ff) << 16 |
                        (puBuffer[i] & 0xFF00FF00) |
                        (puBuffer[i] & 0x00FF0000) >> 16;
                }
            }
            // copy translated image to output texture
            lock (m_syncTexture)
            {
                var texture = m_texture[m_textureIndex];
                if (texture == null)
                {
                    return;
                }
                texture.SetData<int>(
                    m_translateBuffer,
                    0,
                    videoLen);
            }
        }

        #endregion IHostVideo


        #region Event Raise

        private void OnViewOpened()
        {
            var handler = ViewOpened;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnViewClosed()
        {
            var handler = ViewClosed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnRequestFrame()
        {
            var handler = RequestFrame;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion Event Raise


        #region XNA

        protected override void Initialize()
        {
            m_deviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            m_deviceManager.ApplyChanges();
            base.Initialize();

            m_host = new XnaHost(this);
            OnViewOpened();

            OnRequestFrame();
            m_deviceManager.PreferredBackBufferWidth = m_videoData.Size.Width * 2;
            m_deviceManager.PreferredBackBufferHeight = m_videoData.Size.Height * 2;
            m_deviceManager.PreferredBackBufferFormat = SurfaceFormat.Color;
            m_deviceManager.ApplyChanges();
            m_sprite = new SpriteBatch(m_deviceManager.GraphicsDevice);
            //m_deviceManager.SynchronizeWithVerticalRetrace = false;
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (string.IsNullOrEmpty(KeyboardMapFile))
            {
                return;
            }
            try
            {
                m_host.Keyboard.LoadConfiguration(KeyboardMapFile);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            OnViewClosed();
            if (m_host != null)
            {
                m_host.Dispose();
                m_host = null;
            }
        }

        protected override bool ShowMissingRequirementMessage(Exception exception)
        {
            Logger.Error(exception);
            return base.ShowMissingRequirementMessage(exception);
        }

        protected override void LoadContent()
        {
            Content = new ResourceContentManager(Services, Resources.ResourceManager);
            base.LoadContent();
            m_font = Content.Load<SpriteFont>("SansSerifBold");
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var kbdState = Keyboard.GetState();
            var mouseState = Mouse.GetState();
            m_host.Update(kbdState, mouseState);
            
            var isAlt = kbdState[Keys.LeftAlt] == KeyState.Down ||
                kbdState[Keys.RightAlt] == KeyState.Down;
            var isCtrl = kbdState[Keys.LeftControl] == KeyState.Down ||
                kbdState[Keys.RightControl] == KeyState.Down;
            if (isAlt && isCtrl &&
                CommandVmWarmReset != null &&
                CommandVmWarmReset.CanExecute(null))
            {
                CommandVmWarmReset.Execute(kbdState[Keys.Insert] == KeyState.Down);
            }
        }

        private void CheckTexture()
        {
            var texture = m_texture[m_textureIndex];
            if (texture == null ||
                texture.Width != m_videoData.Size.Width ||
                texture.Height != m_videoData.Size.Height)
            {
                if (m_texture[m_textureIndex] != null)
                {
                    m_texture[m_textureIndex].Dispose();
                    m_texture[m_textureIndex] = null;
                }
                if (m_texture[m_textureIndex] != null)
                {
                    m_texture[m_textureIndex].Dispose();
                    m_texture[m_textureIndex] = null;
                }
                m_texture[m_textureIndex] = new Texture2D(
                    m_deviceManager.GraphicsDevice,
                    m_videoData.Size.Width,
                    m_videoData.Size.Height,
                    false,
                    SurfaceFormat.Color);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            m_frameEvent.Set();
            m_fpsRender.Frame();
            base.Draw(gameTime);

            m_deviceManager.GraphicsDevice.Clear(Color.Black);
            m_sprite.Begin(
                SpriteSortMode.Immediate, 
                BlendState.AlphaBlend,
                SamplerState.PointClamp, // disable anti-aliasing
                null,
                null);
            
            var texture = (Texture2D)null;
            lock (m_syncTexture)
            {
                CheckTexture();
                texture = m_texture[m_textureIndex];
                m_textureIndex++;
                m_textureIndex &= 1;
            }
            if (texture != null)
            {
                var rect = new Rectangle(
                    0,
                    0,
                    m_deviceManager.PreferredBackBufferWidth,
                    m_deviceManager.PreferredBackBufferHeight);
                m_sprite.Draw(
                    texture,
                    rect,
                    Color.White);
                
                m_writePos = 0F;
                WriteLine("Render FPS: {0:F3}", m_fpsRender.Value);
                WriteLine("Update FPS: {0:F3}", m_fpsUpdate.Value);
                WriteLine(
                    "Back: [{0}, {1}, {2}]", 
                    m_deviceManager.GraphicsDevice.PresentationParameters.BackBufferWidth,
                    m_deviceManager.GraphicsDevice.PresentationParameters.BackBufferHeight,
                    m_deviceManager.GraphicsDevice.PresentationParameters.BackBufferFormat);
                WriteLine(
                    "Surface: [{0}, {1}]", 
                    m_videoData.Size.Width,
                    m_videoData.Size.Height);
                WriteLine("FrameStart: {0}T", m_debugFrameStart);
            }
            m_sprite.End();
        }

        #endregion XNA

        #region Frame Console Emulation

        private float m_writePos;

        private void WriteLine(string fmt, params object[] args)
        {
            var strText = string.Format(fmt, args);
            m_sprite.DrawString(
                m_font,
                strText,
                new Vector2(5, 5 + m_writePos),
                Color.Yellow);
            var strSize = m_font.MeasureString(strText);
            m_writePos += strSize.Y;
        }

        #endregion Frame Console Emulation
    }
}
