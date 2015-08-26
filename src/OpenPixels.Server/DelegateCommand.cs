using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server
{
    public class DelegateCommand : ICommand
    {
        int _channel;
        Action<IPixelRenderer> _execute;

        public DelegateCommand(int channel, Action<IPixelRenderer> execute)
        {
            _channel = channel;
            _execute = execute;
        }

        public int Channel
        {
            get { return _channel; }
        }

        public Action<IPixelRenderer> Execute
        {
            get { return _execute; }
        }
    }
}
