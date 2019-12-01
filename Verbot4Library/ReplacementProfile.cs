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
using System.Xml.Serialization;

namespace Conversive.Verbot4
{
    /// <summary>
    ///     Replaces input and output text with the given strings.
    /// </summary>
    public class ReplacementProfile
    {
        /*
         * Unserialized Attributes
         */

        [XmlArrayItem("InputReplacement")] public ArrayList InputReplacements;

        [XmlArrayItem("Replacement")] public ArrayList Replacements;

        public ReplacementProfile()
        {
            Replacements = new ArrayList();
            InputReplacements = new ArrayList();
        }

        [XmlIgnore] public bool Changed { get; set; }
    }

    [Serializable]
    public class Replacement : ISerializable
    {
        private string _textForAgent;

        private string _textForOutput;
        private string _textToFind;

        public Replacement()
        {
            TextToFind = "";
            TextForAgent = "";
            TextForOutput = "";
        }

        protected Replacement(SerializationInfo info, StreamingContext context)
        {
            TextToFind = info.GetString("ttf");
            TextForAgent = info.GetString("tfa");
            TextForOutput = info.GetString("tfo");
            //use a try/catch block around any new vales
        }

        public string TextToFind
        {
            get => _textToFind;
            set => _textToFind = value;
        }

        public string TextForAgent
        {
            get => _textForAgent;
            set => _textForAgent = value;
        }

        public string TextForOutput
        {
            get => _textForOutput;
            set => _textForOutput = value;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ttf", TextToFind);
            info.AddValue("tfa", TextForAgent);
            info.AddValue("tfo", TextForOutput);
        }
    } //class Replacement

    [Serializable]
    public class InputReplacement : ISerializable
    {
        private string _textToFind;

        private string _textToInput;

        public InputReplacement()
        {
            _textToFind = "";
            _textToInput = "";
        }

        public InputReplacement(string stTextToFind, string stTextToInput)
        {
            _textToFind = stTextToFind;
            _textToInput = stTextToInput;
        }

        protected InputReplacement(SerializationInfo info, StreamingContext context)
        {
            TextToFind = info.GetString("ttf");
            _textToInput = info.GetString("tti");
            //use a try/catch block around any new vales
        }

        public string TextToFind
        {
            get => _textToFind;
            set => _textToFind = value;
        }

        public string TextToInput
        {
            get => _textToInput;
            set => _textToInput = value;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ttf", TextToFind);
            info.AddValue("tti", TextToInput);
        }
    } //class InputReplacement
}