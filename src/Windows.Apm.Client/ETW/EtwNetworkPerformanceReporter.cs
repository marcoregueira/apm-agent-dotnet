﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;


/// <summary>
/// CREDIT TO:
/// https://stackoverflow.com/a/45076244/2982757
/// THIS METHOD REQUIRES ADMIN PRIVILEGE
/// </summary>
namespace ProcessMonitoring
{
	public sealed class EtwNetworkPerformanceReporter : IDisposable
	{
		//private DateTime m_EtwStartTime;
		private TraceEventSession m_EtwSession;

		private readonly Counter m_NetworkIn = new Counter() { TimeStamp = DateTime.Now };
		private readonly Counter m_NetworkOut = new Counter() { TimeStamp = DateTime.Now };

		private EtwNetworkPerformanceReporter() { }

		public static EtwNetworkPerformanceReporter Create()
		{
			var networkPerformancePresenter = new EtwNetworkPerformanceReporter();
			networkPerformancePresenter.Initialise();
			return networkPerformancePresenter;
		}

		// Note that the ETW class blocks processing messages, so should be run on a different thread if you want the application to remain responsive.
		private void Initialise() => Task.Run(() => StartEtwSession());

		public class Counter
		{

			public DateTime TimeStamp { get; set; }
			public long Units { get; set; }
			public double UnitsPerSecond { get; set; }
		}

		private void StartEtwSession()
		{
			try
			{
				var processId = Process.GetCurrentProcess().Id;
				ResetCounters();

				using (m_EtwSession = new TraceEventSession("MyKernelAndClrEventsSession"))
				{
					m_EtwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);
					m_EtwSession.Source.Kernel.TcpIpRecv += data =>
					{
						if (data.ProcessID == processId)
						{
							lock (m_NetworkIn)
							{
								var mSecods = (data.TimeStamp - m_NetworkIn.TimeStamp).TotalMilliseconds;
								m_NetworkIn.TimeStamp = data.TimeStamp;
								m_NetworkIn.UnitsPerSecond = 1000 * (data.size - m_NetworkIn.Units) / mSecods;
								Console.WriteLine("out " + m_NetworkIn.UnitsPerSecond);
							}
						}
					};

					m_EtwSession.Source.Kernel.TcpIpSend += data =>
					{
						if (data.ProcessID == processId)
						{
							lock (m_NetworkOut)
							{
								var mSecods = (data.TimeStamp - m_NetworkOut.TimeStamp).TotalMilliseconds;
								m_NetworkOut.TimeStamp = data.TimeStamp;
								m_NetworkOut.UnitsPerSecond = 1000 * (data.size - m_NetworkOut.Units) / mSecods;
								Console.WriteLine("out " + m_NetworkOut.UnitsPerSecond);
							}
						}
					};

					m_EtwSession.Source.Process();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				ResetCounters(); // Stop reporting figures
								 // Probably should log the exception
			}
		}

		//public NetworkPerformanceData GetNetworkPerformanceData()
		//{
		//	var timeDifferenceInSeconds = (DateTime.Now - m_EtwStartTime).TotalSeconds;
		//
		//	NetworkPerformanceData networkData;
		//
		//	lock (m_Counters)
		//	{
		//		networkData = new NetworkPerformanceData
		//		{
		//			BytesReceived = Convert.ToInt64(m_Counters.Received / timeDifferenceInSeconds),
		//			BytesSent = Convert.ToInt64(m_Counters.Sent / timeDifferenceInSeconds)
		//		};
		//
		//	}
		//
		//	// Reset the counters to get a fresh reading for next time this is called.
		//	ResetCounters();
		//
		//	return networkData;
		//}
		//
		private void ResetCounters()
		{
			//lock (m_Counters)
			//{
			//	m_Counters.Sent = 0;
			//	m_Counters.Received = 0;
			//}
			//m_EtwStartTime = DateTime.Now;
		}

		public void Dispose() => m_EtwSession?.Dispose();
	}

	public sealed class NetworkPerformanceData
	{
		public long BytesReceived { get; set; }
		public long BytesSent { get; set; }
	}
}