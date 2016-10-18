using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketWatch.Web.Fundamentals
{
	public static class FundamentalsExtensions
	{
		public static double? Parse(this string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;
			if (value.StartsWith("$"))
				return double.Parse(value.Substring(1));
			if (value.EndsWith("%"))
				return double.Parse(value.Substring(0, value.Length - 1));
			if (value.EndsWith("K"))
				return double.Parse(value.Substring(0, value.Length - 1)) * 1000;
			if (value.EndsWith("M"))
				return double.Parse(value.Substring(0, value.Length - 1)) * 1000000;
			if (value.EndsWith("B"))
				return double.Parse(value.Substring(0, value.Length - 1)) * 1000000000;

			double number;
			if (!double.TryParse(value, out number))
				return null;
			return number;
		}
	}
}
