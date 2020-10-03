using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Domain.Routing
{
	public class NetworkFile
	{
		public List<NetworkFileEntry> Entries { get; } = new List<NetworkFileEntry>();

		public int HeaderSize { get => Entries.Count * NetworkFileEntry.SizeInBytes; }

		public static void Write(string outputFilename, List<NetworkNode> nodes, List<NetworkLink> links)
		{
			var nf = new NetworkFile();

			links.Sort((x, y) => x.StartNodeId.CompareTo(y.StartNodeId));

			Console.WriteLine($"Writing to file {outputFilename}...");

			Dictionary<long, NetworkNode> nodesDict = new Dictionary<long, NetworkNode>();

			nf.Entries.Add(new NetworkFileEntry
			{
                EntryType = NetworkFileEntryType.Nodes,
				Name = "Nodes",
				Offset = 0,
				RecordCount = (uint)nodes.Count
			});

			nf.Entries.Add(new NetworkFileEntry
			{
                EntryType = NetworkFileEntryType.Links,
				Name = "Links",
				Offset = 0,
				RecordCount = (uint)links.Count
			});

			using (var sw = new StreamWriter(outputFilename, append: false))
			{
				nf.WriteHeader(sw);

				nf.Entries.First(e => e.EntryType == NetworkFileEntryType.Nodes).Offset = sw.BaseStream.Position;

				foreach (var n in nodes) nodesDict.Add(n.Id, n);
				long nodeId = -1;
				NetworkNode node = default(NetworkNode);

				for (int i = 0; i < links.Count; i++)
				{
					var link = links[i];

					if (link.StartNodeId != nodeId)
					{
						if (nodeId > -1)
						{
							nf.WriteNodeRecord(sw, node);
						}

						nodeId = link.StartNodeId;

						try
						{
							node = nodesDict[nodeId];
							node.FirstLinkIndex = i;
						}
						catch (Exception)
						{
							continue;
						}
					}

					node.LinkCount++;
				}

				nf.Entries.First(e => e.EntryType == NetworkFileEntryType.Links).Offset = sw.BaseStream.Position;

				foreach (var link in links)
				{
					nf.WriteLinkRecord(sw, link);
				}

				sw.BaseStream.Seek(0, SeekOrigin.Begin);
				nf.WriteHeader(sw);
			}
		}

        public static void CheckFile(string fileName)
        {
            // Read header

            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                var entry = new NetworkFileEntry
                {
                    EntryType = (NetworkFileEntryType)reader.ReadUInt32(),
                    Offset = reader.ReadInt64(),
                    RecordCount = reader.ReadUInt32(),
                    RecordSize = reader.ReadUInt32()
                };

                Console.WriteLine(entry.ToString());
            }
        }

		private void WriteHeader(StreamWriter sw)
		{
			foreach (var entry in Entries)
			{
				sw.Write((uint)entry.EntryType);
				sw.Write(entry.Offset);
				sw.Write(entry.RecordCount);
				sw.Write(entry.RecordSize);
			}
		}

		private void WriteNodeRecord(StreamWriter sw, NetworkNode node)
		{
			sw.Write(node.Id);
			sw.Write(node.Latitude);
			sw.Write(node.Longitude);
			sw.Write(node.LinkCount);
			sw.Write(node.FirstLinkIndex);
		}

		private void WriteLinkRecord(StreamWriter sw, NetworkLink link)
		{
			sw.Write(link.EndNodeId);
			sw.Write(link.Pedestrian);
		}
	}
}
