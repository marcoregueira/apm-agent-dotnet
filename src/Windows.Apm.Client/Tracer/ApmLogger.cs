namespace WMS_Infrastructure.Instrumentation
{
	public class ApmLogger
	{
		public static IApmLogger Default { get; set; }
		static ApmLogger() => Default = new ApmLoggerForms();
	}
}

