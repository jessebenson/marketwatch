using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketWatch.Web.Fundamentals
{
	public sealed class PerformanceRank
	{
		public string YearToDate { get; set; }
		public string OneYear { get; set; }
		public string ThreeYear { get; set; }
		public string FiveYear { get; set; }
		public string TenYear { get; set; }
	}
}
