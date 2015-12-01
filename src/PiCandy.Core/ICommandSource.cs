using PiCandy.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy
{
    /// <summary>
    /// Receives pixel-rendering commands from the outside world
    /// </summary>
    public interface ICommandSource
    {
        event EventHandler<ICommand> CommandAvailable;
    }
}
