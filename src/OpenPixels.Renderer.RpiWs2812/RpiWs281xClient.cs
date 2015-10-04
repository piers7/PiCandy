using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using OpenPixels.Renderer.RpiWs2812.Interop;
using OpenPixels.Server;
using System.Threading;
using OpenPixels.Server.Renderers;

namespace OpenPixels.Renderer.RpiWs2812
{
    /// <summary>
    /// Implementation of an <see cref="IPixelRenderer"/>
    /// that uses the libws2811 library on Raspberry Pi
    /// </summary>
    /// <remarks>
    /// Wraps Jeremy Garff's rpi_281x native library from https://github.com/jgarff/rpi_ws281x 
    /// This wrapper originally from 
    /// https://github.com/piers7/rpi_ws281x/tree/master/mono
    /// Now using more native pointer assignments, rather than Marshal.Copy
    /// Neither is more 'unsafe' than another, but direct pointer math actually cleaner in most cases
    /// </remarks>
    public class RpiWs281xClient : IPixelRenderer, IDisposable
    {
        private Ws281x_t _data; // not readonly as by-ref interop
        private readonly byte _defaultChannel;
        private readonly ILog _log;
        private readonly PixelOrder _pixelOrder;
        private int _init;

        public RpiWs281xClient(int ledCount, 
            PixelOrder pixelOrder = PixelOrder.GRB, 
            int gpioPin = RpiWs281xSetupInfo.DefaultGpio, 
            ILog log = null
        )
            : this(new RpiWs281xSetupInfo(ledCount)
            {
                GpioPin = gpioPin,
                PixelOrder = pixelOrder,
            }, log)
        {
        }

        public RpiWs281xClient(RpiWs281xSetupInfo setupInfo, ILog log = null)
            :this(setupInfo.CreateDataStructure(), setupInfo.PixelOrder, log)
        {
        }

        internal RpiWs281xClient(Ws281x_t data, PixelOrder pixelOrder = PixelOrder.GRB, ILog log = null)
        {
            _data = data;
            _defaultChannel = 0;
            _log = log ?? NullLogger.Instance;
            _pixelOrder = PixelOrder.GRB;

            _init = NativeMethods.ws2811_init(ref _data);
            if (_init != 0)
                throw new Exception(string.Format("ws2811_init failed - returned {0}", _init));
        }

        public int GpioPin
        {
            get { return _data.channel[_defaultChannel].gpionum; }
        }

        public int Brightness 
        {
            get { return _data.channel[_defaultChannel].brightness; }
            set { _data.channel[_defaultChannel].brightness = value; }
        }

        public int PixelCount
        {
            get { return _data.channel[_defaultChannel].count; }
        }

        public PixelOrder PixelOrder { get { return _pixelOrder; } }

        /// <summary>
        /// Clears all pixels to black
        /// </summary>
        public void Clear()
        {
            _log.Verbose("Clear");
            var channel = _data.channel[_defaultChannel];
            SetPixelsInternal(channel, new uint[channel.count]);
        }

        /// <summary>
        /// Sets pixel <param name="n"/> from a 32 bit color in RGB order (0x00RRGGBB).
        /// </summary>
        public unsafe void SetPixelColor(int n, uint rgb)
        {
            var color = RGBToPixelOrder(rgb);
            SetPixelInternal(n, color);
        }

        /// <summary>
        /// Sets pixel <param name="n"/> from separate R,G,B components
        /// </summary>
        public void SetPixelColor(int n, byte r, byte g, byte b)
        {
            var color = RGBToPixelOrder(r, g, b);
            SetPixelInternal(n, color);
        }

        /// <summary>
        /// Sets pixel <param name="n"/> from a 32 bit color *already in target byte order*
        /// given the <see cref="PixelOrder"/> settings for this instance
        /// (ie RGB vs GRB etc...)
        /// </summary>
        public unsafe void SetPixelInternal(int n, uint rawColor)
        {
            _log.VerboseFormat("SetPixelColor {0} = #{1:x6}", n, rawColor);
            BoundsCheck(n, 0, PixelCount - 1, "n");

            // Use 'unsafe' pointers, rather than Marshal.Copy (which has no uint overload)
            // Benefits of this approach include no more worrying about endianness
            var ptr = _data.channel[_defaultChannel].leds;
            var active = (uint*)ptr.ToPointer();
            active[n] = rawColor;
        }


        /// <summary>
        /// Sets the entire pixel buffer using a mapping function
        /// </summary>
        public unsafe void SetPixels(Func<int, uint, uint> map)
        {
            _log.VerboseFormat("SetPixels(map)");

            // Use 'unsafe' pointers, rather than Marshal.Copy (which has no uint overload)
            // Benefits of this approach include no more worrying about endianness
            var channel = _data.channel[_defaultChannel];
            var ptr = channel.leds;
            var active = (uint*)ptr.ToPointer();

            for (int n = 0; n < PixelCount; n++)
            {
                // Get existing
                var color = active[n];
                var rgb = RGBFromPixelOrder(color);

                // call the mapper
                rgb = map(n, rgb);

                // and set the result
                color = RGBToPixelOrder(rgb);
                active[n] = color;
            }
        }

