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
            Channel = (int)metadata["Channel"];
        }

        public int Channel { get; set; }
    }
}
