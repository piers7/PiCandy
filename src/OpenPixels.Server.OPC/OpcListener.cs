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
    /// A simple message-level server abstraction over sockets
    /// for the OPC protocol
    /// </summary>
    public class OpcListener : IDisposable
    {
        private readonly System.Net.IPEndPoint _endpoint;
        private readonly CancellationTokenSource _cts;
        private System.Net.Sockets.TcpListener _listener;

        public static OpcListener Start(IPEndPoint endpoint)
        {
            return new OpcListener(endpoint);
        }

        public static OpcListener Start(IPAddress ipAddress, int port)
        {
            var endpoint = new IPEndPoint(ipAddress, port);
            return Start(endpoint);
        }

        private OpcListener(System.Net.IPEndPoint endpoint)
        {
            _endpoint = endpoint;
            _cts = new CancellationTokenSource();

            Console.WriteLine("Starting up listener");
            _listener = new System.Net.Sockets.TcpListener(_endpoint);
            _listener.Start();

            Task.Run(() => AcceptClients(_cts.Token), _cts.Token);
        }

        public IPEndPoint EndPoint { get { return _endpoint; } }

        public event EventHandler<OpcClientSession> ClientConnected;

        protected void OnClientConnected(OpcClientSession client)
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
                    var client = await _listener.AcceptTcpClientAsync();
                    if (client == null)
                        return;

                    var worker = new OpcClientSession(client);
                    OnClientConnected(worker);
                    Task
                        .Run(() => worker.Run(token), token)
                        //.Delay(1000)
                        //.ContinueWith(_ => worker.Run(token), token)
                        ;
                }
            }
            finally
            {
                Console.WriteLine("AcceptClients loop stopping");
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Shutting down listener");
            _cts.Cancel();
            _listener.Stop();
        }


    }
}
