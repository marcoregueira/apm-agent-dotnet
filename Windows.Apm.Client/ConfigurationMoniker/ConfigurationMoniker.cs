using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Apm.Config;
using Newtonsoft.Json;
using Windows.Apm.Client.Nlog;
using Windows.Metrics.Ingest.Dto;

namespace Windows.Apm.Client
{
	public class ConfigurationMoniker
	{
		public const string ConfigEndpointUrl = "station/configuration";

		private static HttpClient Client { get; set; }

		private static string _serviceName;
		private static readonly SemaphoreSlim _locker = new SemaphoreSlim(1);

		private static Action<ClientInfoRequest> _event;

		public static void OnTrace(Action<ClientInfoRequest> onTrace) => _event = onTrace;

		public static Timer TimerMoniker { get; set; }

		public static void EnableMoniker(IConfigurationReader reader, bool checkInmediate = false)
		{
			Client = new HttpClient
			{
				BaseAddress = reader.ServerUrls.FirstOrDefault()
			};

			_serviceName = reader.ServiceName;
			TimerMoniker = new Timer(async (e) => CheckActivationChangesAsync(e), null, 4000, 4000);
			if (checkInmediate)
				CheckActivationChangesAsync(null).Wait();
		}

		public static void DisableAll() => GlobalOverrides.DisableAll();

		private static async Task CheckActivationChangesAsync(object sender)
		{
			try
			{

				var request = new ClientInfoRequest() { Client = Environment.MachineName, App = _serviceName };
				_event?.Invoke(request);

				var val = JsonConvert.SerializeObject(request);
				var content = new StringContent(val, Encoding.UTF8, "application/json");


				if (!_locker.Wait(0))
					return;

				try
				{
					var CtsInstance = new CancellationTokenSource();
					CtsInstance.CancelAfter(1000);

					Console.WriteLine(val.ToString());
					var result = await Client.PostAsync(ConfigEndpointUrl, content, CtsInstance.Token);

					if (result != null && result.IsSuccessStatusCode)
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
						// Logger.Instance.Debug("Error recuperando la configuración de traza remota.");
					}
				}
				catch (Exception ex)
				{
					// Logger.Instance.Debug("Error recuperando la configuración de traza remota.", ex);
				}
				finally
				{
					_locker.Release();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Moniker not working");
			}
		}
	}
}
