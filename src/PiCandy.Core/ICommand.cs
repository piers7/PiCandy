using PiCandy.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy
{
    /// <summary>
    /// Represents a request within the system to render pixels on a device
    /// </summary>
    public interface ICommand
    {
        int Channel { get; }
        Action<IPixelRenderer> Execute { get; }
    }
}
