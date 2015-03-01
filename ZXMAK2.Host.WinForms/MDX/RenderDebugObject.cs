using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using D3dFont = Microsoft.DirectX.Direct3D.Font;
using ZXMAK2.Engine;


namespace ZXMAK2.Host.WinForms.Mdx
{
    public class RenderDebugObject : RenderObject
    {
        #region Constants

        private const int GraphLength = 150;

        #endregion Constants
        
        
        #region Fields

        private readonly object _syncRoot = new object();
        private readonly GraphMonitor _graphRender = new GraphMonitor(GraphLength);
        private readonly GraphMonitor _graphLoad = new GraphMonitor(GraphLength);
        private readonly GraphMonitor _graphUpdate = new GraphMonitor(GraphLength);
#if SHOW_LATENCY
        private readonly GraphMonitor _graphLatency = new GraphMonitor(GraphLength);
        //private readonly GraphMonitor _copyGraph = new GraphMonitor(GraphLength);
#endif

        private D3dFont _font;

        private bool _isEnabled;
        private bool _isRunning;

        #endregion Fields


        #region RenderObject

        public override void ReleaseResources()
        {
            lock (_syncRoot)
            {
                Dispose(ref _font);
            }
        }

        public override void Render(Device device, Size size)
        {
            if (!IsEnabled)
            {
                return;
            }
            lock (_syncRoot)
            {
                if (_font == null)
                {
                    var gdiFont = new System.Drawing.Font(
                        "Microsoft Sans Serif",
                        10f/*8.25f*/,
                        System.Drawing.FontStyle.Bold,
                        GraphicsUnit.Pixel);
                    _font = new D3dFont(device, gdiFont);
                }
                var wndSize = new SizeF(size.Width, size.Height);
                RenderDebugInfo(device, wndSize);
            }
        }

        #endregion RenderObject


        #region Public

        public int FrameStartTact { get; set; }

        public int SampleRate { get; set; }

