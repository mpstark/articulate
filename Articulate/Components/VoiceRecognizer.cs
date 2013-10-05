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
		public bool IsSetup 
		{
			get;
			private set;
		}

		public bool Enabled
		{
			get { return Engine != null && Engine.AudioState != AudioState.Stopped; }
			set
			{
				if (Engine == null) return;
				
				if (!value && Engine.AudioState != AudioState.Stopped) Engine.RecognizeAsyncStop();
				else if (value && Engine.AudioState == AudioState.Stopped) Engine.RecognizeAsync();
			}
		}

		RecognizeMode recognizeMode = RecognizeMode.Multiple;
		public RecognizeMode Mode
		{
			get { return recognizeMode; }
			set
			{
				if (Engine.AudioState != AudioState.Stopped) throw new InvalidOperationException("Cannot change the recognition mode while the recognizer is active, please stop the recognizer before changing the mode.");

				recognizeMode = value;
			}
		}

		public SpeechRecognitionEngine Engine { get; private set; }

		public List<string> MonitoredExecutables = new List<string>();
		
		public int ConfidenceMargin
		{
			get { return Engine != null ? (int)Engine.QueryRecognizerSetting("CFGConfidenceRejectionThreshold") : 90; }
			set
			{
				if(Engine != null)
					Engine.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", value);
			}
		}

		/// <summary>
		/// Ugly way of providing exception message.
		/// </summary>
		public string SetupError
		{
			get;
			private set;
		}

		/// <summary>
		/// Default constructor. Sets up the voice recognizer with default settings.
		/// 
		/// Namely, default options are: en-US, default input device, listen always, confidence level at .90
		/// </summary>
		public VoiceRecognizer()
		{
			try
			{
				// Create a new SpeechRecognitionEngine instance.
				Engine = new SpeechRecognitionEngine(new CultureInfo("en-US"));

				// Setup the audio device
				Engine.SetInputToDefaultAudioDevice();

				// Set the confidence setting
				ConfidenceMargin = 75;
			
				// Create the Grammar instance and load it into the speech recognition engine.
				Grammar g = new Grammar(CommandPool.BuildSrgsGrammar());
				Engine.LoadGrammar(g);
				
				// Register a handler for the SpeechRecognized event
				Engine.SpeechRecognized += sre_SpeechRecognized;
				Engine.SpeechRecognitionRejected += sre_SpeechRecognitionRejected;
				
				IsSetup = true;
			}
			catch(Exception e)
			{
				// Something went wrong setting up the voiceEngine.
				Trace.WriteLine(e.Message);
				SetupError = e.ToString();
				IsSetup = false;
			}
		}

		// TODO: add constructor with options

		/// <summary>
		/// Some speech was recognized by the voiceEngine.
		/// </summary>
		void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs recognizedPhrase)
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
		void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs recognizedPhrase)
		{
			Trace.WriteLine("Rejected with confidence: " + recognizedPhrase.Result.Confidence);
		}

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
	}
}
