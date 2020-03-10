using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Elastic.Apm;
using Elastic.Apm.Api;
using ProcessMonitoring;
using Windows.Apm.Client;
using Windows.Apm.Client.Nlog;
using WMS_Infrastructure.Instrumentation;

namespace WindowsFormsClient
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			//NetworkPerformanceReporter.Create();

			var transaction2 = Agent.Tracer.StartTransaction("Transaction2", "TestTransaction", DistributedTracingData.TryDeserializeFromString("somedata"));

			try
			{
				//Task.Run(() => WmiCounters.GetNetworkUtilization(WmiCounters.EnabledInterfaces.First().Key, WmiCounters.EnabledInterfaces.First().Value));



				transaction2.CaptureSpan("TestSpan", "TestSpanType", () => Task.Run(() =>
				{
					Thread.Sleep(500);
					//var s = "a";
					//for (var i = 0; i < 100000000; i++)
					//{
					//	s += "b";
					//}
				}));
			}
			finally
			{
				transaction2.End();
			}
		}

		private void BtnException_Click(object sender, EventArgs e)
		{
			try
			{
				throw new InvalidOperationException(TxtExceptionMessage.Text.Replace("{datetime}", DateTime.Now.ToString()));
			}
			catch (Exception ex)
			{
				ApmLogger.Default.LogExceptionToApm(ex, "exception");
			}
		}

		private void BtnLog_Click(object sender, EventArgs e)
			=> ApmLogger.Default.LogTraceToApm(TxtLogMessage.Text.Replace("{datetime}", DateTime.Now.ToString()));

		private void BtnNlog_Click(object sender, EventArgs e)
			=> Logger.Instance.Debug(TxtNLog.Text.Replace("{datetime}", DateTime.Now.ToString()));

		private void BtnExcepcionNLOG_Click(object sender, EventArgs e)
			=> Logger.Instance.Error(TxtExcepcionNLOG.Text.Replace("{datetime}", DateTime.Now.ToString()),
				new InvalidOperationException(TxtExcepcionNLOG.Text.Replace("{datetime}", DateTime.Now.ToString())));

		private void button1_Click(object sender, EventArgs e)
		{
			using (var LOG1 = ApmLogger.Default.InitTrasaction("FrmStockDetail_Constructor", "SCC"))
			{
				ApmLogger.Default.LogTraceToApm("some message in transaction");
				Logger.Instance.Debug(TxtNLog.Text.Replace("{datetime}", DateTime.Now.ToString()));
				Logger.Instance.Info(TxtNLog.Text.Replace("{datetime}", DateTime.Now.ToString()));

				Logger.Instance.Error(TxtExcepcionNLOG.Text.Replace("{datetime}", DateTime.Now.ToString()),
				   new InvalidOperationException(TxtExcepcionNLOG.Text.Replace("{datetime}", DateTime.Now.ToString())));

			}
		}
	}
}