        /// <summary>
        /// Sets the entire pixel buffer from as an array of packed RGB integers (0x00RRGGBB)
        /// The array is mapped to the pixel buffer from offset onwards,
        /// clipping as required (if the array is larger than the buffer)
        /// </summary>
        public unsafe void SetPixels(uint[] pixels, int offset = 0)
        {
            _log.VerboseFormat("SetPixels(uint[{0}])", pixels.Length);

            var channel = _data.channel[_defaultChannel];
            var ptr = channel.leds;
            var maxLength = channel.count;

            // Avoid buffer overruns
            var length = pixels.Length;
            if (length + offset > maxLength)
                length = maxLength - offset;

            BoundsCheck(length + offset, 0, maxLength, "pixels");

            // Use 'unsafe' pointers, rather than Marshal.Copy (which has no uint overload)
            // Benefits of this approach include no more worrying about endianness
            var active = (uint*)ptr.ToPointer();

            for (int n = 0; n < length; n++)
            {
                var rgb = pixels[n];
                var color = RGBToPixelOrder(rgb);
                active[n + offset] = color;
            }
        }

        /// <summary>
        /// Sets the entire pixel buffer as-is from the buffer provided.
        /// Note: no byte order conversion is performed. You are on your own.
        /// </summary>
        public void SetPixels(byte[] pixels)
        {
            _log.VerboseFormat("SetPixels(byte[{0}])", pixels.Length);
            var channel = _data.channel[_defaultChannel];
            var maxLength = channel.count * 4;
            BoundsCheck(pixels.Length, 0, maxLength, "pixels.Length (bytes)");

            SetPixelsInternal(channel, pixels);
        }


        /// <summary>
        /// Renders the pixel buffer to the hardware
        /// </summary>
        public void Show()
        {
            _log.Verbose("Show()");
            var ret = NativeMethods.ws2811_render(ref _data);
            if (ret != 0)
                throw new Exception(string.Format("ws2811_render failed - returned {0}", ret));
        }

        private uint RGBToPixelOrder(uint rgb)
        {
            if (PixelOrder == PixelOrder.RGB)
                return rgb;

            var r = (byte)(rgb >> 16);
            var g = (byte)(rgb >> 8);
            var b = (byte)(rgb);

            return RGBToPixelOrder(r, g, b);
        }

        private uint RGBToPixelOrder(byte r, byte g, byte b)
        {
            switch (PixelOrder)
            {
                case PixelOrder.GRB:
                    return (uint)(g << 16) | (uint)(r << 8) | b;

                case PixelOrder.BRG:
                    return (uint)(b << 16) | (uint)(r << 8) | g;

                case PixelOrder.RGB:
                default:
                    return (uint)(r << 16) | (uint)(g << 8) | b;
            }
        }

        private uint RGBFromPixelOrder(uint color)
        {
            if (PixelOrder == PixelOrder.RGB)
                return color;

            var c1 = (byte)(color >> 16);
            var c2 = (byte)(color >> 8);
            var c3 = (byte)(color);

            switch (PixelOrder)
            {
                case PixelOrder.GRB:
                    return Pack(c2, c1, c3);

                case PixelOrder.BRG:
                    return Pack(c2, c3, c1);

                case PixelOrder.RGB:
                default:
                    return color;
            }
        }

        /// <summary>
        /// Gets the color of pixel <param name="n"/> as a 32 bit RGB color (0x00RRGGBB)
        /// </summary>
        public uint GetPixelColor(int n)
        {
            BoundsCheck(n, 0, PixelCount - 1, "n");

            var offset = n * 4;
            var ptr = _data.channel[_defaultChannel].leds;

            var a = Marshal.ReadByte(ptr, offset + 0);
            var r = Marshal.ReadByte(ptr, offset + 1);
            var g = Marshal.ReadByte(ptr, offset + 2);
            var b = Marshal.ReadByte(ptr, offset + 3);

            var bytes = new byte[] { b, g, r, a }; // little endian layout
            if (!BitConverter.IsLittleEndian)
                bytes = Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Gets the entire pixel buffer, as an array of packed integers (0x00RRGGBB)
        /// </summary>
        public uint[] GetPixels()
        {
            var channel = _data.channel[_defaultChannel];
            var buffer = GetPixelsInternal(channel);
            return FromBytes(buffer);
        }

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposed)
        {
            // Only tear down if we sucesfully init'd in the first place
            // (and even then, only do it once)
            if(Interlocked.Exchange(ref _init, 0) > 0)
                NativeMethods.ws2811_fini(ref _data);

            GC.SuppressFinalize(this);
        }

