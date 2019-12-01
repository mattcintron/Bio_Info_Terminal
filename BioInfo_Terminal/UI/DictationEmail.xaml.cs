using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BioInfo_Terminal.Methods.Operations;
using Microsoft.CognitiveServices.SpeechRecognition;
using SpeechRecognition.Methods;

namespace BioInfo_Terminal.UI
{
    public partial class DictationEmail
    {
        private readonly ChemOperations _chemOperations;
        private bool _commandMode;
        private readonly EmailOperations _emailOperations;
        private string _fullResponces;
        private string _partialResponces;
        private TextBox _tbCtrl;
        private readonly VoiceToText _voiceToText;

        public DictationEmail()
        {
            InitializeComponent();
            _tbCtrl = TbDictation;
            _voiceToText = new VoiceToText();
            _emailOperations = new EmailOperations();
            _chemOperations = new ChemOperations();
            _fullResponces = string.Empty;
            _partialResponces = string.Empty;
            _commandMode = false;
        }

        public DictationEmail(bool record)
        {
            _commandMode = false;
            InitializeComponent();
            _voiceToText = new VoiceToText();
            _emailOperations = new EmailOperations();
            _chemOperations = new ChemOperations();
            _fullResponces = string.Empty;
            _partialResponces = string.Empty;
            _tbCtrl = TbEmailAddress;

            //begin recording immediately
            if (record)
                StartRecording();
        }

        #region UI Events

        private void BtnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            SendEmail();
        }

