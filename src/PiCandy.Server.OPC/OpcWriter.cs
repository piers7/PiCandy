using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PiCandy.Server.OPC
{
    /// <summary>
    /// Allows writing Open Pixel Control protocol messages to a stream
    /// </summary>
    public class OpcWriter : IDisposable
    {
        private const int DefaultPort = 7890;
        readonly Stream _output;
        Action _dispose;

        public OpcWriter(Stream output, bool ownsStream = false)
        {
            _output = output;
            if(ownsStream)
                _dispose = () => output.Close();
        }

        public static OpcWriter Create(string host, int port = DefaultPort)
        {
            var client = new System.Net.Sockets.TcpClient();
            client.Connect(host, port);
            var stream = client.GetStream();
            return new OpcWriter(stream, true);
        }

        public static OpcWriter Create(IPEndPoint target)
        {
            var client = new System.Net.Sockets.TcpClient();
            client.Connect(target.Address, target.Port);
            var stream = client.GetStream();
            return new OpcWriter(stream, true);
        }

        public static async Task<OpcWriter> CreateAsync(string host, int port = DefaultPort)
        {
            var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(host, port).ConfigureAwait(false);
            var stream = client.GetStream();
            return new OpcWriter(stream, true);
        }

        public static async Task<OpcWriter> CreateAsync(IPEndPoint target)
        {
            var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(target.Address, target.Port).ConfigureAwait(false);
            var stream = client.GetStream();
            return new OpcWriter(stream, true);
        }

        public void Write(byte channel, OpcCommandType command, byte[] data)
        {
            var length = checked((ushort)data.Length);
            Write(new OpcMessage
            {
                Channel = channel,
                Command = command,
                Length = length,
                Data = data,
            });
        }

        public void WriteSetPixels(byte channel, uint[] pixels)
        {
            // convert 'packed integer' into 3 byte RGB
            // essentially, just drop the first byte from each entry in 'data'
            var length = checked((ushort)(pixels.Length * 3));
            var rawData = new byte[length];
            for (int i = 0; i < pixels.Length; i++)
            {
                rawData[i * 3 + 0] = (byte)(pixels[i] >> 16);
                rawData[i * 3 + 1] = (byte)(pixels[i] >> 8);
                rawData[i * 3 + 2] = (byte)(pixels[i]);
            }
            Write(new OpcMessage
            {
                Channel = channel,
                Command = OpcCommandType.SetPixels,
                Length = length,
                Data = rawData,
            });
        }

        public void Write(OpcMessage message)
        {
            _output.Write(message);
            _output.Flush();
        }

        public void Dispose()
        {
            var action = Interlocked.Exchange(ref _dispose, null);
            if (action != null)
                action();
        }
    }
}
