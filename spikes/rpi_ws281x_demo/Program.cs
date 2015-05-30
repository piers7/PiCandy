using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace rpi_ws281x
{
    class Program
    {
        const int TARGET_FREQ                              = 800000;
        const int GPIO_PIN                                 = 18;
        const int DMA                                      = 5;

        const int WIDTH                                    = 8;
        const int HEIGHT                                   = 8;
        const int LED_COUNT                                = (WIDTH * HEIGHT);

        static byte _cancelRequested;

        static int Main(string[] args)
        {
            var cancelRequested = false;
            Console.TreatControlCAsInput = true;
            Console.CancelKeyPress += (o,e) => _cancelRequested = 1;

            var client = new NeoPixels(LED_COUNT);

            using (var session = client.Open())
            {
                for (int i = 0; i < LED_COUNT; i++)
                {
                    if (Thread.VolatileRead(ref _cancelRequested) > 0)
                        return 0;

                    Console.WriteLine("Set LED {0}", i);
                    session.SetPixel(i, 0x00FF0000);
                    session.Render();
                    Thread.Sleep(100);
                }
            }

            //var data = new ws2811_t{
            //    freq = TARGET_FREQ,
            //    dmanum = DMA,
            //    channel = new ws2811_channel_t[2] {
            //        new ws2811_channel_t {
            //            gpionum = GPIO_PIN,
            //            count = LED_COUNT,
            //            invert = 0,
            //            brightness = 255,
            //        },
            //        new ws2811_channel_t {
            //        },
            //    }
            //};

            //int ret;
            //if ((ret = NativeMethods.ws2811_init(ref data)) != 0)
            //{
            //    Console.WriteLine("ws2811_init failed - exit code {0}", ret);
            //    return -1;
            //}

            //var ledData = new UInt32[LED_COUNT];

            //try{
            //    for (int i = 0; i < LED_COUNT; i++)
            //    {
            //        if (cancelRequested)
            //            return 0;

            //        Console.WriteLine("Set LED {0}", i);
            //        ledData[i] = 0x00FF0000;

            //        // Push array as structure back to pointer address
            //        // TODO: should I be passing 'delete old=true'?
            //        //Marshal.StructureToPtr(ledData, data.channel[0].leds, false);

            //        var offset = i * 4;
            //        Marshal.WriteByte(data.channel[0].leds, offset + 0, 0);
            //        Marshal.WriteByte(data.channel[0].leds, offset + 1, 255);
            //        Marshal.WriteByte(data.channel[0].leds, offset + 2, 0);
            //        Marshal.WriteByte(data.channel[0].leds, offset + 3, 0);

            //        if ((ret = NativeMethods.ws2811_render(ref data)) != 0)
            //        {
            //            Console.WriteLine("ws2811_init failed - exit code {0}", ret);
            //            return -1;
            //        }

            //        Thread.Sleep(TimeSpan.FromMilliseconds(100));
            //    }

            //}finally{
            //    Console.WriteLine("Tear down");
            //    NativeMethods.ws2811_fini(ref data);
            //}
            return 0;
        }
    }
}
