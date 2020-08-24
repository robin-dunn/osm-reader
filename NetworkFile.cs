using System;
using System.Collections.Generic;
using System.IO;

namespace OsmReader
{
	public class NetworkFile
	{
		List<NetworkFileEntry> entries = new List<NetworkFileEntry>();

		public static void Write(string outputFilename, List<NetworkNode> nodes, List<NetworkLink> links)
		{
			Console.WriteLine($"Writing to file {outputFilename}...");

			Dictionary<long, NetworkNode> nodesDict = new Dictionary<long, NetworkNode>();

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
							file.WriteLine($"{node.Id} {node.Latitude} {node.Longitude} {node.LinkCount} {node.FirstLinkIndex}");
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
	}
}
