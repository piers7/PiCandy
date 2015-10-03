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
        readonly ILookup<int, IPixelRenderer> _channels;
        readonly ILog _log;

        public OpenPixelsServer(
            IEnumerable<ICommandSource> sources,
            IEnumerable<Lazy<IPixelRenderer, ChannelInfo>> channels,
            ILog log
            )
        {
            _sources = sources;
            _channels = channels.ToLookup(
                c => c.Meta.Channel,
                c => c.Value
                );
            _log = log;

            // Hook up to all listeners
            foreach (var listener in _sources)
                listener.CommandAvailable += DispatchCommand;

            // sanity check
            if (!_channels.Any())
                throw new InvalidOperationException("No channels configured");
        }

        public IEnumerable<ICommandSource> Sources
        {
            get { return _sources; }
        }

        public ILookup<int, IPixelRenderer> Channels
        {
            get { return _channels; }
        }

        public IEnumerable<IPixelRenderer> AllRenderers
        {
            get { return Channels.SelectMany(c => c); }
        }

        private void DispatchCommand(object sender, ICommand command)
        {
            var renderers = _channels[command.Channel];
            foreach (var renderer in renderers)
            {
                _log.VerboseFormat("Dispatch {0} to {1}", command, renderer);
                command.Execute(renderer);
            }
        }

        public void Dispose()
        {
            foreach (var listener in _sources)
                listener.CommandAvailable -= DispatchCommand;

            foreach (var renderer in _channels.SelectMany(c => c))
            {
                var disposable = renderer as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }
    }
}
