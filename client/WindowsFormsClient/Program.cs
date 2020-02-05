using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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
		static void Main()
		{
			//Registrar nuestro conector APM en NLOG
			FormsApmConfigurer.SetLoggerTargetFolder("c:/temp");
			Target.Register<NLogApmTarget>("apm");
			FormsApmConfigurer.UseApm();

			WmiCounters.LogInterfaceNames();
			WmiCounters.EnableNetworkCounter(); //<-- pasar como parámetro la tarjeta de red, tal y como aparece en el log
												//<-- si no se pasa la tarjeta, se utilizará la que tenga la mayor cuenta de bytes hasta el momento

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}
