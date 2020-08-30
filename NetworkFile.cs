using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OsmReader
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
				Name = "Nodes",
				Offset = 0,
				RecordCount = (uint)nodes.Count
			});

			nf.Entries.Add(new NetworkFileEntry
			{
				Name = "Links",
				Offset = 0,
				RecordCount = (uint)links.Count
			});

			using (var sw = new StreamWriter(outputFilename, append: false))
			{
				nf.WriteHeader(sw);

				nf.Entries.First(e => e.Name == "Nodes").Offset = sw.BaseStream.Position;

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
						catch (Exception ex)
						{
							continue;
						}
					}

					node.LinkCount++;
				}

				nf.Entries.First(e => e.Name == "Links").Offset = sw.BaseStream.Position;

				foreach (var link in links)
				{
					nf.WriteLinkRecord(sw, link);
				}

				sw.BaseStream.Seek(0, SeekOrigin.Begin);
				nf.WriteHeader(sw);
			}
		}

		private void WriteHeader(StreamWriter sw)
		{
			foreach (var entry in Entries)
			{
				sw.Write(entry.NameBytes);
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
