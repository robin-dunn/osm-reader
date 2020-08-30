using System.Collections.Generic;
using System.Text;

namespace OsmReader
{
	public class NetworkFileEntry
	{
		public const int MaxNameCharLength = 100;

		public string Name;

		public long Offset;

		public uint RecordCount;

		public int RecordSize;

		public byte[] NameBytes
		{
			get
			{
				List<byte> bytes = new List<byte>();
				bytes.AddRange(Encoding.UTF8.GetBytes(Name));

				while (bytes.Count < MaxNameCharLength)
				{
					bytes.Add(0x00);
				}

				return bytes.ToArray();
			}
		}

		public static int SizeInBytes => MaxNameCharLength * 4 + sizeof(long) + sizeof(long) + sizeof(int); 
	}
}
