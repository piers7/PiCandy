using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy.Server.OPC
{
    /// <summary>
    /// A message in the Open Pixel Control protocol format
    /// </summary>
    /// <remarks>See http://openpixelcontrol.org/ </remarks>
    public struct OpcMessage
    {
        /// <summary>
        /// Target channel. Note the doco says that channel 0 should be used as a broadcast channel
        /// </summary>
        public byte Channel;

        /// <summary>
        /// The type of operation being performed
        /// </summary>
        public OpcCommandType Command;

        /// <summary>
        /// The length of the attached data payload.
        /// </summary>
        public ushort Length;

        /// <summary>
        /// A series of r,g,b bytes representing the data to set.
        /// </summary>
        /// <remarks>
        /// From the site:
        /// The data block contains 8-bit RGB values: three bytes in red, green, blue order for each pixel to set. 
        /// If the data block has length 3*n, then the first n pixels of the specified channel are set. 
        /// All other pixels are unaffected and retain their current colour values. 
        /// If the data length is not a multiple of 3, or there is data for more pixels than are present, the extra data is ignored
        /// </remarks>
        public byte[] Data;

        public override string ToString()
        {
            return string.Format("{0} {1} {2}"
                ,Channel
                ,(byte)Command
                ,Length
                //,string.Join(" ", Data.Select(b => b.ToString("x2")))
                )
                ;
        }

        public string ToString(int numberOfPayloadBytesShown)
        {
            return string.Format("{0} {1} {2} {3}"
                , Channel
                , (byte)Command
                , Length
                , BitConverter.ToString(Data.Take(numberOfPayloadBytesShown).ToArray())
                //.Select(b => b.ToString("x2")))
                )
                ;
        }
    }
}
