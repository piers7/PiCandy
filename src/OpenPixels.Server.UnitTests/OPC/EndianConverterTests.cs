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
        [InlineData("0001", 0, 1)]
        [InlineData("0010", 0, 16)]
        [InlineData("0100", 0, 256)]
        [InlineData("1000", 0, 4096)]
        [InlineData("F000", 0, 61440)]
        [InlineData("FFFF", 0, 65535)]
        [InlineData("000100", 0, 0x0001)] // offset 0 - reads first & second pair
        [InlineData("000102", 1, 0x0102)] // offset 1 - reads second & third pair
        public void FromBigEndianUInt16_AsExpected(string bytes, int offset, UInt16 expected)
        {
            var inputBuffer = StringToByteArray(bytes);
            var actual = EndianConverter.BigEndianConverter.ToUInt16(inputBuffer, offset);

            Assert.Equal(expected, actual);
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
