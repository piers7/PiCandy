using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GPIO
{
    class Program
    {
        static void Main(string[] args)
        {
            var mode = args[0];
            var pin = Convert.ToInt32(args[1]);

            setup_io();

            switch (mode.ToLowerInvariant())
            {
                case "on":
                    set_out(pin);
                    switch_gpio(1, pin);
                    break;
                case "off":
                    set_out(pin);
                    switch_gpio(0, pin);
                    break;
            }
        }

        [DllImport("libpigpio")]
        static extern void setup_io();
        [DllImport("libpigpio")]
        static extern void switch_gpio(int val, int pin);
        [DllImport("libpigpio")]
        static extern int check_gpio(int pin);
        [DllImport("libpigpio")]
        static extern void set_in(int gpio);
        [DllImport("libpigpio")]
        static extern void set_out(int gpio);
    }
}
