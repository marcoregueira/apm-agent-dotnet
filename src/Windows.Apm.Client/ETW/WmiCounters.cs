using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using Windows.Apm.Client.Nlog;

/// <summary>
/// CREDIT TO:
/// https://stackoverflow.com/a/45076244/2982757
/// THIS METHOD REQUIRES ADMIN PRIVILEGE
/// </summary>
namespace ProcessMonitoring
{
	public static class WmiCounters

	{
		private static PerformanceCounter _networkCounter = null;
		private static NetworkInterface _nic;

		static WmiCounters()
		{

		}

		public static void LogInterfaceNames()
		{
			var enabledInterfaces = NetworkInterface.GetAllNetworkInterfaces()
				.Where(x => x.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
				.OrderByDescending(x => x.GetIPStatistics().BytesReceived)
				.ToList();

			var b = new StringBuilder();
			b.AppendLine("NETWORK INTERFACES");
			b.AppendLine("==================");

			foreach (var nic in enabledInterfaces)
			{
				b.AppendLine($"{nic.Description} || {nic.Name}");
			}
			Logger.Instance.Debug(b.ToString());
		}

		public static void EnableNetworkCounter(string nicName = null)
		{
			//EnabledInterfaces = NetworkInterface.GetAllNetworkInterfaces().ToDictionary(x => x.Name);
			_networkCounter?.Dispose();

			var enabledInterfaces = NetworkInterface.GetAllNetworkInterfaces()
				.Where(x => x.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
				.Where(x => x.Description == nicName || string.IsNullOrWhiteSpace(nicName))
				.OrderByDescending(x => x.GetIPStatistics().BytesReceived)
				.ToList();

			foreach (var nic in enabledInterfaces)
			{
				PerformanceCounter bwc = null;
				try
				{
					bwc = new PerformanceCounter("Network Interface", "Current Bandwidth", nic.Description);
					_ = bwc.NextValue();
					_nic = nic;
					_networkCounter = bwc;

					Logger.Instance.Debug("Obteniendo métricas de la tarjeta: " + _nic.Description);

					break; //we only want one of those
				}
				catch
				{
					bwc?.Dispose();
				}
			}
		}

		public static NetworkMetrics GetOneInterface() =>
			GetNetworkUtilization();

		public static NetworkMetrics GetNetworkUtilization()
		{
			if (_nic == null) return null;
			if (_networkCounter == null) return null;

			var sendSum = _nic.GetIPStatistics().BytesSent;
			var receiveSum = _nic.GetIPStatistics().BytesReceived;
			return new NetworkMetrics(sendSum, receiveSum)
			{
				NetworkInterface = _nic.Description,
				Card = _nic.Name,
			};
		}
	}

	public class NetworkMetrics
	{
		public float SentSum { get; }
		public float ReceivedSum { get; }
		public string NetworkInterface { get; internal set; }
		public string Card { get; internal set; }

		public NetworkMetrics(float sentSum, float receiveSum)
		{
			SentSum = sentSum;
			ReceivedSum = receiveSum;
		}
	}
}
