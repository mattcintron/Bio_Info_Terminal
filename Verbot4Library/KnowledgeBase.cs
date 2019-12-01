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
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Xml.Serialization;

namespace Conversive.Verbot4
{
    /// <summary>
    ///     Represents a KnowledgeBase that is used by the Verbot4Engine.
    /// </summary>
    public class KnowledgeBase
    {
        /*
         * Unserialized Attributes
         */


        public KnowledgeBaseInfo Info;

        [XmlArrayItem("ResourceFile")] public ArrayList ResourceFiles;

        [XmlArrayItem("Rule")] public ArrayList Rules;

        public KnowledgeBase()
        {
            Rules = new ArrayList();
            ResourceFiles = new ArrayList();
            Version = "1.0";
            Build = 0;
            Changed = false;
            Info = new KnowledgeBaseInfo();
        }

        public string Id { get; set; }

        public string Version { get; set; }

        public int Build { get; set; }

        [XmlIgnore] public bool Changed { get; set; }

        /*
         * Modifier Methods
         */

        public Rule AddRule()
        {
            var ruleNew = new Rule();
            ruleNew.Id = GetNewRuleId();
            Rules.Add(ruleNew);
            return ruleNew;
        }

        public string AddRule(string ruleName)
        {
            var ruleNew = AddRule();
            ruleNew.Name = ruleName;
            return ruleNew.Id;
        }

        public string AddRuleChild(Rule parent, string ruleName)
        {
            if (parent != null)
            {
                var ruleNew = new Rule();
                ruleNew.Id = GetNewRuleId();
                ruleNew.Name = ruleName;
                parent.Children.Add(ruleNew);
                return ruleNew.Id;
            }

            return AddRule(ruleName);
        }

        public void DeleteRule(string ruleId)
        {
            var r = GetRule(ruleId);
            deleteRule(r, Rules);
        } //DeleteRule(int ruleId)

        private void deleteRule(Rule ruleToDelete, ArrayList rules)
        {
            if (rules.Contains(ruleToDelete))
                rules.Remove(ruleToDelete);
            else
                foreach (Rule r in rules)
                    deleteRule(ruleToDelete, r.Children);
        } //deleteRule(Rule ruleToDelete, ArrayList rules)

        public KnowledgeBase DecompressTemplates(string path)
        {
            var dataTables = LoadTemplateData(path);
            if (dataTables.Count == 0)
                return null;

            var newKb = new KnowledgeBase();
            newKb.Build = Build;
            newKb.Changed = true;
            newKb.Id = Id;
            newKb.Version = Version;
            newKb.Info = Info;
            newKb.Rules = Rules;

            foreach (ResourceFile rf in ResourceFiles)
                if (rf.Filetype != ResourceFileType.TemplateDataFile)
                    newKb.ResourceFiles.Add(rf);

            foreach (DataTable dataTable in dataTables)
                if (ContainsTemplateRule(newKb.Rules, dataTable))
                    newKb.Rules = DecompressRules(newKb.Rules, dataTable);
            return newKb;
        } //DecompressTemplates(string path)

        private ArrayList DecompressRules(ArrayList rules, DataTable dataTable)
        {
            var newRules = new ArrayList();
            foreach (Rule r in rules)
                if (isTemplateRule(r, dataTable))
                {
                    for (var i = 0; i < dataTable.Rows.Count; i++)
                        newRules.Add(CreateRuleFromTemplateRow(r, dataTable.Columns, dataTable.Rows[i], i));
                }
                else
                {
                    var newRule = CloneRule(r);
                    newRule.Children = DecompressRules(r.Children, dataTable);
                    newRules.Add(newRule);
                }

            return newRules;
        } //decompressRule(Rule r, System.Data.DataTable dataTable)

