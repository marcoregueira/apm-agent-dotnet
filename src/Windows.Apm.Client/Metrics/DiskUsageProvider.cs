using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Elastic.Apm.Api;
using Elastic.Apm.Logging;
using Elastic.Apm.Metrics;
using Elastic.Apm.Metrics.MetricsProvider;

namespace Windows.Apm.Client.Metrics
{
	internal class DiskUsageProvider : IMetricsProvider, IDisposable
	{

		public string _MainDrive = null;

		internal const string DiskQueueTotalLength = "system.disk.queue.total";
		internal const string DiskSpace = "system.disk.space.total";
		internal const string DiskDrive = "system.disk.space.drive";

		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly IApmLogger _logger;
		private readonly PerformanceCounter _queueLengthPerformanceCounter;
		private readonly PerformanceCounter _diskFreeSpaceCounter;
		private readonly StreamReader _procStatStreamReader;

		public DiskUsageProvider(IApmLogger logger = null, string drive = null)
		{
			_logger = logger?.Scoped(nameof(DiskUsageProvider));

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				try
				{
					_MainDrive = (drive ?? "_Total").Trim();
					if (_MainDrive != "_Total")
					{
						_MainDrive = _MainDrive.ToUpper()[0].ToString();
					}

					if (_MainDrive.Length == 1)
						_MainDrive += ":";

					// \PhysicalDisk(*)\Current Disk Queue Length
					// \LogicalDisk(_Total)\% Free Space
					_queueLengthPerformanceCounter = new PerformanceCounter("PhysicalDisk", "Current Disk Queue Length", "_Total");
					_diskFreeSpaceCounter = new PerformanceCounter("LogicalDisk", "% Free Space", _MainDrive ?? "C:");
					//The perf. counter API returns 0 the for the 1. call (probably because there is no delta in the 1. call) - so we just call it here first
					_queueLengthPerformanceCounter.NextValue();
				}
				catch (Exception e)
				{
					_logger?.Error()
						?.LogException(e, "Failed instantiating PerformanceCounter "
							+ "- please make sure the current user has permissions to read performance counters. E.g. make sure the current user is member of "
							+ "the 'Performance Monitor Users' group");

					_queueLengthPerformanceCounter?.Dispose();
					_queueLengthPerformanceCounter = null;
				}

			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;


		}

		internal DiskUsageProvider(IApmLogger logger, StreamReader procStatStreamReader)
			=> (_logger, _procStatStreamReader) = (_logger?.Scoped(nameof(SystemTotalCpuProvider)), procStatStreamReader);


		public int ConsecutiveNumberOfFailedReads { get; set; }
		public string DbgName => "total disk space";

		public bool IsMetricAlreadyCaptured => false;

		private StreamReader GetProcStatAsStream()
			=> _procStatStreamReader ?? (File.Exists("/proc/stat") ? new StreamReader("/proc/stat") : null);

		public IEnumerable<MetricSample> GetSamples()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				var driveNumber = 0;
				if (_MainDrive != "_Total" && !string.IsNullOrWhiteSpace(_MainDrive))
				{
					driveNumber = 1 + 'C' - _MainDrive[0];
				}

				if (_queueLengthPerformanceCounter == null) return null;
				var valQueue = _queueLengthPerformanceCounter.NextValue();
				var valSpace = _diskFreeSpaceCounter.NextValue();
				return new List<MetricSample> {
					new MetricSample(DiskQueueTotalLength, (double)valQueue),
					new MetricSample(DiskSpace, 100 - (double)valSpace),
					new MetricSample(DiskDrive, driveNumber ),
				};
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
			}

			return null;
		}

		public void Dispose()
		{
			_procStatStreamReader?.Dispose();
			_queueLengthPerformanceCounter?.Dispose();
		}
	}
}
