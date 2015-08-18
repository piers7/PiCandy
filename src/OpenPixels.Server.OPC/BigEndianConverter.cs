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

        public ushort ToUInt16(byte[] header, int startIndex)
        {
            var bytes = ReadBytes(header, startIndex, 2, _flipRequired);
            return BitConverter.ToUInt16(bytes, 0);
        }

        private static byte[] ReadBytes(byte[] buffer, int startIndex, int length, bool flip)
        {
            var output = new byte[length];
            buffer.CopyTo(output, startIndex);
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
