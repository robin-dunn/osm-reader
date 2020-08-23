using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OsmSharp.Streams;

namespace OsmReader
{
	public class Worker
	{
		private readonly DbClient _dbClient;

		public Worker(string dbConnString)
		{
			_dbClient = new DbClient(dbConnString);
		}

		public void ImportData(string osmFilename)
		{
			var nodes = new List<OsmSharp.Node>();
			var ways = new List<OsmSharp.Way>();

			_dbClient.CreateTables();

			long countNodesWritten = 0;
			long countWaysWritten = 0;
			int batchSize = 20000;

			using (var fileStream = new FileInfo(osmFilename).OpenRead())
			{
				var source = new PBFOsmStreamSource(fileStream);

				foreach (var element in source)
				{
					if (element.GetType() == typeof(OsmSharp.Node))
					{
						nodes.Add(element as OsmSharp.Node);

						if (nodes.Count >= batchSize)
						{
							countNodesWritten += _dbClient.BulkInsertNodes(nodes);
							nodes.Clear();
							Console.WriteLine("Loaded " + countNodesWritten + " nodes.");
						}
					}
					else if (element.GetType() == typeof(OsmSharp.Way))
					{
						ways.Add(element as OsmSharp.Way);

						if (ways.Count >= batchSize)
						{
							countWaysWritten += _dbClient.BulkInsertWays(ways);
							ways.Clear();
							Console.WriteLine("Loaded " + countWaysWritten + " ways.");
						}
					}
				}
			}

			_dbClient.CreateGeography();

			Console.WriteLine("Upload complete.");
		}

		public void ExportNetworkFile(string outputFilename)
		{
			var links = _dbClient.GetLinks();
			HashSet<long> nodeIds = new HashSet<long>(links.Count);

			foreach (var link in links)
			{
				nodeIds.Add(link.StartNodeId);
				nodeIds.Add(link.EndNodeId);
			}

			Console.WriteLine("Count links: " + links.Count);
			var nodes = _dbClient.GetNodes();
			Console.WriteLine("Count nodes: " + nodes.Count);

			Console.WriteLine($"Writing to file {outputFilename}...");

			Dictionary<long, NetworkNode> nodesDict = new Dictionary<long, NetworkNode>();

			using (var file = new StreamWriter(outputFilename, append: false))
			{
				file.WriteLine(nodes.Count);

				foreach (var n in nodes) nodesDict.Add(n.Id, n);

				file.WriteLine(links.Count);

				long nodeId = -1;
				NetworkNode node = default(NetworkNode);

				for(int i = 0; i < links.Count; i++)
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
