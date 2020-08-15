using System;
using System.Collections.Generic;
using System.IO;
using Npgsql;
using OsmSharp.Streams;

namespace OsmReader
{
	class Program
	{
		static void Main(string[] args)
		{
			string osmFilename = args[0];
			string dbConnString = "";

			var nodes = new List<OsmSharp.Node>();
			var ways = new List<OsmSharp.Way>();

			using (var fileStream = new FileInfo(osmFilename).OpenRead())
			{
				var source = new PBFOsmStreamSource(fileStream);

				// TODO: load notes and ways into DB tables.
				// https://github.com/OsmSharp/core/blob/develop/src/OsmSharp/Node.cs
				// https://github.com/OsmSharp/core/blob/develop/src/OsmSharp/Way.cs

				foreach (var element in source)
				{
					Console.WriteLine(element.GetType().ToString());

					if (element.GetType() == typeof(OsmSharp.Node))
					{
						nodes.Add(element as OsmSharp.Node);

						if (nodes.Count >= 1000)
						{
							BulkInsertNodes(nodes, dbConnString);
							nodes.Clear();
						}
					}
					else if (element.GetType() == typeof(OsmSharp.Way))
					{
						ways.Add(element as OsmSharp.Way);

						if (ways.Count >= 1000)
						{
							BulkInsertWays(ways, dbConnString);
							ways.Clear();
						}
					}
				}
			}
		}

		private static void BulkInsertNodes(List<OsmSharp.Node> nodes, string dbConnString)
		{
			using (var conn = new NpgsqlConnection(dbConnString))
			{
				conn.Open();
			}
		}

		private static void BulkInsertWays(List<OsmSharp.Way> ways, string dbConnString)
		{
			using (var conn = new NpgsqlConnection(""))
			{
				conn.Open();
			}
		}
	}
}
