using System.Collections.Generic;
using System.Runtime.InteropServices;
using Elastic.Apm.Api;
using Elastic.Apm.Logging;
using Elastic.Apm.Metrics;
using ProcessMonitoring;

namespace AspNetFullFrameworkSampleApp
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

		public NetworkMetricProvider()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				//try
				//{
				//	_processorTimePerfCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
				//	//The perf. counter API returns 0 the for the 1. call (probably because there is no delta in the 1. call) - so we just call it here first
				//	_processorTimePerfCounter.NextValue();
				//}
				//catch (Exception e)
				//{
				//	_logger.Error()
				//		?.LogException(e, "Failed instantiating PerformanceCounter "
				//			+ "- please make sure the current user has permissions to read performance counters. E.g. make sure the current user is member of "
				//			+ "the 'Performance Monitor Users' group");
				//
				//	_processorTimePerfCounter?.Dispose();
				//	_processorTimePerfCounter = null;
				//}

			}
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;
		}

		public IEnumerable<MetricSample> GetSamples()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{

				var counter = WmiCounters.GetOneInterface();
				return new List<MetricSample> {
					 new MetricSample(NetworkOut, counter.SendSum),
					 new MetricSample(NetworkIn, counter.ReceiveSum)
					 //new MetricSample(NetworkName, counter.NetworkInterface),
				};
			}
			return null;
		}
	}
}
