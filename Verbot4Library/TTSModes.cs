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

using System.Collections;
using System.Xml.Serialization;

namespace Conversive.Verbot4
{
    /// <summary>
    ///     List of TTS Modes.
    /// </summary>
    public class TtsModes
    {
        [XmlArrayItem("TTSMode", typeof(TtsMode))]
        public ArrayList TtsModesList;

        public TtsModes()
        {
            TtsModesList = new ArrayList();
        }
    }

    public class TtsMode
    {
        [XmlElement("name", typeof(string))] public string Name;
        [XmlElement("id", typeof(string))] public string Id;


        public TtsMode()
        {
            Name = "";
            Id = "";
        }
    }
}