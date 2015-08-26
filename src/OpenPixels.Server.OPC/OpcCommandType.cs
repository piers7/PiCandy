using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.OPC
{
    public enum OpcCommandType : byte
    {
        // as per definitions on http://openpixelcontrol.org/
        SetPixels = 0,

        SystemExclusive = 0xff
    }
}
