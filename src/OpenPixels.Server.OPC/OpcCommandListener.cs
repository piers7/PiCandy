using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.OPC
{
    class OpcCommandListener : ICommandSource, IDisposable
    {
        SimpleSocketServer<OpcClientSession> _listener;

        public OpcCommandListener(IPEndPoint endpoint)
        {
            _listener = new SimpleSocketServer<OpcClientSession>(endpoint, 
                c => new OpcClientSession(c),
                s => s.DoWorkAsync
            );
            _listener.ClientConnected += HandleClientConnected;
        }

        private void HandleClientConnected(object sender, OpcClientSession e)
        {
            e.MessageReceived += HandleMessageReceived;
        }

        private void HandleMessageReceived(object sender, OpcMessage e)
        {
            switch (e.Command)
            {
                case 0:
                    OnExecuteCommand(c =>
                    {
                        if (c.Channel == e.Channel)
                            c.SetPixels(e.Data);
                    });
                    break;
            };
        }

        public event EventHandler<Action<IPixelRenderer>> ExecuteCommand;

        private void OnExecuteCommand(Action<IPixelRenderer> action)
        {
            var handler = ExecuteCommand;
            if (handler != null) handler(this, action);
        }

        public override string ToString()
        {
            return string.Format("{0}({1}:{2})", GetType(), 
                _listener.EndPoint.Address == IPAddress.Any ? "*" : _listener.EndPoint.Address.ToString(), 
                _listener.EndPoint.Port
                );
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}
