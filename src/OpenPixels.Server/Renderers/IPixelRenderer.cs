using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server
{
    /// <summary>
    /// Allows control of actual pixel-rendering hardware
    /// </summary>
    public interface IPixelRenderer
    {
        int PixelCount { get; }

        void Clear();

        void SetPixelColor(int index, byte r, byte g, byte b);
        void SetPixels(byte[] data);

        void Show();
    }
}
