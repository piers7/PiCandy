using OpenPixels.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Renderer.RpiWs2812
{
    public class RpiWs2812Channel : IPixelChannel, IDisposable
    {
        private int _channel;
        private RpiWs281xClient _client;

        public RpiWs2812Channel(int channel, RpiWs281xSetupInfo setupInfo)
        {
            _channel = channel;
            _client = RpiWs281xClient.Create(setupInfo);
        }

        public int Channel { get { return _channel; } }
        public IPixelRenderer Renderer { get { return _client; } }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
