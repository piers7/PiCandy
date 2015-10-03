using OpenPixels.Server.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace OpenPixels.Server
{
    class MappedPixelsDecorator : IPixelRenderer
    {
        IPixelRenderer _renderer;
        Func<int, int> _map;

        public MappedPixelsDecorator(IPixelRenderer renderer, Func<int,int> indexMap)
        {
            _renderer = renderer;
            _map = indexMap;
        }

        public int PixelCount
        {
            get { return _renderer.PixelCount; }
        }

        public uint[] GetPixels()
        {
            var mapped = _renderer.GetPixels();
            var pixels = UnTranspose(mapped);
            return pixels;
        }

        public void Clear()
        {
            _renderer.Clear();
        }

        public void SetPixels(uint[] pixels, int offset = 0)
        {
            Contract.Requires(pixels != null);

            var mapped = Transpose(pixels, offset);
            _renderer.SetPixels(mapped, offset);
        }

        public void SetPixelColor(int index, byte r, byte g, byte b)
        {
            index = _map(index);
            _renderer.SetPixelColor(index, r, g, b);
        }

        public void Show()
        {
            _renderer.Show();
        }

        private uint[] Transpose(uint[] pixels, int offset = 0)
        {
            // Map all pixels to different offsets using the index map
            var mapped = new uint[pixels.Length];

            for (int pixelIndex = offset; pixelIndex < pixels.Length; pixelIndex++)
            {
                var mappedIndex = _map(pixelIndex);
                mapped[mappedIndex] = pixels[pixelIndex];
            }
            return mapped;
        }

        public uint[] UnTranspose(uint[] mapped)
        {
            // Unmap all pixels to different offsets using the index map
            var pixels = new uint[mapped.Length];

            for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
            {
                var mappedIndex = _map(pixelIndex);
                pixels[pixelIndex] = mapped[mappedIndex];
            }
            return pixels;
        }
    }
}
