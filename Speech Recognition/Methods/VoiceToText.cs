using System;
using System.Configuration;
using System.Threading;
using System.Windows;
using Microsoft.CognitiveServices.SpeechRecognition;

namespace SpeechRecognition.Methods
{
    public class VoiceToText
    {
        public AutoResetEvent FinalResponceEvent { get; set; }//cap off the final responce event
        public MicrophoneRecognitionClient MicrophoneRecognitionClient { get; set; }//perform mic recognition on the client
        public bool Recording { get; set; }// perform check on the current mode of the Voice to text engine

        public VoiceToText()
        {
            FinalResponceEvent = new AutoResetEvent(false);
            Recording = false;
        }

        public void StartRecording()
        {
            var speechRecognitionMode = SpeechRecognitionMode.LongDictation;
            var language = "en-us";
            var subkey = ConfigurationManager.AppSettings["MicosoftSpeechAPIKey"];
            MicrophoneRecognitionClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient(
                speechRecognitionMode,language,subkey);
        }

        public void StopRecording()
        {
            try
            {
                FinalResponceEvent.Set();
                MicrophoneRecognitionClient.EndMicAndRecognition();
                MicrophoneRecognitionClient.Dispose();
                MicrophoneRecognitionClient = null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
