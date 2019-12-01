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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Xml.Serialization;

namespace Conversive.Verbot4
{
    /// <summary>
    ///     Defines a list of equivalent words or phrases.
    /// </summary>
    public class SynonymGroup
    {
        /*
         * Unserialized Attributes
         */

        [XmlArrayItem("Synonym")] public ArrayList Synonyms;

        public SynonymGroup()
        {
            Changed = false;
            Synonyms = new ArrayList();
        }

        [XmlIgnore] public bool Changed { get; set; }

        /*
         * Modifier Methods
         */

        public string AddSynonym(string synonymName)
        {
            var synonymNew = new Synonym();
            synonymNew.Id = GetNewSynoymId();
            synonymNew.Name = synonymName;
            Synonyms.Add(synonymNew);
            return synonymNew.Id;
        }

        /*
         * Accessor Methods
         */

        public string GetNewSynoymId()
        {
            return TextToolbox.GetNewId();
        }

        public Synonym GetSynonym(string id)
        {
            //return null if not found
            foreach (Synonym s in Synonyms)
                if (s.Id == id)
                    return s;
            return null;
        } //GetPhrase(string id)
    } //class SynonymGroup

    [Serializable]
    public class Synonym : ISerializable
    {
        private string _id;
        private string _name;

        [XmlArrayItem("Phrase")] public ArrayList Phrases;

        public Synonym()
        {
            _name = "";
            Phrases = new ArrayList();
        }

        protected Synonym(SerializationInfo info, StreamingContext context)
        {
            _name = info.GetString("n");
            Phrases = (ArrayList) info.GetValue("p", typeof(ArrayList));
            //use a try/catch block around any new vales
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("n", _name);
            info.AddValue("p", Phrases);
        }

        /*
         * Modifier Methods
         */

        public string AddPhrase(string phraseText)
        {
            var phraseNew = new Phrase();
            phraseNew.Id = GetNewPhraseId();
            phraseNew.Text = phraseText;
            Phrases.Add(phraseNew);
            return phraseNew.Id;
        }

        /*
         * Accessor Methods
         */

        public string GetNewPhraseId()
        {
            return TextToolbox.GetNewId();
        }

        public Phrase GetPhrase(string id)
        {
            //return null if not found
            foreach (Phrase p in Phrases)
                if (p.Id == id)
                    return p;
            return null;
        } //GetPhrase(string id)

        public string GetPhrases()
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (Phrase p in Phrases)
            {
                if (!first)
                    sb.Append("|");
                sb.Append(p.Text);
                first = false;
            }

            return sb.ToString();
        } //GetPhrases()
    } //class Synonym

    [Serializable]
    public class Phrase : ISerializable, IComparable
    {
        private string _id;

        private string _text;

        public Phrase()
        {
            _text = "";
            _id = "";
        }

        protected Phrase(SerializationInfo info, StreamingContext context)
        {
            _text = info.GetString("t");
            _id = info.GetString("i");
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
            set => _text = value;
        }

        public int CompareTo(object other)
        {
            var siOther = (Phrase) other;
            return -1 * _text.Length.CompareTo(siOther._text.Length);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("t", _text);
            info.AddValue("i", _id);
        }
    } //Synonym
} //namespace Conversive.Verbot4