using System.Collections.Generic;
using System.IO;

namespace Windows.Metrics.Ingest
{
	public static class Extensions
	{
		internal static IEnumerable<string> ReadLines(this string s)
		{
			string line;
			using (var sr = new StringReader(s))
				while ((line = sr.ReadLine()) != null)
					yield return line;
		}
	}
}
