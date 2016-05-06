using System;

namespace jenkins_notifier.Models
{
	public class Settings
	{
		public Settings ()
		{
		}

		public string ExecutingPath { get;set; }
		public PlatformID CurrentPlatform { get;set; }

		public string DefaultEmulator { get; set; }
		public string DefaultEmulatorArguments { get; set; }
		public string DefaultItemFolder { get; set; }

		public string ImageFolderPath { get; set; }
		public string JobsFileName{get;set;}
		public int PollingInterval { get; set; }


	}
}

