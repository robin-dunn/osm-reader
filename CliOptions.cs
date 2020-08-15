using CommandLine;

namespace OsmReader
{
	public  class CliOptions
	{
		[Option('f', "osm-file", Required = true, HelpText = "OSM (.pbf) file.")]
		public string OsmFilename { get; set; }

		[Option('h', "host", Required = true, HelpText = "Database hostname e.g. 'localhost' or IP address.")]
		public string Host { get; set; }

		[Option('d', "database", Required = true, HelpText = "Database name.")]
		public string Database { get; set; }

		[Option('u', "username", Required = true, HelpText = "Database username e.g. 'postgres'.")]
		public string Username { get; set; }

		[Option('p', "password", Required = true, HelpText = "Database password.")]
		public string Password { get; set; }
	}
}
