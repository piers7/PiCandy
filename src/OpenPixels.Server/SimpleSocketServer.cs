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
    /// A simple socket server which services each incoming connection
    /// with a new instance of <typeparam name="TClient"/>
    /// </summary>
    public class SimpleSocketServer<TClient> : IDisposable
    {
        public delegate TClient CreateWorker(TcpClient client);

        private readonly IPEndPoint _endpoint;
        private readonly CancellationTokenSource _cts;
        private readonly TcpListener _listener;
        private readonly CreateWorker _createWorker;
        private readonly ILog _log;

        public SimpleSocketServer(
            IPAddress ipAddress, int port,
            CreateWorker createWorker,
            ILog log = null
        )
            : this(new IPEndPoint(ipAddress, port), createWorker, log)
        {
        }

        public SimpleSocketServer(IPEndPoint endpoint,
            CreateWorker createWorker,
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

        public event EventHandler<TClient> ClientConnected;

        protected void OnClientConnected(TClient client)
        {
            var handler = ClientConnected;
            if (handler != null) handler(this, client);
        }

        private async Task AcceptClients(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    if (client == null)
                        return;

                    var worker = _createWorker(client);
                    OnClientConnected(worker);
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
