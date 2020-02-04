using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetFullFrameworkSampleApp;
using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.Config;
using Elastic.Apm.Helpers;
using Elastic.Apm.Logging;
using Elastic.Apm.Metrics;
using Elastic.Apm.Report;

namespace Windows.Apm.Client
{
	public class FormsApmConfigurer
	{

		public static void UseApm()
		{
			var configurationReader = new LocalConfigurationReader();
			var logger = ConsoleLogger.LoggerOrDefault(configurationReader.LogLevel);
			var service = Service.GetDefaultService(configurationReader, logger);
			var systemInfoHelper = new SystemInfoHelper(logger);
			var system = systemInfoHelper.ParseSystemInfo();

			var centralConfigFetecher = new LocalConfigFetcher();
			var configStore = new ConfigStore(new ConfigSnapshotFromReader(configurationReader, "local"), logger);
			var sender = new LocalPayloadSenderV2(logger, configStore.CurrentSnapshot, service, system);

			var collector = new MetricsCollector(logger, sender, configurationReader);

			collector.MetricsProviders.Add(new NetworkMetricProvider());

			var components = new AgentComponents(
				logger: logger,
				configurationReader: configurationReader,
				payloadSender: sender,
				metricsCollector: collector,
				currentExecutionSegmentsContainer: null,
				centralConfigFetcher: new LocalConfigFetcher());

   Agent.Setup(components);
		}
	}
}
