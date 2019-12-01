using System;
using System.Collections.Generic;
using BioInfo_Terminal.Data;
using BioInfo_Terminal.Methods.Recognizers;
using BioInfo_Terminal.UI;
using log4net;

// ReSharper disable RedundantAssignment

namespace BioInfo_Terminal.Methods.Dialog_Handling
{
    internal class DialogHandler
    {
        internal static ILog DialogLog = LogManager.GetLogger("BioInfo.Dialog");
        private static readonly ILog UnknownLog = LogManager.GetLogger("BioInfo.Unknown");
        private readonly List<IRecognizers> _recognizers;
        private readonly IBioInfoUserIo _userIo;
        private Context _context;
        private SkillBuilder _skillBuilder;

        internal DialogHandler(IBioInfoUserIo userIo)
        {
            _userIo = userIo;
            LanguageProcessor = new LanguageProcessor();
            _skillBuilder = new SkillBuilder();
            _context = new Context();

            var chemRec = new ChemRecognizer();
            var skillRec = new SkillRecognizer();
            var unitRec = new UnitRecognizer();
            var emailRec = new EmailRecognizer();

            _recognizers = new List<IRecognizers>
            {
                chemRec,
                skillRec,
                unitRec,
                emailRec
            };
        }

        internal LanguageProcessor LanguageProcessor { get; set; }

        internal string HandleUnstructuredInput(string text)
        {
            text = text.ToLower();
            var response = string.Empty;

            //Handle Direct User Commands
            if (HandleCommands(ref text, ref response)) return response;

            //Structure text
            var st = LanguageProcessor.StructureText(text);
            if (!st.NeedsRecognition) return st.StructureText;

            // Perform recognition of current context
            RecognizeFromStructuredText(st);

            // skill recognition 
            foreach (var item in _skillBuilder.Skills)
                item.RecognizeSkill(_context.Values);

            //skill execution
            foreach (var skill in _skillBuilder.Skills)
                if (skill.RunSkill(ref text, ref response))
                {
                    _skillBuilder.EmailOperations.EmailContent = response;
                    return response.Replace(" Data Saved", "");
                }

            //if nothing is understood
            LogErrors(text);
            response = "Sorry I do not understand";
            return response;
        }

        private void RecognizeFromStructuredText(StructuredText st)
        {
            _context.Values = new Dictionary<string, string>();
            //fill context values 
            foreach (var recognizer in _recognizers)
            {
                var text = st.RawText;
                if (recognizer.IsSikllIdRec() && !string.IsNullOrEmpty(st.StructureText)) text = st.StructureText;
                var newValues = recognizer.ParseInfo(text);
                foreach (var newValue in newValues) _context.Values.Add(newValue.Key, newValue.Value);
            }

            //ready all operations
            foreach (var operation in _skillBuilder.Operations)
                operation.FillValues(_context.Values);
        }

        internal void SpeakToUser(string text)
        {
            _userIo.SpeakToUser(text);
        }

        //Direct Commands
        private bool HandleCommands(ref string text, ref string response)
        {
            //Immediate Changes to the spoken response 
            if (HandleResponseCommands(text, ref response)) return true;

            //Set display visuals commands
            if (HandleSystemCommands(text, ref response)) return true;

            //Voice transformation
            if (HandleVoiceCommands(ref text, ref response))
            {
                _skillBuilder.EmailOperations.EmailContent = response;
                return true;
            }

            return false;
        }

        private bool HandleSystemCommands(string text, ref string response)
        {
            response = "null";
            if (text == "clear window")
            {
                _userIo.ClearMessages();
                {
                    return true;
                }
            }

            if (text == "clear memory" || text == "wipe memory" || text == "memory wipe")
            {
                _context = new Context();
                _skillBuilder = new SkillBuilder();

                response = "Memory wipe complete, previous conversation data erased";
                return true;
            }

            if (text == "exit")
            {
                _skillBuilder = new SkillBuilder();

                response = "Ok lets start over, what can I help you with today";
                return true;
            }

            if (text.Contains("repeat back"))
            {
                response = text.Replace("repeat back", "");
                return true;
            }

            if (text.Contains("open dictation") || text.Contains("open email dictation"))
            {
                _userIo.OpenEmailDictation(false);
                return true;
            }

            if (text.Contains("start dictation") || text.Contains("start email dictation"))
            {
                _userIo.OpenEmailDictation(true);
                return true;
            }

            return false;
        }

        internal bool HandleVoiceCommands(ref string text, ref string response)
        {
            if (text == "show voice options" || text == "edit voice options" || text == "voice options" ||
                text == "change voice")
            {
                string voices;
                voices = _userIo.SetVoiceOptions("options");
                response = " here are all the current voice options for Bio Info,  " + voices +
                           " To change just state - set voice to - followed by the voice name";
                return true;
            }

            if (text.Contains("change voice to") || text.Contains("set voice to"))
                try
                {
                    var remove = "change voice to ";
                    text = text.Replace(remove, "");
                    remove = "set voice to ";
                    text = text.Replace(remove, "");
                    response = _userIo.SetVoiceOptions(text);
                    if (!string.IsNullOrEmpty(response)) return true;

                    response = "Sorry voice name " + text + " not recognized, please try again";
                    return true;
                }
                catch (Exception ex)
                {
                    response = "Sorry voice name Error :" + ex.Message;
                    return true;
                }

            return false;
        }

        private bool HandleResponseCommands(string text, ref string response)
        {
            if (text != "continue" && _userIo.GetSpeechIsPaused())
                _userIo.SetSpeechState("reset");

            if (text == "continue")
            {
                response = "null";
                if (!_userIo.GetSpeechIsPaused())
                    response = "sorry, no saved conversation to continue reading";
                else
                    _userIo.SetSpeechState("continue");
                return true;
            }

            response = "null";
            if (text == "stop" || text == "pause")
            {
                _userIo.SetSpeechState("pause");
                return true;
            }

            return false;
        }

        private void LogErrors(string text)
        {
            UnknownLog.Info(text);
        }
    }
}