using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.OPC
{
    internal static class StreamEx
    {
        /// <summary>
        /// Fills a buffer from a stream, using a series of continuations
        /// if partial reads are received. This takes care of network fragmentation issues.
        /// </summary>
        public static async Task<bool> TryReadAsync(this Stream stream, byte[] buffer)
        {
            return await TryReadAsync(stream, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Fills a portion of a buffer from a stream, using a series of continuations
        /// if partial reads are received. This takes care of network fragmentation issues.
        /// </summary>
        public static async Task<bool> TryReadAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            Contract.Requires(stream != null);
            Contract.Requires(buffer != null);
            Contract.Requires(offset >= 0);
            Contract.Requires(count >= 0);
            Contract.Requires(buffer.Length >= count);

            var read = 0;
            while (read < count)
            {
                var remaining = count - read;
                var received = await stream.ReadAsync(buffer, offset + read, remaining)
                                            .ConfigureAwait(false)
                                            ;

                if (received == 0)
                    // indicates stream has ended (eg socket has been closed)
                    return false;

                read += received;
            }
            return true;
        }
    }
}
