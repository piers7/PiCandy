using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server
{
    /// <summary>
    /// Allows control of actual pixel-rendering hardware
    /// </summary>
    public interface IPixelRenderer
    {
        void SetPixels(byte[] data);
    }
}