        private ArrayList LoadTemplateData(string path)
        {
            var dataTables = new ArrayList();
            DataTable dataTable;
            foreach (ResourceFile rf in ResourceFiles)
                if (rf.Filetype == ResourceFileType.TemplateDataFile)
                {
                    var dataFilename = path + rf.Filename;
                    var sr = new StreamReader(dataFilename, Encoding.Default, true);
                    var data = sr.ReadToEnd();
                    var lines = ConversiveGeneralTextToolbox.SplitCsv(data, ',', '"');
                    if (lines.Count > 1)
                    {
                        dataTable = new DataTable();
                        foreach (string colName in (ArrayList) lines[0])
                        {
                            var dc = new DataColumn();
                            dc.DataType = Type.GetType("System.String");
                            dc.ColumnName = colName.ToLower();
                            dc.Caption = colName;
                            dataTable.Columns.Add(dc);
                        }

                        for (var i = 1; i < lines.Count; i++)
                        {
                            var fieldValues = (ArrayList) lines[i];
                            var dr = dataTable.NewRow();
                            for (var j = 0; j < fieldValues.Count; j++) dr[j] = (string) fieldValues[j];
                            dataTable.Rows.Add(dr);
                        } //for each data line

                        dataTables.Add(dataTable);
                    } //if we have a valid file
                } //end if TemplateDataFile

            return dataTables;
        } //loadTemplateData(ArrayList resourceFiles)

        private bool ContainsTemplateRule(ArrayList rules, DataTable dataTable)
        {
            foreach (Rule r in rules)
            {
                if (isTemplateRule(r, dataTable))
                    return true;
                if (ContainsTemplateRule(r.Children, dataTable))
                    return true;
            }

            return false;
        } //containsTemplateRule(ArrayList rules, System.Data.DataTable dataTable)

        private bool isTemplateRule(Rule r, DataTable dataTable)
        {
            if (dataTable != null)
                foreach (DataColumn col in dataTable.Columns)
                {
                    //check the rule name
                    if (r.Name.ToLower().IndexOf("#" + col.ColumnName, StringComparison.Ordinal) != -1)
                        return true;
                    //check the inputs
                    foreach (Input i in r.Inputs)
                        if (i.Text.ToLower().IndexOf("#" + col.ColumnName, StringComparison.Ordinal) != -1)
                            return true;
                    //check the outputs and commands
                    foreach (Output o in r.Outputs)
                    {
                        if (o.Text.ToLower().IndexOf("#" + col.ColumnName, StringComparison.Ordinal) != -1)
                            return true;
                        if (o.Cmd.ToLower().IndexOf("#" + col.ColumnName, StringComparison.Ordinal) != -1)
                            return true;
                    }
                } //foreach column

            return false;
        } //isTemplateRule(Rule r, System.Data.DataTable dataTable)

        public Rule CreateRuleFromTemplateRow(Rule baseRule, DataColumnCollection columns, DataRow row, int index)
        {
            var newRule = new Rule();
            newRule.Id = baseRule.Id + "_" + index;
            newRule.Name = replaceTemplateData(baseRule.Name, columns, row);
            foreach (Input i in baseRule.Inputs)
            {
                var newInput = new Input();
                newInput.Id = i.Id + "_" + index;
                newInput.Text = replaceTemplateData(i.Text, columns, row);
                newInput.Condition = replaceTemplateData(i.Condition, columns, row);
                newRule.Inputs.Add(newInput);
            }

            foreach (Output o in baseRule.Outputs)
            {
                var newOutput = new Output();
                newOutput.Id = o.Id + "_" + index;
                newOutput.Text = replaceTemplateData(o.Text, columns, row);
                newOutput.Cmd = replaceTemplateData(o.Cmd, columns, row);
                newOutput.Condition = replaceTemplateData(o.Condition, columns, row);
                newRule.Outputs.Add(newOutput);
            }

            foreach (Rule child in baseRule.Children)
                newRule.Children.Add(CreateRuleFromTemplateRow(child, columns, row, index));
            newRule.VirtualParents = baseRule.VirtualParents;
            return newRule;
        } //createRuleFromTemplateRow(Rule baseRow, DataRow row, int index)

