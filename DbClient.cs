using Npgsql;
using OsmSharp;
using OsmSharp.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmReader
{
	class DbClient
	{
		private NpgsqlConnection _conn;

		public DbClient(string dbConnString)
		{
			_conn = new NpgsqlConnection(dbConnString);
			_conn.Open();

			ExecuteNonQuery("set client_encoding = 'UTF8';");
		}
		
		public void CreateTables()
		{
			ExecuteNonQuery("DROP TABLE IF EXISTS nodes;");
			ExecuteNonQuery("DROP TABLE IF EXISTS ways;");
			ExecuteNonQuery("DROP TABLE IF EXISTS links;");

			ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS nodes (
id bigint null,
latitude float8 null,
longitude float8 null, 
location geography(point, 4326) null,
tags text[] null);");
			ExecuteNonQuery("CREATE TABLE IF NOT EXISTS ways (id bigint null, nodes bigint[], tags text[] null);");
			ExecuteNonQuery("CREATE TABLE IF NOT EXISTS links (id bigint null, start_node_id bigint, end_node_id bigint, pedestrian bool);");
		}

		private void ExecuteNonQuery(string sql)
		{
			var sw = Stopwatch.StartNew();

			try
			{
				using (var cmd = new NpgsqlCommand(";" + sql + ";", _conn))
				{
					cmd.CommandTimeout = 60000;
					cmd.ExecuteNonQuery();
				}
			}
			catch (System.Exception ex)
			{
				System.Console.WriteLine($"Query took {sw.Elapsed.TotalSeconds} seconds.");
				throw;
			}
		}

		public void BulkInsertNodes(List<OsmSharp.Node> nodes)
		{
			using (var writer = _conn.BeginBinaryImport("COPY nodes (id, latitude, longitude, tags) FROM STDIN (FORMAT BINARY)"))
			{
				foreach (var node in nodes)
				{
					if (node.Tags == null || !node.Tags.Any(t => t.Key == "highway"))
					{
						continue;
					}
					writer.StartRow();
					writer.WriteNullableLong(node.Id);
					writer.WriteNullableDouble(node.Latitude);
					writer.WriteNullableDouble(node.Longitude);
					WriteTags(writer, node);
				}

				writer.Complete();
			}
		}

		public void BulkInsertWays(List<OsmSharp.Way> ways)
		{
			List<NetworkLink> links = new List<NetworkLink>();

			using (var writer = _conn.BeginBinaryImport("COPY ways (id, nodes, tags) FROM STDIN (FORMAT BINARY)"))
			{
				foreach (var way in ways)
				{
					writer.StartRow();
					writer.WriteNullableLong(way.Id);

					bool isPedestrian = false;

					writer.Write(way.Nodes.ToArray(), NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint);
					WriteTags(writer, way, tag => isPedestrian = tag.Key == "highway" && tag.Value == "pedestrian");

					for (int i = 0; i < way.Nodes.Length - 1; i++)
					{
						links.Add(new NetworkLink
						{
							StartNodeId = way.Nodes[i],
							EndNodeId = way.Nodes[i+1],
							Pedestrian = isPedestrian
						});
					}
				}

				writer.Complete();
			}

			BulkInsertLinks(links);
		}

		private void WriteTags(NpgsqlBinaryImporter writer, OsmGeo item, Action<Tag> processTag = null)
		{
			if (item.Tags != null)
			{
				List<string> tags = new List<string>(item.Tags.Count);
				foreach (var tag in item.Tags)
				{
					tags.Add($"{tag.Key}={tag.Value}");
					processTag?.Invoke(tag);
				}
				writer.Write(tags.ToArray(), NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text);
			}
			else
			{
				writer.Write<string[]>(null, NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text);
			}
		}

		private void BulkInsertLinks(List<NetworkLink> links)
		{
			if (links == null || !links.Any())
			{
				return;
			}

			using (var writer = _conn.BeginBinaryImport("COPY links (start_node_id, end_node_id, pedestrian) FROM STDIN (FORMAT BINARY)"))
			{
				foreach (var link in links)
				{
					writer.StartRow();
					writer.Write(link.StartNodeId);
					writer.Write(link.EndNodeId);
					writer.Write(link.Pedestrian);
				}

				writer.Complete();
			}
		}

		public List<NetworkNode> GetNodes(List<long> nodeIds = null)
		{
			var nodes = new List<NetworkNode>();

			string commandText = nodeIds == null
				? "COPY nodes (id, latitude, longitude) TO STDOUT (FORMAT BINARY)"
				: $"COPY (select id, latitude, longitude from nodes where id in ({string.Join(',', nodeIds)})) TO STDOUT (FORMAT BINARY)";

			using (var reader = _conn.BeginBinaryExport(commandText))
			{
				while (reader.StartRow() > -1)
				{
					var node = default(NetworkNode);
					node.Id = reader.Read<long>();
					node.Latitude = reader.Read<double>();
					node.Longitude = reader.Read<double>();
					nodes.Add(node);
				}
			}

			return nodes;
		}

		public List<NetworkLink> GetLinks()
		{
			var links = new List<NetworkLink>();

			using (var reader = _conn.BeginBinaryExport("COPY links (start_node_id, end_node_id, pedestrian) TO STDOUT (FORMAT BINARY)"))
			{
				while (reader.StartRow() > -1)
				{
					var link = default(NetworkLink);
					link.StartNodeId = reader.Read<long>();
					link.EndNodeId = reader.Read<long>();
					link.Pedestrian = reader.Read<bool>();
					if (link.Pedestrian)
					{
						continue;
					}
					links.Add(link);
				}
			}

			return links;
		}

		public void CreateGeography()
		{
			ExecuteNonQuery($"update nodes set location = ST_SetSRID(ST_Point(longitude, latitude), 4326)::geography;");

			// TODO: add spatial index on location column.
		}
	}
}
