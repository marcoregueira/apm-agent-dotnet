using System;
using System.Diagnostics;
using System.IO;
using Windows.Apm.Client.Nlog;

namespace Elastic.Apm.Logging
{
	public class LoggerActivityMonitor : IApmLogger, IFinishedMonitor
	{
		public static readonly LogLevel DefaultLogLevel = LogLevel.Error;
		private readonly TextWriter _errorOut;
		private readonly TextWriter _standardOut;

		public LoggerActivityMonitor(LogLevel level, TextWriter standardOut = null, TextWriter errorOut = null)
		{
			Level = level;
			_standardOut = standardOut ?? Console.Out;
			_errorOut = errorOut ?? Console.Error;
		}

		public static LoggerActivityMonitor Instance { get; } = new LoggerActivityMonitor(DefaultLogLevel);

		private LogLevel Level { get; }

		public static LoggerActivityMonitor LoggerOrDefault(LogLevel? level)
		{
			if (level.HasValue && level.Value != DefaultLogLevel)
				return new LoggerActivityMonitor(level.Value);

			return Instance;
		}

		public bool IsEnabled(LogLevel level) => Level <= level;

		private Action<LogLevel, string> _event;

		public void OnTrace(Action<LogLevel, string> onTrace) => _event = onTrace;

		public void Log<TState>(LogLevel level, TState state, Exception e, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(level))
				return;

			var dateTime = DateTime.Now;
			var message = formatter(state, e);

			_event?.Invoke(level, message);

			var fullMessage = $"[{dateTime:yyyy-MM-dd HH:mm:ss.fff zzz}][{LevelToString(level)}] - {message}";
			if (e != null)
				fullMessage += $"{Environment.NewLine}+-> Exception: {e.GetType().FullName}: {e.Message}{Environment.NewLine}{e.StackTrace}";

			switch (level)
			{
				case LogLevel.Critical when Level <= LogLevel.Critical:
				case LogLevel.Error when Level <= LogLevel.Error:
					_errorOut.WriteLineAsync(fullMessage);
					break;
				case LogLevel.Warning when Level <= LogLevel.Warning:
				case LogLevel.Debug when Level <= LogLevel.Debug:
				case LogLevel.Information when Level <= LogLevel.Information:
				case LogLevel.Trace when Level <= LogLevel.Trace:
					_standardOut.WriteLineAsync(fullMessage);
					break;
				// ReSharper disable once RedundantCaseLabel
				case LogLevel.None:
				default:
					break;
			}
		}

		internal static string LevelToString(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Error:
					return "Error";
				case LogLevel.Warning:
					return "Warning";
				case LogLevel.Information:
					return "Info";
				case LogLevel.Debug:
					return "Debug";
				case LogLevel.Trace:
					return "Trace";
				case LogLevel.Critical:
					return "Critical";
				// ReSharper disable once RedundantCaseLabel
				case LogLevel.None:
				default:
					return "None";
			}
		}

		public void WaitForFinished(int testSecondsInterval)
		{
			var sentOut = false;
			var sentError = false;

			var time = new Stopwatch();
			time.Start();
			OnTrace((level, message) =>
			{
				if (message.StartsWith("{LocalPayloadSenderV2} Sent items to server"))
					sentOut = true;

				if (message.StartsWith("{LocalPayloadSenderV2} Failed"))
					sentError = true;
			});

			//omit metrics, or the following loop will execute forever
			GlobalOverrides.MetricsEnabled = false;

			while (true)
			{
				System.Threading.Thread.Sleep(500);
				if (time.Elapsed.TotalSeconds > testSecondsInterval)
				{
					time.Restart();
					if (sentError)
						return;
					if (!sentOut)
						return;
					sentOut = false;
				}
			}
		}
	}
}
