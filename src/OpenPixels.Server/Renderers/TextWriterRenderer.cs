using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.Renderers
{
    /// <summary>
    /// A sample implementation of an <see cref="IPixelRenderer"/> that just dumps content
    /// to a text writer (typically Console.Out)
    /// </summary>
    public class TextWriterRenderer : IPixelChannel, IPixelRenderer
    {
        private TextWriter _writer;
        private int _pixelCount;
        private int _byteLimit;
        private byte[] _buffer;

        public TextWriterRenderer(TextWriter writer, int pixelCount, int byteLimit = 0)
        {
            _writer = writer;
            _byteLimit = byteLimit;

            // TODO: Not sure a byte[] buffer is actually the right play here
            // think should start favoring some kind of ARGB / uint struct
            _pixelCount = pixelCount;
            _buffer = new byte[pixelCount*4];
        }

        public int Channel { get; set; }

        public int PixelCount { get { return _pixelCount; } }

        public void Clear() { }

        public void SetPixels(byte[] data)
        {
            Contract.Requires(data != null);

            _buffer = data;
        }

        public void SetPixelColor(int pixel, byte r, byte g, byte b)
        {
            _buffer[pixel * 4] = b;
            _buffer[pixel * 4 + 1] = g;
            _buffer[pixel * 4 + 2] = r;
        }

        public void Show()
        {
            var data = _buffer;
            if (_byteLimit > 0)
                data = data.Take(_byteLimit / 2).ToArray();

            _writer.WriteLine("[{0,2}] " + BitConverter.ToString(data).Replace("-", ""), Channel);
        }

        IPixelRenderer IPixelChannel.Renderer
        {
            get { return this; }
        }
    }
}
