using OpenPixels.Server.Filters;
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
        readonly ILookup<int, Lazy<IPixelRenderer>> _channels;
        readonly Func<string, IPositionalMap> _getMap;
        readonly ILog _log;

        public OpenPixelsServer(
            IEnumerable<ICommandSource> sources,
            IEnumerable<Lazy<IPixelRenderer, ChannelMetadata>> channels,
            Func<string, IPositionalMap> getMap,
            ILog log = null
            )
        {
            _log = log ?? NullLogger.Instance;
            _sources = sources;
            _channels = ValidOnly(channels).ToLookup(
                c => c.Meta.Channel,
                c => GetRenderer(c)
                );
            _getMap = getMap;

            // Hook up to all listeners
            foreach (var listener in _sources)
                listener.CommandAvailable += DispatchCommand;

            // sanity check
            if (!_channels.Any())
                throw new InvalidOperationException("No channels configured");
        }

        private IEnumerable<Lazy<TValue, ChannelMetadata>> ValidOnly<TValue>(IEnumerable<Lazy<TValue, ChannelMetadata>> seq)
        {
            return seq
                .Where(item =>
                {
                    try
                    {
                        var ignored = item.Value;
                        return true;
                    }
                    catch (Exception err)
                    {
                        var errorMessage = string.Format("A renderer on channel #{0} failed to initialize: {1}", item.Meta.Channel, err.Message);
                        _log.Warn(errorMessage);
                        return false;
                    }
                })
                .ToArray()
                ;
        }

        private Lazy<IPixelRenderer> GetRenderer(Lazy<IPixelRenderer, ChannelMetadata> c)
        {
            return new Lazy<IPixelRenderer>(() =>
            {
                if (!string.IsNullOrEmpty(c.Meta.Map))
                {
                    var renderer = c.Value;
                    var map = _getMap(c.Meta.Map);
                    var adapter = new MappedPixelsDecorator(renderer, map.GetMappedIndex);
                    return adapter;
                }
                return c.Value;
            });
        }

        public IEnumerable<ICommandSource> Sources
        {
            get { return _sources; }
        }

        public ILookup<int, Lazy<IPixelRenderer>> Channels
        {
            get { return _channels; }
        }

        public IEnumerable<IPixelRenderer> AllRenderers
        {
            get { return Channels.SelectMany(c => c.Select(v => v.Value)); }
        }

        private void DispatchCommand(object sender, ICommand command)
        {
            var renderers = _channels[command.Channel];
            foreach (var renderer in renderers)
            {
                _log.VerboseFormat("Dispatch {0} to {1}", command, renderer.Value);
                command.Execute(renderer.Value);
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
