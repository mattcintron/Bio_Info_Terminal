using System;
using System.IO;
using BioInfo_Terminal.Data;
using Conversive.Verbot4;

namespace BioInfo_Terminal.Methods.Dialog_Handling
{
    internal class LanguageProcessor
    {
        private readonly State _state;
        private readonly Verbot4Engine _verbot;

        // ReSharper disable once EmptyConstructor
        internal LanguageProcessor()
        {
            _verbot = new Verbot4Engine();
            var kb = new KnowledgeBase();
            var kbi = new KnowledgeBaseItem();
            _state = new State();

            //load all current knowledge 
            LoadStartingKnowledgeBase();

            //save the knowledgebase
            var xToolbox = new XmlToolbox(typeof(KnowledgeBase));
            xToolbox.SaveXml(kb, @"c:\kbi.vkb");

            //load the knowledgebase item
            kbi.Filename = "kbi.vkb";
            kbi.Fullpath = @"c:\";

            //set the knowledge base for verbot
            _verbot.AddKnowledgeBase(kb, kbi);
            _state.CurrentKBs.Add(@"c:\kbi.vkb");
            Ckb = _verbot.CompileKnowledgeBase(kb, kbi);
        }

        internal CompiledKnowledgeBase Ckb { get; set; }

        internal StructuredText StructureText(string rawText)
        {
            // process the reply
            var prepedText = StructuredText.PrepareTextForRecognition(rawText);
            var reply = _verbot.GetReply(prepedText, _state);
            if (reply == null) return new StructuredText(rawText);
            var text = new StructuredText(rawText, reply.AgentText);
            return text;
        }

        internal void LoadCkb(string fileName)
        {
            Ckb = _verbot.AddCompiledKnowledgeBase(fileName);
            _state.CurrentKBs.Clear();
            _state.CurrentKBs.Add(fileName);
        }

        internal void AddCkb(string fileName)
        {
            Ckb = _verbot.AddCompiledKnowledgeBase(fileName);
            _state.CurrentKBs.Clear();
            _state.CurrentKBs.Add(fileName);
        }

        internal void SaveCkb()
        {
            //save the current knowledge base to file 
            var savePath = AppDomain.CurrentDomain.BaseDirectory;
            savePath = savePath.Replace(@"\BioInfo_Terminal\bin\Debug\", "");
            savePath += @"\SavedKnowlegeBases\newckb.ckb";

            _verbot.SaveCompiledKnowledgeBase(Ckb, savePath);
        }

        internal void LoadStartingKnowledgeBase()
        {
            //load knowledge base
            var ckbSource = AppDomain.CurrentDomain.BaseDirectory;
            ckbSource = ckbSource.Replace(@"\BioInfo_Terminal\bin\Debug\", "");
            ckbSource += @"\SavedKnowlegeBases";

            _state.CurrentKBs.Clear();
            foreach (var fileName in Directory.GetFiles(ckbSource))
            {
                Ckb = _verbot.AddCompiledKnowledgeBase(fileName);
                _state.CurrentKBs.Add(fileName);
            }
        }
    }
}