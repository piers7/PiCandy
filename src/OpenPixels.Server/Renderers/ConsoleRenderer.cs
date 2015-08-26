using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.Renderers
{
    public class ConsoleRenderer : TextWriterRenderer
    {
        public ConsoleRenderer(int channel, int width = 20)
            :base(channel, Console.Out, width)
        {

        }
    }
}
