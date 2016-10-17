using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace MarketWatch.Web
{
    public sealed class MarketWatchScraper
    {
        private static readonly Uri MarketWatchAddress = new Uri("http://www.marketwatch.com");
        private static readonly Regex SymbolRegex = new Regex(@"<td class=");

        public async Task<IEnumerable<MutualFund>> GetMutualFundsAsync(CancellationToken cancellationToken)
        {
            var tasks = new List<Task<IEnumerable<MutualFund>>>();
            for (char letter = 'A'; letter <= 'Z'; letter++)
            {
                tasks.Add(Task.Run(() => GetMutualFundsAsync(letter, cancellationToken)));
            }
            return (await Task.WhenAll(tasks)).SelectMany(m => m);
        }

        private async Task<IEnumerable<MutualFund>> GetMutualFundsAsync(char letter, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient { BaseAddress = MarketWatchAddress })
            {
                var contents = await client.GetStringAsync($"tools/mutual-fund/list/{letter}");

                var symbols = GetSymbolRows(contents);
                return symbols.Select(GetMutualFundFromRow);
            }
        }

        private MutualFund GetMutualFundFromRow(XmlElement row)
        {
            if (row?.ChildNodes?.Count != 2)
                throw new ArgumentException("Row does not appear to be a MutualFund", nameof(row));

            var symbol = row.ChildNodes[0] as XmlElement;
            var name = row.ChildNodes[1] as XmlElement;

            return new MutualFund()
            {
                Symbol = symbol.InnerText,
                Name = name.InnerText,
            };
        }

        private IEnumerable<XmlElement> GetSymbolRows(string contents)
        {
            var table = GetQuoteListTable(contents);
            return GetSymbolRows(table);   
        }

        private IEnumerable<XmlElement> GetSymbolRows(XmlElement table)
        {
            foreach (XmlElement row in table.SelectNodes("//tr"))
            {
                if (IsSymbolRow(row))
                    yield return row;
            }
        }

        private bool IsSymbolRow(XmlElement row)
        {
            if (row?.ChildNodes?.Count != 2)
                return false;

            var symbol = row.ChildNodes[0] as XmlElement;
            var name = row.ChildNodes[1] as XmlElement;

            if (symbol?.GetAttribute("class") != "quotelist-symb")
                return false;
            if (name?.GetAttribute("class") != "quotelist-name")
                return false;
            if (symbol.Name == "th")
                return false;

            var sym = symbol.InnerText;
            var nme = name.InnerText;
            return true;
        }

        private XmlElement GetQuoteListTable(string contents)
        {
            var xml = new XmlDocument();
            xml.LoadXml(GetQuoteListTableString(contents));
            return xml.SelectSingleNode("//table") as XmlElement;
        }

        private string GetQuoteListTableString(string contents)
        {
            var start = new Regex("<table.*class=\"quotelist\">");
            var end = "</table>";

            var m = start.Match(contents);
            if (!m.Success)
                return string.Empty;

            var startIndex = m.Index;
            int endIndex = contents.IndexOf(end, startIndex) + end.Length + 1;
            return contents.Substring(startIndex, endIndex - startIndex);
        }
    }
}
