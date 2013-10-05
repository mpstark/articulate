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
    class VoiceRecognizer : IDisposable
    {
        public bool IsSetup 
		{
			get;
			private set;
		}

		public SpeechRecognitionEngine Engine { get; private set; }

		public double ConfidenceMargin
		{ get; set; }

        public VoiceRecognizer()
		{
			ConfidenceMargin = 0.85;

            try
            {
                // Create a new SpeechRecognitionEngine instance.
                Engine = new SpeechRecognitionEngine(new CultureInfo("en-US"));

                // Setup the audio device
                Engine.SetInputToDefaultAudioDevice();
            
                // Create the Grammar instance and load it into the speech recognition engine.
                Grammar g = new Grammar(CommandPool.BuildSrgsGrammar());
                Engine.LoadGrammar(g);

                //Engine.EndSilenceTimeout = new TimeSpan(0, 0, 1);

                // Register a handler for the SpeechRecognized event
                Engine.SpeechRecognized += sre_SpeechRecognized;

                // Start listening in multiple mode (that is, don't quit after a single recongition)
                Engine.RecognizeAsync(RecognizeMode.Multiple);
                IsSetup = true;
            }
            catch(Exception e)
            {
                IsSetup = false;
            }
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > ConfidenceMargin)
            {
                // Async deal with it
                Task.Factory.StartNew(() => CommandPool.Execute(e.Result.Semantics));
            }
        }

        public void Dispose()
        {
            Engine.RecognizeAsyncCancel();
            Engine.Dispose();
        }
    }
}
