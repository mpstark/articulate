using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using SharpRaven;
using System.Reflection;

namespace Articulate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		public const string AppMutex = "Articulate";

		internal static App Instance
		{
			get;
			private set;
		}

		static RavenClient _Sentry = null;
		internal static RavenClient Sentry
		{
			get {
                return _Sentry = _Sentry ?? new RavenClient(Constants.SentryDSN)
                {
#if DEBUG
                    Environment = "debug",
#else
                    Environment = "release",
#endif
                    Release = Assembly.GetExecutingAssembly().GetVersion()?.ToString(3),
                    Logger = "Desktop"
                };
            }
		}

		[STAThread]
		public static void Main()
		{
			AppDomain.CurrentDomain.UnhandledException += (o, e) =>
			{
				HandleError(e.ExceptionObject as Exception);
			};
			
			bool newInstance = false;
			using (Mutex appMutex = new Mutex(true, AppMutex, out newInstance))
			{
				if (!newInstance)				
					return;
				
				Instance = new App();
				Instance.InitializeComponent();
				Instance.Run();
			}
		}

		static bool IsErrorHandling = false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static void HandleError(Exception ex)
		{
			Sentry.CaptureException(ex);

			if (IsErrorHandling) return;
			IsErrorHandling = true;

			var crashLogFolder = Environment.ExpandEnvironmentVariables(@"%AppData%\Articulate\Crashes");

			if (!Directory.Exists(crashLogFolder))
				Directory.CreateDirectory(crashLogFolder);
			using (StreamWriter sw = new StreamWriter(Path.Combine(crashLogFolder, "Crash - " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt")))
			{
				sw.WriteLine("Articulate Application Crash");
				sw.WriteLine("OS: " + Environment.OSVersion.VersionString);
				sw.WriteLine("64-Bit: " + (IntPtr.Size == 8 ? "Yes" : "No"));
				sw.WriteLine(".NET Version: " + Environment.Version.ToString());
				sw.WriteLine("Command Line: " + Environment.CommandLine);
				sw.WriteLine();
				sw.Flush();
				sw.AutoFlush = true;

				WriteError(sw, ex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static void WriteError(StreamWriter sw, Exception ex)
		{

			if (ex == null)
				return;
			try
			{
				sw.WriteLine("Exception Message");
				sw.WriteLine(ex.Message);
				sw.WriteLine();

				if (ex.TargetSite != null)
				{
					sw.WriteLine("Exception Target Site");
					sw.WriteLine(ex.TargetSite.DeclaringType.FullName + " - " + ex.TargetSite.Name);
				}

				sw.WriteLine();
				sw.WriteLine("Stack Trace");
				sw.WriteLine(ex.StackTrace);
			}
			catch
			{ }

			if (ex.InnerException != null)
			{
				sw.WriteLine();
				sw.WriteLine();
				sw.WriteLine("INNER EXCEPTION:");
				WriteError(sw, ex.InnerException);
			}
		}

        
    }
}
