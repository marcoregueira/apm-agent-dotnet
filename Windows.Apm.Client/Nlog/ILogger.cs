﻿using System;

namespace Windows.Apm.Client.Nlog
{
	public interface ILogger
	{
		void Info(string message);

		void Warn(string message);

		void Debug(string message, object args = null);

		void Error(string message);
		void Error(string message, Exception x);
		void Error(Exception x);

		void Fatal(string message);
		void Fatal(Exception x);
	}
}
