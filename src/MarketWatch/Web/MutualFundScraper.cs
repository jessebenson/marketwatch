using MarketWatch.Web.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MarketWatch.Web
{
	public sealed class MutualFundScraper
	{
		public static MutualFund GetMutualFund(string symbol, string name, string htmlContent)
		{
			var overviewSection = GetOverviewSection(htmlContent);
			var blockSections = GetDivSections(htmlContent, "block heavytop").ToList();
			var lipperSection = GetLipperSection(htmlContent);
			var performanceSection = GetPerformanceSection(htmlContent);

			return new MutualFund
			{
				Symbol = symbol,
				Name = name,

				Overview = GetOverview(overviewSection),
				Expenses = GetExpenses(blockSections),
				Distributions = GetDistributions(blockSections),
				Risk = GetRisk(blockSections),

				LipperLeader = GetLipperLeader(lipperSection),

				FundPerformance = GetPerformance(performanceSection, "Fund return"),
				CategoryPerformance = GetPerformance(performanceSection, "Category"),
				IndexPerformance = GetPerformance(performanceSection, "Index"),

				PercentRankInCategory = GetPerformanceRank(performanceSection, "% rank in category"),
				QuintileRank = GetPerformanceRank(performanceSection, "Quintile rank"),
			};
		}

		private static Overview GetOverview(XmlElement section)
		{
			if (section == null)
				return new Overview();

			return new Overview
			{
				ReturnYTD = GetOverviewColumn(section, "YTD return"),
				AverageReturnFiveYear = GetOverviewColumn(section, "5 yr avg return"),
				TotalNetAssets = GetOverviewColumn(section, "Total net assets"),
				Price = GetOverviewPrice(section),
				YearLow = GetOverviewHighLow(section, "52 week low"),
				YearHigh = GetOverviewHighLow(section, "52 week high"),
			};
		}

		private static Expenses GetExpenses(List<XmlElement> sections)
		{
			var section = GetBlockSection(sections, "Fees & Expenses");
			if (section == null)
				return new Expenses();

			return new Expenses
			{
				FrontLoad = GetBlockSectionColumn(section, "Front load"),
				DeferredLoad = GetBlockSectionColumn(section, "Deferred load"),
				MaxRedemptionFee = GetBlockSectionColumn(section, "Max. redemption fee"),
				TotalExpenseRatio = GetBlockSectionColumn(section, "Total expense ratio"),
				TwelveB1 = GetBlockSectionColumn(section, "12 b-1"),
				Turnover = GetBlockSectionColumn(section, "Turnover"),
			};
		}

		private static Distributions GetDistributions(List<XmlElement> sections)
		{
			var section = GetBlockSection(sections, "Distributions");
			if (section == null)
				return new Distributions();

			return new Distributions
			{
				IncomeDividend = GetBlockSectionColumn(section, "Income dividend"),
				DividentFrequency = GetBlockSectionColumn(section, "Dividend freq."),
				CapitalGain2015 = GetBlockSectionColumn(section, "Capital gain (2015)"),
				CapitalGainYTD = GetBlockSectionColumn(section, "Capital gain (YTD)"),
			};
		}

		private static Risk GetRisk(List<XmlElement> sections)
		{
			var section = GetBlockSection(sections, "Risk Measures");
			if (section == null)
				return new Risk();

			return new Risk
			{
				Alpha = GetBlockSectionColumn(section, "Alpha"),
				Beta = GetBlockSectionColumn(section, "Beta"),
				StandardDeviation = GetBlockSectionColumn(section, "Standard deviation"),
				RSquared = GetBlockSectionColumn(section, "R. squared"),
			};
		}

		public static LipperLeader GetLipperLeader(XmlElement section)
		{
			if (section == null)
				return new LipperLeader();

			return new LipperLeader
			{
				TotalReturn = GetLipperLeaderValue(section, "Total Return"),
				ConsistentReturn = GetLipperLeaderValue(section, "Consistent Return"),
				Preservation = GetLipperLeaderValue(section, "Preservation"),
				TaxEfficiency = GetLipperLeaderValue(section, "Tax Efficiency"),
				Expense = GetLipperLeaderValue(section, "Expense"),
			};
		}

		private static Performance GetPerformance(XmlElement section, string name)
		{
			if (section == null)
				return new Performance();

			var strings = GetPerformanceStrings(section, name);
			if (strings?.Length != 5)
				return new Performance();

			return new Performance
			{
				YearToDate = strings[0],
				OneYear = strings[1],
				ThreeYear = strings[2],
				FiveYear = strings[3],
				TenYear = strings[4],
			};
		}

		private static PerformanceRank GetPerformanceRank(XmlElement section, string name)
		{
			if (section == null)
				return new PerformanceRank();

			var strings = GetPerformanceStrings(section, name);
			if (strings?.Length != 5)
				return new PerformanceRank();

			return new PerformanceRank
			{
				YearToDate = strings[0],
				OneYear = strings[1],
				ThreeYear = strings[2],
				FiveYear = strings[3],
				TenYear = strings[4],
			};
		}

		private static XmlElement GetOverviewSection(string content)
		{
			var section = GetDivSections(content, "quotedisplay twowidequote").FirstOrDefault();
			return section?.FirstChild as XmlElement;
		}

		private static string GetOverviewPrice(XmlElement section)
		{
			foreach (XmlElement row in section.SelectNodes("//div"))
			{
				if (row.GetAttribute("class") != "lastprice")
					continue;

				var children = row.FirstChild?.ChildNodes;
				if (children?.Count != 2)
					continue;

				var priceChild = children[1] as XmlElement;
				if (priceChild.GetAttribute("class") != "data bgLast")
					continue;

				return priceChild.InnerText.Trim();
			}

			return null;
		}

		private static string GetOverviewColumn(XmlElement section, string name)
		{
			foreach (XmlElement row in section.SelectNodes("//div"))
			{
				var children = row.ChildNodes;
				if (children.Count != 2)
					continue;

				XmlElement column = children[0] as XmlElement;
				XmlElement data = children[1] as XmlElement;

				if (column.GetAttribute("class") != "vertelement column")
					continue;
				if (!data.GetAttribute("class").Contains("lastcolumn data"))
					continue;

				if (column.InnerText == name)
					return data.InnerText.Trim();
			}

			return null;
		}

		private static string GetOverviewHighLow(XmlElement section, string name)
		{
			foreach (XmlElement row in section.SelectNodes("//div"))
			{
				var children = row.ChildNodes;
				if (children.Count != 5)
					continue;

				XmlElement lowColumn = children[0] as XmlElement;
				XmlElement highColumn = children[1] as XmlElement;
				XmlElement lowData = children[2] as XmlElement;
				XmlElement highData = children[3] as XmlElement;

				if (lowColumn.GetAttribute("class") != "column")
					continue;
				if (highColumn.GetAttribute("class") != "lastcolumn")
					continue;
				if (lowData.GetAttribute("class") != "column data")
					continue;
				if (highData.GetAttribute("class") != "data lastcolumn")
					continue;

				if (lowColumn.InnerText == name)
					return lowData.InnerText.Trim();
				if (highColumn.InnerText == name)
					return highData.InnerText.Trim();
			}

			return null;
		}

		private static XmlElement GetBlockSection(List<XmlElement> sections, string name)
		{
			foreach (var section in sections)
			{
				var header = section.SelectSingleNode("//h2") as XmlElement;
				if (header?.InnerText == name)
					return section;
			}

			return null;
		}

		private static string GetBlockSectionColumn(XmlElement section, string name)
		{
			foreach (XmlElement row in section.SelectNodes("//div"))
			{
				var children = row.ChildNodes;
				if (children.Count != 2)
					continue;

				XmlElement column = children[0] as XmlElement;
				XmlElement data = children[1] as XmlElement;

				if (column.GetAttribute("class") != "column")
					continue;
				if (data.GetAttribute("class") != "data lastcolumn")
					continue;

				if (column.InnerText == name)
					return data.InnerText.Trim();
			}

			return null;
		}

		private static XmlElement GetLipperSection(string content)
		{
			return GetDivSections(content, "lipperleader").FirstOrDefault();
		}

		private static string GetLipperLeaderValue(XmlElement section, string name)
		{
			foreach (XmlElement img in section.SelectNodes("//img"))
			{
				var altText = img.GetAttribute("alt");
				if (altText.Contains(name) && altText.Length > name.Length + 1)
					return altText.Substring(name.Length + 1).Trim();
			}

			return null;
		}

		private static XmlElement GetPerformanceSection(string content)
		{
			var sections = GetDivSections(content, "block");
			foreach (XmlElement section in sections)
			{
				var header = section.SelectSingleNode("//h2") as XmlElement;
				if (header?.InnerText == "Lipper Ranking & Performance")
					return section;
			}

			return null;
		}

		private static string[] GetPerformanceStrings(XmlElement section, string column)
		{
			XmlElement table = section.SelectSingleNode("//tbody") as XmlElement;
			if (table?.ChildNodes?.Count != 6)
				return null;

			XmlElement header = table.ChildNodes[0] as XmlElement;
			XmlElement ytd = table.ChildNodes[1] as XmlElement;
			XmlElement oneYr = table.ChildNodes[2] as XmlElement;
			XmlElement threeYr = table.ChildNodes[3] as XmlElement;
			XmlElement fiveYr = table.ChildNodes[4] as XmlElement;
			XmlElement tenYr = table.ChildNodes[5] as XmlElement;

			if (header.ChildNodes?.Count != 6)
				throw new InvalidOperationException("Lipper Ranking & Performance format has changed.");
			if (ytd.ChildNodes?.Count != 6 || ytd.ChildNodes[0].InnerText != "YTD")
				throw new InvalidOperationException("Lipper Ranking & Performance format has changed.");
			if (oneYr.ChildNodes?.Count != 6 || oneYr.ChildNodes[0].InnerText != "1yr")
				throw new InvalidOperationException("Lipper Ranking & Performance format has changed.");
			if (threeYr.ChildNodes?.Count != 6 || threeYr.ChildNodes[0].InnerText != "3yr")
				throw new InvalidOperationException("Lipper Ranking & Performance format has changed.");
			if (fiveYr.ChildNodes?.Count != 6 || fiveYr.ChildNodes[0].InnerText != "5yr")
				throw new InvalidOperationException("Lipper Ranking & Performance format has changed.");
			if (tenYr.ChildNodes?.Count != 6 || tenYr.ChildNodes[0].InnerText != "10yr")
				throw new InvalidOperationException("Lipper Ranking & Performance format has changed.");

			int index = 0;
			for (index = 0; index < header.ChildNodes.Count; index++)
			{
				if (header.ChildNodes[index].InnerText.Contains(column))
					break;
			}

			if (index >= header.ChildNodes.Count)
				throw new InvalidOperationException("Lipper Ranking & Performance format has changed.");

			return new[]
			{
				ytd.ChildNodes[index].InnerText.Trim(),
				oneYr.ChildNodes[index].InnerText.Trim(),
				threeYr.ChildNodes[index].InnerText.Trim(),
				fiveYr.ChildNodes[index].InnerText.Trim(),
				tenYr.ChildNodes[index].InnerText.Trim(),
			};
		}

		private static IEnumerable<XmlElement> GetDivSections(string content, string className)
		{
			var divTag = "<div";
			var startTag = $"<div class=\"{className}\">";
			var endTag = "</div>";

			int searchIndex = 0;
			while (true)
			{
				int startIndex = content.IndexOf(startTag, searchIndex);
				if (startIndex < 0)
					yield break;

				int endIndex = content.IndexOf(endTag, startIndex);
				int divIndex = startIndex;
				while ((divIndex = content.IndexOf(divTag, divIndex + 1, endIndex - divIndex)) > 0)
				{
					endIndex = content.IndexOf(endTag, endIndex + 1);
				}

				int count = (endIndex + endTag.Length + 1) - startIndex;
				var section = content.Substring(startIndex, count);
				searchIndex = startIndex + count;

				var xml = new XmlDocument();
				xml.LoadXml(section.Replace("&", "&amp;").Replace("'", "&apos;"));
				yield return xml.SelectSingleNode("//div") as XmlElement;
			}
		}
	}
}
