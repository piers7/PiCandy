﻿using System;
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
        private int _byteLimit;

        public TextWriterRenderer(TextWriter writer, int byteLimit = 0)
        {
            _writer = writer;
            _byteLimit = byteLimit;
        }

        public int Channel { get; set; }

        public void SetPixels(byte[] data)
        {
            Contract.Requires(data != null);

            if (_byteLimit > 0)
                data = data.Take(_byteLimit / 2).ToArray();

            _writer.WriteLine("[{0,2}] " + BitConverter.ToString(data).Replace("-",""), Channel);
        }

        IPixelRenderer IPixelChannel.Renderer
        {
            get { return this; }
        }
    }
}
