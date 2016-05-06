using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using jenkins_notifier.Models;

namespace jenkins_notifier.Services
{
	public class JobService
	{
		private LoggerService loggerService = new LoggerService ();
		private Settings settings = new Settings();
		
		public JobService ()
		{
			settings = new SettingsService ().GetSettings ();
		}

		public List<Job> GetAllJobs() {
			List<Job> jobs = new List<Job> ();
			string jobFileContents = File.ReadAllText(settings.ExecutingPath + settings.JobsFileName);
			jobs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Job>>(jobFileContents);
			return jobs;
		}

	}
}

