using CommandLine;

namespace OsmReader
{
	class Program
	{
		static void Main(string[] args)
		{
			Parser.Default.ParseArguments<CliOptions>(args)
				  .WithParsed(o =>
				  {
					  Worker.Run(o.OsmFilename, 
						  $"Host={o.Host};Username={o.Username};Password={o.Password};Database={o.Database}");
				  });
		}
	}
}
