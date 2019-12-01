using System;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BioInfo_Terminal.Data;
using BioInfo_Terminal.Methods.Dialog_Handling;
using BioInfo_Terminal.Methods.Messaging;
using BioInfo_Terminal.Methods.Updater;
using log4net;
using Microsoft.CognitiveServices.SpeechRecognition;
using SpeechRecognition.Methods;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Message = BioInfo_Terminal.Methods.Messaging.Message;

namespace BioInfo_Terminal.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IBioInfoUserIo
    {
        private static readonly ILog UiLog = LogManager.GetLogger("BioInfo.UI");
        private readonly DialogHandler _dialogHandler; // sends the response to the user- Main AI Interaction 
        private readonly MessageSide _messageSide; //messenger side that text goes on

        private readonly DoubleAnimation _scrollViewerScrollToEndAnim; //animation for moving through story board     
        private readonly Storyboard _scrollViewerStoryboard; //board where all messages are displayed
        private readonly BioInfoUpdater _updater; //update the project to its latest version 
        private SpeechSynthesizer _bioInfoVoice; //the vocal library output source  
        private readonly VoiceToText _voiceToText; //Speech recognition from cognative services 

        public MainWindow()
        {
            UiLog.Info("bio info UI started");
            InitializeComponent();
            _messageSide = MessageSide.BioInfoSide;
            Messages = new MessageCollection
            {
                new Message
                {
                    Side = MessageSide.BioInfoSide,
                    Text = "Welcome to the B,M,S, Bio Info A,I Terminal. How may I help you today?"
                }
            };

            _updater = new BioInfoUpdater(); // version checks & updates
            _dialogHandler = new DialogHandler(this); // DialogHandler
            _bioInfoVoice = new SpeechSynthesizer();
            _voiceToText = new VoiceToText();

            DataContext = Messages;
            _scrollViewerScrollToEndAnim = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(_scrollViewerScrollToEndAnim, this);
            Storyboard.SetTargetProperty(_scrollViewerScrollToEndAnim, new PropertyPath(_verticalOffsetProperty));

            _scrollViewerStoryboard = new Storyboard();
            _scrollViewerStoryboard.Children.Add(_scrollViewerScrollToEndAnim);
            Resources.Add("foo", _scrollViewerStoryboard);
            tbTextInput.Focus();
        }

        internal MessageCollection Messages { get; set; } // message's

        #region UI Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _dialogHandler.SpeakToUser("Welcome to the B,M,S, Bio Info A,I Terminal. How may I help you today?");
        }

        private void btnAudio_Click(object sender, RoutedEventArgs e)
        {
            _voiceToText.Recording = !_voiceToText.Recording;
            if (_voiceToText.Recording)
            {
                _voiceToText.StartRecording();
                _voiceToText.MicrophoneRecognitionClient.OnPartialResponseReceived += ResponceRecived;
                _voiceToText.MicrophoneRecognitionClient.StartMicAndRecognition();
            }
            else
            {
                Dispatcher.Invoke(() => { _voiceToText.StopRecording(); });
                SendTextQuery();
            }
        }

        private void ResponceRecived(object sender, PartialSpeechResponseEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    if (e.PartialResult.Contains("start dictation"))
                    {
                        ((IBioInfoUserIo) this).OpenEmailDictation(true);
                        _voiceToText.StopRecording();
                        tbTextInput.Text = "";
                        return;
                    }
                    tbTextInput.Text = e.PartialResult;
                    tbTextInput.Text += "\n";
                }
            );
        }

        private void TextInput_GotFocus(object sender, RoutedEventArgs e)
        {
            ScrollConversationToEnd();
        }

        private void TextInput_LostFocus(object sender, RoutedEventArgs e)
        {
            ScrollConversationToEnd();
        }

        private void TextInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendTextQuery();
                e.Handled = true;
            }
        }

        private void MiLoad_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _dialogHandler.LanguageProcessor.LoadCkb(openFileDialog1.FileName);
        }

        private void MiSave_Click(object sender, RoutedEventArgs e)
        {
            _dialogHandler.LanguageProcessor.SaveCkb();
        }

        private void MiAdd_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _dialogHandler.LanguageProcessor.AddCkb(openFileDialog1.FileName);
        }

        private void MiExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MiUpdate_Click(object sender, RoutedEventArgs e)
        {
            var update = new Update(_updater);
            update.ShowDialog();
        }

        private void MiFeedback_Click(object sender, RoutedEventArgs e)
        {
            var feedback = new Feedback();
            feedback.ShowDialog();
        }

        private void MiDictateEmail_Click(object sender, RoutedEventArgs e)
        {
            ((IBioInfoUserIo) this).OpenEmailDictation(false);
        }

        private void MiMute_Click(object sender, RoutedEventArgs e)
        {
            if (_bioInfoVoice.Volume == 0)
            {
                _bioInfoVoice.Volume = 100;
                MiMute.Header = "Mute";
            }
            else
            {
                _bioInfoVoice.Volume = 0;
                MiMute.Header = "Un-mute";
                ((IBioInfoUserIo) this).SetSpeechState("pause");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveTextToFile sttf = new SaveTextToFile();
            sttf.SaveFequencyDictionary();
        }

        #endregion

        #region Methods 

        private readonly DependencyProperty _verticalOffsetProperty = DependencyProperty.Register("VerticalOffset",
            typeof(double), typeof(MainWindow), new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MainWindow app) app.OnVerticalOffsetChanged(e);
        }

        private void OnVerticalOffsetChanged(DependencyPropertyChangedEventArgs e)
        {
            ConversationScrollViewer.ScrollToVerticalOffset((double) e.NewValue);
        }

        private void ScrollConversationToEnd()
        {
            _scrollViewerScrollToEndAnim.From = ConversationScrollViewer.VerticalOffset;
            _scrollViewerScrollToEndAnim.To = ConversationContentContainer.ActualHeight;
            _scrollViewerStoryboard.Begin();
        }

        internal void SpeakToUser(string text)
        {
            new Thread(() =>
            {
                try
                {
                    Thread.CurrentThread.IsBackground = true;
                    _bioInfoVoice.Speak(text);
                }
                catch
                {
                    Thread.CurrentThread.Abort();
                }
            }).Start();
        }

        private void SendTextQuery()
        {
            ((IBioInfoUserIo) this).AddTextBioInfo(tbTextInput.Text);
            SaveTextToFile sttf = new SaveTextToFile();
            sttf.SaveText(tbTextInput.Text);
            ScrollConversationToEnd();
            tbTextInput.Text = "";
            tbTextInput.Focus();
        }

        #region Interface methods userIO

        void IBioInfoUserIo.SpeakToUser(string text)
        {
            SpeakToUser(text);
        }

        void IBioInfoUserIo.OpenEmailDictation(bool record)
        {
            var dictation = new DictationEmail();
            if (record) dictation = new DictationEmail(true);
            dictation.ShowDialog();
        }

        void IBioInfoUserIo.SetSpeechState(string command)
        {
            switch (command)
            {
                case "pause":
                    _bioInfoVoice.Pause();
                    break;
                case "continue":
                    _bioInfoVoice.Resume();
                    break;
                case "reset":
                    _bioInfoVoice = new SpeechSynthesizer();
                    break;
            }
        }

        bool IBioInfoUserIo.GetSpeechIsPaused()
        {
            if (_bioInfoVoice.State == SynthesizerState.Paused) return true;
            return false;
        }

        void IBioInfoUserIo.AddTextBioInfo(string text)
        {
            Messages.Add(new Message
            {
                Side = MessageSide.UserSide,
                Text = text,
                PrevSide = _messageSide
            });

            //send response after message from user and speak response
            var speech = _dialogHandler.HandleUnstructuredInput(text);

            if (speech == "null") return;
            ((IBioInfoUserIo) this).AddTextUser(speech);
            SpeakToUser(speech);
        }

        string IBioInfoUserIo.SetVoiceOptions(string text)
        {
            try
            {
                if (text == "options")
                {
                    var voices = string.Empty;
                    var i = 1;
                    foreach (var voice in _bioInfoVoice.GetInstalledVoices())
                    {
                        voices += i + " Voice Name: " + voice.VoiceInfo.Name + ", ";
                        i++;
                    }

                    return voices;
                }

                text = text.First().ToString().ToUpper() + text.Substring(1);
                _bioInfoVoice.SelectVoice("Microsoft " + text + " Desktop");
                var response = " Voice set to " + text +
                               ", - testing -, Welcome to Bio Info AI Terminal How can i help you";
                return response;
            }
            catch (Exception ex)
            {
                return "Error:  " + ex.Message;
            }
        }

        void IBioInfoUserIo.AddTextUser(string text)
        {
            Messages.Add(new Message
            {
                Side = MessageSide.BioInfoSide,
                Text = text,
                PrevSide = _messageSide
            });
        }

        void IBioInfoUserIo.ClearMessages()
        {
            Messages.Clear();
        }

        #endregion

        #endregion
    }

    public interface IBioInfoUserIo
    {
        string SetVoiceOptions(string text);
        void SpeakToUser(string text);
        void SetSpeechState(string command);
        bool GetSpeechIsPaused();
        void AddTextBioInfo(string text);
        void AddTextUser(string text);
        void ClearMessages();
        void OpenEmailDictation(bool record);
    }
}