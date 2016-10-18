using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketWatch.Web.Fundamentals
{
	public sealed class Risk
	{
		public double? Alpha { get; set; }
		public double? Beta { get; set; }
		public double? StandardDeviation { get; set; }
		public double? RSquared { get; set; }
	}
}
