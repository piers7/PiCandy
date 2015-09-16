using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Renderer.RpiWs2812
{
    /// <summary>
    /// Indicates the expected byte order for the Ws2812's.
    /// Different strands require different inputs.
    /// GRB is normally the default.
    /// </summary>
    public enum PixelOrder
    {
        RGB     = 0x00,
        GRB     = 0x01,
        BRG     = 0x04,
    }
}
