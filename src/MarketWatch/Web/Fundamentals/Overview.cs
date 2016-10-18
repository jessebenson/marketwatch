using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketWatch.Web.Fundamentals
{
	public sealed class Overview
	{
		public string ReturnYTD { get; set; }
		public string AverageReturnFiveYear { get; set; }
		public string TotalNetAssets { get; set; }

		public string Price { get; set; }
		public string YearLow { get; set; }
		public string YearHigh { get; set; }
	}
}