        private string replaceTemplateData(string text, DataColumnCollection columns, DataRow row)
        {
            foreach (DataColumn col in columns)
            {
                var start = text.ToLower().IndexOf("#" + col.ColumnName, StringComparison.Ordinal);
                while (start != -1)
                {
                    var end = start + col.ColumnName.Length + 1;
                    if (end < text.Length)
                        text = text.Substring(0, start) + row[col.ColumnName] + text.Substring(end);
                    else
                        text = text.Substring(0, start) + row[col.ColumnName];
                    if (start + 1 < text.Length)
                        start = text.ToLower().IndexOf("#" + col.ColumnName, start + 1, StringComparison.Ordinal);
                    else
                        start = -1;
                }
            }

            return text;
        } //replaceTemplateData(string text, DataRow row)


        /*
         * Accessor Methods
         * 
         */
        public bool IsDescendant(Rule rParent, Rule rChild)
        {
            return isDescendant(rParent, rChild, rParent.Children);
        }

        private bool isDescendant(Rule rParent, Rule rChild, ArrayList children)
        {
            if (rParent.Children.Contains(rChild))
                return true;
            foreach (Rule r in children)
                return isDescendant(r, rChild, r.Children);
            return false;
        }

        public void IncBuild()
        {
            Build++;
        }

        public string GetNewRuleId()
        {
            return TextToolbox.GetNewId();
        }

        public Rule GetRuleByNameOrId(string stName)
        {
            return getRuleByNameOrId(stName, Rules);
        }

        private Rule getRuleByNameOrId(string stName, ArrayList rules)
        {
            //return null if not found
            if (rules != null)
            {
                var searchName = stName.ToLower().Trim();
                var subName = "";
                var slashPos = searchName.IndexOf('/');
                //if it's a path like "top/child1/child2"
                if (slashPos != -1)
                {
                    if (slashPos != searchName.Length) //if the slash isn't at the end
                        subName = searchName.Substring(slashPos + 1);
                    searchName = searchName.Substring(0, slashPos);
                }

                var todo = new ArrayList();
                todo.AddRange(rules);
                while (rules.Count != 0)
                {
                    var r = (Rule) todo[0];
                    todo.RemoveAt(0);
                    if (r.Name.ToLower() == searchName || r.Id.ToLower() == searchName)
                    {
                        if (subName != "")
                        {
                            var subRule = getRuleByNameOrId(subName, r.Children);
                            if (subRule != null)
                                return subRule;
                            //if we didn't find it here, keep looking
                        }
                        else
                        {
                            return r;
                        }
                    } //end if name or matches

                    //for breadth first search
                    if (r.Children != null && r.Children.Count != 0)
                        todo.AddRange(r.Children);
                } //end foreach rule
            } //if rules isn't null

            return null;
        } //getRuleByNameOrId(string stName, ArrayList rules)

        public Rule GetRule(string id)
        {
            return getRule(id, Rules);
        } //GetRule(int id)

        private Rule getRule(string id, ArrayList rules)
        {
            //return null if not found

            if (rules != null)
                foreach (Rule r in rules)
                {
                    if (r.Id == id) return r;
                    var subRule = getRule(id, r.Children);
                    if (subRule != null)
                        return subRule;
                }

            return null;
        } //getRule(int id, ArrayList rules)

        public Rule CloneRule(Rule r)
        {
            var rClone = new Rule();
            rClone.Name = r.Name;
            rClone.Id = GetNewRuleId();
            foreach (Input i in r.Inputs)
                rClone.Inputs.Add(CloneInput(i, r));
            foreach (Output o in r.Outputs)
                rClone.Outputs.Add(CloneOutput(o, r));
            foreach (string id in r.VirtualParents)
                rClone.VirtualParents.Add(id);
            foreach (Rule rRecursive in r.Children)
                rClone.Children.Add(CloneRule(rRecursive));
            return rClone;
        }

        public Input CloneInput(Input i, Rule r)
        {
            var iClone = new Input();
            iClone.Text = i.Text;
            iClone.Condition = i.Condition;
            iClone.Id = r.GetNewInputId();
            return iClone;
        }

