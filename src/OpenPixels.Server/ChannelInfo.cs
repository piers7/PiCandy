using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server
{
    public class ChannelInfo
    {
        public ChannelInfo(IDictionary<string, object> metadata)
        {
            object channelRaw;
            if(metadata.TryGetValue("Channel", out channelRaw))
                Channel = (int)channelRaw;
        }

        public int Channel { get; set; }
    }
}
