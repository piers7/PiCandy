using PiCandy.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy.Server.Renderers
{
    /// <summary>
    /// A sample implementation of an <see cref="IPixelRenderer"/> that just dumps content
    /// to a text writer (typically Console.Out)
    /// </summary>
    public class TextWriterRenderer : IPixelRenderer, IPixelChannel
    {
        private TextWriter _writer;
        private int _byteLimit;
        private uint[] _buffer;

        public TextWriterRenderer(TextWriter writer, int pixelCount, int byteLimit = 0)
        {
            _writer = writer;
            _byteLimit = byteLimit;
            _buffer = new uint[pixelCount];
        }

        public int Channel { get; set; }

        public int PixelCount { get { return _buffer.Length; } }

        public void Clear() {
            _buffer = new uint[_buffer.Length];
        }

        public void SetPixels(uint[] data, int offset = 0)
        {
            Contract.Requires(data != null);

            _buffer = data;
        }

        public uint[] GetPixels()
        {
            return _buffer;
        }

        public void SetPixelColor(int pixel, byte r, byte g, byte b)
        {
            var packed = Color.FromArgb(r, g, b).ToArgb();
            _buffer[pixel] = (uint)(packed & 0xFFFFFF); // mask and discard 1st byte (alpha)
        }

        public void SetPixelColor(int pixel, uint color)
        {
            _buffer[pixel] = color;
        }

        public void Show()
        {
            var data = _buffer;
            if (_byteLimit > 0)
                data = data.Take(_byteLimit / 2).ToArray();

            _writer.WriteLine("[{0,2}] " + string.Join(" ", data.Select(d => d.ToString("x6"))), Channel);
        }
    }
}
