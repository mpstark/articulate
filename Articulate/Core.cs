using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articulate
{
	public class Core : IDisposable
	{
		public Core()
		{
			Configuration = Settings.Load();
			Keybinder = new KeyMonitor(Configuration);
			Recognizer = new VoiceRecognizer();
		}

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
