using System;
using System.Runtime.InteropServices;

// Generated using the PInvoke Interop Assistant
// http://clrinterop.codeplex.com/releases/view/14120
// (after some encoragement, 'cause it doesn't understand stdint.h types)

namespace rpi_ws281x
{
    public class NeoPixels
    {
        int _ledCount;

        public NeoPixels(int ledCount)
        {
            _ledCount = ledCount;

            Frequency = 800000;
            Dma = 5;
            GpioPin = 18;
            Invert = false;
            Brightness = 255;
        }

        public int LedCount { get { return _ledCount; }}
        public uint Frequency { get ; set;}
        public int Dma { get ; set; } 
        public int GpioPin { get; set; }
        public bool Invert { get; set;}
        public byte Brightness {get;set;}

        public NeoPixelSession Open(){
            var data = new ws2811_t
            {
                freq = Frequency,
                dmanum = Dma,
                channel = new ws2811_channel_t[2] {
                    new ws2811_channel_t {
                        gpionum = GpioPin,
                        count = _ledCount,
                        invert = Invert ? 1 : 0,
                        brightness = Brightness,
                    },
                    new ws2811_channel_t {
                    },
                }
            };
            return new NeoPixelSession(data);
        }

        public class NeoPixelSession : IDisposable
        {
            private ws2811_t _data;

            internal NeoPixelSession (ws2811_t data)
	        {
                _data = data;

                // Have to be careful not to have a race condition here
                // Maybe create the wrapper which will do the cleandown on finallise
                // *before* the call to init?

                var ret = NativeMethods.ws2811_init(ref _data);
                if (ret != 0)
                    throw new Exception(string.Format("ws2811_init failed - returned {0}", ret));
            }

            #region Dispose
            public void Dispose()
            {
                Dispose(true);
            }

            private void Dispose(bool disposed)
            {
                NativeMethods.ws2811_fini(ref _data);
                GC.SuppressFinalize(this);
            }

            ~NeoPixelSession()
            {
                Dispose(false);
            }
            #endregion

            public void SetPixel(int i, UInt32 color)
            {
                var offset = i * 4;
                // Might be nicer to somehow write all 4 bytes at once?
                Marshal.WriteByte(_data.channel[0].leds, offset + 0, (byte)(color >> 24));
                Marshal.WriteByte(_data.channel[0].leds, offset + 1, (byte)(color >> 16));
                Marshal.WriteByte(_data.channel[0].leds, offset + 2, (byte)(color >> 8));
                Marshal.WriteByte(_data.channel[0].leds, offset + 3, (byte)color);
            }

            public void Render()
            {
                var ret = NativeMethods.ws2811_render(ref _data);
                if (ret != 0)
                    throw new Exception(string.Format("ws2811_render failed - returned {0}", ret));
            }
        }

        #region PInvoke signatures
        [StructLayout(LayoutKind.Sequential)]
        internal struct ws2811_channel_t
        {
            /// <summary>
            /// GPIO Pin with PWM alternate function, 0 if unused
            /// </summary>
            /// <remarks>int gpionum;</remarks>
            public int gpionum;

            /// <summary>
            /// Invert output signal
            /// </summary>
            /// <remarks>int invert;</remarks>
            public int invert;

            /// <summary>
            /// Number of LEDs, 0 if channel is unused
            /// </summary>
            /// <remarks>int count;</remarks>
            public int count;

            /// <summary>
            /// Brightness value between 0 and 255
            /// </summary>
            /// <remarks>int brightness;</remarks>
            public int brightness;

            /// <summary>
            /// Pointer to LED buffers, allocated by driver based on count
            /// each is uint32_t (0x00RRGGBB)
            /// </summary>
            /// <remarks>ws2811_led_t *leds;
            /// Going to have to use explicit marshalling to get the value back
            /// See http://stackoverflow.com/questions/1197181/how-to-marshal-a-variable-sized-array-of-structs-c-sharp-and-c-interop-help
            /// </remarks>
            public System.IntPtr leds;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ws2811_t
        {
            private const int RPI_PWM_CHANNELS = 2;

            /// <summary>Private data for driver use</summary>
            /// <remarks>ws2811_device*</remarks>
            public System.IntPtr device;

            /// <summary>Required output frequency</summary>
            /// <remarks>uint32_t->DWORD->unsigned int</remarks>
            public uint freq;

            /// <summary>DMA number _not_ already in use</summary>
            /// <remarks>int</remarks>
            public int dmanum;

            /// <summary></summary>
            /// <remarks>ws2811_channel_t[RPI_PWM_CHANNELS]</remarks>
            [MarshalAsAttribute(UnmanagedType.ByValArray,
                SizeConst = RPI_PWM_CHANNELS,
                ArraySubType = UnmanagedType.Struct)
            ]
            public ws2811_channel_t[] channel;
        }

        private partial class NativeMethods
        {
            /// <summary>Initialize buffers/hardware</summary>
            /// <remarks>
            /// Return Type: int
            /// ws2811: ws2811_t*
            /// </remarks>
            [DllImportAttribute("libws2811", EntryPoint = "ws2811_init")]
            public static extern int ws2811_init(ref ws2811_t ws2811);


            /// <summary>Tear it all down</summary>
            /// <remarks>
            /// Return Type: void
            /// ws2811: ws2811_t*
            /// </remarks>
            [DllImportAttribute("libws2811", EntryPoint = "ws2811_fini")]
            public static extern void ws2811_fini(ref ws2811_t ws2811);


            /// <summary>Send LEDs off to hardware</summary>
            /// <remarks>
            /// Return Type: int
            /// ws2811: ws2811_t*
            /// </remarks>
            [DllImportAttribute("libws2811", EntryPoint = "ws2811_render")]
            public static extern int ws2811_render(ref ws2811_t ws2811);


            /// <summary>Wait for DMA completion</summary>
            /// <remarks>
            /// Return Type: int
            /// ws2811: ws2811_t*
            /// </remarks>
            [DllImportAttribute("libws2811", EntryPoint = "ws2811_wait")]
            public static extern int ws2811_wait(ref ws2811_t ws2811);
        }
        #endregion
    }
}