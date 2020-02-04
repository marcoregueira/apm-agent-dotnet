using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Elastic.Apm;

/// <summary>
/// CREDIT TO:
/// https://stackoverflow.com/a/45076244/2982757
/// THIS METHOD REQUIRES ADMIN PRIVILEGE
/// </summary>
namespace ProcessMonitoring
{
	public static class WmiCounters

	{
		public static Dictionary<string, NetworkInterface> EnabledInterfaces { get; set; } = new Dictionary<string, NetworkInterface>();

		static WmiCounters()
		{
			//EnabledInterfaces = NetworkInterface.GetAllNetworkInterfaces().ToDictionary(x => x.Name);

			EnabledInterfaces = NetworkInterface.GetAllNetworkInterfaces()
				.Where(x => x.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
				.OrderByDescending(x => x.GetIPStatistics().BytesReceived)
				.Take(1)
				//.Where(x => x.Description.Contains("Athero"))
				.ToDictionary(x => x.Description);
		}

		public static NetworkMetrics GetOneInterface() =>
			GetNetworkUtilization(EnabledInterfaces.FirstOrDefault().Key, EnabledInterfaces.FirstOrDefault().Value);

		public static NetworkMetrics GetNetworkUtilization(string networkCard, NetworkInterface networkInterface)
		{
			//var n = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().First();

			//var dataSentCounter =
			//	new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkCard);
			//var dataReceivedCounter =
			//	new PerformanceCounter("Network Interface", "Bytes Received/sec", networkCard);

			float sendSum = 0;
			float receiveSum = 0;
			//for (var index = 0; index < numberOfIterations; index++)
			//{
			//	sendSum += dataSentCounter.NextValue();
			//	receiveSum += dataReceivedCounter.NextValue();
			//}

			var bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", networkCard);
			var bandwidth = bandwidthCounter.NextValue();



			sendSum = networkInterface.GetIPStatistics().BytesSent;
			receiveSum = networkInterface.GetIPStatistics().BytesReceived;
			return new NetworkMetrics(sendSum, receiveSum)
			{
				NetworkInterface = networkInterface.Name,
				Card = networkCard,

			};

			//Console.WriteLine(sendSum.ToString("0.00"));
			//Console.WriteLine(receiveSum.ToString("0.00"));

			//Agent.Instance.Logger.Log<NewClass>(Elastic.Apm.Logging.LogLevel.Information, new NewClass(sendSum, receiveSum), null, null);

			// var dataSent = sendSum;
			// var dataReceived = receiveSum;
			// //double utilization = (8 * (dataSent + dataReceived)) / (bandwidth * numberOfIterations) * 100;
			//return utilization;
		}
	}

	public class NetworkMetrics
	{
		public float SendSum { get; }
		public float ReceiveSum { get; }
		public string NetworkInterface { get; internal set; }
		public string Card { get; internal set; }

		public NetworkMetrics(float sendSum, float receiveSum)
		{
			SendSum = sendSum;
			ReceiveSum = receiveSum;
		}
	}
}
