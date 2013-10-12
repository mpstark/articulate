using System;
using System.Collections.Generic;
using System.ComponentModel;
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


		#region Native API

		[Flags]
		enum ProcessAccessFlags : uint
		{
			All = 0x001F0FFF,
			Terminate = 0x00000001,
			CreateThread = 0x00000002,
			VMOperation = 0x00000008,
			VMRead = 0x00000010,
			VMWrite = 0x00000020,
			DupHandle = 0x00000040,
			SetInformation = 0x00000200,
			QueryInformation = 0x00000400,
			QueryLimitedInformation = 0x1000,
			Synchronize = 0x00100000
		}

		private static string GetExecutablePath(Process Process)
		{
			//If running on Vista or later use the new function
			if (Environment.OSVersion.Version.Major >= 6)
			{
				return GetExecutablePathAboveVista(Process.Id);
			}

			return Process.MainModule.FileName;
		}

		private static string GetExecutablePathAboveVista(int ProcessId)
		{
			var buffer = new StringBuilder(1024);
			IntPtr hprocess = OpenProcess(ProcessAccessFlags.QueryLimitedInformation,
										  false, ProcessId);
			if (hprocess != IntPtr.Zero)
			{
				try
				{
					int size = buffer.Capacity;
					if (QueryFullProcessImageName(hprocess, 0, buffer, out size))
					{
						return buffer.ToString();
					}
				}
				finally
				{
					CloseHandle(hprocess);
				}
			}
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		[DllImport("kernel32.dll")]
		private static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags,
					   StringBuilder lpExeName, out int size);
		[DllImport("kernel32.dll")]
		private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess,
					   bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr hHandle);

		#endregion

		/// <summary>
		/// Gets the Full Path to the current foreground window's executable file
		/// </summary>
		public static string FullPath
		{
			get
			{
				IntPtr processID;
				GetWindowThreadProcessId(GetForegroundWindow(), out processID);

				if (Environment.OSVersion.Version.Major >= 6)				
					return GetExecutablePathAboveVista((int)processID);
				
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