using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;

namespace Articulate
{
	public enum ListenMode
	{
		Continuous = 0,
		PushToTalk = 1,
		PushToArm = 2,
		PushToIgnore = 3
	}

	[Serializable]
	public class Settings
	{
		public Settings()
		{
			// Initialize default settings
			ConfidenceMargin = 80;

			PTTKey = System.Windows.Forms.Keys.None;
			Mode = ListenMode.Continuous;

			Applications = new List<string>();
		}

		#region File Handling

		public static Settings Load()
		{
			var filePath = Environment.ExpandEnvironmentVariables(@"%AppData%\Articulate\Settings.xml");
			if (!File.Exists(filePath)) return new Settings();

			try
			{
				using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var serializer = new XmlSerializer(typeof(Settings));
					return (Settings)serializer.Deserialize(fs);
				}
			}
			catch (Exception ex)
			{
				Trace.Write(ex.Message);
				return new Settings();
			}
		}

		public void Save()
		{
			var filePath = Environment.ExpandEnvironmentVariables(@"%AppData%\Articulate\Settings.xml");
						
			try
			{
				var parentDirectory = Environment.ExpandEnvironmentVariables(@"%AppData%\Articulate");
				if (!Directory.Exists(parentDirectory))
					Directory.CreateDirectory(parentDirectory);
				
				using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					var serializer = new XmlSerializer(typeof(Settings));
					serializer.Serialize(fs, this);
				}
			}
			catch(Exception ex)
			{
				Trace.Write(ex.Message);
			}
		}

		#endregion

		public int ConfidenceMargin
		{ get; set; }

		public System.Windows.Forms.Keys PTTKey
		{ get; set; }

		public ListenMode Mode
		{ get; set; }
		
		public List<string> Applications
		{ get; set; }
	}
}
