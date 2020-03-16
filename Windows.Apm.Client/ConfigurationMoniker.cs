using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Apm.Client.Nlog;
using Windows.Metrics.Ingest.Dto;

namespace Windows.Apm.Client
{
	public class ConfigurationMoniker
	{
		public const string ConfigEndpointUrl = "station/configuration";
		private static HttpClient Client { get; set; }

		private static readonly object _locker = new object();

		public static void EnableMoniker(string configurationService, bool checkInmediate)
		{
			if (!Uri.TryCreate(configurationService, UriKind.Absolute, out var uri))
				return;

			Client = new HttpClient
			{
				BaseAddress = new Uri(configurationService)
			};

			var time = new Timer(async (e) => CheckActivationChangesAsync(e), null, 20000, 20000);

			if (checkInmediate)
				CheckActivationChangesAsync(null).RunSynchronously();
		}

		public static void DisableAll() => GlobalOverrides.DisableAll();

		private static async Task CheckActivationChangesAsync(object sender)
		{
			var request = new ClientInfoRequest();
			var val = JsonConvert.SerializeObject(request);
			var content = new StringContent(val, Encoding.UTF8, "application/x-json");

			if (!Monitor.TryEnter(_locker))
				return;

			try
			{
				var CtsInstance = new CancellationTokenSource();
				CtsInstance.CancelAfter(1000);

				Console.WriteLine(val.ToString());
				var result = await Client.PostAsync(ConfigEndpointUrl, content, CtsInstance.Token);

				if (result != null && !result.IsSuccessStatusCode)
				{
					var responseString = await result.Content.ReadAsStringAsync();
					var config = JsonConvert.DeserializeObject<ClientInfoResponse>(responseString);

					if (config == null)
						return;

					GlobalOverrides.LogSqlEnable = config.LogSqlEnabled;
					GlobalOverrides.TraceEnabled = config.TraceEnabled;
					GlobalOverrides.MetricsEnabled = config.MetricsEnabled;
				}
				else
				{
					Logger.Instance.Debug("Error recuperando la configuración de traza remota.");
				}
			}
			catch (Exception ex)
			{
				Logger.Instance.Debug("Error recuperando la configuración de traza remota.", ex);
			}
			finally
			{
				Monitor.Exit(_locker);
			}
		}
	}
}
