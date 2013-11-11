﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Globalization;
using System.Diagnostics;
using System.Threading;

namespace Articulate
{
	#region CommandDetectedEvent Declarations
	/// <summary>
	/// Event args for CommandDetectedEventHandler type events.
	/// </summary>
	public class CommandDetectedEventArgs : EventArgs
	{
		public CommandDetectedEventArgs(string phrase, float confidence)
			: base()
		{
			Phrase = phrase;
			Confidence = confidence;
		}

		/// <summary>
		/// The phrase that was recognized.
		/// </summary>
		public string Phrase { get; private set; }

		/// <summary>
		/// The confidence that the phrase was recognized at.
		/// </summary>
		public float Confidence { get; private set; }
	}
	#endregion

	/// <summary>
	/// Object that listens for user voice input (as setup by the CommandPool) and then passes the execution to CommandPool.
	/// </summary>
	public class VoiceRecognizer : IDisposable
	{
		#region Private Members

		/// <summary>
		/// Microsoft's in process speech recognition.
		/// </summary>
		private SpeechRecognitionEngine Engine { get; set; }

		/// <summary>
		/// A lock for Condfidence so that only a single thread can modify it at once
		/// </summary>
		private Object ConfidenceLock;

		/// <summary>
		/// Signaled when the RecognizeCompleted event happens
		/// </summary>
		private AutoResetEvent EngineShuttingDown;

		#endregion

		#region Public Members

		/// <summary>
		/// A list of executable names that will be monitored
		/// </summary>
		public List<string> MonitoredExecutables = new List<string>();

		/// <summary>
		/// Enum that exposes the state of the VoiceRecognizer
		/// </summary>
		public enum VoiceRecognizerState
		{
			Error,
			Listening,
			ListeningOnce,
			Paused,
			Pausing,
		}

		/// <summary>
		/// Ugly way of providing VoiceRecognizer status
		/// </summary>
		public VoiceRecognizerState State
		{
			get;
			private set;
		}

		/// <summary>
		/// Ugly way of providing error reporting
		/// </summary>
		public string SetupError
		{
			get;
			private set;
		}

		/// <summary>
		/// The level at which speech is rejected.
		/// </summary>
		public int ConfidenceMargin
		{
			get { return Engine != null ? (int)Engine.QueryRecognizerSetting("CFGConfidenceRejectionThreshold") : 90; }
			set
			{
				if (Engine != null)
				{
					ChangeConfidence(value);
				}
			}
		}

