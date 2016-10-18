using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketWatch.Web.Fundamentals
{
	public sealed class PerformanceRank
	{
		public double? YearToDate { get; set; }
		public double? OneYear { get; set; }
		public double? ThreeYear { get; set; }
		public double? FiveYear { get; set; }
		public double? TenYear { get; set; }
	}
}
