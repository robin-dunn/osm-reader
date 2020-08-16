using Npgsql;
using System.Collections.Generic;

namespace OsmReader
{
	class DbClient
	{
		private NpgsqlConnection _conn;

		public DbClient(string dbConnString)
		{
			_conn = new NpgsqlConnection(dbConnString);
			_conn.Open();

			using (var cmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS nodes (id bigint null, latitude float8 null, longitude float8 null, location geography(point, 4326) null);", _conn))
			{
				cmd.ExecuteNonQuery();
			}

			using (var cmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS ways (id bigint null);", _conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public void BulkInsertNodes(List<OsmSharp.Node> nodes)
		{
			using (var writer = _conn.BeginBinaryImport("COPY nodes (id, latitude, longitude) FROM STDIN (FORMAT BINARY)"))
			{
				foreach (var node in nodes)
				{
					writer.StartRow();
					writer.WriteNullableLong(node.Id);
					writer.WriteNullableDouble(node.Latitude);
					writer.WriteNullableDouble(node.Longitude);
				}

				writer.Complete();
			}
		}

		public void BulkInsertWays(List<OsmSharp.Way> ways)
		{
			using (var writer = _conn.BeginBinaryImport("COPY nodes (id) FROM STDIN (FORMAT BINARY)"))
			{
				foreach (var way in ways)
				{
					writer.StartRow();
					writer.WriteNullableLong(way.Id);
				}

				writer.Complete();
			}
		}

		public void CreateGeography()
		{
			using (var cmd = new NpgsqlCommand($"update nodes set location = ST_SetSRID(ST_Point(longitude, latitude), 4326)::geography;", _conn))
			{
				cmd.ExecuteNonQuery();
			}

			// TODO: add spatial index on location column.
		}
	}
}
