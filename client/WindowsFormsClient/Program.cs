using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Elastic.Apm.AspNetFullFramework;
using Elastic.Apm.Logging;
using NLog.Targets;
using ProcessMonitoring;
using Windows.Apm.Client;
using Windows.Apm.Client.Nlog;

namespace WindowsFormsClient
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
			//Registrar nuestro conector APM en NLOG
			Target.Register<NLogApmTarget>("apm");
			NLogApmTarget.OnTrace(x =>
			{
				x.Properties["host"] = "somehost";
				x.Properties["user"] = "marco";
			});

			var customLogger = new LoggerActivityMonitor(LogLevel.Trace);
			customLogger.OnTrace((level, message) =>
			{
				if (message.StartsWith("{LocalPayloadSenderV2} Sent items to server"))
					Console.WriteLine("******************" + message);
				if (message.StartsWith("{LocalPayloadSenderV2} Failed"))
					Console.WriteLine("******************" + message);
			});

			var completionMonitor = FormsApmConfigurer.GetCompletedMonitor();
			FormsApmConfigurer.SetLoggerTargetFolder("c:/temp");
			FormsApmConfigurer.UseApm("c:", enableMoniker: true);


			WmiCounters.LogInterfaceNames();
			WmiCounters.EnableNetworkCounter(); //<-- pasar como parámetro la tarjeta de red, tal y como aparece en el log
												//<-- si no se pasa la tarjeta, se utilizará la que tenga la mayor cuenta de bytes hasta el momento


			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());

			completionMonitor.WaitForFinished(testSecondsInterval: 10);
		}
	}
}
