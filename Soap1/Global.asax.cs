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


		public override void Init()
		{
			base.Init();
			var module = new ApmLoggerHttpModule();

			if (_onlyOnce)
				ConfigureNLogAndApm(module);

			_onlyOnce = false;
			module.Init(this);
		}

		private static bool _onlyOnce = true;

		private void ConfigureNLogAndApm(ApmLoggerHttpModule module)
		{
			//SoapExceptionHandler.RegisterSoapExtension(); //no tiene nada que ver con apm, es para sacar las expciones de Soap y logarlas.

			//Configurar NLOG
			//if (LogFile.ApmEnabled)
			Target.Register<NLogApmTarget>("apm");
			ApmLogger.Default = module;

			FormsApmConfigurer.UseApm();
			WmiCounters.LogInterfaceNames();
			WmiCounters.EnableNetworkCounter(); //<-- pasar como parámetro la tarjeta de red, tal y como aparece en el log
												//<-- si no se pasa la tarjeta, se utilizará la que tenga la mayor cuenta de bytes hasta el momento
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
