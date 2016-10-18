using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketWatch.Web.Fundamentals
{
	public sealed class LipperLeader
	{
		public double? TotalReturn { get; set; }
		public double? ConsistentReturn { get; set; }
		public double? Preservation { get; set; }
		public double? TaxEfficiency { get; set; }
		public double? Expense { get; set; }
	}
}
