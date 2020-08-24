using System;
using System.Collections.Generic;
using System.IO;

namespace OsmReader
{
	public class NetworkFile
	{
		public List<NetworkFileEntry> Entries { get; } = new List<NetworkFileEntry>();

		public static void Write(string outputFilename, List<NetworkNode> nodes, List<NetworkLink> links)
		{
			var nf = new NetworkFile();

			Console.WriteLine($"Writing to file {outputFilename}...");

			Dictionary<long, NetworkNode> nodesDict = new Dictionary<long, NetworkNode>();

			nf.Entries.Add(new NetworkFileEntry
			{
				Name = "Nodes",
				Offset = 0,
				RecordCount = nodes.Count
			});

			nf.Entries.Add(new NetworkFileEntry
			{
				Name = "Links",
				Offset = 0,
				RecordCount = links.Count
			});

			using (var file = new StreamWriter(outputFilename, append: false))
			{
				file.WriteLine(nodes.Count);

				foreach (var n in nodes) nodesDict.Add(n.Id, n);

				file.WriteLine(links.Count);

				long nodeId = -1;
				NetworkNode node = default(NetworkNode);

				for (int i = 0; i < links.Count; i++)
				{
					var link = links[i];

					if (nodeId != link.StartNodeId)
					{
						if (nodeId > -1)
						{
							nf.WriteNodeRecord(node);
							// file.WriteLine($"{node.Id} {node.Latitude} {node.Longitude} {node.LinkCount} {node.FirstLinkIndex}");
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

					// file.WriteLine($"{link.Id} {link.StartNodeId} {link.EndNodeId}");
				}
			}
		}

		private void WriteNodeRecord(NetworkNode node)
		{

		}

		private void WriteLinkRecord(NetworkLink link)
		{

		}
	}
}
