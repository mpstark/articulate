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
        public bool IsSetup = false;
        SpeechRecognitionEngine voiceEngine;

        public VoiceRecognizer()
        {
            try
            {
                // Create a new SpeechRecognitionEngine instance.
                voiceEngine = new SpeechRecognitionEngine(new CultureInfo("en-US"));

                // Setup the audio device
                voiceEngine.SetInputToDefaultAudioDevice();
            
                // Create the Grammar instance and load it into the speech recognition engine.
                Grammar g = new Grammar(CommandPool.BuildSrgsGrammar());
                voiceEngine.LoadGrammar(g);

                //voiceEngine.EndSilenceTimeout = new TimeSpan(0, 0, 1);

                // Register a handler for the SpeechRecognized event
                voiceEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);

                // Start listening in multiple mode (that is, don't quit after a single recongition)
                voiceEngine.RecognizeAsync(RecognizeMode.Multiple);
                IsSetup = true;
            }
            catch(Exception e)
            {
                IsSetup = false;
            }
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > .90)
            {
                // Async deal with it
                Task.Factory.StartNew(() => CommandPool.Execute(e.Result.Semantics));
            }
        }

        public void Dispose()
        {
            voiceEngine.RecognizeAsyncCancel();
            voiceEngine.Dispose();
        }
    }
}
