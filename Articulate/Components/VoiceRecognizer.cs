using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Globalization;
using System.Diagnostics;

namespace Articulate
{
	/// <summary>
	/// Object that listens for user voice input (as setup by the CommandPool) and then passes the execution to CommandPool.
	/// </summary>
	class VoiceRecognizer : IDisposable
	{
		#region Private Members
		/// <summary>
		/// Microsoft's in process speech recognition.
		/// </summary>
		private SpeechRecognitionEngine Engine { get; set; }

		private Object ConfidenceLock;
		#endregion

		#region Public Members
		/// <summary>
		/// Enum that exposes the state of the VoiceRecognizer
		/// </summary>
		public enum VoiceRecognizerState
		{
			Error,
			Listening,
			ListeningOnce,
			Paused,
		}

		/// <summary>
		/// Ugly way of providing VoiceRecognizer status
		/// </summary>
		public VoiceRecognizerState State
		{
			get;
			private set;
		}

		public List<string> MonitoredExecutables = new List<string>();
		
		/// <summary>
		/// Ugly way of providing exception message.
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
				if(Engine != null)
				{
					ChangeConfidence(value);

					// TODO: this is not FIFO 
					// roll off a thread to set it
					//Task.Factory.StartNew(() => ChangeConfidence(value));
				}
				else
				{
					Trace.WriteLine("Can't change confidence");
				}
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor. Sets up the voice recognizer with default settings.
		/// 
		/// Namely, default options are: en-US, default input device, listen always, confidence level at .90
		/// </summary>
		public VoiceRecognizer()
		{
			try
			{
				ConfidenceLock = new Object();
				State = VoiceRecognizerState.Paused;
				CultureInfo cultureInfo = new CultureInfo("en-US");

				// Create a new SpeechRecognitionEngine instance.
				Engine = new SpeechRecognitionEngine(cultureInfo);
				
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
				Grammar g = new Grammar(CommandPool.BuildSrgsGrammar(cultureInfo));
				Engine.LoadGrammar(g);

				// Register a handler for the SpeechRecognized event
				Engine.SpeechRecognized += sre_SpeechRecognized;
				Engine.SpeechRecognitionRejected += sre_SpeechRecognitionRejected;

				StartListening();
			}
			catch(Exception ex)
			{
				// Something went wrong setting up the voiceEngine.
				Trace.WriteLine(ex.Message);
				SetupError = ex.ToString();
				State = VoiceRecognizerState.Error;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Starts Async listening.
		/// </summary>
		public void StartListening()
		{
			if (Engine != null && State == VoiceRecognizerState.Paused)
			{
				// Start listening in multiple mode (that is, don't quit after a single recongition)
				Engine.RecognizeAsync(RecognizeMode.Multiple);
				State = VoiceRecognizerState.Listening;
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
				State = VoiceRecognizerState.Paused;
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
				State = VoiceRecognizerState.Paused;
			}
		}

		/// <summary>
		/// Listens for a single utterance and then stops
		/// </summary>
		public void ListenOnce()
		{
			if (Engine != null && State == VoiceRecognizerState.Paused)
			{
				// only listen for a single utterance
				Engine.RecognizeAsync(RecognizeMode.Single);
				State = VoiceRecognizerState.ListeningOnce;
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
		#endregion

		#region SpeechRecognitionEngine Events
		/// <summary>
		/// Some speech was recognized by the voiceEngine.
		/// </summary>
		private void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs recognizedPhrase)
		{
			Trace.WriteLine("Recognized with confidence: " + recognizedPhrase.Result.Confidence);

			var activeApplication = ForegroundProcess.ExecutableName;

			if (!MonitoredExecutables.Any(x => x.Equals(activeApplication, StringComparison.OrdinalIgnoreCase)))
			{
				Trace.WriteLine(string.Format("Skipping command, {0} is not in the list of monitored applications", activeApplication));
				return;
			}

			// Get a thread from the thread pool to deal with it
			Task.Factory.StartNew(() => CommandPool.Execute(recognizedPhrase.Result.Semantics));
		}

		/// <summary>
		/// Some speech was rejected by the voiceEngine
		/// </summary>
		private void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs recognizedPhrase)
		{
			Trace.WriteLine("Rejected with confidence: " + recognizedPhrase.Result.Confidence);
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
