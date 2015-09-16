using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPixels.Server.OPC
{
    /// <summary>
    /// A message-level abstraction over a stream
    /// using the OPC protocol (typically NetworkStream).
    /// The session will live until the session is closed.
    /// </summary>
    public class OpcReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly EndPoint _localEndpoint;
        private readonly EndPoint _remoteEndpoint;
        private readonly ILog _log;

        public OpcReader(TcpClient client, ILog log = null)
            :this(client.GetStream(), client.Client.LocalEndPoint, client.Client.RemoteEndPoint, log)
        {
        }

        public OpcReader(Stream stream, EndPoint localEndpoint, EndPoint remoteEndpoint, ILog log = null)
        {
            _stream = stream;
            _localEndpoint = localEndpoint;
            _remoteEndpoint = remoteEndpoint;
            _log = log ?? NullLogger.Instance;
        }

        public static Task PumpAllMessages(OpcReader reader, CancellationToken token, Action<OpcMessage> handler)
        {
            return Task.Run(async () =>
            {
                OpcMessage? message;
                do
                {
                    message = await reader.ReadMessageAsync(token).ConfigureAwait(false);
                    if (message != null)
                        handler(message.Value);
                } while (message != null && !token.IsCancellationRequested);

                reader.Dispose();

            }, token);
        }

        public Task<OpcMessage?> ReadMessageAsync()
        {
            var ignored = new CancellationTokenSource();
            return ReadMessageAsync(ignored.Token);
        }

        public async Task<OpcMessage?> ReadMessageAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested) return null;

            // Handle message header
            _log.Verbose("Wait for header...");
            var header = new byte[4];
            if (!await _stream.TryReadAsync(header, token).ConfigureAwait(false))
            {
                _log.Warn("Aborting - header not read");
                return null;
            }
            _log.Verbose("Got header");

            var message = new OpcMessage()
            {
                Channel = header[0],
                Command = (OpcCommandType)header[1],
                Length = ReadUInt16(header, 2),
                Data = new byte[0],
            };

            if (token.IsCancellationRequested) return null;

            // Handle message payload
            if (message.Length > 0)
            {
                _log.VerboseFormat("Wait for content of {0}b...", message.Length);

                // read in message content
                // slightly rubbish impl. - just use one big array
                // but then we have to generate an array from it anyway, so (shrugs)
                var data = new byte[message.Length];
                if (!await _stream.TryReadAsync(data, token).ConfigureAwait(false))
                {
                    _log.Warn("Aborting - body not read");
                    return null;
                }
                _log.VerboseFormat("Got message content of {0}b", message.Length);

                message.Data = data;
            }
            return message;
        }

        internal static ushort ReadUInt16(byte[] header, int startIndex)
        {
            return EndianConverter.BigEndianConverter.ToUInt16(header, startIndex);
        }

        public EndPoint LocalEndPoint { get { return _localEndpoint; } }
        public EndPoint RemoteEndPoint { get { return _remoteEndpoint; } }

        public void Dispose()
        {
            _stream.Close();
        }
    }
}
