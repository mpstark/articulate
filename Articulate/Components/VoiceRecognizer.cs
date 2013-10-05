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

		public SpeechRecognitionEngine Engine { get; private set; }
		
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
                CultureInfo cultureInfo = new CultureInfo("en-US");

				// Create a new SpeechRecognitionEngine instance.
                		Engine = new SpeechRecognitionEngine(cultureInfo);

				// Setup the audio device
				Engine.SetInputToDefaultAudioDevice();

				// Set the confidence setting
				ConfidenceMargin = 90;
			
				// Create the Grammar instance and load it into the speech recognition engine.
                		Grammar g = new Grammar(CommandPool.BuildSrgsGrammar(cultureInfo));
				Engine.LoadGrammar(g);
				
				// Register a handler for the SpeechRecognized event
				Engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
				Engine.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(sre_SpeechRecognitionRejected);

				// Start listening in multiple mode (that is, don't quit after a single recongition)
				Engine.RecognizeAsync(RecognizeMode.Multiple);
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
