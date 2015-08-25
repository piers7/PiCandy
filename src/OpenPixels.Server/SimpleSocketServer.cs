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
    /// A simple socket server which services each incomming connection
    /// with a new instance of <typeparam name="TClient"/>
    /// </summary>
    public class SimpleSocketServer<TClient> : IDisposable
    {
        public delegate TClient CreateWorker(TcpClient client);
        public delegate Func<CancellationToken, Task> RunWorkerAsync(TClient worker);

        private readonly IPEndPoint _endpoint;
        private readonly CancellationTokenSource _cts;
        private TcpListener _listener;
        private readonly CreateWorker _createWorker;
        private readonly RunWorkerAsync _getRunWorkerAction;

        public SimpleSocketServer(IPAddress ipAddress, int port,
            CreateWorker createWorker,
            RunWorkerAsync runWorkerAsync
        )
            : this(new IPEndPoint(ipAddress, port), createWorker, runWorkerAsync)
        {
        }

        public SimpleSocketServer(IPEndPoint endpoint,
            CreateWorker createWorker,
            RunWorkerAsync runWorkerAsync
            )
        {
            _endpoint = endpoint;
            _cts = new CancellationTokenSource();
            _createWorker = createWorker;
            _getRunWorkerAction = runWorkerAsync;

            Log = NullLogger.Instance;

            Log.Debug("Starting up listener");
            _listener = new TcpListener(_endpoint);
            _listener.Start();

            Task.Run(() => AcceptClients(_cts.Token), _cts.Token);
        }

        public ILog Log { get; set; }
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
                    var client = await _listener.AcceptTcpClientAsync()
                        .ConfigureAwait(false)
                        ;
                    if (client == null)
                        return;

                    var worker = _createWorker(client);
                    OnClientConnected(worker);

                    var runWorkerAsync = _getRunWorkerAction(worker);
                    Task.Run(() => runWorkerAsync(token), token);
                }
            }
            finally
            {
                Log.Debug("AcceptClients loop stopping");
            }
        }

        public void Dispose()
        {
            Log.Debug("Shutting down listener");
            _cts.Cancel();
            _listener.Stop();
        }
    }
}
