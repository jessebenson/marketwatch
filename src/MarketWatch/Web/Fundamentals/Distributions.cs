using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketWatch.Web.Fundamentals
{
	public sealed class Distributions
	{
		public double? IncomeDividend { get; set; }
		public string DividendFrequency { get; set; }
		public double? CapitalGain2015 { get; set; }
		public double? CapitalGainYTD { get; set; }
	}
}
