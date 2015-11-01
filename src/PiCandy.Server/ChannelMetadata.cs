using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy.Server
{
    public class ChannelMetadata
    {
        public ChannelMetadata(IDictionary<string, object> metadata)
        {
            object raw;
            if (metadata.TryGetValue("Channel", out raw))
                Channel = (int)raw;
            if (metadata.TryGetValue("Map", out raw))
                Map = (string)raw;
        }

        public int Channel { get; set; }
        public string Map { get; set; }
    }
}
