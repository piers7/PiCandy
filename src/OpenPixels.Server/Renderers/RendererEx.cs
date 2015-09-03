using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.Renderers
{
    public static class RendererEx
    {
        public static void SetPixels(this IPixelRenderer renderer, Func<int, int> map)
        {
            for (int i = 0; i < renderer.PixelCount; i++)
            {
                var color = map(i);
                var r = (byte)(color >> 16);    // r
                var g = (byte)(color >> 8);     // g
                var b = (byte)(color);          // b
                renderer.SetPixelColor(i, r, g, b);
            }
        }
    }
}
