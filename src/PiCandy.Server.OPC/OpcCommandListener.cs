﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PiCandy.Logging;
using PiCandy.Rendering;
using System.Net.Sockets;

namespace PiCandy.Server.OPC
{
    /// <summary>
    /// Services a <see cref="SimpleSocketServer"/> with <see cref="OpcReader"/>s,
    /// and raises the commands received as events for general consumption
    /// </summary>
    public class OpcCommandListener : ICommandSource, IDisposable
    {
        readonly SimpleSocketServer<OpcReader> _listener;
        readonly ILog _log;
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public OpcCommandListener(IPEndPoint endpoint, ILog log = null)
        {
            _log = log ?? NullLogger.Instance;
            _listener = new SimpleSocketServer<OpcReader>(
                endpoint, 
                CreateSession,
                log
            );
        }

        private void CreateSession(System.Net.Sockets.TcpClient client)
        {
            // Cancel any existing sessions
            _cancel.Cancel();
            _cancel = new CancellationTokenSource();

            _log.InfoFormat("Client {0} connected", client.Client.RemoteEndPoint);
            var reader = new OpcReader(client, _log);
            var token = _cancel.Token;
            Task.Run(async () =>
            {
                try
                {
                    OpcMessage? message;
                    do
                    {
                        message = await reader.ReadMessageAsync(token).ConfigureAwait(false);
                        if (message != null)
                            HandleMessageReceived(message.Value);
                    } while (message != null && !token.IsCancellationRequested);
                }
                catch (Exception err)
                {
                    if(!IsHandled(err))
                        throw;
                }
                finally
                {
                    _log.InfoFormat("Client {0} disconnected", client.Client.RemoteEndPoint);
                    reader.Dispose();
                }

            }, token);
        }

        private bool IsHandled(Exception err)
        {
            var socketErr = err.GetBaseException() as SocketException;
            if(socketErr!=null)
            {
                switch(socketErr.SocketErrorCode){
                    case SocketError.ConnectionReset:
                        //_log.Info("Connection reset");
                        return true;
                    default:
                        _log.WarnFormat("Aborted reader due to {0}: {1}", err.GetType().Name, err.Message);
                        return true;
                }
            }

            var errorMessage = string.Format("Aborted reader due to {0}: {1}", err.GetType().Name, err.Message);
            _log.Error(errorMessage, err);
            return true;
        }

        private void HandleMessageReceived(OpcMessage e)
        {
            // Expose the raw message (debugging / tests)
            OnMessageReceived(e);

            // Also expose the system-level command
            switch (e.Command)
            {
                case OpcCommandType.SetPixels:
                    _log.VerboseFormat("Dispatch SetPixels(byte[{1}]) to channel {0}", e.Channel, e.Data.Length);
                    HandleSetPixels(e.Channel, e.Data);
                    break;

                case OpcCommandType.SystemExclusive:{
                    // Payload starts with a system ID 0x00 0x01 = FadeCandy
                    // It seems to have 2 byte 'command id' for these:
                    // 01 - color correction
                    // 02 - firmware config
                    var systemId = OpcReader.ReadUInt16(e.Data, 0);
                    _log.WarnFormat("System-specific command for {0:x4} not handled", systemId);
                   break;
                }

                default:
                    _log.WarnFormat("Command {0}-{1} unhandled", e.Channel, e.Command);
                    break;
            };
        }

        private void HandleSetPixels(byte channel, byte[] messagePayload)
        {
            // With the OPC protocol, the payload is a series of R/G/B byte triplets
            // so... map to Color type
            var pixelCount = messagePayload.Length / 3; // int rounding intentional
            var pixels = new Color[pixelCount];
            for (int i = 0; i < pixelCount; i++)
            {
                var r = messagePayload[i*3];
                var g = messagePayload[i*3+1];
                var b = messagePayload[i*3+2];
                pixels[i] = Color.FromArgb(r, g, b);
            }

            // And dispatch a delegate that can apply the pixels to any selected renderer
            OnCommandAvailable(channel, r =>
            {
                r.SetPixels(pixels);
                r.Show();       // <- I keep forgetting this one crucial step
            });
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
            var command = new OpcCommand(channel, action);
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
            _cancel.Cancel();
            _listener.Dispose();
        }

        class OpcCommand : ICommand
        {
            int _channel;
            Action<IPixelRenderer> _action;

            public OpcCommand(int channel, Action<IPixelRenderer> action)
            {
                _channel = channel;
                _action = action;
            }

            public int Channel
            {
                get { return _channel; }
            }

            public Action<IPixelRenderer> Execute
            {
                get { return _action; }
            }
        }
    }
}
