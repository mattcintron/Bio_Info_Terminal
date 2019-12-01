using System.Windows;
using System.Windows.Media;
using Microsoft.CognitiveServices.SpeechRecognition;
using SpeechRecognition.Methods;

namespace SpeechRecognition
{
    public partial class MainWindow
    {
        private VoiceToText _voiceToText;

        public MainWindow()
        {
            InitializeComponent();
            _voiceToText = new VoiceToText();
            SpeakBtn.Content = "Start Recording";
            ResponceTxt.Background = Brushes.White;
            ResponceTxt.Foreground = Brushes.Black;
        }

        #region UI Events

        private void SpeakBtn_Click(object sender, RoutedEventArgs e)
        {
            SpeakBtn.Content = "Listening...";
            SpeakBtn.IsEnabled = false;
            ResponceTxt.Background = Brushes.Green;
            ResponceTxt.Foreground = Brushes.White;
            ConvertSpeech_ToText();
        }

        private void ResponceRecived(object sender, PartialSpeechResponseEventArgs e)
        {
            var result = e.PartialResult;
            Dispatcher.Invoke(() =>
                {
                    ResponceTxt.Text = e.PartialResult;
                    ResponceTxt.Text += "\n";
                }
            );
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _voiceToText.StopRecording();
                SpeakBtn.Content = "Start/Recording";
                SpeakBtn.IsEnabled = true;
                ResponceTxt.Background = Brushes.White;
                ResponceTxt.Foreground = Brushes.Black;
            });
        }

        #endregion

        private void ConvertSpeech_ToText()
        {
            _voiceToText.StartRecording();
            _voiceToText.MicrophoneRecognitionClient.OnPartialResponseReceived += ResponceRecived;
            _voiceToText.MicrophoneRecognitionClient.StartMicAndRecognition();
        }
    }
}