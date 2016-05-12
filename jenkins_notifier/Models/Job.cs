using System;
using jenkins_notifier.Services;
namespace jenkins_notifier.Models
{
	public class Job
	{
		private SettingsService settingsService = new SettingsService();
		private Settings settings;

		public Job ()
		{
			settings = settingsService.GetSettings ();
		}

		public string Name { get; set; }
		public string Url { get; set; }


		public string JobUrl { get; set; }
		public string color { get; set; }
		public LastBuild lastBuild {get;set;}

		public string ImgPath { 
			get { 
				if (color.Contains ("anime")) {
					return settings.ImageFolderPath + "history.png";
				} else {
					return settings.ImageFolderPath + color + ".png";
				}
			}
		}

		public string JobStatusMessage {
			get {
				if (color == "red")
					return "Build Failure";

				if (color == "blue_anime")
					return "Building job...";

				if (color != "grey")
					return "Job was built successfully.";
				
				return "";
			}
		}
	}


}

