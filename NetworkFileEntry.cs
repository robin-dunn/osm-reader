
using System.Runtime.InteropServices;

namespace OsmReader
{
	public struct NetworkFileEntry
	{
		public string Name;

		public long Offset;

		public int RecordSize;

		public long RecordCount;
	}
}
