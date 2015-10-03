using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.Filters
{
    public interface IPositionalMap
    {
        int GetMappedIndex(int index);
    }
}
