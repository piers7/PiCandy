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

        static int _cancelRequested;

        static int Main(string[] args)
        {
            //var cancelRequested = false;
            //Console.TreatControlCAsInput = true;
            //Console.CancelKeyPress += (o,e) => _cancelRequested = 1;

            // Here's a good post on capturing *all* termination signals
            // http://www.developersalley.com/blog/post/2011/05/13/How-To-Catch-CTRL-C-Or-Break-In-C-Console-Application.aspx
            // http://www.codeproject.com/Articles/2357/Console-Event-Handling
            // Better than Console.TreatControlCAsInput
            // but *oh dear* not cross-platform
            ProcessCancellationToken.SetupHooks();

            var client = new NeoPixels(LED_COUNT);
            using (var session = client.Open())
            {
                for (int i = 0; i < LED_COUNT; i++)
                {
                    if (ProcessCancellationToken.CancelRequested || CancelKeyPressed())
                        return 0;

                    Console.WriteLine("Set LED {0}", i);
                    session.SetPixel(i, 0x00FF0000);
                    session.Render();

                    if (ProcessCancellationToken.CancelRequested || CancelKeyPressed())
                        Thread.Sleep(100);
                }
            }
            return 0;
        }

        private static bool CancelKeyPressed()
        {
            while (Console.KeyAvailable)
            {
                var key = Console.ReadKey();
                switch(key.Key)
                {
                    case ConsoleKey.Escape:
                        return true;
                }
            }
            return false;
        }
    }
}
