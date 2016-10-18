using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Configuration;
using MarketWatch.Web;
using System.Threading;

namespace MarketWatch
{
	class Program
	{
		static void Main(string[] args)
		{
			var elasticsearchUri = new Uri(ConfigurationManager.AppSettings["ElasticsearchUri"]);
			var elasticsearchUsername = ConfigurationManager.AppSettings["ElasticsearchUsername"];
			var elasticsearchPassword = ConfigurationManager.AppSettings["ElasticsearchPassword"];

			var options = new ElasticsearchSinkOptions(elasticsearchUri)
			{
				ModifyConnectionSettings = c => c.BasicAuthentication(elasticsearchUsername, elasticsearchPassword),
			};

			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.WriteTo.Elasticsearch(options)
				.CreateLogger();

			// Download mutual fund information.
			DownloadAsync().GetAwaiter().GetResult();

			Console.WriteLine("Press enter to exit ...");
			Console.ReadLine();
		}

		private static async Task DownloadAsync()
		{
			var watch = new MarketWatchScraper();
			var funds = (await watch.GetMutualFundsAsync()).ToList();
			Console.WriteLine(funds.Count);
		}
	}
}
