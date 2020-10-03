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
					  var worker = new Worker($"Host={o.Host};Username={o.Username};Password={o.Password};Database={o.Database}", o.Schema);

					  worker.ImportData(o.OsmFilename);

                      string networkFileName = o.OsmFilename + ".dat";

					  worker.ExportNetworkFile(networkFileName);

					  Console.WriteLine("Export complete.");

                      worker.CheckNetworkFile(networkFileName);

                      Console.WriteLine("Process complete.");
                  });
		}
	}
}
