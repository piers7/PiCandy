using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy.Rendering
{
    /// <summary>
    /// Allows control of actual pixel-rendering hardware
    /// </summary>
    public interface IPixelRenderer
    {
        int PixelCount { get; }

        uint[] GetPixels();

        void Clear();

        void SetPixels(uint[] pixels, int offset = 0);
        void SetPixelColor(int index, byte r, byte g, byte b);

        void Show();
    }
}
