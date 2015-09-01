using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPixels.Server.OPC
{
    /// <summary>
    /// Services a <see cref="SimpleSocketServer"/> with <see cref="OpcClientSession"/>s,
    /// and raises the commands received as events for general consumption
    /// </summary>
    public class OpcCommandListener : ICommandSource, IDisposable
    {
        readonly SimpleSocketServer<OpcClientSession> _listener;
        readonly ILog _log;

        public OpcCommandListener(IPEndPoint endpoint, ILog log = null)
        {
            _log = log ?? NullLogger.Instance;
            _listener = new SimpleSocketServer<OpcClientSession>(
                endpoint, 
                CreateSession,
                log
            );
            _listener.ClientConnected += HandleClientConnected;
        }

        private OpcClientSession CreateSession(System.Net.Sockets.TcpClient client)
        {
            return new OpcClientSession(client, _log);
        }

        private void HandleClientConnected(object sender, OpcClientSession session)
        {
            var cancel = new CancellationTokenSource();
            Task.Run(async () =>
            {
                OpcMessage message;
                do
                {
                    message = await session.ReadMessageAsync(cancel.Token);
                } while (message != null);
            });

            session.MessageReceived += HandleMessageReceived;
        }

        private void HandleMessageReceived(object sender, OpcMessage e)
        {
            // Expose the raw message (debugging / tests)
            OnMessageReceived(e);

            // Also expose the system-level command
            switch (e.Command)
            {
                case OpcCommandType.SetPixels:
                    _log.VerboseFormat("Dispatch SetPixels(byte[{1}]) to channel {0}", e.Channel, e.Data.Length);
                    OnCommandAvailable(e.Channel, r => r.SetPixels(e.Data));
                    break;

                case OpcCommandType.SystemExclusive:{
                    // Payload starts with a system ID 0x00 0x01 = FadeCandy
                    // It seems to have 2 byte 'command id' for these:
                    // 01 - color correction
                    // 02 - firmware config
                    var systemId = OpcClientSession.ReadUInt16(e.Data, 0);
                    _log.WarnFormat("System-specific command for {0:x4} not handled", systemId);
                   break;
                }

                default:
                    _log.WarnFormat("Command {0}-{1} unhandled", e.Channel, e.Command);
                    break;
            };
        }

        public event EventHandler<OpcMessage> MessageReceived;

        private void OnMessageReceived(OpcMessage message)
        {
            var handler = MessageReceived;
            if (handler != null) handler(this, message);
        }

        public event EventHandler<ICommand> CommandAvailable;

        private void OnCommandAvailable(int channel, Action<IPixelRenderer> action)
        {
            var command = new DelegateCommand(channel, action);
            OnCommandAvailable(command);
        }

        private void OnCommandAvailable(ICommand action)
        {
            var handler = CommandAvailable;
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
