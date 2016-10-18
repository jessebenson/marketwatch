using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketWatch.Web.Fundamentals
{
	public sealed class Overview
	{
		public double? ReturnYTD { get; set; }
		public double? AverageReturnFiveYear { get; set; }
		public double? TotalNetAssets { get; set; }

		public double? Price { get; set; }
		public double? YearLow { get; set; }
		public double? YearHigh { get; set; }
	}
}