        private void BtnStopRecording_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
        }

        private void BtnStartRecording_Click(object sender, RoutedEventArgs e)
        {
            StartRecording();
        }

        private void PartialResponceRecived(object sender, PartialSpeechResponseEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    CallCommand(e);
                    if (e.PartialResult.Contains("command mode") || e.PartialResult.Contains("pause"))
                    {
                        _partialResponces = "";
                        _tbCtrl.Text = _fullResponces + " " + _partialResponces;
                        _commandMode = true;
                        return;
                    }
                    if (_commandMode||_partialResponces =="###")
                    {
                        _partialResponces = string.Empty;
                        return;
                    }
                    _partialResponces = AjustPartialResponseText(e.PartialResult);
                    _tbCtrl.Text = _fullResponces + " " + _partialResponces;
                    _tbCtrl.SelectionStart = _fullResponces.Length;
                    _tbCtrl.SelectionLength = _fullResponces.Length + _partialResponces.Length + 1;
                });
        }

        private void CallCommand(PartialSpeechResponseEventArgs e)
        {
            if (e.PartialResult.Contains("stop recording"))
            {
                _partialResponces = "###";
                _tbCtrl.Text = _fullResponces;
                StopRecording();
            }

            if (_commandMode && e.PartialResult.Contains("chemical") && e.PartialResult.Contains("properties"))
            {
                string chemical;
                try
                {
                    chemical = e.PartialResult.Replace("chemical ", "");
                    chemical = chemical.Replace(" properties", "");
                    _tbCtrl.Text += "looking up data for " + chemical + " ...\n";
                    _chemOperations.FillChemicalValues(chemical);
                    _tbCtrl.Text += " All Data fully loaded for chemical :" + chemical;
                }
                catch (Exception ex)
                {
                    _tbCtrl.Text += " " + ex.Message;
                }
            }

            if (_commandMode && e.PartialResult.Contains("send email"))
            {
                _emailOperations.SendDictationEmail(TbEmailAddress.Text, TbDictation.Text);
                Close();
            }

            if (_commandMode && e.PartialResult.Contains("email address"))
            {
                _partialResponces = "###";
                _tbCtrl.Background = Brushes.White;
                _tbCtrl = TbEmailAddress;
                _fullResponces = _tbCtrl.Text;
                SetTextBox();
                _commandMode = false;
            }

            if (_commandMode && e.PartialResult.Contains("subject"))
            {
                _partialResponces = "###";
                _tbCtrl.Background = Brushes.White;
                _tbCtrl = TbSubject;
                _fullResponces = _tbCtrl.Text;
                SetTextBox();
                _commandMode = false;
            }

            if (_commandMode && e.PartialResult.Contains("content"))
            {
                _partialResponces = "###";
                _tbCtrl.Background = Brushes.White;
                _tbCtrl = TbDictation;
                _fullResponces = _tbCtrl.Text;
                SetTextBox();
                _commandMode = false;
            }

            if (_commandMode && e.PartialResult.Contains("exit command") || e.PartialResult.Contains("resume"))
            {
                _partialResponces = "###";
                _commandMode = false;
                _fullResponces = TbDictation.Text;
            }

            //chemical properties
            if (_commandMode && e.PartialResult.Contains("target chemical")) _tbCtrl.Text += _chemOperations.Chemical;

            if (_commandMode && e.PartialResult.Contains("mw") || e.PartialResult.Contains("molecular weight"))
                _tbCtrl.Text += " Molecular Weight = " + _chemOperations.MolecularWeight;

            if (_commandMode && e.PartialResult.Contains("formula"))
            {
                var text = " Formula = " + _chemOperations.MolecularWeight;
                _tbCtrl.Text += text;
                _fullResponces += text;
            }

            if (_commandMode && e.PartialResult.Contains("synonym"))
            {
                var text = "Synonyms  = " + _chemOperations.Synonyms;
                _tbCtrl.Text += text;
                _fullResponces += text;
            }

            if (_commandMode && e.PartialResult.Contains("chemical id"))
            {
                var text = "CID's  = " + _chemOperations.CiDs;
                _tbCtrl.Text += text;
                _fullResponces += text;
            }

            if (_commandMode && e.PartialResult.Contains("keys"))
            {
                var text = "InCHI keys  = " + _chemOperations.CiDs;
                _tbCtrl.Text += text;
                _fullResponces += text;
            }
        }

        private void ResponceRecived(object sender, SpeechResponseEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    if (_commandMode) return;
                    _partialResponces = AjustResponseText(_partialResponces);
                    _fullResponces += " " + _partialResponces;

                    SaveTextToFile sttf = new SaveTextToFile();
                    sttf.SaveText(_partialResponces);

                    _tbCtrl.Text = _fullResponces;
                    _tbCtrl.SelectionStart = _fullResponces.Length;
                    _tbCtrl.SelectionLength = _fullResponces.Length + _partialResponces.Length + 1;
                    _partialResponces = string.Empty;
                }
            );
        }

        #endregion

        #region Methods

        private void StartRecording()
        {
            SetTextBox();
            ConvertSpeech_ToText();
        }

        private void SendEmail()
        {
            _emailOperations.SendDictationEmail(TbEmailAddress.Text, TbDictation.Text);
            Close();
        }

        private void SetTextBox()
        {
            _tbCtrl.Focus();
            _fullResponces = _tbCtrl.Text;
            BtnStartRecording.IsEnabled = false;
            _tbCtrl.Background = Brushes.LightGreen;
        }

        private void ConvertSpeech_ToText()
        {
            _voiceToText.StartRecording();
            _voiceToText.MicrophoneRecognitionClient.OnResponseReceived += ResponceRecived;
            _voiceToText.MicrophoneRecognitionClient.OnPartialResponseReceived += PartialResponceRecived;
            _voiceToText.MicrophoneRecognitionClient.StartMicAndRecognition();
        }

        private string AjustPartialResponseText(string response)
        {
            response = response.ToLower();
            if (response.Contains("new line") || response == "nuala") response = "\n";

            if (response.Contains("clear line")) response = "";

            if (response.Contains("clear all"))
            {
                _tbCtrl.Text = "";
                _fullResponces = "";
                response = "";
            }

            if (response == "undo")
            {
                response = "";
                try
                {
                    _fullResponces = _fullResponces.Replace("  ", " ");
                    _fullResponces = _fullResponces.Remove(_fullResponces.LastIndexOf(' '));
                    _fullResponces = _fullResponces.Remove(_fullResponces.LastIndexOf(' '));
                }
                catch
                {
                    response = "";
                }

                return response;
            }

            if (response == "stop recording") StopRecording();
            //chemical terms
            response = response.Replace("acido nitro", "Acetonitrile");
            response = response.Replace("i so profile", "Isopropyl");
            response = response.Replace("methana", "methanol");
            response = response.Replace("call um", "column");
            response = response.Replace("hilock", "hilic");
            response = response.Replace("helic", "hilic");
            response = response.Replace("hillock", "hilic");
            response = response.Replace("hillac", "hilic");
            response = response.Replace("helik", "hilic");
            response = response.Replace("elseya masquerade", "lcms grade");
            response = response.Replace("mobile face", "mobile phase");

            //grammar and punctuation
            response = response.Replace("colon", ":");
            response = response.Replace("period", ".");

            return response;
        }

        private string AjustResponseText(string response)
        {
            try
            {
                if (response.Contains("email id "))
                {
                    response = response.Replace("email id ", "");
                    response = _emailOperations.EmailRecords[response];
                }
            }
            catch
            {
                response = "";
            }

            return response;
        }

        private void StopRecording()
        {
            _tbCtrl.Background = Brushes.White;
            _voiceToText.StopRecording();
            BtnStartRecording.IsEnabled = true;
        }

        #endregion
    }
}