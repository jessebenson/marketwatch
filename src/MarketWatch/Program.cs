using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Configuration;

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

            Console.WriteLine("Press enter to exit ...");
            Console.ReadLine();
        }
    }
}
