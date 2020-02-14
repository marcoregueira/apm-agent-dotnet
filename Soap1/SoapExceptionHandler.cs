using System;
using System.Reflection;
using System.Web;
using System.Web.Services.Configuration;
using System.Web.Services.Protocols;
using Windows.Apm.Client.Nlog;

namespace Libertis.WMS_WS.Code
{
	public class SoapExceptionHandler : SoapExtension
	{
		// http://geekswithblogs.net/pavelka/archive/2005/09/05/HowToCreateAGlobalExceptionHandlerForAWebService.aspx

		public override void ProcessMessage(System.Web.Services.Protocols.SoapMessage message)
		{
			if (message.Stage == SoapMessageStage.AfterSerialize)
			{
				if (message.Exception != null)
				{
					Logger.Instance.Error(message.Exception.InnerException);
				}
			}
		}

		public override object GetInitializer(Type serviceType) => null;
		public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute) => null;
		public override void Initialize(object initializer) { }

		public static void RegisterSoapExtension(/* Type type, int priority, PriorityGroup group */)
		{
			// https://social.msdn.microsoft.com/Forums/es-ES/88f8dbbb-3aa9-418b-bbc0-156e023abb9d/programmatically-registering-a-soap-extension?forum=netfxnetcom

			var type = typeof(SoapExceptionHandler);
			var priority = 1;
			var group = PriorityGroup.Low;

			if (!type.IsSubclassOf(typeof(SoapExtension)))
			{
				throw new ArgumentException("Type must be derived from SoapException.", "type");
			}

			if (priority < 1)
			{
				throw new ArgumentOutOfRangeException("priority", priority, "Priority must be greater or equal to 1.");
			}

			// get the current web services settings...  
			var wss = WebServicesSection.Current;

			// set SoapExtensionTypes collection to read/write...  
			var readOnlyField = typeof(System.Configuration.ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
			readOnlyField.SetValue(wss.SoapExtensionTypes, false);

			// inject SoapExtension...  
			var soapInterceptor = new SoapExtensionTypeElement
			{
				Type = type,
				Priority = priority,
				Group = group
			};
			wss.SoapExtensionTypes.Add(soapInterceptor);

			// set SoapExtensionTypes collection back to readonly and clear modified flags...  
			var resetModifiedMethod = typeof(System.Configuration.ConfigurationElement).GetMethod("ResetModified", BindingFlags.NonPublic | BindingFlags.Instance);
			resetModifiedMethod.Invoke(wss.SoapExtensionTypes, null);
			var setReadOnlyMethod = typeof(System.Configuration.ConfigurationElement).GetMethod("SetReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
			setReadOnlyMethod.Invoke(wss.SoapExtensionTypes, null);
		}
	}
}
