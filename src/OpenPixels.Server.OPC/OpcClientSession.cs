using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPixels.Server.OPC
{
    /// <summary>
    /// A message-level abstraction over a socket connection
    /// using the OPC protocol.
    /// The session will live until the session is closed.
    /// </summary>
    public class OpcClientSession : IWorker
    {
        private readonly System.Net.Sockets.TcpClient _client;

        public OpcClientSession(System.Net.Sockets.TcpClient client)
        {
            _client = client;
        }

        public event EventHandler<OpcMessage> MessageReceived;

        protected void OnMessageReceived(OpcMessage message)
        {
            var handler = MessageReceived;
            if (handler != null) handler(this, message);
        }

        public async Task DoWorkAsync(CancellationToken token)
        {
            try
            {
                Console.WriteLine("Starting up client");
                var stream = _client.GetStream();
                while (!token.IsCancellationRequested)
                {
                    // Handle message header
                    Console.WriteLine("Wait for header...");
                    var header = new byte[4];
                    if (!await stream.TryReadAsync(header).ConfigureAwait(false))
                    {
                        Console.WriteLine("Aborting - header not read");
                        return;
                    }
                    Console.WriteLine("Got header");

                    var message = new OpcMessage()
                    {
                        Channel = header[0],
                        Command = header[1],
                        Length = ReadUInt16(header, 2),
                        Data = new byte[0],
                    };

                    // Handle message payload
                    if (message.Length > 0)
                    {
                        Console.WriteLine("Wait for content of {0}...", message.Length);

                        // read in message content
                        // slightly rubbish impl. - just use one big array
                        // but then we have to generate an array from it anyway, so (shrugs)
                        var data = new byte[message.Length];
                        if (!await stream.TryReadAsync(data).ConfigureAwait(false))
                        {
                            Console.WriteLine("Aborting - body not read");
                            return;
                        }
                        Console.WriteLine("Got message content of {0}", message.Length);

                        message.Data = data;
                    }

                    // Broadcast message for handling
                    OnMessageReceived(message);

                    // Seems like (as far as I can tell)
                    // the protocol (or at least the test implementation)
                    // doesn't bother about nice things like keeping sockets open
                    // or any protocol-level ACKing
                    // So... I think I'll drop the client here
                    // so I don't leak
                    // NB: This is where RX would be handy
                    // because the clients could de-register cleanly
                    // whereas with events we've got a memory leak :-(
                    // return;
                }
            }
            finally
            {
                _client.Close();
            }
        }

        private ushort ReadUInt16(byte[] header, int startIndex)
        {
            return EndianConverter.BigEndianConverter.ToUInt16(header, startIndex);
        }

        public EndPoint LocalEndPoint { get { return _client.Client.LocalEndPoint; }}
        public EndPoint RemoteEndPoint { get { return _client.Client.RemoteEndPoint; }}
    }
}
