using Serilog;
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

		public async Task<MutualFund> GetMutualFundAsync(string symbol, CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var client = new HttpClient { BaseAddress = MarketWatchAddress })
			{
				return await GetMutualFundAsync(client, symbol, "");
			}
		}

		public async Task<IEnumerable<MutualFund>> GetMutualFundsAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var tasks = new List<Task<IEnumerable<MutualFund>>>();
			for (char letter = 'A'; letter <= 'Z'; letter++)
			{
				tasks.Add(GetMutualFundsAsync(letter, cancellationToken));
			}
			return (await Task.WhenAll(tasks)).SelectMany(m => m);
		}

		private async Task<IEnumerable<MutualFund>> GetMutualFundsAsync(char letter, CancellationToken cancellationToken = default(CancellationToken))
		{
			var funds = new List<MutualFund>();
			using (var client = new HttpClient { BaseAddress = MarketWatchAddress })
			{
				var content = await client.GetStringAsync($"tools/mutual-fund/list/{letter}");

				var rows = GetSymbolRows(content);
				foreach (var row in rows)
				{
					var fund = await GetMutualFundAsync(client, row);
					if (fund != null)
						funds.Add(fund);
				}

				return funds;
			}
		}

		private Task<MutualFund> GetMutualFundAsync(HttpClient client, XmlElement row)
		{
			if (row?.ChildNodes?.Count != 2)
				throw new ArgumentException("Row does not appear to be a MutualFund", nameof(row));

			var symbolElement = row.ChildNodes[0] as XmlElement;
			var nameElement = row.ChildNodes[1] as XmlElement;

			var symbol = symbolElement.InnerText;
			var name = nameElement.InnerText;

			return GetMutualFundAsync(client, symbol, name);
		}

		private async Task<MutualFund> GetMutualFundAsync(HttpClient client, string symbol, string name)
		{
			try
			{
				var content = await client.GetStringAsync($"investing/fund/{symbol}");
				var fund = MutualFundScraper.GetMutualFund(symbol, name, content);
				LogMutualFund(fund);
				return fund;
			}
			catch (Exception e)
			{
				Log.Warning(e, "Exception reading mutual fund {Symbol}.", symbol);
				return null;
			}
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

		private static void LogMutualFund(MutualFund fund)
		{
			Log.Information("Mutual fund. {Symbol} {Name} " +
				"{ReturnYTD} {Return5yr} {TotalNetAssets} {Price} {YearLow} {YearHigh} " +
				"{FrontLoad} {DeferredLoad} {MaxRedemptionFee} {TotalExpenseRatio} {Expense12B1} {Turnover} " +
				"{IncomeDividend} {DividendFrequency} {CapitalGain2015} {CapitalGainYTD} " +
				"{RiskAlpha} {RiskBeta} {RiskStdDev} {RiskRSquared} " +
				"{LipperTotalReturn} {LipperConsistentReturn} {LipperPreservation} {LipperTaxEfficiency} {LipperExpense} " +
				"{FundPerformanceYTD} {FundPerformance1yr} {FundPerformance3yr} {FundPerformance5yr} {FundPerformance10yr} " +
				"{CategoryPerformanceYTD} {CategoryPerformance1yr} {CategoryPerformance3yr} {CategoryPerformance5yr} {CategoryPerformance10yr} " +
				"{IndexPerformanceYTD} {IndexPerformance1yr} {IndexPerformance3yr} {IndexPerformance5yr} {IndexPerformance10yr} " +
				"{PercentRankYTD} {PercentRank1yr} {PercentRank3yr} {PercentRank5yr} {PercentRank10yr} " +
				"{QuintileRankYTD} {QuintileRank1yr} {QuintileRank3yr} {QuintileRank5yr} {QuintileRank10yr}",
				
				fund.Symbol,
				fund.Name,

				fund.Overview.ReturnYTD,
				fund.Overview.AverageReturnFiveYear,
				fund.Overview.TotalNetAssets,
				fund.Overview.Price,
				fund.Overview.YearLow,
				fund.Overview.YearHigh,

				fund.Expenses.FrontLoad,
				fund.Expenses.DeferredLoad,
				fund.Expenses.MaxRedemptionFee,
				fund.Expenses.TotalExpenseRatio,
				fund.Expenses.TwelveB1,
				fund.Expenses.Turnover,

				fund.Distributions.IncomeDividend,
				fund.Distributions.DividendFrequency,
				fund.Distributions.CapitalGain2015,
				fund.Distributions.CapitalGainYTD,

				fund.Risk.Alpha,
				fund.Risk.Beta,
				fund.Risk.StandardDeviation,
				fund.Risk.RSquared,

				fund.LipperLeader.TotalReturn,
				fund.LipperLeader.ConsistentReturn,
				fund.LipperLeader.Preservation,
				fund.LipperLeader.TaxEfficiency,
				fund.LipperLeader.Expense,

				fund.FundPerformance.YearToDate,
				fund.FundPerformance.OneYear,
				fund.FundPerformance.ThreeYear,
				fund.FundPerformance.FiveYear,
				fund.FundPerformance.TenYear,

				fund.CategoryPerformance.YearToDate,
				fund.CategoryPerformance.OneYear,
				fund.CategoryPerformance.ThreeYear,
				fund.CategoryPerformance.FiveYear,
				fund.CategoryPerformance.TenYear,

				fund.IndexPerformance.YearToDate,
				fund.IndexPerformance.OneYear,
				fund.IndexPerformance.ThreeYear,
				fund.IndexPerformance.FiveYear,
				fund.IndexPerformance.TenYear,

				fund.PercentRankInCategory.YearToDate,
				fund.PercentRankInCategory.OneYear,
				fund.PercentRankInCategory.ThreeYear,
				fund.PercentRankInCategory.FiveYear,
				fund.PercentRankInCategory.TenYear,

				fund.QuintileRank.YearToDate,
				fund.QuintileRank.OneYear,
				fund.QuintileRank.ThreeYear,
				fund.QuintileRank.FiveYear,
				fund.QuintileRank.TenYear);
		}
	}
}
