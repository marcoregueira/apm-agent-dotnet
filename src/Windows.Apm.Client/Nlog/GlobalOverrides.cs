namespace Windows.Apm.Client.Nlog
{
	public static class GlobalOverrides
	{
		public static string LogLevel { get; set; } = null;
		public static bool TraceEnabled { get; set; } = true;
		public static bool LogSqlEnable { get; set; } = false;
		public static bool MetricsEnabled { get; set; } = true;
		public static bool ForceDisableMetrics { get; set; } = false;

		public static bool AnyEnabled => TraceEnabled || LogSqlEnable || (MetricsEnabled && !ForceDisableMetrics);

		public static bool ConfigurationMonikerEnabled { get; set; }

		public static void DisableAll()
		{
			TraceEnabled = false;
			LogSqlEnable = false;
			MetricsEnabled = false;
		}
	}
}
