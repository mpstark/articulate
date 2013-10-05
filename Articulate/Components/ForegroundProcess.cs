using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Articulate
{
	/// <summary>
	/// Gets information about the currently active window
	/// </summary>
	public static class ForegroundProcess
	{
		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		/// <summary>
		/// Gets the Full Path to the current foreground window's executable file
		/// </summary>
		public static string FullPath
		{
			get
			{
				IntPtr processID;
				GetWindowThreadProcessId(GetForegroundWindow(), out processID);

				Process p = Process.GetProcessById((int)processID);

				return p.MainModule.FileName;
			}
		}

		/// <summary>
		/// Gets the executable's filename without extension
		/// </summary>
		public static string ExecutableName
		{
			get
			{
				return Path.GetFileNameWithoutExtension(FullPath);
			}
		}
	}
}
