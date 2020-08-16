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
							_dbClient.BulkInsertNodes(nodes);
							nodes.Clear();
							countNodesWritten += batchSize;
							Console.WriteLine("Loaded " + countNodesWritten + " nodes.");
						}
					}
					else if (element.GetType() == typeof(OsmSharp.Way))
					{
						ways.Add(element as OsmSharp.Way);

						if (ways.Count >= batchSize)
						{
							_dbClient.BulkInsertWays(ways);
							ways.Clear();
							countWaysWritten += batchSize;
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
			var nodes = _dbClient.GetNodes(nodeIds.ToList());
			Console.WriteLine("Count nodes: " + nodes.Count);

			Console.WriteLine($"Writing to file {outputFilename}...");

			using (var file = new StreamWriter(outputFilename, append: false))
			{
				file.WriteLine(nodes.Count);

				foreach (var node in nodes)
					file.WriteLine($"{node.Id} {node.Latitude} {node.Longitude}");

				file.WriteLine(links.Count);

				foreach (var link in links)
					file.WriteLine($"{link.Id} {link.StartNodeId} {link.EndNodeId}");
			}
		}
	}
}