        public Output CloneOutput(Output o, Rule r)
        {
            var oClone = new Output();
            oClone.Text = o.Text;
            oClone.Cmd = o.Cmd;
            oClone.Condition = o.Condition;
            oClone.Id = r.GetNewOutputId();
            return oClone;
        }
    } //class KnowledgeBase

    [Serializable]
    public class KnowledgeBaseInfo : ISerializable
    {
        public enum CategoryType
        {
            Conversational,
            Educational,
            PersonalAssistant,
            Entertainment,
            Reference,
            Other
        }

        public enum LanguageType
        {
            Dutch,
            English,
            French,
            German,
            Italian,
            Japanese,
            Korean,
            Portuguese,
            Russian,
            Spanish,
            Other
        }

        private string _comment; //drop description in favor of this

        public string Author;
        public string AuthorWebsite;
        public CategoryType Category;
        public string Copyright;
        public DateTime CreationDate;
        public LanguageType Language;
        public DateTime LastUpdateDate;
        public string License;
        public KnowledgeBaseRating Rating;
        public string Test;

        public KnowledgeBaseInfo()
        {
            Author = "";
            Copyright = "";
            License = "";
            AuthorWebsite = "";
            CreationDate = DateTime.Now;
            LastUpdateDate = DateTime.Now;
            Rating = new KnowledgeBaseRating();
            Language = LanguageType.English;
            Category = CategoryType.Other;
            Comment = "";
        } //KnowledgeBaseInfo

        protected KnowledgeBaseInfo(SerializationInfo info, StreamingContext context)
        {
            Author = info.GetString("a");
            Copyright = info.GetString("copy");
            License = info.GetString("lic");
            AuthorWebsite = info.GetString("aw");
            CreationDate = (DateTime) info.GetValue("cd", typeof(DateTime));
            LastUpdateDate = (DateTime) info.GetValue("lud", typeof(DateTime));
            Rating = (KnowledgeBaseRating) info.GetValue("r", typeof(KnowledgeBaseRating));
            Language = (LanguageType) info.GetValue("lang", typeof(LanguageType));
            Category = (CategoryType) info.GetValue("cat", typeof(CategoryType));
            Comment = info.GetString("comment");
            //use a try/catch block around any new vales
        }

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                var index = _comment.IndexOf('\n');
                if (index != -1 && (index == 0 || _comment[index - 1] != '\r'))
                    _comment = _comment.Replace("\n", "\r\n");
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("a", Author);
            info.AddValue("copy", Copyright);
            info.AddValue("lic", License);
            info.AddValue("aw", AuthorWebsite);
            info.AddValue("cd", CreationDate);
            info.AddValue("lud", LastUpdateDate);
            info.AddValue("r", Rating);
            info.AddValue("lang", Language);
            info.AddValue("cat", Category);
            info.AddValue("comment", Comment);
        }

