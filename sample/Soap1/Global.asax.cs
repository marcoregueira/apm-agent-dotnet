using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using NLog.Targets;
using ProcessMonitoring;
using Windows.Apm.Client;
using Windows.Apm.Client.Nlog;
using WMS_Infrastructure.Instrumentation;

namespace Soap1
{
	public class Global : System.Web.HttpApplication
	{
		private readonly object locker = new object();

		
		public override void Init()
		{
			base.Init();
			//var module = new ApmLoggerHttpModule();
			//module.Init(this);

			if (_onlyOnce)
				lock (locker)
				{
					if (_onlyOnce)
						ConfigureNLogAndApm();

					_onlyOnce = false;
				}
		}

		private static bool _onlyOnce = true;


		private void ConfigureNLogAndApm()
		{
			//SoapExceptionHandler.RegisterSoapExtension(); //no tiene nada que ver con apm, es para sacar las expciones de Soap y logarlas.

			//Configurar NLOG
			//if (LogFile.ApmEnabled)
			Target.Register<NLogApmTarget>("apm");
			ApmLogger.Default = new ApmLoggerHttpModule();
			FormsApmConfigurer.UseApm(enableMoniker:true);
			WmiCounters.LogInterfaceNames();
			WmiCounters.EnableNetworkCounter(); //<-- pasar como parámetro la tarjeta de red, tal y como aparece en el log
												//<-- si no se pasa la tarjeta, se utilizará la que tenga la mayor cuenta de bytes hasta el momento


			NLogApmTarget.OnTrace(x =>
			{
				var transaction = HttpContext.Current?.Items["__APM_TRANSACTION"] as ExecutionSegment;
				x.Properties["transaction"] = transaction?.CurrentTransaction.Id;
				x.Properties["terminal"] = "XXXXX terminal";
				x.Properties["user"] = "XXXXX user";
				x.Properties["database"] = "XXXXX database";
				if (x.Parameters?.Length > 0)
				{
					if (x.Parameters[0] is Dictionary<string, string> overrides)
						foreach (var pair in overrides)
						{
							x.Properties[pair.Key] = pair.Value;
						}
				}
			});
		}

		protected void Application_Start(object sender, EventArgs e)
		{

		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_EndRequest(object sender, EventArgs e)
		{
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{

		}
	}
}
