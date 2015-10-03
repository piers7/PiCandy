using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenPixels.Server.Filters
{
	public class ZigZagPositionalMapTest
	{
		[Fact]
		public void TestGetMappedIndex()
		{
			var filter = new ZigZagPositionalMap(4);
			var inputs = Enumerable.Range(0, 16).ToArray();
			var expecteds = new[]{
				0,1,2,3,
				7,6,5,4,
				8,9,10,11,
				15,14,13,12
			};

			for (int i = 0; i < inputs.Length; i++)
			{
				var input = inputs[i];
				var expected = expecteds[i];
				var actual = filter.GetMappedIndex(input);
				Assert.Equal(expected, actual);
			}
		}
	}
}
