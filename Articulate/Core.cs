using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articulate
{
	public class Core : IDisposable
	{
		private Core()
		{
			Configuration = Settings.Load();
			Keybinder = new KeyMonitor(Configuration);
			Recognizer = new VoiceRecognizer();
		}

		#region Singleton

		private static Core _Instance = null;

		public static Core Instance
		{
			get
			{
				return _Instance = (_Instance ?? new Core());
			}
		}

		#endregion

		#region Public Properties

		public Settings Configuration
		{ get; private set; }

		public KeyMonitor Keybinder
		{ get; private set; }

		public VoiceRecognizer Recognizer
		{ get; private set; }

		#endregion

		public void Dispose()
		{
			if (Keybinder != null)
			{
				Keybinder.Dispose();
				Keybinder = null;
			}

			if (Recognizer != null)
			{
				Recognizer.Dispose();
				Recognizer.Dispose();
			}
		}
	}
}
