using System.Collections.Generic;
using System.Text;

namespace Domain.Routing
{
	public class NetworkFileEntry
	{
        public NetworkFileEntryType EntryType;

		public const int MaxNameCharLength = 100;

		public string Name;

		public long Offset;

		public uint RecordCount;

		public uint RecordSize;

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

		public static int SizeInBytes => sizeof(uint) + sizeof(long) + sizeof(uint) + sizeof(uint);

        public override string ToString()
        {
            return $"{nameof(EntryType)}={EntryType.ToString()}; {nameof(Offset)}={Offset}; {nameof(RecordCount)}={RecordCount}; {nameof(RecordSize)}={RecordSize};";
        }
    }
}
