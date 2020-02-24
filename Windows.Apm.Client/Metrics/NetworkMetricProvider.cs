using System.Collections.Generic;
using System.Runtime.InteropServices;
using Elastic.Apm.Api;
using Elastic.Apm.Logging;
using Elastic.Apm.Metrics;
using ProcessMonitoring;

namespace Windows.Apm.Client.Metrics
{
	public class NetworkMetricProvider : IMetricsProvider
	{
		internal const string NetworkOut = "system.network.out.bytes";
		internal const string NetworkIn = "system.network.in.bytes";
		internal const string NetworkName = "system.network.name";

		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly IApmLogger _logger;

		public int ConsecutiveNumberOfFailedReads { get; set; }
		public string DbgName => "custom network traffic";

		public NetworkMetricProvider(IApmLogger logger = null) => _logger = logger;

		public IEnumerable<MetricSample> GetSamples()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				var counter = WmiCounters.GetOneInterface();
				return new List<MetricSample> {
					 new MetricSample(NetworkOut, counter.SentSum),
					 new MetricSample(NetworkIn, counter.ReceivedSum)
					 //new MetricSample(NetworkName, counter.NetworkInterface),
				};
			}
			return null;
		}
	}
}
