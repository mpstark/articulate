using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Articulate
{
	/// <summary>
	/// Listens for events from other parts of the application and plays
	/// user-configured sounds in response.
	/// </summary>
	public class SoundEffectsPlayer
	{

		public enum SoundEffects
		{
			CommandAccepted,
			CommandRejected,
			StartListening,
			StopListening,
		}

		#region Private Members

		private IDictionary<SoundEffects, SoundPlayer> m_SoundEffectPlayers = new SortedDictionary<SoundEffects, SoundPlayer>();

		private static object PlaybackLock = new object();

		/// <summary>
		/// Time in milliseconds to wait when trying to play a sound.
		/// </summary>
		private const int C_PLAYBACK_TIMEOUT = 1000;

		#endregion

		#region Public Members

		public string SoundFolder
		{ get; set; }

		#endregion

		#region Public Methods

		public SoundEffectsPlayer(Settings configuration)
		{
			this.SoundFolder = configuration.SoundPackFolder;

			string parentFolder = Environment.ExpandEnvironmentVariables(this.SoundFolder);
			this.m_SoundEffectPlayers.Add(SoundEffects.CommandAccepted, new SoundPlayer(Path.Combine(parentFolder, "command_accepted.wav")));
			this.m_SoundEffectPlayers.Add(SoundEffects.CommandRejected, new SoundPlayer(Path.Combine(parentFolder, "command_rejected.wav")));
			this.m_SoundEffectPlayers.Add(SoundEffects.StartListening, new SoundPlayer(Path.Combine(parentFolder, "started_listening.wav")));
			this.m_SoundEffectPlayers.Add(SoundEffects.StopListening, new SoundPlayer(Path.Combine(parentFolder, "stopped_listening.wav")));
		}

		public void CommandAccepted(object sender, CommandDetectedEventArgs eventArgs)
		{
			this.PlaySound(SoundEffects.CommandAccepted);
		}

		public void CommandRejected(object sender, CommandDetectedEventArgs eventArgs)
		{
			this.PlaySound(SoundEffects.CommandRejected);
		}

		public void StartedListening(object sender, EventArgs eventArgs)
		{
			this.PlaySound(SoundEffects.StartListening);
		}

		public void StoppedListening(object sender, EventArgs eventArgs)
		{
			this.PlaySound(SoundEffects.StopListening);
		}

		#endregion

		#region Private Methods

		private void PlaySound(SoundEffects sound)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.PlaySoundThread), sound);
		}

		/// <summary>
		/// This method attempts to play the requested sound, but gives up if the
		/// timeout elapses.
		/// </summary>
		/// <param name="state">An instance of the SoundEffects enum.</param>
		private void PlaySoundThread(object state)
		{
			bool gotLock = false;

			try
			{
				gotLock = Monitor.TryEnter(PlaybackLock, C_PLAYBACK_TIMEOUT);
				if (gotLock)
				{
					SoundEffects sound = (SoundEffects)state;
					this.m_SoundEffectPlayers[sound].PlaySync();
				}
			}
			finally
			{
				if (gotLock)
				{
					Monitor.PulseAll(PlaybackLock);
					Monitor.Exit(PlaybackLock);
				}
			}
		}

		#endregion

	}
}
