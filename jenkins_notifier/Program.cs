using GTK = Gtk;
using Gdk;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

using jenkins_notifier.Models;
using jenkins_notifier.Services;
using System.Timers;
using Notifications;

namespace jenkins_notifier
{
	class MainClass
	{

		private static GTK.StatusIcon trayIcon;
		private static Settings settings;
		private static JobService jobService = new JobService();
		private static SettingsService settingsService = new SettingsService();
		private static PlatformService platformService = new PlatformService();
		private static LoggerService loggerService = new LoggerService();
		private static WebService webService = new WebService ();
		private static List<JobRequest> jobStatuses = new List<JobRequest> ();

		public static void Main (string[] args)
		{
			try {
				settings = settingsService.GetSettings ();

				if (platformService.IsWindows) {
					platformService.HideConsoleWindow ();
				}
				Timer timer = new Timer();
				timer.Elapsed += Timer_Elapsed;
				timer.Interval = settings.PollingInterval;
				timer.Start();


				GTK.Application.Init ();
				string iconPath = settings.ImageFolderPath + "favicon.ico";

				trayIcon = new GTK.StatusIcon (new Pixbuf (iconPath));
				trayIcon.PopupMenu += OnTrayIconPopup;
				trayIcon.Visible = true;


				GTK.Application.Run ();
			} catch (Exception ex) {
				loggerService.Log (ex.Message);
			}
		}

		async static void Timer_Elapsed (object sender, ElapsedEventArgs e)
		{
			var jobs = jobService.GetAllJobs ();
			var tempStatuses = new List<JobRequest> ();
			foreach (var item in jobs) {
				webService.Url = item.Url;
				var result = await webService.GetAsync<JobRequest> ("api/json?pretty=true&tree=color,lastBuild[url]");
				JobRequest request = new JobRequest ();
				if (result.Errored) {
					loggerService.Log (result.Exception);
					request.Name = item.Name;
					request.color = "grey";
					request.ImgPath = settings.ImageFolderPath + request.color + ".png";
					request.lastBuild = new LastBuild ();
					request.lastBuild.url = item.Url;
					request.JobUrl = item.Url;
					tempStatuses.Add (request);
					continue;
				}
					
				request = result.Payload;
				request.Name = item.Name;
				request.ImgPath = settings.ImageFolderPath + request.color + ".png";
				request.JobUrl = item.Url;
				tempStatuses.Add (request);

				var prevJob = jobStatuses
					.Where (x => x.Name == item.Name)
					.FirstOrDefault ();

				if (prevJob == null)
					continue;

				if (prevJob.color != request.color && request.color == "red") {
					try {
						Process.Start ("notify-send", "\"" + item.Name + "\" \"Build Failure\"");
					} catch (Exception ex) {
						loggerService.Log (ex.Message);
					}
				} else if (prevJob.color != request.color && request.color == "blue") {
					try {
						Process.Start ("notify-send", "\"" + item.Name + "\" \"Job was built successfully.\"");
					} catch (Exception ex) {
						loggerService.Log (ex.Message);
					}
				} else if (prevJob.color != request.color && request.color == "blue_anime") {
					try {
						Process.Start ("notify-send", "\"" + item.Name + "\" \"Building job...\"");
					} catch (Exception ex) {
						loggerService.Log (ex.Message);
					}
				}
			}
			jobStatuses = tempStatuses;
		}


		// Create the popup menu, on right click.
		static void OnTrayIconPopup (object o, EventArgs args) {
			GTK.Menu popupMenu = new GTK.Menu();
			GTK.ImageMenuItem menuItemQuit = new GTK.ImageMenuItem ("Quit");
			Gtk.Image appimg = new Gtk.Image(GTK.Stock.Quit, GTK.IconSize.Menu);
			menuItemQuit.Image = appimg;

			foreach (var item in jobStatuses) {
				if (item.Name == null)
					continue;

				GTK.Image statusImg = new GTK.Image (item.ImgPath);
				GTK.ImageMenuItem parentItem = new GTK.ImageMenuItem (item.Name);
				parentItem.Image = statusImg;
				parentItem.AlwaysShowImage = true;

				GTK.Menu subMenu = new GTK.Menu ();

				var jopImg = new GTK.Image (new Pixbuf(settings.ImageFolderPath + "open_in_browser.png"));
				jopImg.Visible = true;
				var jobOpenPage = new GTK.ImageMenuItem ("Open Web Page");
				jobOpenPage.Image = jopImg;
				jobOpenPage.AlwaysShowImage = true;
				jobOpenPage.Activated += (object sender, EventArgs e) => {
					Process.Start (item.lastBuild.url);
				};


				var jbaImg = new GTK.Image (new Pixbuf(settings.ImageFolderPath + "build.png"));
				var jobBuildAction = new GTK.ImageMenuItem ("Build Now");
				jobBuildAction.Image = jbaImg;
				jobBuildAction.AlwaysShowImage = true;
				jobBuildAction.Activated += (object sender, EventArgs e) => {
					WebService buildWebService = new WebService();
					buildWebService.Url = item.JobUrl;
					buildWebService.GetAsync<string>("build");
				};

				var jbcImg = new GTK.Image (new Pixbuf(settings.ImageFolderPath + "history.png"));
				var jobBuildConsole = new GTK.ImageMenuItem ("Current Build Console");
				jobBuildConsole.Image = jbcImg;
				jobBuildConsole.AlwaysShowImage = true;
				jobBuildConsole.Activated += (object sender, EventArgs e) => {
					Process.Start(item.lastBuild.url + "console");
				};

				subMenu.Add (jobBuildAction);
				subMenu.Add (jobOpenPage);
				subMenu.Add (jobBuildConsole);

				parentItem.Submenu = subMenu;
				popupMenu.Add (parentItem);
			}


			popupMenu.Add(menuItemQuit);

			menuItemQuit.Activated += delegate { GTK.Application.Quit(); };
			popupMenu.ShowAll();
			popupMenu.Popup();
		}



	}

}