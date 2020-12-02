﻿using System;
using System.IO;
using System.Linq;
using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.AspNetFullFramework;
using Elastic.Apm.Config;
using Elastic.Apm.Helpers;
using Elastic.Apm.Logging;
using Elastic.Apm.Metrics;
using Elastic.Apm.Report;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using Windows.Apm.Client.Metrics;
using Windows.Apm.Client.Nlog;

namespace Windows.Apm.Client
{
	public class FormsApmConfigurer
	{

		public static void UseApm(string diskDrive = null, bool enableMoniker = false, bool skipNLogTarget = false)
		{
			var configurationReader = new LocalConfigurationReader();

			if (enableMoniker)
			{
				ConfigurationMoniker.EnableMoniker(configurationReader, true);
			}

			var logger = AgentDependencies.Logger ?? ConsoleLogger.LoggerOrDefault(Elastic.Apm.Logging.LogLevel.Error);
			var service = Service.GetDefaultService(configurationReader, logger);
			var systemInfoHelper = new SystemInfoHelper(logger);
			var system = systemInfoHelper.ParseSystemInfo();

			var configStore = new ConfigStore(new ConfigSnapshotFromReader(configurationReader, "local"), logger);
			var sender = new LocalPayloadSenderV2(logger, configStore.CurrentSnapshot, service, system);
			var collector = new MetricsCollector(logger, sender, configurationReader);

			collector.MetricsProviders?.Add(new NetworkMetricProvider(logger));
			collector.MetricsProviders?.Add(new DiskUsageProvider(logger, diskDrive));

			var components = new AgentComponents(
				logger: logger,
				configurationReader: configurationReader,
				payloadSender: sender,
				metricsCollector: collector,
				currentExecutionSegmentsContainer: null,
				centralConfigFetcher: new LocalConfigFetcher());

			Agent.Setup(components);

			if (!skipNLogTarget)
				Target.Register<NLogApmTarget>("apm");
		}

		public static IFinishedMonitor GetCompletedMonitor()
		{
			var customLogger = new LoggerActivityMonitor(Elastic.Apm.Logging.LogLevel.Trace);
			AgentDependencies.Logger = customLogger;
			return customLogger;
		}

		public static void SetLoggerTargetFolder(string path)
		{
			var canSave = false;
			try
			{
				if (Directory.Exists(path))
				{
					var test = Guid.NewGuid().ToString().Substring(4) + ".lck";
					var testFile = Path.Combine(path, test);
					File.WriteAllText(testFile, "<test>");
					File.Delete(testFile);
					canSave = true;
				}
			}
			catch { /* nothing to do */ }

			if (canSave)
			{
				var target = (FileTarget)LogManager.Configuration.FindTargetByName("file");
				var currentTarget = ((SimpleLayout)target.FileName).OriginalText.Split('/').Last();
				target.FileName = Path.Combine(path, currentTarget);
				LogManager.ReconfigExistingLoggers();
			}
		}
	}
}