        public override string ToString()
        {
            var stRet = "";
            stRet += "Author: " + Author + "\r\n";
            stRet += "Author's Website: " + AuthorWebsite + "\r\n";
            stRet += "Copyright: " + Copyright + "\r\n";
            stRet += "License: " + License + "\r\n";
            stRet += "Creation Date: " + CreationDate.ToString("G", null) + "\r\n";
            stRet += "Last Update Date: " + LastUpdateDate.ToString("G", null) + "\r\n";
            stRet += "Rating: " + Rating + "\r\n";
            stRet += "Category: " + Category + "\r\n";
            stRet += "Language: " + Language + "\r\n";
            stRet += "Comment: " + Comment;
            return stRet;
        } //ToString()
    } //KnowledgeBaseInfo

    [Serializable]
    public class KnowledgeBaseRating : ISerializable
    {
        public enum RatingLevel
        {
            Kids, //Targeted for kids (educational value, positive, etc...)
            General, //This is for everyone
            Teens, //Teens and above.  Think twice before letting a kid use it
            MatureAudience, //Adults only
            Unknown //Not rated
        }

        public string Description; //tell us why you rated it that way
        public bool Language; //contains strong language
        public bool Other; //some other crazy reason
        public RatingLevel Rating;
        public bool Sexual; //contains sexual context (probably strong language too)
        public bool Violence; //people getting hurt by others (fictional or based on reality?)

        public KnowledgeBaseRating()
        {
            Rating = RatingLevel.Unknown;
            Language = false;
            Sexual = false;
            Violence = false;
            Other = false;
            Description = "";
        }

        protected KnowledgeBaseRating(SerializationInfo info, StreamingContext context)
        {
            Rating = (RatingLevel) info.GetValue("r", typeof(RatingLevel));
            Language = info.GetBoolean("l");
            Sexual = info.GetBoolean("s");
            Violence = info.GetBoolean("v");
            Other = info.GetBoolean("o");
            Description = info.GetString("d");
            //use a try/catch block around any new vales		
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("r", Rating);
            info.AddValue("l", Language);
            info.AddValue("s", Sexual);
            info.AddValue("v", Violence);
            info.AddValue("o", Other);
            info.AddValue("d", Description);
        }


        public override string ToString()
        {
            var stRet = "";
            stRet += Rating.ToString();
            if (Language || Sexual || Violence || Other)
            {
                stRet += " for: ";
                if (Language)
                    stRet += "Language ";
                if (Sexual)
                    stRet += "Sexual ";
                if (Violence)
                    stRet += "Violence ";
                if (Other)
                    stRet += "Other ";
            }

            stRet += "\r\nRating Description: " + Description + "\r\n";
            return stRet;
        } //ToString()
    } //class KnowledgeBaseRating

    //NOTE: This is only used for copy/paste not for binary serialization to a file
    [Serializable]
    public class Rule
    {
        private string _id;

        private string _name;

        [XmlArrayItem("Rule")] public ArrayList Children;

        [XmlArrayItem("Input")] public ArrayList Inputs;

        [XmlArrayItem("Output")] public ArrayList Outputs;

        [XmlArrayItem("VirtualParent")] public ArrayList VirtualParents; //Backreference from ActivationList

        public Rule()
        {
            Inputs = new ArrayList();
            Outputs = new ArrayList();
            Children = new ArrayList();
            VirtualParents = new ArrayList();
        }

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /*
         * Modifier Methods
         */

        public void AddInput(string stText, string stCond)
        {
            var inputNew = new Input();
            inputNew.Text = stText;
            inputNew.Condition = stCond;
            inputNew.Id = GetNewInputId();
            Inputs.Add(inputNew);
        }

        public void AddOutput(string stText, string stCond, string stCmd)
        {
            var outputNew = new Output();
            outputNew.Text = stText;
            outputNew.Condition = stCond;
            outputNew.Cmd = stCmd;
            outputNew.Id = GetNewOutputId();
            Outputs.Add(outputNew);
        }

        public void UpdateInput(string stText, string stCond, string id)
        {
            var i = GetInput(id);
            i.Text = stText;
            i.Condition = stCond;
        }

        public void UpdateOutput(string stText, string stCond, string stCmd, string id)
        {
            var o = GetOutput(id);
            o.Text = stText;
            o.Condition = stCond;
            o.Cmd = stCmd;
        }

        /*
         * Accessor Methods
         * 
         */
        public Input GetInput(string id)
        {
            foreach (Input i in Inputs)
                if (i.Id == id)
                    return i;
            return null;
        }

        public Output GetOutput(string id)
        {
            foreach (Output o in Outputs)
                if (o.Id == id)
                    return o;
            return null;
        }

        public string GetNewInputId()
        {
            return TextToolbox.GetNewId();
        }

        public string GetNewOutputId()
        {
            return TextToolbox.GetNewId();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Rule Name: " + Name + "\r\n");
            foreach (Input i in Inputs) sb.Append(i);
            foreach (Output o in Outputs) sb.Append(o);
            foreach (Rule r in Children) sb.Append(r);
            return sb.ToString();
        }

        public string ToRtf()
        {
            return ToRtf(0);
        }

        public string ToRtf(int spaces)
        {
            var stPar = @"\pard ";
            if (spaces > 0)
                stPar = @"\pard\li" + 15 * spaces + " ";
            var sb = new StringBuilder();
            sb.Append(stPar + @"\cf1 " + "Rule Name: " + @"\cf2 " + Name + "\\par\r\n");
            foreach (Input i in Inputs) sb.Append(i.ToRtf());
            foreach (Output o in Outputs) sb.Append(o.ToRtf());
            foreach (Rule rc in Children) sb.Append(rc.ToRtf(spaces + 10));
            return sb.ToString();
        }
    } //class Rule

    //NOTE: This is only used for copy/paste not for binary serialization to a file
    [Serializable]
    public class Input
    {
        private string _condition;

        private string _id;

        private string _text;

        public Input()
        {
            _id = "";
            _text = "";
            _condition = "";
        }

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public string Text
        {
            get => _text;
            set => _text = value;
        }

        public string Condition
        {
            get => _condition;
            set => _condition = value;
        }

        public override string ToString()
        {
            var stCond = "";
            if (Condition != null && Condition != "")
                stCond = "|Cond: " + Condition + "\r\n";
            return "Input Text: " + Text + "\r\n" + stCond;
        }

        public string ToRtf()
        {
            var stCond = "";
            if (Condition != null && Condition != "")
                stCond = "|Cond: " + Condition + "\r\n";
            return "\\cf1 Input Text: " + @"\cf3 " + Text.Replace("\r\n", "\\par\r\n") + "\\par\r\n" + stCond;
        }
    } //class Input

    [Serializable]
    public class Output : ISerializable
    {
        private string _cmd;

        private string _condition;
        private string _id;

        private string _text;

        public Output()
        {
            _id = "";
            _text = "";
            _condition = "";
            _cmd = "";
        }

        protected Output(SerializationInfo info, StreamingContext context)
        {
            _id = info.GetString("i");
            _text = info.GetString("t");
            _cmd = info.GetString("c");
            try
            {
                _condition = info.GetString("cond");
            }
            catch
            {
                _condition = "";
            }

            //use a try/catch block around any new vales
        }

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                var index = _text.IndexOf('\n');
                if (index != -1 && (index == 0 || _text[index - 1] != '\r'))
                    _text = _text.Replace("\n", "\r\n");
            }
        }

        public string Condition
        {
            get => _condition;
            set => _condition = value;
        }

        public string Cmd
        {
            get => _cmd;
            set => _cmd = value;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("i", _id);
            info.AddValue("t", _text);
            info.AddValue("c", _cmd);
            info.AddValue("cond", _condition);
        }

        public override string ToString()
        {
            var stCmd = "";
            if (Cmd != null && Cmd != "")
                stCmd = "|Cmd: " + Cmd + "\r\n";
            var stCond = "";
            if (Condition != null && Condition != "")
                stCond = "|Cond: " + Condition + "\r\n";
            return "Output Text: " + Text + "\r\n" + stCmd + stCond;
        }

        public string ToRtf()
        {
            var stCmd = "";
            if (Cmd != null && Cmd != "")
                stCmd = "\\cf1 |Cmd: \\cf4 " + Cmd + "\\par\r\n";
            var stCond = "";
            if (Condition != null && Condition != "")
                stCond = "\\cf1 |Cond: \\cf4 " + Condition + "\\par\r\n";
            return "\\cf1 Output Text: " + @"\cf4 " + Text.Replace("\r\n", "\\par\r\n") + "\\par\r\n" + stCmd + stCond;
        }
    } //class Output

    public enum ResourceFileType
    {
        SynonymFile,
        VerbotPluginFile,
        ReplacementProfileFile,
        TemplateDataFile,
        CodeModuleFile,
        Other
    }

    public class ResourceFile
    {
        public string Filename { get; set; }

        public ResourceFileType Filetype { get; set; }

        public override string ToString()
        {
            return Filename;
        }
    } //class ResourceFile
} //namespace Conversive.Verbot4