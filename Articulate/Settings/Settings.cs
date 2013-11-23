using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;
using System.Globalization;

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

			EndCommandPause = 500;

			Mode = ListenMode.Continuous;

			Applications = new List<string>();
			KeyBinds = new ObservableCollection<CompoundKeyBind>();

			Language = (Thread.CurrentThread.CurrentUICulture ?? Thread.CurrentThread.CurrentCulture ?? new CultureInfo("en")).Name;

			KeyPressDelay = 75;
			KeyReleaseDelay = 25;
			MouseReleaseDelay = 25;

			FileLock = new object();
		}

		private object FileLock;

		#region File Handling

		public static Settings Load()
		{
			var filePath = Environment.ExpandEnvironmentVariables(@"%AppData%\Articulate\config.dat");
			if (!File.Exists(filePath)) return new Settings();

			try
			{
				using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var serializer = new BinaryFormatter();
					var output = (Settings)serializer.Deserialize(fs);
					output.EnsureRanges();
					return output;
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
			var filePath = Environment.ExpandEnvironmentVariables(@"%AppData%\Articulate\config.dat");

			try
			{
				var parentDirectory = Environment.ExpandEnvironmentVariables(@"%AppData%\Articulate");
				if (!Directory.Exists(parentDirectory))
					Directory.CreateDirectory(parentDirectory);

				ThreadPool.QueueUserWorkItem((state) =>
				{
					lock (FileLock)
					{
						try
						{

							using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
							{
								var serializer = new BinaryFormatter();
								serializer.Serialize(fs, state);
							}
						}
						catch (Exception ex)
						{
							Trace.Write(ex.Message);
						}
					}
				}, this);
			}
			catch
			{

			}

			
		}

		public void EnsureRanges()
		{
			KeyPressDelay = Math.Max(Math.Min(KeyPressDelay, 200), 0);
			KeyReleaseDelay = Math.Max(Math.Min(KeyReleaseDelay, 500), 10);
			MouseReleaseDelay = Math.Max(Math.Min(MouseReleaseDelay, 500), 10);
		}

		#endregion

		public int ConfidenceMargin
		{ get; set; }

		public int EndCommandPause
		{ get; set; }

		public ObservableCollection<CompoundKeyBind> KeyBinds
		{ get; set; }

		public ListenMode Mode
		{ get; set; }
		
		public List<string> Applications
		{ get; set; }

		public string Language
		{ get; set; }

		/// <summary>
		/// The delay between one key's press and its subsequent release
		/// </summary>
		public int KeyReleaseDelay
		{ get; set; }

		/// <summary>
		/// The delay between one key's release and the next key's press
		/// </summary>
		public int KeyPressDelay
		{ get; set; }

		public int MouseReleaseDelay
		{ get; set; }

		public SoundEffectsPlayer.EffectMode SoundEffectMode
		{ get; set; }

		public string SoundEffectFolder
		{ get; set; }
	}
}
