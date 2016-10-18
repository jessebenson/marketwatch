using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketWatch.Web.Fundamentals
{
	public sealed class Expenses
	{
		public double? FrontLoad { get; set; }
		public double? DeferredLoad { get; set; }
		public double? MaxRedemptionFee { get; set; }
		public double? TotalExpenseRatio { get; set; }
		public double? TwelveB1 { get; set; }
		public double? Turnover { get; set; }
	}
}
