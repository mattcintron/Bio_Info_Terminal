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
    ///     Defines a "virtual" class file for use in the Verbot 4 Engine.
    /// </summary>
    [Serializable]
    public class CodeModule : ISerializable
    {
        /*
         * Unserialized Attributes
         */


        private bool _changed;

        private string _includes;

        private CodeLanguages _language;
        private string _name;

        private string _vars;

        [XmlArrayItem("Function")] public ArrayList Functions;

        public CodeModule()
        {
            _name = "";
            _language = CodeLanguages.CSharp;
            _includes = "";
            _vars = "";
            _changed = false;
            Functions = new ArrayList();
        }

        protected CodeModule(SerializationInfo info, StreamingContext context)
        {
            _changed = false;
            _name = info.GetString("n");
            _language = (CodeLanguages) info.GetValue("l", typeof(CodeLanguages));
            _includes = info.GetString("i");
            _vars = info.GetString("v");
            Functions = new ArrayList();
            Functions = (ArrayList) info.GetValue("f", typeof(ArrayList));

            //use a try/catch block around any new vales
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public CodeLanguages Language
        {
            get => _language;
            set => _language = value;
        }

        public string Includes
        {
            get => _includes;
            set => _includes = value;
        }

        public string Vars
        {
            get => _vars;
            set => _vars = value;
        }

        [XmlIgnore]
        public bool Changed
        {
            get => _changed;
            set => _changed = value;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("n", _name);
            info.AddValue("l", _language);
            info.AddValue("i", _includes);
            info.AddValue("v", _vars);
            info.AddValue("f", Functions);
        }

        public override string ToString()
        {
            return _name;
        }

        public Function GetFunction(string id)
        {
            foreach (Function f in Functions)
                if (f.Id == id)
                    return f;
            return null;
        }

        public Function AddFunction(string name, string id)
        {
            var f = new Function();
            f.Name = name;
            f.Id = id;
            Functions.Add(f);
            return f;
        }

        public void DeleteFunction(string id)
        {
            var f = GetFunction(id);
            Functions.Remove(f);
        }
    } //class CodeModule

    [Serializable]
    public class Function : ISerializable
    {
        private string _code;
        private string _id;

        private string _name;

        private string _parameters;

        private string _returnType;

        public Function()
        {
            _name = "";
            _returnType = "string";
            _parameters = "";
            _code = "";
            _id = "";
        }

        protected Function(SerializationInfo info, StreamingContext context)
        {
            _name = info.GetString("n");
            _returnType = info.GetString("rt");
            _parameters = info.GetString("p");
            _code = info.GetString("c");
            _id = info.GetString("id");

            //use a try/catch block around any new vales
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

        public string ReturnType
        {
            get => _returnType;
            set => _returnType = value;
        }

        public string Parameters
        {
            get => _parameters;
            set => _parameters = value;
        }

        public string Code
        {
            get => _code;
            set => _code = value;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("n", _name);
            info.AddValue("rt", _returnType);
            info.AddValue("p", _parameters);
            info.AddValue("c", _code);
            info.AddValue("id", _id);
        }
    } //class code module

    public enum CodeLanguages
    {
        CSharp,
        Php
    }
}