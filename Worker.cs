using System;
using System.Collections.Generic;
using System.IO;
using OsmSharp.Streams;

namespace OsmReader
{
	public static class Worker
	{
		public static void Run(string osmFilename, string dbConnString)
		{
			var nodes = new List<OsmSharp.Node>();
			var ways = new List<OsmSharp.Way>();

			var dbClient = new DbClient(dbConnString);

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
							dbClient.BulkInsertNodes(nodes);
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
							dbClient.BulkInsertWays(ways);
							ways.Clear();
							countWaysWritten += batchSize;
							Console.WriteLine("Loaded " + countWaysWritten + " ways.");
						}
					}
				}
			}

			Console.WriteLine("Upload complete.");
		}
	}
}
