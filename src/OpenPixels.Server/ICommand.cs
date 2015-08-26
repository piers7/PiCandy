using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server
{
    public interface ICommand
    {
        int Channel { get; }
        Action<IPixelRenderer> Execute { get; }
    }
}
