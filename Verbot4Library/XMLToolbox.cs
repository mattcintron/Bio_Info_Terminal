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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Conversive.Verbot4
{
    /// <summary>
    ///     Toolbox for reading and writing XML serialized files.
    /// </summary>
    public class XmlToolbox
    {
        public delegate void XmlLoadError(Exception e, string stPath);

        public delegate void XmlSaveError(Exception e, string stPath);

        private readonly XmlSerializer _xmlSerializer;

        public XmlToolbox(Type type)
        {
            Type[] types =
            {
                typeof(KnowledgeBase),
                typeof(Rule),
                typeof(Input),
                typeof(Output),
                typeof(ResourceFile),
                typeof(SynonymGroup),
                typeof(Synonym),
                typeof(Phrase),
                typeof(ReplacementProfile),
                typeof(Replacement),
                typeof(InputReplacement),
                typeof(CodeModule),
                typeof(Function),
                typeof(Verbot4Preferences),
                typeof(KnowledgeBaseItem),
                typeof(KnowledgeBaseInfo),
                typeof(KnowledgeBaseRating),
                typeof(Verbot4Skin),
                typeof(TtsModes),
                typeof(TtsMode),
                typeof(Font),
                typeof(ArrayList)
            };
            _xmlSerializer = new XmlSerializer(type, types);
        }

        public event XmlSaveError OnXmlSaveError;
        public event XmlLoadError OnXmlLoadError;

        private void RaiseXmlSaveError(Exception e, string stPath)
        {
            if (OnXmlSaveError != null) OnXmlSaveError(e, stPath);
        }

        private void RaiseXmlLoadError(Exception e, string stPath)
        {
            if (OnXmlLoadError != null) OnXmlLoadError(e, stPath);
        }

        public void SaveXml(object o, string stPath)
        {
            try
            {
                var fs = new FileStream(stPath, FileMode.Create);
                var sw = new StreamWriter(fs, Encoding.UTF8);
                _xmlSerializer.Serialize(sw, o);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                RaiseXmlSaveError(e, stPath);
                //TODO: add a line to an error log
            }
        }

        public object LoadXml(string stPath)
        {
            object obj = null;
            try
            {
                var fs = new FileStream(stPath, FileMode.Open);
                var xtr = new XmlTextReader(fs);
                obj = _xmlSerializer.Deserialize(xtr);
                fs.Flush();
                fs.Close();
                xtr.Close();
                return obj;
            }
            catch (Exception e)
            {
                RaiseXmlLoadError(e, stPath);
                //TODO: add a line to an error log
            }

            return obj;
        }
    }
}