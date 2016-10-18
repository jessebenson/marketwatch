using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketWatch.Web.Fundamentals;

namespace MarketWatch.Web
{
	public sealed class MutualFund
	{
		public string Symbol { get; set; }
		public string Name { get; set; }

		public Overview Overview { get; set; }
		public Expenses Expenses { get; set; }
		public Distributions Distributions { get; set; }
		public Risk Risk { get; set; }

		public LipperLeader LipperLeader { get; set; }

		public Performance FundPerformance { get; set; }
		public Performance CategoryPerformance { get; set; }
		public Performance IndexPerformance { get; set; }

		public PerformanceRank PercentRankInCategory { get; set; }
		public PerformanceRank QuintileRank { get; set; }
	}
}
