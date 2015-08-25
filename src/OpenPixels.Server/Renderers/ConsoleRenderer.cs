using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.Renderers
{
    public class ConsoleRenderer : TextWriterRenderer
    {
        public ConsoleRenderer(int channel)
            :base(channel, Console.Out, 20)
        {

        }
    }
}
