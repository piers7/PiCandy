using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PiCandy.Server.OPC
{
    /// <summary>
    /// A simple socket server which services each incoming connection
    /// with a new instance of <typeparam name="TClient"/>
    /// </summary>
    public class SimpleSocketServer<TClient> : IDisposable
    {
        public delegate TClient CreateWorker(TcpClient client);

        private readonly IPEndPoint _endpoint;
        private readonly CancellationTokenSource _cts;
        private readonly TcpListener _listener;
        private readonly Action<TcpClient> _createWorker;
        private readonly ILog _log;

        public SimpleSocketServer(
            IPAddress ipAddress, int port,
            Action<TcpClient> createWorker,
            ILog log = null
        )
            : this(new IPEndPoint(ipAddress, port), createWorker, log)
        {
        }

        public SimpleSocketServer(IPEndPoint endpoint,
            Action<TcpClient> createWorker,
            ILog log = null
            )
        {
            _endpoint = endpoint;
            _cts = new CancellationTokenSource();
            _createWorker = createWorker;

            _log = log ?? NullLogger.Instance;

            _log.Debug("Starting up listener");
            _listener = new TcpListener(_endpoint);
            _listener.Start();

            Task.Run(() => AcceptClients(_cts.Token), _cts.Token);
        }

        public IPEndPoint EndPoint { get { return _endpoint; } }

        private async Task AcceptClients(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    if (client == null)
                        return;

                    _createWorker(client);
                }
            }
            finally
            {
                _log.Debug("AcceptClients loop stopping");
            }
        }

        public void Dispose()
        {
            _log.Debug("Shutting down listener");
            _cts.Cancel();
            _listener.Stop();
        }
    }
}
