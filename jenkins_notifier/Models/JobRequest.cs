using System;

namespace jenkins_notifier
{
	public class JobRequest
	{
		public JobRequest ()
		{



		}

		public string Name { get; set; }
		public string ImgPath { get; set; }
		public string JobUrl { get; set; }

		public string color { get; set; }
		public LastBuild lastBuild {get;set;}
	}
}

