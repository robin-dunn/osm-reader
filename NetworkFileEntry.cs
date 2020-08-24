
using System.Runtime.InteropServices;

namespace OsmReader
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct NetworkFileEntry
	{
		public fixed char Name[50];

		public long Offset;

		public int RecordSize;

		public long RecordCount;
	}
}
