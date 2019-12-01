/*
	Copyright 2004-2006 Conversive, Inc.
	http://www.conversive.com
	3806 Cross Creek Rd., Unit F
	Malibu, CA 90265
 
	This file is part of Verbot 4 Library: a natural language processing engine.

    Verbot 4 Library is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    Verbot 4 Library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Verbot 4 Library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
	
	Verbot 4 Library may also be available under other licenses.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;

// TODO find out why class fails to run is this qualifiers are removed
// ReSharper disable ArrangeThisQualifier

namespace Conversive.Verbot4
{
    /// <summary>
    ///     Core NLP engine code for Verbot 4 Library.
    /// </summary>
    public class Verbot4Engine
    {
        public delegate void CompileError(string errorText, string lineText);

        public delegate void CompileWarning(string warningText, string lineText);

        public delegate void RuleCompiled(int completed, int total);

        public delegate void RuleCompileFailed(string ruleId, string ruleName, string errorMessage);

        private Hashtable _compiledKnowledgeBases;
        private ICryptoTransform _decryptor;
        private ICryptoTransform _encryptor;

        private XmlToolbox _xmlToolbox;

        public Verbot4Engine()
        {
            //This isn't strong encryption.  It's more for obfuscation.
            var k = this.gk(32, "Copyright 2004 - Conversive, Inc.");
            var v = this.gk(16, "Start the Dialog™");
            this.Init(k, v);
        }

        public Verbot4Engine(string encryptionKey, string encryptionVector)
        {
            var k = this.gk(32, encryptionKey);
            var v = this.gk(16, encryptionVector);
            this.Init(k, v);
        }

        public event RuleCompiled OnRuleCompiled;
        public event RuleCompileFailed OnRuleCompileFailed;
        public event CompileWarning OnCompileWarning;
        public event CompileError OnCompileError;

        private void Init(byte[] k, byte[] v)
        {
            this._compiledKnowledgeBases = new Hashtable();
            _xmlToolbox = new XmlToolbox(typeof(KnowledgeBase));
            var crypto = new RijndaelManaged();
            this._encryptor = crypto.CreateEncryptor(k, v);
            this._decryptor = crypto.CreateDecryptor(k, v);
        }

        public CompiledKnowledgeBase CompileKnowledgeBase(KnowledgeBase kb, KnowledgeBaseItem knowledgeBaseItem)
        {
            return LoadKnowledgeBase(kb, knowledgeBaseItem);
        }

        public void SaveCompiledKnowledgeBase(CompiledKnowledgeBase ckb, string stPath)
        {
            var bf = new BinaryFormatter();
            var fs = new FileStream(stPath, FileMode.Create);
            bf.Serialize(fs, ckb);
            fs.Close();
        }

        public void SaveEncryptedCompiledKnowledgeBase(CompiledKnowledgeBase ckb, string stPath)
        {
            var bf = new BinaryFormatter();
            var fs = new FileStream(stPath, FileMode.Create);
            var csEncrypt = new CryptoStream(fs, this._encryptor, CryptoStreamMode.Write);
            bf.Serialize(csEncrypt, ckb);
            csEncrypt.FlushFinalBlock();
            fs.Flush();
            fs.Close();
        }

        public CompiledKnowledgeBase LoadCompiledKnowledgeBase(string stPath)
        {
            var bf = new BinaryFormatter();
            var fs = Stream.Null;
            CompiledKnowledgeBase ckb = null;
            try
            {
                fs = new FileStream(stPath, FileMode.Open, FileAccess.Read);
                ckb = (CompiledKnowledgeBase) bf.Deserialize(fs);
                ckb.AddConditionsAndCode();
                fs.Close();
            }
            catch (Exception eOpenOrDeserial)
            {
                // ReSharper disable once UnusedVariable
                //TODO: use or discard method 
                var openOrSerserialError = eOpenOrDeserial.ToString();
                try //to open an encrypted CKB
                {
                    if (fs != Stream.Null)
                        fs.Seek(0, SeekOrigin.Begin);
                    var csDecrypt = new CryptoStream(fs, this._decryptor, CryptoStreamMode.Read);
                    ckb = (CompiledKnowledgeBase) bf.Deserialize(csDecrypt);
                    ckb.AddConditionsAndCode();
                }
                catch (Exception e)
                {
                    // ReSharper disable once UnusedVariable
                    //TODO: use or remove var
                    var str = e.ToString();
                }
            }

            return ckb;
        } //LoadCompiledKnowledgeBase(string stPath)

        public KnowledgeBase LoadKnowledgeBase(string stPath)
        {
            var vkb = (KnowledgeBase) this._xmlToolbox.LoadXml(stPath);
            return vkb;
        }

        public CompiledKnowledgeBase LoadKnowledgeBase(KnowledgeBase kb, KnowledgeBaseItem knowledgeBaseItem)
        {
            var ckb = new CompiledKnowledgeBase
            {
                Build = kb.Build,
                Name = knowledgeBaseItem.Fullpath + knowledgeBaseItem.Filename
            };
            ckb.KbList.Add(kb);
            ckb.OnRuleCompiled += this.compiledKnowledgeBase_OnRuleCompiled;
            ckb.OnRuleCompileFailed += this.compiledKnowledgeBase_OnRuleCompileFailed;
            ckb.OnCompileError += ckb_OnCompileError;
            ckb.OnCompileWarning += ckb_OnCompileWarning;
            ckb.LoadKnowledgeBase(kb, knowledgeBaseItem);
            return ckb;
        }

        public CompiledKnowledgeBase AddCompiledKnowledgeBase(string stPath)
        {
            var ckb = LoadCompiledKnowledgeBase(stPath);
            if (ckb != null)
                this._compiledKnowledgeBases[stPath] = ckb;
            return ckb;
        }

        public void RemoveCompiledKnowledgeBase(string stPath)
        {
            this._compiledKnowledgeBases.Remove(stPath);
        }

        public void RemoveCompiledKnowledgeBase(CompiledKnowledgeBase ckb)
        {
            var stPath = ckb.KnowledgeBaseItem.Fullpath + ckb.KnowledgeBaseItem.Filename;
            this.RemoveCompiledKnowledgeBase(stPath);
        }

        public CompiledKnowledgeBase AddKnowledgeBase(KnowledgeBase kb, KnowledgeBaseItem knowledgeBaseItem)
        {
            var ckb = this.LoadKnowledgeBase(kb, knowledgeBaseItem);
            var stPath = knowledgeBaseItem.Fullpath + knowledgeBaseItem.Filename;
            if (ckb != null)
                this._compiledKnowledgeBases[stPath] = ckb;
            return ckb;
        }

        public void ReloadKnowledgeBase(KnowledgeBase kb, KnowledgeBaseItem knowledgeBaseItem)
        {
            var ckb = this.LoadKnowledgeBase(kb, knowledgeBaseItem);
            var stPath = knowledgeBaseItem.Fullpath + knowledgeBaseItem.Filename;
            this._compiledKnowledgeBases[stPath] = ckb;
        }

        private void compiledKnowledgeBase_OnRuleCompiled(int completed, int total)
        {
            if (this.OnRuleCompiled != null)
                this.OnRuleCompiled(completed, total);
        } //engine_OnRuleCompiled(int current, int total)

        private void compiledKnowledgeBase_OnRuleCompileFailed(string ruleId, string ruleName, string errorMessage)
        {
            if (this.OnRuleCompileFailed != null)
                this.OnRuleCompileFailed(ruleId, ruleName, errorMessage);
        }

        public Reply GetReply(string input, State state)
        {
            state.Vars["_input"] = input;
            state.Vars["_lastinput"] = state.Lastinput;
            state.Vars["_lastfired"] = state.Lastfired;
            state.Vars["_time"] = DateTime.Now.ToString("h:mm tt");
            state.Vars["_time24"] = DateTime.Now.ToString("HH:mm");
            state.Vars["_date"] = DateTime.Now.ToString("MMM. d, yyyy");
            state.Vars["_month"] = DateTime.Now.ToString("MMMM");
            state.Vars["_dayofmonth"] = DateTime.Now.ToString("d ").Trim();
            state.Vars["_year"] = DateTime.Now.ToString("yyyy");
            state.Vars["_dayofweek"] = DateTime.Now.ToString("dddd");

            if (input.Length == 0)
                input = "_blank";

            state.Lastinput = input;

            foreach (string stPath in state.CurrentKBs)
            {
                var ckb = (CompiledKnowledgeBase) this._compiledKnowledgeBases[stPath];
                if (ckb != null)
                {
                    var reply = ckb.GetReply(input, state.Lastfired, state.Vars);
                    if (reply != null)
                    {
                        state.Lastfired = reply.RuleId;
                        state.Vars["_lastoutput"] = reply.Text;
                        return reply;
                    }
                }
            }

            return null; //if there's no reply, return null
        }

        private byte[] gk(byte s, string t)
        {
            var sha256 = new SHA256Managed();
            var x = Encoding.ASCII.GetBytes(t);
            x = sha256.ComputeHash(x, 0, x.Length);
            var o = new byte[s];
            for (var i = 0; i < s; i++)
                o[i] = x[i % x.Length];
            return o;
        } //gk(byte s, string t)

        private void ckb_OnCompileError(string errorText, string lineText)
        {
            if (this.OnCompileError != null)
                this.OnCompileError(errorText, lineText);
        }

        private void ckb_OnCompileWarning(string warningText, string lineText)
        {
            if (this.OnCompileWarning != null)
                this.OnCompileWarning(warningText, lineText);
        }
    } //class Verbot4Engine

    [Serializable]
    public class CompiledKnowledgeBase : ISerializable
    {
        public delegate void CompileError(string errorText, string lineText);

        public delegate void CompileWarning(string warningText, string lineText);

        public delegate void RuleCompiled(int completed, int total);

        public delegate void RuleCompileFailed(string ruleId, string ruleName, string errorMessage);

        private readonly ArrayList _inputReplacements;

        private readonly Hashtable _inputs;

        private readonly Hashtable _outputs;

        [NonSerialized] private readonly Random _random;

        private readonly Hashtable _recentOutputsByRule;
        private readonly ArrayList _replacements;

        private readonly Hashtable _synonyms;

        private int _build;
        private CSharpToolbox _csToolbox;

        private KnowledgeBaseInfo _knowledgeBaseInfo;


        private KnowledgeBaseItem _knowledgeBaseItem;

        [NonSerialized] public string Name; //This is just used for comparison when reloading

        public CompiledKnowledgeBase()
        {
            this.KbList = new List<KnowledgeBase>();
            this._synonyms = new Hashtable();
            this._replacements = new ArrayList();
            this._inputReplacements = new ArrayList();
            this._inputs = new Hashtable();
            this._outputs = new Hashtable();
            this._knowledgeBaseItem = new KnowledgeBaseItem();
            this._knowledgeBaseInfo = new KnowledgeBaseInfo();
            this._build = -1;
            this.Name = "";
            this._random = new Random();
            this._recentOutputsByRule = new Hashtable();
            this.InitializeCsToolbox();
        }

        protected CompiledKnowledgeBase(SerializationInfo info, StreamingContext context)
        {
            this._synonyms = (Hashtable) info.GetValue("s", typeof(Hashtable));
            this._replacements = (ArrayList) info.GetValue("r", typeof(ArrayList));
            this._inputs = (Hashtable) info.GetValue("i", typeof(Hashtable));
            this._outputs = (Hashtable) info.GetValue("o", typeof(Hashtable));
            this._knowledgeBaseItem = (KnowledgeBaseItem) info.GetValue("k", typeof(KnowledgeBaseItem));
            this._knowledgeBaseInfo = (KnowledgeBaseInfo) info.GetValue("kbi", typeof(KnowledgeBaseInfo));
            this._build = info.GetInt32("b");
            this._random = new Random();
            this._recentOutputsByRule = new Hashtable();

            //use a try/catch block around any new vales
            try
            {
                this._inputReplacements = (ArrayList) info.GetValue("ir", typeof(ArrayList));
            }
            catch
            {
                this._inputReplacements = new ArrayList();
            }

            this.InitializeCsToolbox();
            try
            {
                this._csToolbox.CodeModules = (ArrayList) info.GetValue("cm", typeof(ArrayList));
            }
            catch
            {
                this._csToolbox.CodeModules = new ArrayList();
            }
        }

        public bool ContainsCode => this._csToolbox.ContainsCode;

        public List<KnowledgeBase> KbList { get; set; }

        public string Code => this._csToolbox.Code;

        public int Build
        {
            get => this._build;
            set => this._build = value;
        }

        public KnowledgeBaseItem KnowledgeBaseItem
        {
            get => this._knowledgeBaseItem;
            set => this._knowledgeBaseItem = value;
        }

        public KnowledgeBaseInfo KnowledgeBaseInfo
        {
            get => this._knowledgeBaseInfo;
            set => this._knowledgeBaseInfo = value;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("s", this._synonyms);
            info.AddValue("r", this._replacements);
            info.AddValue("i", this._inputs);
            info.AddValue("o", this._outputs);
            info.AddValue("k", this._knowledgeBaseItem);
            info.AddValue("kbi", this._knowledgeBaseInfo);
            info.AddValue("b", this._build);
            info.AddValue("ir", this._inputReplacements);

            info.AddValue("cm", this._csToolbox.CodeModules);
        }

        public event RuleCompiled OnRuleCompiled;
        public event RuleCompileFailed OnRuleCompileFailed;
        public event CompileWarning OnCompileWarning;
        public event CompileError OnCompileError;

        private void InitializeCsToolbox()
        {
            this._csToolbox = new CSharpToolbox();
            this._csToolbox.OnCompileError += csToolbox_OnCompileError;
            this._csToolbox.OnCompileWarning += csToolbox_OnCompileWarning;
        }

        public void AddConditionsAndCode()
        {
            try
            {
                foreach (ArrayList irs in this._inputs.Values)
                foreach (InputRecognizer ir in irs)
                    if (ir.Condition != "")
                        this._csToolbox.AddCondition(ir.InputId, ir.Condition);
                foreach (ArrayList os in this._outputs.Values)
                foreach (Output o in os)
                {
                    if (o.Condition != "")
                        this._csToolbox.AddCondition(o.Id, o.Condition);
                    if (this._csToolbox.ContainsCSharpTags(o.Text))
                        this._csToolbox.AddOutput(o.Id, o.Text);
                    if (this._csToolbox.ContainsCSharpTags(o.Cmd))
                        this._csToolbox.AddOutput(o.Id + "_cmd", o.Cmd);
                }

                this._csToolbox.Compile();
            }
            catch (Exception exBin)
            {
                // ReSharper disable once UnusedVariable
                //TODO: set or remove var 
                var st = exBin.ToString();
            }
        }

        public Hashtable GetInputs()
        {
            return this._inputs;
        }

        public void LoadKnowledgeBase(KnowledgeBase kb, KnowledgeBaseItem knowledgeBaseItem)
        {
            this._knowledgeBaseItem = knowledgeBaseItem;
            this._knowledgeBaseInfo = kb.Info;
            this.LoadResourceFiles(kb.ResourceFiles);
            var decompressedKb = kb.DecompressTemplates(knowledgeBaseItem.Fullpath);
            if (decompressedKb != null)
                this.CompileRules("_root", decompressedKb.Rules);
            else
                this.CompileRules("_root", kb.Rules);
            this._csToolbox.Compile();
        }

        public void LoadResourceFiles(ArrayList resourceFiles)
        {
            this.LoadSynonyms(resourceFiles);
            this.LoadReplacementProfiles(resourceFiles);
            this.LoadCodeModules(resourceFiles);
        } //LoadResourceFiles(ArrayList resourceFiles)

        private void LoadSynonyms(ArrayList resourceFiles)
        {
            var xmlToolbox = new XmlToolbox(typeof(SynonymGroup));
            SynonymGroup sg;
            foreach (ResourceFile rf in resourceFiles)
                if (rf.Filetype == ResourceFileType.SynonymFile)
                {
                    sg = (SynonymGroup) xmlToolbox.LoadXml(this._knowledgeBaseItem.Fullpath + rf.Filename);
                    foreach (Synonym s in sg.Synonyms)
                    {
                        s.Phrases.Sort();
                        this._synonyms[s.Name.ToLower()] = s;
                    }
                } //end if SynonymFile
        } //loadSynonyms(ArrayList resourceFiles)

        private void LoadReplacementProfiles(ArrayList resourceFiles)
        {
            var xmlToolbox = new XmlToolbox(typeof(ReplacementProfile));
            ReplacementProfile rp;
            foreach (ResourceFile rf in resourceFiles)
                if (rf.Filetype == ResourceFileType.ReplacementProfileFile)
                {
                    rp = (ReplacementProfile) xmlToolbox.LoadXml(this._knowledgeBaseItem.Fullpath + rf.Filename);
                    this._replacements.AddRange(rp.Replacements);
                    this._inputReplacements.AddRange(rp.InputReplacements);
                } //end if ReplacementProfileFile
        } //loadReplacementProfiles(ArrayList resourceFiles)

        private void LoadCodeModules(ArrayList resourceFiles)
        {
            var xmlToolbox = new XmlToolbox(typeof(CodeModule));
            CodeModule cm;
            foreach (ResourceFile rf in resourceFiles)
                if (rf.Filetype == ResourceFileType.CodeModuleFile)
                {
                    cm = (CodeModule) xmlToolbox.LoadXml(this._knowledgeBaseItem.Fullpath + rf.Filename);
                    this._csToolbox.AddCodeModule(cm);
                } //end if ReplacementProfileFile
        }

        private void CompileRules(string parentId, ArrayList rules)
        {
            var ruleCount = rules.Count;
            var ruleCurrent = 0;
            foreach (Rule r in rules)
            {
                try
                {
                    ruleCurrent++;
                    foreach (Input i in r.Inputs)
                    {
                        if (i.Condition != "")
                            this._csToolbox.AddCondition(i.Id, i.Condition);
                        var ir = new InputRecognizer(i.Text, r.Id, i.Id, i.Condition, this._synonyms,
                            this._inputReplacements);
                        if (this._inputs[parentId] == null)
                            this._inputs[parentId] = new ArrayList();
                        ((ArrayList) this._inputs[parentId]).Add(ir);
                        //Go through virtualparents and add this input recognizer to virtual parent id keys
                        foreach (string virtualParentId in r.VirtualParents)
                        {
                            if (this._inputs[virtualParentId] == null)
                                this._inputs[virtualParentId] = new ArrayList();
                            ((ArrayList) this._inputs[virtualParentId]).Add(ir);
                        }
                    }

                    foreach (Output o in r.Outputs)
                    {
                        if (o.Condition != "")
                            this._csToolbox.AddCondition(o.Id, o.Condition);
                        if (this._csToolbox.ContainsCSharpTags(o.Text))
                            this._csToolbox.AddOutput(o.Id, o.Text);
                        if (this._csToolbox.ContainsCSharpTags(o.Cmd))
                            this._csToolbox.AddOutput(o.Id + "_cmd", o.Cmd);
                        if (this._outputs[r.Id] == null)
                            this._outputs[r.Id] = new ArrayList();
                        ((ArrayList) this._outputs[r.Id]).Add(o);
                    }
                }
                catch (Exception e)
                {
                    OnRuleCompileFailed?.Invoke(r.Id, r.Name, e + "\r\n" + e.StackTrace);
                }

                //compile children
                this.CompileRules(r.Id, r.Children);
                if (this.OnRuleCompiled != null && parentId == "_root")
                    this.OnRuleCompiled(ruleCurrent, ruleCount);
            }
        } //compileRules(string parentId, ArrayList rules)

        public Reply GetReply(string input, string lastfired, Hashtable vars)
        {
            var matches = new ArrayList();
            //do replacements, strip áccents is done in ReplaceOnInput if there are no input replacements
            var inputReplaced = TextToolbox.ReplaceOnInput(input, this._inputReplacements);
            //search the children and virtual children (links) of lastfired rule
            if ((ArrayList) this._inputs[lastfired] != null)
                foreach (InputRecognizer ir in (ArrayList) this._inputs[lastfired])
                {
                    //if the input recognizer is a capture, use the original input
                    var match = ir.Matches(ir.IsCapture ? input : inputReplaced, vars, this._csToolbox);


                    if (match.ConfidenceFactor > 0.0)
                    {
                        //copy shortTermMemory vars to inputVars
                        var inputVars = new Hashtable();
                        foreach (var key in vars.Keys)
                            inputVars[key] = vars[key];
                        //captures the variables and adds them to the inputVars object
                        ir.CaptureVars(input, inputVars);
                        matches.Add(match);
                    }
                }

            if (matches.Count == 0 && (ArrayList) this._inputs["_root"] != null)
                foreach (InputRecognizer ir in (ArrayList) this._inputs["_root"])
                {
                    //if the input recognizer is a capture, use the original input
                    var match = ir.Matches(ir.IsCapture ? input : inputReplaced, vars, this._csToolbox);
                    if (match.ConfidenceFactor > 0.0)
                    {
                        //copy shortTermMemory vars to inputVars
                        var inputVars = new Hashtable();
                        foreach (var key in vars.Keys)
                            inputVars[key] = vars[key];
                        //captures the variables and adds them to the inputVars object
                        ir.CaptureVars(input, inputVars);
                        matches.Add(match);
                    }
                }

            if (matches.Count == 0)
                return null;

            matches.Sort();
            var matchBest = (Match) matches[0];
            //use the matching vars, but maintain the original object so that it persists

            // ReSharper disable once UnusedVariable
            //TODO: use or remove var 
            var clone = vars.Clone();
            foreach (var key in matchBest.Vars.Keys)
                vars[key] = matchBest.Vars[key];
            //increment the usage count on the chosed InputRecognizer
            matchBest.InputRecognizer.IncUsageCount();

            var ruleId = matchBest.InputRecognizer.RuleId;

            if (this._outputs[ruleId] == null || ((ArrayList) this._outputs[ruleId]).Count == 0)
                return new Reply("No output found.", "", "", ruleId, this._knowledgeBaseItem);

            var alOutputs = new ArrayList((ArrayList) this._outputs[ruleId]);

            //filter out outputs with false conditions
            for (var i = 0; i < alOutputs.Count; i++)
            {
                var o = (Output) alOutputs[i];
                if (!this._csToolbox.ExecuteCondition(o.Id, vars))
                {
                    alOutputs.RemoveAt(i);
                    i--;
                }
            }

            if (alOutputs.Count == 0) //all outputs were removed
                return new Reply("No output found.", "", "", ruleId, this._knowledgeBaseItem);

            //choose an output at random
            Output outputChosen = null;
            for (var i = 0; i < alOutputs.Count; i++) //the try again loop
            {
                outputChosen = (Output) alOutputs[_random.Next(alOutputs.Count)];
                if (this._recentOutputsByRule[ruleId] == null ||
                    !((ArrayList) this._recentOutputsByRule[ruleId]).Contains(outputChosen.Id))
                    break;
            }

            //update the recent list for this rule
            if (alOutputs.Count > 1)
            {
                if (this._recentOutputsByRule[ruleId] == null)
                    this._recentOutputsByRule[ruleId] = new ArrayList(alOutputs.Count - 1);
                var recent = (ArrayList) this._recentOutputsByRule[ruleId];
                if (outputChosen != null)
                {
                    var index = recent.IndexOf(outputChosen.Id);
                    if (index != -1)
                        recent.RemoveAt(index);
                    else if (recent.Count == alOutputs.Count - 1)
                        recent.RemoveAt(0);
                }

                if (outputChosen != null) recent.Add(outputChosen.Id);
            }

            //replace vars and output synonyms
            var outputChosenText = outputChosen?.Text;
            if (this._csToolbox.OutputExists(outputChosen?.Id))
                outputChosenText = this._csToolbox.ExecuteOutput(outputChosen?.Id, vars);
            outputChosenText = TextToolbox.ReplaceVars(outputChosenText, vars);
            outputChosenText = TextToolbox.ReplaceOutputSynonyms(outputChosenText, this._synonyms);
            //execute c# code in the command field
            var outputChosenCmd = outputChosen?.Cmd;
            if (this._csToolbox.OutputExists(outputChosen?.Id + "_cmd"))
                outputChosenCmd = this._csToolbox.ExecuteOutput(outputChosen?.Id + "_cmd", vars);

            var outputText = this.DoTextReplacements(outputChosenText);
            var agentText = this.DoAgentTextReplacements(outputChosenText);
            var outputCmd = TextToolbox.ReplaceVars(outputChosenCmd, vars);

            return new Reply(outputText, agentText, outputCmd, matchBest.InputRecognizer.RuleId,
                this._knowledgeBaseItem);
        } //GetReply(string input, string lastfired)

        private string DoTextReplacements(string text)
        {
            foreach (Replacement r in this._replacements)
                if (r.TextToFind != null && r.TextToFind != "")
                {
                    var pos = text.IndexOf(r.TextToFind, StringComparison.Ordinal);
                    while (pos != -1)
                    {
                        if (!TextToolbox.IsInCommand(text, pos, r.TextToFind.Length))
                        {
                            if (pos + r.TextToFind.Length < text.Length - 1)
                                text = text.Substring(0, pos)
                                       + r.TextForOutput
                                       + text.Substring(pos + r.TextToFind.Length);
                            else
                                text = text.Substring(0, pos)
                                       + r.TextForOutput;
                        }

                        if (pos < text.Length - 1)
                            pos = text.IndexOf(r.TextToFind, pos + 1, StringComparison.Ordinal);
                        else
                            pos = -1;
                    } //while
                } //if

            return text;
        } //doTextReplacements(string text)

        private string DoAgentTextReplacements(string text)
        {
            foreach (Replacement r in this._replacements)
                if (r.TextToFind != null && r.TextToFind != "")
                {
                    var pos = text.IndexOf(r.TextToFind, StringComparison.Ordinal);
                    while (pos != -1)
                    {
                        if (!TextToolbox.IsInCommand(text, pos, r.TextToFind.Length))
                        {
                            if (pos + r.TextToFind.Length < text.Length - 1)
                                text = text.Substring(0, pos)
                                       + r.TextForAgent
                                       + text.Substring(pos + r.TextToFind.Length);
                            else
                                text = text.Substring(0, pos)
                                       + r.TextForAgent;
                        }

                        if (pos < text.Length - 1)
                            pos = text.IndexOf(r.TextToFind, pos + 1, StringComparison.Ordinal);
                        else
                            pos = -1;
                    } //while
                } //if

            return text;
        } //doAgentTextReplacements(string text)

        private void csToolbox_OnCompileError(string errorText, string lineText)
        {
            if (this.OnCompileError != null)
                this.OnCompileError(errorText, lineText);
        }

        private void csToolbox_OnCompileWarning(string warningText, string lineText)
        {
            if (this.OnCompileWarning != null)
                this.OnCompileWarning(warningText, lineText);
        }
    } //class CompiledKnowledgeBase

    public class Match : IComparable
    {
        public double ConfidenceFactor;
        public InputRecognizer InputRecognizer;
        public Hashtable Vars;

        public Match()
        {
            this.InputRecognizer = null;
            this.ConfidenceFactor = 0.0;
            this.Vars = null;
        }

        public int CompareTo(object o)
        {
            var diff = ((Match) o).ConfidenceFactor - this.ConfidenceFactor;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            //TODO-handle floating point comparison 
            if (diff == 0)
                return 0;
            if (diff > 0)
                return 1;
            return -1;
        }
    }

    public class Reply
    {
        public string AgentText;
        public string Cmd;
        public KnowledgeBaseItem KbItem;
        public string RuleId;
        public string Text;

        public Reply(string text, string agentText, string cmd, string ruleId, KnowledgeBaseItem knowledgeBaseItem)
        {
            this.Text = text;
            this.AgentText = agentText;
            this.Cmd = cmd;
            this.RuleId = ruleId;
            this.KbItem = knowledgeBaseItem;
        }
    } //class Reply

    [Serializable]
    public class InputRecognizer : ISerializable
    {
        [NonSerialized] private static Random _random;

        private readonly bool _bIsCapture;

        private readonly int _length;

        private string _condition;

        private string _inputId;

        [NonSerialized]
        // ReSharper disable once NotAccessedField.Local
        //TODO: use or remove var
        private ArrayList _inputReplacements;

        private Regex _regex;

        private string _ruleId;

        [NonSerialized] private int _usageCount;

        static InputRecognizer()
        {
            _random = null;
        }

        public InputRecognizer(string text, string ruleId, string inputId, string condition, Hashtable synonyms,
            ArrayList alInputReplacements)
        {
            this._inputReplacements = alInputReplacements;

            var regexVars = new Regex(@"\[.*?\]");
            var regexSyns = new Regex(@"\(.*?\)");
            var textReplaced = regexVars.Replace(text, "x");
            textReplaced = regexSyns.Replace(textReplaced, "x");
            textReplaced = textReplaced.Replace("*", "");
            this._length = textReplaced.Length;

            text = TextToolbox.ReplaceSynonyms(text, synonyms);
            //do this last because replacements haven't been applied to synonyms
            text = TextToolbox.ReplaceOnInput(text, alInputReplacements, out this._bIsCapture);

            var pattern = TextToolbox.TextToPattern(text);
            this._regex = new Regex(pattern, /*RegexOptions.Compiled | */
                RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase); //TODO: Does the IgnoreCase Work?

            //regex.IsMatch("x");
            this._ruleId = ruleId;
            this._inputId = inputId;
            this._condition = condition;

            if (_random == null)
                _random = new Random();
        }

        protected InputRecognizer(SerializationInfo info, StreamingContext context)
        {
            this._regex = (Regex) info.GetValue("r", typeof(Regex));
            this._ruleId = info.GetString("i");
            try
            {
                this._inputId = info.GetString("ii");
            }
            catch
            {
                this._inputId = "";
            }

            try
            {
                this._condition = info.GetString("c");
            }
            catch
            {
                this._condition = "";
            }

            try
            {
                this._bIsCapture = info.GetBoolean("ic");
            }
            catch
            {
                this._bIsCapture = false;
            }

            this._length = info.GetInt32("l");
            if (_random == null)
                _random = new Random();
        }

        public Regex Regex
        {
            get => this._regex;
            set => this._regex = value;
        }

        public string RuleId
        {
            get => this._ruleId;
            set => this._ruleId = value;
        }

        public string InputId
        {
            get => this._inputId;
            set => this._inputId = value;
        }

        public string Condition
        {
            get => this._condition;
            set => this._condition = value;
        }

        public bool IsCapture => this._bIsCapture;

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("r", this._regex, typeof(Regex));
            info.AddValue("i", this._ruleId);
            info.AddValue("ii", this._inputId);
            info.AddValue("c", this._condition);
            info.AddValue("l", this._length);
            info.AddValue("ic", this._bIsCapture);
        }

        public Match Matches(string input, Hashtable vars, CSharpToolbox csToolbox)
        {
            var match = new Match();
            if (this._regex.IsMatch(input))
            {
                //copy shortTermMemory vars to inputVars
                var inputVars = new Hashtable();
                foreach (var key in vars.Keys)
                    inputVars[key] = vars[key];
                //captures the variables and adds them to the inputVars object
                this.CaptureVars(input, inputVars);

                if (csToolbox.ExecuteCondition(this._inputId, inputVars))
                {
                    var noise = _random.NextDouble() * 0.0001;
                    var usageBonus = 0.01 / (this._usageCount + 1); //goes lower the more it's used
                    double cf;
                    if (this._length == 0) //is this ever true? yes, when input is *
                        cf = usageBonus + noise + 0.0001;
                    else
                        cf = usageBonus + noise + this._length / (double) input.Length;
                    match.InputRecognizer = this;
                    match.Vars = inputVars;
                    match.ConfidenceFactor = cf;
                }
            } //if(this.regex.IsMatch(input))

            return match;
        } //Matches(string input)

        public void CaptureVars(string input, Hashtable vars)
        {
            var tempVars = new Hashtable();
            var gc = this._regex.Match(input).Groups;
            if (gc.Count > 0)
            {
                var groupNames = this._regex.GetGroupNames();
                foreach (var name in groupNames)
                {
                    var v = gc[name].Value;
                    var start = input.IndexOf(v, StringComparison.Ordinal);
                    var targetLength = v.Length;
                    if (start != -1)
                        tempVars[name.ToLower()] = input.Substring(start, targetLength);
                }

                //move all of the non-nested vars into the vars Hashtable
                foreach (DictionaryEntry entry in tempVars)
                    if (((string) entry.Key).IndexOf("_s_", StringComparison.Ordinal) == -1)
                        vars[((string) entry.Key).ToLower()] = entry.Value;

                //process all of the vars that have vars in their name
                foreach (DictionaryEntry entry in tempVars)
                {
                    var key = (string) entry.Key;
                    var start = key.IndexOf("_s_", StringComparison.Ordinal);
                    while (start != -1)
                    {
                        var end = key.IndexOf("_e_", start, StringComparison.Ordinal);
                        if (end != -1)
                        {
                            var subVal = (string) vars[key.Substring(start + 3, end - start - 3)];
                            if (subVal == null)
                                subVal = "";
                            if (end + 1 == key.Length)
                                key = key.Substring(0, start) + subVal;
                            else
                                key = key.Substring(0, start) + subVal + key.Substring(end + 3);
                            start = key.IndexOf("_s_", StringComparison.Ordinal);
                        } //end if there was a var within this key
                        else
                        {
                            break; //out of working on this key, it is messed up
                        }
                    } //end while there are more internal vars

                    vars[key.ToLower()] = entry.Value;
                } //end foreach entry in the vars of this IR
            } //if(gc.Count > 0)
        } //CaptureVars(string input, Hashtable vars)

        public void IncUsageCount()
        {
            this._usageCount++;
        }
    } //class InputRecognizer
}