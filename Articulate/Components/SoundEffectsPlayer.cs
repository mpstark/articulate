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

		public enum EffectMode
		{
			None,
			Default,
			Files,
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
		{ get; private set; }

		public EffectMode Mode
		{ get; private set; }

		#endregion

		#region Public Methods

		public SoundEffectsPlayer(Settings configuration)
		{
			this.ChangeSource(configuration.SoundEffectMode, configuration.SoundEffectFolder);
		}

		/// <summary>
		/// Changes which sound effects are played.
		/// </summary>
		/// <param name="mode">The sound effect set to play (must be None or Default).</param>
		public void ChangeSource(EffectMode mode)
		{
			this.ChangeSource(mode, null);
		}

		/// <summary>
		/// Changes which sound effects are played.
		/// </summary>
		/// <param name="mode">The sound effect set to play.</param>
		/// <param name="path">If Set is Files, path indicates the directory containing the sound files.</param>
		public void ChangeSource(EffectMode mode, string path)
		{
			this.Mode = mode;

			if (mode == EffectMode.Files)
			{
				if(Directory.Exists(path))
				{
					this.SoundFolder = path;
				}
				else
				{
					this.SoundFolder = String.Empty;
					this.Mode = EffectMode.None;
				}
				
			}

			this.CreatePlayers();
		}

		/// <summary>
		/// Event handler called when a command is accepted.
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="eventArgs">Arguments</param>
		public void CommandAccepted(object sender, CommandDetectedEventArgs eventArgs)
		{
			if (this.Mode != EffectMode.None)
			{
				this.PlaySound(SoundEffects.CommandAccepted);
			}
		}

		/// <summary>
		/// Event handler called when a command is rejected.
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="eventArgs">Arguments</param>
		public void CommandRejected(object sender, CommandDetectedEventArgs eventArgs)
		{
			if (this.Mode != EffectMode.None)
			{
				this.PlaySound(SoundEffects.CommandRejected);
			}
		}

		/// <summary>
		/// Event handler called when the application starts listening.
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="eventArgs">Arguments</param>
		public void StartedListening(object sender, EventArgs eventArgs)
		{
			if (this.Mode != EffectMode.None)
			{
				this.PlaySound(SoundEffects.StartListening);
			}
		}

		/// <summary>
		/// Event handler called when the application stops listening.
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="eventArgs">Arguments</param>
		public void StoppedListening(object sender, EventArgs eventArgs)
		{
			if (this.Mode != EffectMode.None)
			{
				this.PlaySound(SoundEffects.StopListening);
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Plays the provided sound effect in a new thread.
		/// </summary>
		/// <param name="sound">The sound to be played.</param>
		private void PlaySound(SoundEffects sound)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.PlaySoundThread), sound);
		}

		/// <summary>
		/// This method attempts to play the requested sound, but gives up if the
		/// timeout expires.
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

					// Only try to play the sound if it exists.
					// If we're using a sound pack, missing files will cause the sound to be disabled.
					if(this.m_SoundEffectPlayers.ContainsKey(sound))
					{
						this.m_SoundEffectPlayers[sound].PlaySync();
					}
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

		/// <summary>
		/// Creates sound players based on the current sound source settings.
		/// </summary>
		private void CreatePlayers()
		{
			try
			{
				this.DestroyPlayers();

				Monitor.Enter(PlaybackLock);

				switch (this.Mode)
				{
					case EffectMode.Default:
					{
						this.m_SoundEffectPlayers.Add(SoundEffects.CommandAccepted, new SoundPlayer(Resources.SoundEffects.SoundEffects.command_accepted));
						this.m_SoundEffectPlayers.Add(SoundEffects.CommandRejected, new SoundPlayer(Resources.SoundEffects.SoundEffects.command_rejected));
						this.m_SoundEffectPlayers.Add(SoundEffects.StartListening, new SoundPlayer(Resources.SoundEffects.SoundEffects.started_listening));
						this.m_SoundEffectPlayers.Add(SoundEffects.StopListening, new SoundPlayer(Resources.SoundEffects.SoundEffects.stopped_listening));

						break;
					}

					case EffectMode.Files:
					{
						string parentFolder = Environment.ExpandEnvironmentVariables(this.SoundFolder);
						string commandAcceptedPath = Path.Combine(parentFolder, "command_accepted.wav");
						string commandRejectedPath = Path.Combine(parentFolder, "command_rejected.wav");
						string startedListeningPath = Path.Combine(parentFolder, "started_listening.wav");
						string stoppedListeningPath = Path.Combine(parentFolder, "stopped_listening.wav");

						if(File.Exists(commandAcceptedPath))
						{
							this.m_SoundEffectPlayers.Add(SoundEffects.CommandAccepted, new SoundPlayer(commandAcceptedPath));
						}

						if (File.Exists(commandRejectedPath))
						{
							this.m_SoundEffectPlayers.Add(SoundEffects.CommandRejected, new SoundPlayer(commandRejectedPath));
						}

						if(File.Exists(startedListeningPath))
						{
							this.m_SoundEffectPlayers.Add(SoundEffects.StartListening, new SoundPlayer(startedListeningPath));
						}
						
						if(File.Exists(stoppedListeningPath))
						{
							this.m_SoundEffectPlayers.Add(SoundEffects.StopListening, new SoundPlayer(stoppedListeningPath));
						}

						break;
					}

					case EffectMode.None:
					default:
					{
						// Do nothing
						break;
					}
				}
			}
			finally
			{
				Monitor.PulseAll(PlaybackLock);
				Monitor.Exit(PlaybackLock);
			}
		}

		/// <summary>
		/// Frees resources used by any existing sound players.
		/// </summary>
		private void DestroyPlayers()
		{
			try
			{
				Monitor.Enter(PlaybackLock);

				foreach (KeyValuePair<SoundEffects, SoundPlayer> playerPair in this.m_SoundEffectPlayers)
				{
					playerPair.Value.Dispose();
				}

				this.m_SoundEffectPlayers.Clear();
			}
			finally
			{
				Monitor.PulseAll(PlaybackLock);
				Monitor.Exit(PlaybackLock);
			}
		}

		#endregion

	}
}
