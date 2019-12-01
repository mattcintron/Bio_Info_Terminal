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
    ///     Stores the user's preferences.
    /// </summary>
    public class Verbot4Preferences
    {
        public string AgentFile;
        public int AgentPitch;
        public int AgentSpeed;
        public string AgentTtsMode;
        public int AutoenterTime;

        public int BoredomResponseTime;

        public string CharacterFile;
        public int CharacterTtsMode;

        [XmlArrayItem("KnowledgeBase")] public ArrayList KnowledgeBases;

        public string ScheduleFilePath;
        public string SkinPath;
        public bool UseConversiveCharacter = false;

        public Verbot4Preferences()
        {
            AgentFile = "merlin.acs";
            AgentTtsMode = ""; //Whatever the MSAgent wants
            AgentSpeed = 0;
            AgentPitch = 0;

            CharacterFile = "julia.ccs";
            CharacterTtsMode = 0;

            BoredomResponseTime = 2;
            AutoenterTime = 0;
            SkinPath = "";
            ScheduleFilePath = "";

            KnowledgeBases = new ArrayList();
        }
    }

    [Serializable]
    public class KnowledgeBaseItem : ISerializable
    {
        public int Build;
        public string Filename;
        public string Fullpath;

        [NonSerialized] [XmlIgnore] public KnowledgeBaseInfo Info;

        public bool Trusted;
        public bool Untrusted;
        public bool Used;

        public KnowledgeBaseItem()
        {
            Filename = "";
            Fullpath = "";
            Used = false;
            Trusted = false;
            Build = -1;
            Info = null;
        }

        protected KnowledgeBaseItem(SerializationInfo info, StreamingContext context)
        {
            Filename = info.GetString("fn");
            Fullpath = info.GetString("fp");
            Used = info.GetBoolean("u");
            Trusted = info.GetBoolean("t");
            Build = info.GetInt32("b");
            Info = null;
            //use a try/catch block around any new vales
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("fn", Filename);
            info.AddValue("fp", Fullpath);
            info.AddValue("u", Used);
            info.AddValue("t", Trusted);
            info.AddValue("b", Build);
        }

        public override string ToString()
        {
            return Filename;
        }
    }
}