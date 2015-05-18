using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ws2812test
{
    class Program
    {
        static void Main(string[] args)
        {
            var output = Wheel(50);
            Console.WriteLine(output);
        }

        /*
         * typedef struct {
	        unsigned char r;
	        unsigned char g;
	        unsigned char b;
         } Color_t;
         */

        struct Color_t
        {
            public byte r;
            public byte g;
            public byte b;

            public override string ToString()
            {
                return string.Format("{0}/{1}/{2}", r, g, b);
            }
        }

        /*
            Color_t Wheel(uint8_t WheelPos)
        */

        [DllImport("libws2812")]
        static extern Color_t Wheel(byte wheelPos);
    }
}
