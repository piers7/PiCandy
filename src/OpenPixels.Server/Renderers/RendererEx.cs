using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.Renderers
{
    public static class RendererEx
    {
        public static void SetPixelColor(this IPixelRenderer renderer, int index, Color color)
        {
            renderer.SetPixelColor(index, color.R, color.G, color.B);
        }

        public static void SetPixels(this IPixelRenderer renderer, IEnumerable<Color> pixels)
        {
            var rgbs = pixels
                .Take(renderer.PixelCount)
                .Select(ToRgb)
                .ToArray()
                ;
            renderer.SetPixels(rgbs);
        }

        public static void SetPixels(this IPixelRenderer renderer, Func<int, Color> map)
        {
            var rgbs = Enumerable.Range(0, renderer.PixelCount)
                .Select(map)
                .Select(ToRgb)
                .ToArray()
                ;

            renderer.SetPixels(rgbs);
        }

        /// <summary>
        /// Sets all pixels based on a map function over pixel index
        /// </summary>
        /// <remarks>For convenience this overload maps to int, not uint</remarks>
        public static void SetPixels(this IPixelRenderer renderer, Func<int, int> map)
        {
            var rgbs = Enumerable.Range(0, renderer.PixelCount)
                .Select(map)
                .Cast<uint>()
                .ToArray()
                ;

            renderer.SetPixels(rgbs);
        }

        /// <summary>
        /// Applies a transform function over the current state of all pixels
        /// </summary>
        public static void Apply(this IPixelRenderer renderer, Func<Color, int, Color> map)
        {
            Apply(renderer, (rgb, i) =>
            {
                var color = FromRgb(rgb);
                var newColor = map(color, i);
                return ToRgb(newColor);
            });
        }

        /// <summary>
        /// Applies a transform function over the current state of all pixels
        /// </summary>
        /// <param name="map">A Func of the form (inputPixel,index) => outputPixel</param>
        public static void Apply(this IPixelRenderer renderer, Func<uint, int, uint> map)
        {
            var pixels = renderer.GetPixels();
            var output = pixels.Select(map).ToArray();
            renderer.SetPixels(output);
        }

        private static Color FromRgb(uint color)
        {
            // masking just discards alpha channel
            return Color.FromArgb((int)(color & 0xFFFFFF));
        }

        private static uint ToRgb(Color color)
        {
            // masking just discards alpha channel
            return (uint)(color.ToArgb() & 0xFFFFFF);
        }
    }
}