        ~RpiWs281xClient()
        {
            Dispose(false);
        }
        #endregion

        private static void BoundsCheck(int value, int lowerBound, int upperBound, string paramName)
        {
            // Probably get sued by Oracle for this
            if (value < lowerBound || value > upperBound)
            {
                var errorMessage = string.Format("'{0}' must be between {1} and {2}; was {3}",
                    paramName, lowerBound, upperBound, value);
                throw new ArgumentOutOfRangeException(paramName, errorMessage);
            }
        }

        /// <summary>
        /// Wheel function to cycle color through RGB over 255 points
        /// </summary>
        /// <remarks>
        /// from https://github.com/adafruit/Adafruit_NeoPixel/blob/master/examples/buttoncycler/buttoncycler.ino
        /// </remarks> 
        public static uint Wheel(byte i)
        {
            unchecked // wrap-around overflows are fine
            {
                i = (byte)(255 - i);
                if (i < 85)
                {
                    return Pack((byte)(255 - i * 3), 0, (byte)(i * 3));
                }
                else if (i < 170)
                {
                    i -= 85;
                    return Pack(0, (byte)(i * 3), (byte)(255 - i * 3));
                }
                else
                {
                    i -= 170;
                    return Pack((byte)(i * 3), (byte)(255 - i * 3), 0);
                }
            }
        }

        public static uint Wheel(int value, int lowerBound, int length)
        {
            // Scale to 0-255
            var scaledValue = (value * 255 / (double)length) - lowerBound;
            return Wheel((byte)scaledValue);
        }

        /// <summary>
        /// Packs r,g,b bytes into a packed (big endian) uint32 representation
        /// </summary>
        public static uint Pack(byte r, byte g, byte b)
        {
            return (uint)(r << 16) | (uint)(g << 8) | b;
        }

        private static void Unpack(uint rgb, out byte r, out byte g, out byte b){
            r = (byte)(rgb >> 16);
            g = (byte)(rgb >> 8);
            b = (byte)(rgb);
        }

        /// <summary>
        /// Unpacks a uint32 into it's a,r,g,b components
        /// </summary>
        /// <remarks>This essentially gives a big-endian representation of the
        /// actual bits in <param name="color"/></remarks>
        public static byte[] Color(uint color)
        {
            return new byte[]{
                (byte)(color >> 24),
                (byte)(color >> 16), // r
                (byte)(color >> 8),  // g
                (byte)(color)        // b
            };
        }

        private static byte[] ToBytes(uint[] pixels)
        {
            var buffer = new byte[pixels.Length * 4];
            for (int i = 0; i < pixels.Length; i++)
            {
                var packedColor = pixels[i];
                var colorBytes = Color(packedColor);

                if (BitConverter.IsLittleEndian)
                    colorBytes = Reverse(colorBytes);

                colorBytes.CopyTo(buffer, i * 4);
            }
            return buffer;
        }

        private static uint[] FromBytes(byte[] buffer)
        {
            // Don't think this actually works properly right now
            // not endian, and pixel order aware
            throw new NotImplementedException();
            //if (buffer.Length % 4 != 0)
            //    throw new ArgumentException("Can only deal with multiples of 4 bytes");

            //var pixels = new uint[buffer.Length / 4];
            //for (int i = 0; i < pixels.Length; i++)
            //{
            //    var colorBytes = new byte[4];
            //    buffer.CopyTo(colorBytes, i * 4);
            //    var pixel = Color(colorBytes);
            //    pixels[i] = pixel;
            //}
            //return pixels;
        }

        private static T[] Reverse<T>(T[] input)
        {
            var output = new T[input.Length];
            for (int i = 0; i < input.Length; i++)
                output[i] = input[input.Length - 1 - i];
            return output;
        }

        #region Private marshalling methods
        private void SetPixelsInternal(Ws281x_channel_t channel, uint[] pixels)
        {
            var blittable = (int[])(object)pixels;
            Marshal.Copy(blittable, 0, channel.leds, blittable.Length);
        }

        private void SetPixelsInternal(Ws281x_channel_t channel, byte[] pixels)
        {
            Marshal.Copy(pixels, 0, channel.leds, pixels.Length);
        }

        private byte[] GetPixelsInternal(Ws281x_channel_t channel)
        {
            var buffer = new byte[channel.count * 4];
            Marshal.Copy(channel.leds, buffer, 0, buffer.Length);
            return buffer;
        }
        #endregion
        // Marshal.StructureToPtr
        // See http://blogs.msdn.com/b/dsvc/archive/2009/11/02/p-invoke-marshal-structuretoptr.aspx
        // for better description of 3rd parameter 'fDeleteOld'
        // However, turns out we don't need that anyway - Marshal.Copy is what I should have been using
    }
}
