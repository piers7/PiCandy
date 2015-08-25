using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.OPC
{
    public class EndianConverter
    {
        public static EndianConverter LittleEndianConverter = new EndianConverter(!BitConverter.IsLittleEndian);
        public static EndianConverter BigEndianConverter = new EndianConverter(BitConverter.IsLittleEndian);
        
        private bool _flipRequired;

        private EndianConverter(bool flipRequired)
        {
            _flipRequired = flipRequired;
        }

        public UInt16 ToUInt16(byte[] header, int startIndex)
        {
            return ReadBytes(header, startIndex, sizeof(UInt16), BitConverter.ToUInt16);
        }

        public UInt32 ToUInt32(byte[] header, int startIndex)
        {
            return ReadBytes(header, startIndex, sizeof(UInt32), BitConverter.ToUInt32);
        }

        public UInt64 ToUInt64(byte[] header, int startIndex)
        {
            return ReadBytes(header, startIndex, sizeof(UInt64), BitConverter.ToUInt64);
        }

        public Int16 ToInt16(byte[] header, int startIndex)
        {
            return ReadBytes(header, startIndex, sizeof(Int16), BitConverter.ToInt16);
        }

        public Int32 ToInt32(byte[] header, int startIndex)
        {
            return ReadBytes(header, startIndex, sizeof(Int32), BitConverter.ToInt32);
        }

        public Int64 ToInt64(byte[] header, int startIndex)
        {
            return ReadBytes(header, startIndex, sizeof(Int64), BitConverter.ToInt64);
        }

        private T ReadBytes<T>(byte[] header, int startIndex, int length, Func<byte[], int, T> converter)
        {
            var bytes = ReadBytes(header, startIndex, length, _flipRequired);
            return converter(bytes, 0);
        }

        private static byte[] ReadBytes(byte[] buffer, int startIndex, int length, bool flip)
        {
            var output = new byte[length];
            Buffer.BlockCopy(buffer, startIndex, output, 0, length);
            if (flip)
                output = Reverse(output);
            return output;
        }

        private static T[] Reverse<T>(T[] input)
        {
            var output = new T[input.Length];
            for (int i = 0; i < input.Length; i++)
                output[i] = input[input.Length - 1 - i];
            return output;
        }
    }
}