        public Size FrameSize { get; set; }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value)
                {
                    return;
                }
                _isEnabled = value;
                _graphRender.Clear();
                _graphLoad.Clear();
                _graphUpdate.Clear();
            }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (_isRunning == value)
                {
                    return;
                }
                _isRunning = value;
                _graphUpdate.ResetPeriod();
                //_graphRender.ResetPeriod();
                //_renderGraph.Clear();
                //_loadGraph.Clear();
            }
        }

        public void UpdateFrame(double updateTime)
        {
            if (IsEnabled)
            {
                _graphUpdate.PushPeriod();
                _graphLoad.PushValue(updateTime);
            }
        }

        public void UpdatePresent()
        {
            if (IsEnabled)
            {
                _graphRender.PushPeriod();
            }
        }

        #endregion Public


        #region Private

        private void RenderDebugInfo(Device device, SizeF wndSize)
        {
            var frameRate = device.DisplayMode.RefreshRate;
            var graphRender = _graphRender.Get();
            var graphLoad = _graphLoad.Get();
#if SHOW_LATENCY
            var graphLatency = _graphLatency.Get();
            //var graphCopy = _copyGraph.Get();
#endif
            var graphUpdate = _graphUpdate.Get();
            var frequency = GraphMonitor.Frequency;
            var limitDisplay = frequency / frameRate;
            var limit50 = frequency / 50D;
            var limit1ms = frequency / 1000D;
            var maxRender = graphRender.Max();
            var maxLoad = graphLoad.Max();
            var minT = graphRender.Min() * 1000D / frequency;
            var avgT = graphRender.Average() * 1000D / frequency;
            var maxT = maxRender * 1000D / frequency;
#if SHOW_LATENCY
            var minL = _graphLatency.IsDataAvailable ? graphLatency.Min() * 1000D / frequency : 0D;
            var avgL = _graphLatency.IsDataAvailable ? graphLatency.Average() * 1000D / frequency : 0D;
            var maxL = _graphLatency.IsDataAvailable ? graphLatency.Max() * 1000D / frequency : 0D;
#endif
            var avgE = graphLoad.Average() * 1000D / frequency;
            var avgU = graphUpdate.Average() * 1000D / frequency;
            var maxScale = Math.Max(maxRender, maxLoad);
            maxScale = Math.Max(maxScale, limit50);
            maxScale = Math.Max(maxScale, limitDisplay);
            var fpsRender = 1000D / avgT;
            var fpsUpdate = 1000D / avgU;
            var textValue = string.Format(
                "Render FPS: {0:F3}\n" + 
                "Update FPS: {1:F3}\n" +
                "Device FPS: {2}\n" +
                "Back: [{3}, {4}]\n"+ 
                "Frame: [{5}, {6}]\n" +
                "Sound: {7:F3} kHz\n" +
                "FrameStart: {8}T",
                fpsRender,
                IsRunning ? fpsUpdate : (double?)null,
                frameRate,
                wndSize.Width,
                wndSize.Height,
                FrameSize.Width,
                FrameSize.Height,
                SampleRate / 1000D,
                FrameStartTact);
            var textRect = _font.MeasureString(
                null,
                textValue,
                DrawTextFormat.NoClip,
                Color.Yellow);
            textRect = new Rectangle(
                textRect.Left,
                textRect.Top,
                Math.Max(textRect.Width + 10, GraphLength),
                textRect.Height);
            FillRect(device, textRect, Color.FromArgb(192, Color.Green));
            _font.DrawText(
                null,
                textValue,
                textRect,
                DrawTextFormat.NoClip,
                Color.Yellow);
            // Draw graphs
            var graphRect = new Rectangle(
                textRect.Left,
                textRect.Top + textRect.Height,
                GraphLength,
                (int)(wndSize.Height - textRect.Top - textRect.Height));
            FillRect(device, graphRect, Color.FromArgb(192, Color.Black));
            RenderGraph(device, graphRender, maxScale, graphRect, Color.FromArgb(196, Color.Lime));
            RenderGraph(device, graphLoad, maxScale, graphRect, Color.FromArgb(196, Color.Red));
            //RenderGraph(graphCopy, maxTime, graphRect, Color.FromArgb(196, Color.Yellow));
            RenderLimit(device, limitDisplay, maxScale, graphRect, Color.FromArgb(196, Color.Yellow));
            RenderLimit(device, limit50, maxScale, graphRect, Color.FromArgb(196, Color.Magenta));
            DrawGraphGrid(device, maxScale, limit1ms, graphRect, _graphRender.GetIndex(), Color.FromArgb(64, Color.White));

            var msgTime = string.Format(
                "MinT: {0:F3} [ms]\nAvgT: {1:F3} [ms]\nMaxT: {2:F3} [ms]\nAvgE: {3:F3} [ms]",
                minT,
                avgT,
                maxT,
                avgE);
#if SHOW_LATENCY
                if (_graphLatency.IsDataAvailable)
                {
                    msgTime = string.Format(
                        "{0}\nMinL: {1:F3} [ms]\nAvgL: {2:F3} [ms]\nMaxL: {3:F3} [ms]",
                        msgTime,
                        minL,
                        avgL,
                        maxL);
                }
#endif
            _font.DrawText(
                null,
                msgTime,
                graphRect,
                DrawTextFormat.NoClip,
                Color.FromArgb(156, Color.Yellow));
        }

        private void DrawGraphGrid(
            Device device,
            double maxValue, 
            double step, 
            Rectangle rect, 
            int index, 
            Color color)
        {
            var icolor = color.ToArgb();
            var list = new List<Point>();
            var lineCount = maxValue / step;
            if (lineCount > 40 * 40D)
            {
                step = maxValue / 20D;
                icolor = Color.FromArgb(color.A, Color.Violet).ToArgb();
            }
            else if (lineCount > 40D)
            {
                step *= 10D;
                icolor = Color.FromArgb(color.A, Color.Red).ToArgb();
            }
            for (var t = 0D; t < maxValue; t += step)
            {
                var value = (int)((1D - (t / maxValue)) * rect.Height);
                list.Add(new Point(rect.Left, rect.Top + value));
                list.Add(new Point(rect.Left + rect.Width, rect.Top + value));
            }
            for (var t = 0; t < GraphLength; t += 25)
            {
                var ts = GraphLength - (t + index) % GraphLength;
                list.Add(new Point(rect.Left + ts, rect.Top));
                list.Add(new Point(rect.Left + ts, rect.Top + rect.Height));
            }

            var vertices = list
                .Select(p => new CustomVertex.TransformedColored(p.X, p.Y, 0, 1f, icolor))
                .ToArray();
            device.VertexFormat = CustomVertex.TransformedColored.Format | VertexFormats.Diffuse;
            device.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length / 2, vertices);
        }

        private void RenderLimit(
            Device device,
            double limit, 
            double maxValue, 
            Rectangle rect, 
            Color color)
        {
            if (limit < 0 || limit > maxValue)
            {
                return;
            }
            var icolor = color.ToArgb();
            var list = new List<Point>();
            var value = 1D - (limit / maxValue);
            if (value < 0D)
            {
                value = 0;
            }
            var hValue = (int)(value * rect.Height);
            list.Add(new Point(rect.Left, rect.Top + hValue));
            list.Add(new Point(rect.Left + rect.Width, rect.Top + hValue));
            var vertices = list
                .Select(p => new CustomVertex.TransformedColored(p.X, p.Y, 0, 1f, icolor))
                .ToArray();
            device.VertexFormat = CustomVertex.TransformedColored.Format | VertexFormats.Diffuse;
            device.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length / 2, vertices);
        }

        private void RenderGraph(
            Device device, 
            double[] graph, 
            double maxValue, 
            Rectangle rect, 
            Color color)
        {
            if (graph.Length < 1)
            {
                return;
            }
            var icolor = color.ToArgb();
            var list = new List<Point>();
            for (var x = 0; x < graph.Length && x < rect.Width; x++)
            {
                var value = 1D - (graph[x] / maxValue);
                if (value < 0D)
                {
                    value = 0;
                }
                var hValue = (int)(value * rect.Height);
                list.Add(new Point(rect.Left + x, rect.Top + rect.Height));
                list.Add(new Point(rect.Left + x, rect.Top + hValue));
            }
            var vertices = list
                .Select(p => new CustomVertex.TransformedColored(p.X, p.Y, 0, 1f, icolor))
                .ToArray();
            device.VertexFormat = CustomVertex.TransformedColored.Format | VertexFormats.Diffuse;
            device.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length / 2, vertices);
        }

        private void FillRect(Device device, Rectangle rect, Color color)
        {
            var icolor = color.ToArgb();
            var rectv = new[]
            {
                new CustomVertex.TransformedColored(rect.Left, rect.Top+rect.Height+0.5F, 0, 1f, icolor),
                new CustomVertex.TransformedColored(rect.Left, rect.Top, 0, 1f, icolor),
                new CustomVertex.TransformedColored(rect.Left+rect.Width, rect.Top+rect.Height+0.5F, 0, 1f, icolor),
                new CustomVertex.TransformedColored(rect.Left+rect.Width, rect.Top, 0, 1f, icolor),
            };
            device.VertexFormat = CustomVertex.TransformedColored.Format | VertexFormats.Diffuse;
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rectv);
        }

        #endregion Private
    }
}
