using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityMonitorWPF
{
	public struct WindowInfo
	{
		public string WindowFullName;
		public string ProccessName;
	};

	public class WindowGrabber
	{

		public WindowGrabber()
		{
		}

		//Functions imported to grab your current active window as well as the information about said window
		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();
		[DllImport("user32.dll")]
		static extern IntPtr GetWindowText(IntPtr hWnd, StringBuilder ss, int count);

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);


		public WindowInfo GetActivewindowInfo()
		{
			WindowInfo info = new WindowInfo();

			const int nChar = 256;
			StringBuilder sb = new StringBuilder(nChar);

			IntPtr handle = IntPtr.Zero;
			handle = GetForegroundWindow();

			IntPtr returnVal = IntPtr.Zero;
			returnVal = GetWindowText(handle, sb, nChar);

			info.WindowFullName = " ";
			info.ProccessName = " ";
			if (returnVal != IntPtr.Zero)
			{
				info.WindowFullName = sb.ToString();
				info.ProccessName = GetActiveFileProcessName(handle);
			}

			return info;
		}

		public string GetActiveFileProcessName(IntPtr handle)
		{
			uint pid;
			GetWindowThreadProcessId(handle, out pid);

			try
			{
				Process p = Process.GetProcessById((int)pid);
				return p.ProcessName;
			}
			catch(Exception ex)
			{
				return "ERROR PROCESS";
			}
		}
	}
}
