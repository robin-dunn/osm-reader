using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OsmSharp.Streams;
using Domain.Routing;

namespace OsmReader
{
	public class Worker
	{
		private readonly DbClient _dbClient;

		public Worker(string dbConnString, string dbSchema)
		{
			_dbClient = new DbClient(dbConnString, dbSchema);
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
			Console.WriteLine("Count links: " + links.Count);

			var nodes = _dbClient.GetNodes();
			Console.WriteLine("Count nodes: " + nodes.Count);

			NetworkFile.Write(outputFilename, nodes, links);
		}

        public void CheckNetworkFile(string fileName)
        {
            NetworkFile.CheckFile(fileName);
        }
	}
}
