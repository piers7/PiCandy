using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server
{
	public class PixelChannel : IPixelChannel
	{
		private int _channel;
		private IPixelRenderer _renderer;

		public PixelChannel(int channel, IPixelRenderer renderer)
		{
			_channel = channel;
			_renderer = renderer;
		}

		public int Channel { get { return _channel; } }
		public IPixelRenderer Renderer { get { return _renderer; } }
	}
}
