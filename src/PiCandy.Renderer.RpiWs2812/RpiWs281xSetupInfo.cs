using System;
using System.Collections.Generic;
using System.Text;
using PiCandy.Renderer.RpiWs2812.Interop;
using PiCandy.Server.Renderers;

namespace PiCandy.Renderer.RpiWs2812
{
    public class RpiWs281xSetupInfo
    {
        internal const uint DefaultFrequency = 800000;
        internal const int DefaultDma = 5;
        internal const int DefaultGpio = 18;
        internal const int DefaultBrightness = 255;

        readonly int _ledCount;

        public RpiWs281xSetupInfo(int ledCount)
        {
            _ledCount = ledCount;

            Frequency = DefaultFrequency;
            Dma = DefaultDma;
            GpioPin = DefaultGpio;
            Invert = false;
            Brightness = DefaultBrightness;
            PixelOrder = PixelOrder.GRB;
        }

        public int LedCount { get { return _ledCount; } }
        public uint Frequency { get; set; }
        public int Dma { get; set; }
        public int GpioPin { get; set; }
        public bool Invert { get; set; }
        public byte Brightness { get; set; }
        public PixelOrder PixelOrder { get; set; }

        internal Ws281x_t CreateDataStructure()
        {
            var data = new Ws281x_t
            {
                freq = Frequency,
                dmanum = Dma,
                channel = new [] {
                        new Ws281x_channel_t {
                            gpionum = GpioPin,
                            count = LedCount,
                            invert = Invert ? 1 : 0,
                            brightness = Brightness,
                        },
                        new Ws281x_channel_t {
                        },
                    }
            };

            return data;
        }
    }
}
