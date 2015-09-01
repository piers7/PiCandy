using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server
{
    /// <summary>
    /// Routes <see cref="ICommands"/> to the relevant <see cref="IPixelRenderer"/>
    /// based on source/destination channel matching.
    /// </summary>
    public class OpenPixelsServer : IDisposable
    {
        readonly IEnumerable<ICommandSource> _sources;
        readonly IEnumerable<IPixelChannel> _renderers;
        readonly ILog _log;

        public OpenPixelsServer(
            IEnumerable<ICommandSource> sources,
            IEnumerable<IPixelChannel> renderers,
            ILog log
            )
        {
            _sources = sources;
            _renderers = renderers;
            _log = log;

            // Hook up to all listeners
            foreach (var listener in _sources)
                listener.CommandAvailable += DispatchCommand;
        }

        public IEnumerable<ICommandSource> Sources
        {
            get { return _sources; }
        }

        public IEnumerable<IPixelChannel> Renderers
        {
            get { return _renderers; }
        }

        private void DispatchCommand(object sender, ICommand command)
        {
            foreach (var renderer in _renderers.Where(r => r.Channel == command.Channel))
            {
                _log.VerboseFormat("Dispatch {0} to {1}", command, renderer);
                command.Execute(renderer.Renderer);
            }
        }

        public void Dispose()
        {
            foreach (var listener in _sources)
                listener.CommandAvailable -= DispatchCommand;
        }
    }
}
