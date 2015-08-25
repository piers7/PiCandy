using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server
{
    public interface IPixelRenderer
    {
        int Channel { get; }
        void SetPixels(byte[] data);
    }
}