		/// <summary>
		/// Gets or Sets the amount of time in milliseconds after which a command will be assumed to be completed
		/// </summary>
		public int EndSilenceTimeout
		{
			get
			{
				return Engine != null ? (int)Engine.EndSilenceTimeout.TotalMilliseconds : 0;
			}
			set
			{
				if (Engine == null) return;

				Engine.EndSilenceTimeout = TimeSpan.FromMilliseconds(value);
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Fired when a phrase is recognized and accepted
		/// </summary>
		public event EventHandler<CommandDetectedEventArgs> CommandAccepted;

		/// <summary>
		/// Fired when a phrase is recognized and rejected or simply rejected
		/// </summary>
		public event EventHandler<CommandDetectedEventArgs> CommandRejected;

		/// <summary>
		/// Fired when the recognizer starts listening for commands.
		/// </summary>
		public event EventHandler StartedListening;

		/// <summary>
		/// Fired when the recognizer stops listening for commands.
		/// </summary>
		public event EventHandler StoppedListening;

		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor. Sets up the voice recognizer with default settings.
		/// 
		/// Namely, default options are: en-US, default input device, listen always, confidence level at .90
		/// </summary>
		public VoiceRecognizer()
		{
			if (SpeechRecognitionEngine.InstalledRecognizers().Count == 0)
			{
				SetupError = "You don't appear to have any Microsoft Speech Recognizer packs installed on your system.";
				State = VoiceRecognizerState.Error;
				return;
			}

			try
			{
				var installedRecognizers = SpeechRecognitionEngine.InstalledRecognizers();
				RecognizerInfo speechRecognizer = null;

				switch (CultureInfo.InstalledUICulture.Name)
				{
					case "en-GB":
						speechRecognizer = (installedRecognizers.FirstOrDefault(x => x.Culture.Name == "en-GB") ?? installedRecognizers.FirstOrDefault(x => x.Culture.TwoLetterISOLanguageName == "en"));
						break;

					case "en-US":
					default:
						speechRecognizer = installedRecognizers.FirstOrDefault(x => x.Culture.TwoLetterISOLanguageName == "en");
						break;
				}


				if (speechRecognizer == null)
				{
					SetupError = String.Format("You don't appear to have the {0} Speech Recognizer installed. Articulate requires this recognizer to be present in order to function correctly.", "English");
					State = VoiceRecognizerState.Error;
					return;
				}

				// Setup members
				ConfidenceLock = new Object();
				EngineShuttingDown = new AutoResetEvent(false);
				State = VoiceRecognizerState.Paused;

				// Create a new SpeechRecognitionEngine instance.
				Engine = new SpeechRecognitionEngine(speechRecognizer);

				try
				{
					// Setup the audio device
					Engine.SetInputToDefaultAudioDevice();
				}
				catch (InvalidOperationException ex)
				{
					// No default input device
					Trace.WriteLine(ex.Message);
					SetupError = "Check input device.\n\n";
					State = VoiceRecognizerState.Error;
					return;
				}

				// Set the confidence setting
				ConfidenceMargin = 90;

				// Create the Grammar instance and load it into the speech recognition engine.
				Grammar g = new Grammar(CommandPool.BuildSrgsGrammar(speechRecognizer.Culture));
				Engine.LoadGrammar(g);

				// Register a handlers for the SpeechRecognized and SpeechRecognitionRejected event
				Engine.SpeechRecognized += sre_SpeechRecognized;
				Engine.SpeechRecognitionRejected += sre_SpeechRecognitionRejected;
				Engine.RecognizeCompleted += sre_RecognizeCompleted;

				StartListening();
			}
			catch (Exception ex)
			{
				// Something went wrong setting up the voiceEngine.
				Trace.WriteLine(ex.Message);
				SetupError = String.Format("{0}\nCurrent Culture: {1}\nAvailable Recognizers: {2}\nStack Trace:\n{3}", ex.Message, CultureInfo.InstalledUICulture.Name, SpeechRecognitionEngine.InstalledRecognizers().Select(x => x.Culture.Name).Aggregate((x, y) => x + ", " + y), ex.StackTrace);
				State = VoiceRecognizerState.Error;
			}
		}
		#endregion

		#region Private Methods
		private void ChangeConfidence(int value)
		{
			lock (ConfidenceLock)
			{
				Engine.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", value);
			}
		}

		/// <summary>
		/// Triggers the PhraseRecognizedEvent if it has any subscribers
		/// </summary>
		/// <param name="phrase">The phrase that was recognized</param>
		/// <param name="confidence">The confidence that the phrase was recognized with.</param>
		private void TriggerCommandAccepted(string phrase, float confidence)
		{
			// if we have subscribers, trigger the event
			if (CommandAccepted != null)
			{
				CommandAccepted(this, new CommandDetectedEventArgs(phrase, confidence));
			}
		}

		/// <summary>
		/// Triggers the PhraseRecognizedEvent if it has any subscribers
		/// </summary>
		/// <param name="phrase">The phrase that was rejected (best match)</param>
		/// <param name="confidence">The confidence that the phrase was rejected with.</param>
		private void TriggerCommandRejected(string phrase, float confidence)
		{
			// if we have subscribers, trigger the event
			if (CommandRejected != null)
			{
				CommandRejected(this, new CommandDetectedEventArgs(phrase, confidence));
			}
		}

		/// <summary>
		/// Triggers the StartedListening event if it has any subscribers.
		/// </summary>
		private void TriggerStartedListening()
		{
			if (this.StartedListening != null)
			{
				this.StartedListening(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Triggers the StoppedListening event if it has any subscribers.
		/// </summary>
		private void TriggerStoppedListening()
		{
			if (this.StoppedListening != null)
			{
				this.StoppedListening(this, EventArgs.Empty);
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Starts Async listening.
		/// </summary>
		public void StartListening()
		{
			// if pausing, block until it is paused
			if (State == VoiceRecognizerState.Pausing)
				EngineShuttingDown.WaitOne();

			if (Engine != null && State == VoiceRecognizerState.Paused)
			{
				// Start listening in multiple mode (that is, don't quit after a single recongition)
				Engine.RecognizeAsync(RecognizeMode.Multiple);
				State = VoiceRecognizerState.Listening;
				TriggerStartedListening();
			}
		}

		/// <summary>
		/// Stops Async listening gracefully.
		/// </summary>
		public void StopListening()
		{
			if (Engine != null && (State == VoiceRecognizerState.Listening || State == VoiceRecognizerState.ListeningOnce))
			{
				// Stop listening gracefully
				Engine.RecognizeAsyncStop();
				State = VoiceRecognizerState.Pausing;
				TriggerStoppedListening();
			}
		}

		/// <summary>
		/// Stops Async listening immediately.
		/// </summary>
		public void CancelListening()
		{
			if (Engine != null && (State == VoiceRecognizerState.Listening || State == VoiceRecognizerState.ListeningOnce))
			{
				Engine.RecognizeAsyncCancel();
				State = VoiceRecognizerState.Pausing;
				TriggerStoppedListening();
			}
		}

		/// <summary>
		/// Listens for a single utterance and then stops
		/// </summary>
		public void ListenOnce()
		{
			// if pausing, block until it is paused
			if (State == VoiceRecognizerState.Pausing)
				EngineShuttingDown.WaitOne();

			if (Engine != null && State == VoiceRecognizerState.Paused)
			{
				// only listen for a single utterance
				Engine.RecognizeAsync(RecognizeMode.Single);
				State = VoiceRecognizerState.ListeningOnce;
				TriggerStartedListening();
			}
		}
		#endregion

		#region SpeechRecognitionEngine Events
		/// <summary>
		/// Some speech was recognized by the voiceEngine.
		/// </summary>
		private void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs recognizedPhrase)
		{
			// Trigger the event for command recoginized 
			TriggerCommandAccepted(recognizedPhrase.Result.Words.Aggregate("", (phraseSoFar, word) => phraseSoFar + word.Text + " "), recognizedPhrase.Result.Confidence);

			// Makes sure that the active application is valid for input
			var activeApplication = ForegroundProcess.ExecutableName;

			if (!MonitoredExecutables.Any(x => x.Equals(activeApplication, StringComparison.OrdinalIgnoreCase)))
			{
				Trace.WriteLine(string.Format("Skipping command, {0} is not in the list of monitored applications", activeApplication));
				return;
			}

			// Get a thread from the thread pool to execute the command
			CommandPool.ExecuteAsync(recognizedPhrase.Result.Semantics);
		}

		/// <summary>
		/// Some speech was rejected by the voiceEngine
		/// </summary>
		private void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs recognizedPhrase)
		{
			TriggerCommandRejected(recognizedPhrase.Result.Words.Aggregate("", (phraseSoFar, word) => phraseSoFar + word.Text + " "), recognizedPhrase.Result.Confidence);
		}

		/// <summary>
		/// An async recognize has completed.
		/// </summary>
		private void sre_RecognizeCompleted(object sender, RecognizeCompletedEventArgs args)
		{
			State = VoiceRecognizerState.Paused;

			// Signal that async recognize has completed
			EngineShuttingDown.Set();
		}

		#endregion

		#region IDispose Implementation
		/// <summary>
		/// Cleanup before destoying the object.
		/// </summary>
		public void Dispose()
		{
			// make sure that the voiceEngine is turned off
			Engine.RecognizeAsyncCancel();

			// dispose of the voiceEngine
			Engine.Dispose();
			Engine = null;
		}
		#endregion
	}
}
