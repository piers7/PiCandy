using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy
{
    public class Lazy<T,TMeta>
    {
        Lazy<T> _lazy;
        TMeta _meta;

        public Lazy(Lazy<T> lazy, TMeta meta)
        {
            _lazy = lazy;
            _meta = meta;
        }

        public T Value
        {
            get { return _lazy.Value; }
        }

        public bool IsValueCreated
        {
            get { return _lazy.IsValueCreated; }
        }

        public TMeta Meta
        {
            get { return _meta; }
        }
    }
}
