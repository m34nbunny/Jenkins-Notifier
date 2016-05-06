using System;
using System.Diagnostics;

namespace jenkins_notifier.Services
{
	public class LoggerService
	{
		public LoggerService ()
		{
		}


		public void Log(string message, bool includeTimestamp = true) {
			string log = string.Empty;
			if (string.IsNullOrWhiteSpace (message) == false) {
				log += DateTime.Now + " - " + message;
			}
			Trace.WriteLine (log);
		}
	}
}

