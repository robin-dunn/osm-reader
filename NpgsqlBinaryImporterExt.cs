using Npgsql;
using NpgsqlTypes;

namespace OsmReader
{
	public static class NpgsqlBinaryImporterExt
	{
		public static void WriteNullableLong(this NpgsqlBinaryImporter importer, long? val)
		{
			if (val.HasValue)
			{
				importer.Write(val.Value, NpgsqlDbType.Bigint);
			}
			else
			{
				importer.Write<long?>(null, NpgsqlDbType.Bigint);
			}
		}

		public static void WriteNullableDouble(this NpgsqlBinaryImporter importer, double? val)
		{
			if (val.HasValue)
			{
				importer.Write(val.Value, NpgsqlDbType.Double);
			}
			else
			{
				importer.Write<double?>(null, NpgsqlDbType.Double);
			}
		}
	}
}
