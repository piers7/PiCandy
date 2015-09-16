using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.OPC
{
    public static class OpcStreamEx
    {
        /// <summary>
        /// Writes an <see cref="OpcMessage"/> to the stream provided.
        /// Note: payload data is truncated, if longer than message.Length
        /// </summary>
        public static void Write(this Stream stream, OpcMessage message)
        {
            Contract.Requires(message.Length <= message.Data.Length);

            stream.WriteByte(message.Channel);
            stream.WriteByte((byte)message.Command);
            stream.WriteByte((byte)(message.Length >> 8));
            stream.WriteByte((byte)(message.Length));
            stream.Write(message.Data, 0, message.Length);
        }

        /// <summary>
        /// Writes an <see cref="OpcMessage"/> to the stream provided.
        /// Note: payload data is truncated, if longer than message.Length
        /// </summary>
        public static Task WriteAsync(this Stream stream, OpcMessage message)
        {
            Contract.Requires(message.Length <= message.Data.Length);

            var sent = new byte[4 + message.Length];
            sent[0] = message.Channel;
            sent[1] = (byte)message.Command;
            sent[2] = (byte)(message.Length >> 8);
            sent[3] = (byte)(message.Length);
            Array.Copy(message.Data, 0, sent, 4, message.Length);

            return stream.WriteAsync(sent, 0, sent.Length);
        }
    }
}
