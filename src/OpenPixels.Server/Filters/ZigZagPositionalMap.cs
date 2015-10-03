using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.Filters
{
    public class ZigZagPositionalMap : IPositionalMap
    {
        int _cadence;

        public ZigZagPositionalMap(int cadence)
        {
            _cadence = cadence;
        }

        public int GetMappedIndex(int index)
        {
            int row = index / _cadence;
            // for even rows we don't need to do anything
            // nb: this includes 0th row
            if (row % 2 == 0)
                return index;

            // for even rows, we need to invert the X position on the row
            var x = index % _cadence;
            var adustedX = _cadence - x -1;

            return row * _cadence + adustedX;
        }
    }
}
