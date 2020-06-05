using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.Config;
using Elastic.Apm.Report;
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

		private static Action<ClientInfoDetails> _event;

		public static void OnTrace(Action<ClientInfoDetails> onTrace) => _event = onTrace;

		public static Timer TimerMoniker { get; set; }

		public static void CompleteTraceLabels()
		{
			ISpan FilterSpan(ISpan span)
			{
				GetClientInfo(span.Labels);
				return span;
			}

			ITransaction FilterTransaction(ITransaction transaction)
			{
				GetClientInfo(transaction.Labels);
				return transaction;
			}

			IError FilterError(IError transaction)
			{
				//GetClientInfo(transaction.Labels);
				return transaction;
			}


			if (Agent.Instance.PayloadSender is LocalPayloadSenderV2)
			{
				var localSender = Agent.Instance.PayloadSender as LocalPayloadSenderV2;
				localSender.TransactionFilters.Add(FilterTransaction);
				localSender.SpanFilters.Add(FilterSpan);
				localSender.ErrorFilters.Add(FilterError);
			}
			else
			{
				Agent.AddFilter(FilterTransaction);
				Agent.AddFilter(FilterSpan);
			}
		}

		public static void EnableMoniker(IConfigurationReader reader, bool checkInmediate = false)
		{

			Client = new HttpClient
			{
				BaseAddress = reader.ServerUrls.FirstOrDefault()
			};


			_serviceName = reader.ServiceName;
			TimerMoniker = new Timer(async (e) => CheckActivationChangesAsync(e), null, 4000, 4000);
			GlobalOverrides.ConfigurationMonikerEnabled = true;
			if (checkInmediate)
				CheckActivationChangesAsync(null).Wait();
		}

		public static void DisableAll() => GlobalOverrides.DisableAll();



		private static async Task CheckActivationChangesAsync(object sender)
		{
			try
			{


				if (!GlobalOverrides.ConfigurationMonikerEnabled) return;

				var clientInfo = GetClientInfo();
				var clientStatusRequest = new ClientInfoRequest()
				{
					App = clientInfo.App,
					Client = clientInfo.Client
				};

				var val = JsonConvert.SerializeObject(clientStatusRequest);
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
						GlobalOverrides.LogLevel = config.LogLevel;
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

		private static ClientInfoDetails GetClientInfo(Dictionary<string, string> labels = null)
		{
			var request = new ClientInfoDetails()
			{
				Client = Environment.MachineName,
				App = _serviceName,
				Labels = labels ?? new Dictionary<string, string>()
			};

			_event?.Invoke(request);
			return request;
		}
	}
}
