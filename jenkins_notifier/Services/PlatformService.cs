using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using jenkins_notifier.Models;

namespace jenkins_notifier.Services
{
	public class PlatformService
	{

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		PlatformID CurrentPlatform = Environment.OSVersion.Platform;
		public PlatformService ()
		{
			
		}

		public string DirChar {
			get {
				if (IsWindows) {
					return "\\";
				} else {
					return "/";
				}
			}
		}

		public bool IsWindows { 
			get {
				if (CurrentPlatform == PlatformID.Win32NT ||
				    CurrentPlatform == PlatformID.Win32S ||
				    CurrentPlatform == PlatformID.Win32Windows ||
				    CurrentPlatform == PlatformID.WinCE) {
					return true;
				} else {
					return false;
				}
			}
		}

		public bool IsLinux {
			get {
				if (CurrentPlatform == PlatformID.Unix) {
					return true;
				} else {
					return false;
				}
			}
		}

		public bool IsMac {
			get {
				if (CurrentPlatform == PlatformID.MacOSX) {
					return true;
				} else {
					return false;
				}
			}
		}

		public void HideConsoleWindow() {
			if (IsWindows) {
				IntPtr h = Process.GetCurrentProcess ().MainWindowHandle;
				ShowWindow (h, 0);
			}
		}
	}
}

