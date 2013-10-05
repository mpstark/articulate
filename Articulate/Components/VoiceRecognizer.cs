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
        /// <summary>
        /// Ugly way of checking if the voiceEngine was setup.
        /// </summary>
        public bool IsSetup = false;

        /// <summary>
        /// Ugly way of providing exception message.
        /// </summary>
        public string SetupError = "";

        /// <summary>
        /// The voice recognition engine.
        /// </summary>
        private SpeechRecognitionEngine voiceEngine;

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
                voiceEngine = new SpeechRecognitionEngine(cultureInfo);

                // Setup the audio device
                voiceEngine.SetInputToDefaultAudioDevice();

                // Set the confidence setting
                voiceEngine.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 90);
            
                // Create the Grammar instance and load it into the speech recognition engine.
                Grammar g = new Grammar(CommandPool.BuildSrgsGrammar(cultureInfo));
                voiceEngine.LoadGrammar(g);
                
                // Register a handler for the SpeechRecognized event
                voiceEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
                voiceEngine.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(sre_SpeechRecognitionRejected);

                // Start listening in multiple mode (that is, don't quit after a single recongition)
                voiceEngine.RecognizeAsync(RecognizeMode.Multiple);
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
            voiceEngine.RecognizeAsyncCancel();

            // dispose of the voiceEngine
            voiceEngine.Dispose();
            voiceEngine = null;
        }
    }
}
