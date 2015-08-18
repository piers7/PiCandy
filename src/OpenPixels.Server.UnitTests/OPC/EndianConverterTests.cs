using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenPixels.Server.OPC
{
    public class EndianConverterTests
    {
        [Theory]
        [InlineData("0001", 1)]
        [InlineData("000F", 15)]
        [InlineData("0010", 16)]
        [InlineData("0100", 256)]
        [InlineData("1000", 4096)]
        [InlineData("F000", 61440)]
        [InlineData("FFFF", 65535)]
        public void FromBigEndianUInt16_AsExpected(string bytes, UInt16 expected)
        {
            var inputBuffer = StringToByteArray(bytes);
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
