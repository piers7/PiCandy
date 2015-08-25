﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.OPC
{
    public struct OpcMessage
    {
        public byte Channel;
        public byte Command;
        public ushort Length;
        public byte[] Data;

        public override string ToString()
        {
            return string.Format("{0} {1} {2}"
                ,Channel
                ,Command
                ,Length
                //,string.Join(" ", Data.Select(b => b.ToString("x2")))
                )
                ;
        }

        public string ToString(int numberOfPayloadBytesShown)
        {
            return string.Format("{0} {1} {2} {3}"
                , Channel
                , Command
                , Length
                , BitConverter.ToString(Data.Take(numberOfPayloadBytesShown).ToArray())
                //.Select(b => b.ToString("x2")))
                )
                ;
        }
    }
}
