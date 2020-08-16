using CommandLine;
using System;

namespace OsmReader
{
	class Program
	{
		static void Main(string[] args)
		{
			Parser.Default.ParseArguments<CliOptions>(args)
				  .WithParsed(o =>
				  {
					  var worker = new Worker($"Host={o.Host};Username={o.Username};Password={o.Password};Database={o.Database}");

					  worker.ImportData(o.OsmFilename);

					  worker.ExportNetworkFile(o.OsmFilename + ".dat");

					  Console.WriteLine("Process complete.");
				  });
		}
	}
}
