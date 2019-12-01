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
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CSharp;

namespace Conversive.Verbot4
{
    /// <summary>
    ///     Toolbox for dynamically compiling and executing C# code.
    /// </summary>
    public class CSharpToolbox
    {
        public delegate void CompileError(string errorText, string lineText);

        public delegate void CompileWarning(string warningText, string lineText);

        private readonly Hashtable _conditions; //keys are ids, values are condition strings
        private readonly Hashtable _outputs; //keys are ids, values are output strings

        private readonly Hashtable _threadJobs; //keys are Thread objects, values are jobs or results

        private readonly string closeTag = "?>";

        private readonly string COND_PREFIX = "Cond_";

        private readonly string openTag = "<?csharp";
        private readonly string OUTPUT_PREFIX = "Output_";

        private Assembly _assembly;

        public CSharpToolbox()
        {
            CodeModules = new ArrayList();
            _conditions = new Hashtable();
            _outputs = new Hashtable();
            _assembly = null;
            _threadJobs = new Hashtable();
        }

        public bool ContainsCode
        {
            get
            {
                var bRet = true;
                if (CodeModules.Count == 0 && _conditions.Count == 0 && _outputs.Count == 0)
                    bRet = false;
                return bRet;
            }
        }

        public ArrayList CodeModules { get; set; }

        public string Code { get; private set; }
        public event CompileWarning OnCompileWarning;
        public event CompileError OnCompileError;

        public void AddCodeModule(CodeModule cm)
        {
            CodeModules.Add(cm);
        }

        public void AddCondition(string id, string condition)
        {
            _conditions[id] = condition;
        }

        public bool ContainsCSharpTags(string text)
        {
            //need to see escaped csharp tags as csharp tags
            //so that we can remove the backslashes later in the code
            return text.IndexOf(openTag, StringComparison.Ordinal) != -1;
        }

        public void AddOutput(string id, string output)
        {
            _outputs[id] = output;
        }

        public void AssembleCode()
        {
            var assemblyCode = "";
            assemblyCode += "using System;\r\n"; //using lines?
            assemblyCode += "using System.Collections;\r\n";
            assemblyCode += "using Conversive.Verbot4;\r\n";
            foreach (CodeModule cm in CodeModules)
            {
                assemblyCode += "\r\n";
                //convert cm to class
                assemblyCode += codeModule2Class(cm);
            } //foreach(CodeModule cm in codeModules)

            assemblyCode += Conditions2Class();
            assemblyCode += Outputs2Class();

            Code = assemblyCode;
        }

        public bool Compile() //true if successful
        {
            bool success;
            AssembleCode(); //convert the data structures to code

            //compile class into assembly
            var codeProvider = new CSharpCodeProvider();

            //TODO: find how to create Complier in C# 6 
#pragma warning disable 618
            var compiler = codeProvider.CreateCompiler();
#pragma warning restore 618


            var parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;

            //this should come from some Using collection
            parameters.ReferencedAssemblies.Add("system.dll");
            //the following line doesn't work in web mode
            //parameters.ReferencedAssemblies.Add(Application.StartupPath + Path.DirectorySeparatorChar + "Verbot4Library.dll");
            //use this instead
            parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

            var results = compiler.CompileAssemblyFromSource(parameters, Code);
            if (!results.Errors.HasErrors) //if no errors
            {
                success = true;
                _assembly = results.CompiledAssembly;
            }
            else // some errors
            {
                success = false;
                foreach (CompilerError error in results.Errors)
                    if (error.IsWarning)
                    {
                        if (OnCompileWarning != null)
                            OnCompileWarning(error.ErrorText, getLineSubstring(Code, error.Line));
                    }
                    else
                    {
                        if (OnCompileError != null)
                            OnCompileError(error.ErrorText, getLineSubstring(Code, error.Line));
                    }
            }

            return success;
        } //Compile()

        public bool ExecuteCondition(string id)
        {
            return ExecuteCondition(id, new Hashtable());
        } //ExecuteCondition(string id)

        public bool ExecuteCondition(string id, Hashtable vars)
        {
            try
            {
                if (_conditions[id] == null) return true;

                if (_assembly != null)
                {
                    var exeThread = new Thread(RunCondition);
                    var job = new Hashtable();
                    job["name"] = COND_PREFIX + id;
                    job["vars"] = new StringTable(vars);
                    _threadJobs[exeThread] = job;
                    exeThread.IsBackground = true;
                    exeThread.Start();
                    exeThread.Join(5000); //join when done or in 5 sec.
                    var result = (bool) _threadJobs[exeThread];
                    _threadJobs[exeThread] = null;
                    return result;
                }
            }
            catch
            {
                // ignored
            }

            return false; //error
        } //ExecuteCondition(string id, Hashtable vars)

        public void RunCondition()
        {
            try
            {
                var job = (Hashtable) _threadJobs[Thread.CurrentThread];
                if (job != null)
                {
                    var type = _assembly.GetType("Conditions");
                    object[] args = {job["vars"]};
                    var result = (bool) type.InvokeMember((string) job["name"], BindingFlags.InvokeMethod, null,
                        _assembly, args);
                    _threadJobs[Thread.CurrentThread] = result;
                }
            }
            catch
            {
                _threadJobs[Thread.CurrentThread] = false;
            }
        } //runCondition()

        public string ExecuteOutput(string id)
        {
            return ExecuteOutput(id, new Hashtable());
        } //ExecuteOutput(string id)

        public string ExecuteOutput(string id, Hashtable vars)
        {
            var output = "";
            var consoleOut = Console.Out;
            try
            {
                if (_assembly != null)
                {
                    var exeThread = new Thread(RunOutput);
                    var job = new Hashtable();
                    job["name"] = OUTPUT_PREFIX + id;
                    job["vars"] = new StringTable(vars);
                    _threadJobs[exeThread] = job;
                    exeThread.IsBackground = true;
                    exeThread.Start();
                    exeThread.Join(5000); //join when done or in 5 sec.
                    //copy any vars changes back into the main vars object
                    vars.Clear(); //we need to do this in case any were deleted
                    foreach (string key in ((StringTable) job["vars"]).Keys)
                        if (vars[key] == null || vars[key] is string)
                            vars[key] = ((StringTable) job["vars"])[key];
                    output = (string) _threadJobs[exeThread];
                    _threadJobs[exeThread] = null;
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                Console.SetOut(consoleOut);
            }

            return output;
        } //ExecuteOutput(string id, Hashtable vars)

        public void RunOutput()
        {
            try
            {
                var job = (Hashtable) _threadJobs[Thread.CurrentThread];
                if (job != null)
                {
                    var memStream = new MemoryStream(512);
                    var writer = new StreamWriter(memStream);
                    Console.SetOut(writer);

                    var type = _assembly.GetType("Outputs");
                    object[] args = {job["vars"]};
                    type.InvokeMember((string) job["name"], BindingFlags.InvokeMethod, null, _assembly, args);

                    writer.Flush();
                    var byteArray = memStream.ToArray();
                    var charArray = Encoding.UTF8.GetChars(byteArray);
                    _threadJobs[Thread.CurrentThread] = new string(charArray);
                }
            }
            catch
            {
                _threadJobs[Thread.CurrentThread] = "";
            }
        } //runCondition()

        public string ShowCodeModuleClassCode(CodeModule cm)
        {
            return codeModule2Class(cm);
        }

        public bool ConditionExists(string id)
        {
            return _conditions[id] != null;
        }

        public bool OutputExists(string id)
        {
            return _outputs[id] != null;
        }

        private string codeModule2Class(CodeModule cm)
        {
            var sb = new StringBuilder();
            sb.Append("public class ");
            sb.Append(cm.Name);
            sb.Append(" {\r\n"); //open class
            foreach (Function f in cm.Functions)
            {
                sb.Append("public ");
                sb.Append("static "); //all methods are static
                sb.Append(f.ReturnType);
                sb.Append(" ");
                sb.Append(f.Name);
                sb.Append("(");
                sb.Append(f.Parameters);
                sb.Append(") {\r\n"); //open method
                sb.Append(f.Code);
                sb.Append("}\r\n"); //close method
            }

            sb.Append("}"); //close class
            return sb.ToString();
        } //CodeModule2Class()

        private string Conditions2Class()
        {
            var sb = new StringBuilder();
            sb.Append("public class Conditions");
            sb.Append(" {\r\n"); //open class
            foreach (string id in _conditions.Keys)
            {
                sb.Append("public static bool ");
                sb.Append(COND_PREFIX); //conditional prefix
                sb.Append(id);
                sb.Append("(StringTable vars) {\r\n"); //open method
                sb.Append("return ");
                sb.Append((string) _conditions[id]);
                sb.Append(";}\r\n"); //close method
            }

            sb.Append("}"); //close class
            return sb.ToString();
        } //conditions2Class()

        private string Outputs2Class()
        {
            var sb = new StringBuilder();
            sb.Append("public class Outputs");
            sb.Append(" {\r\n"); //open class
            foreach (string id in _outputs.Keys)
            {
                sb.Append("public static void ");
                sb.Append(OUTPUT_PREFIX); //output prefix
                sb.Append(id);
                sb.Append("(StringTable vars) {\r\n"); //open method
                sb.Append(OutputToCode((string) _outputs[id]));
                sb.Append("}\r\n"); //close method
            }

            sb.Append("}"); //close class
            return sb.ToString();
        } //outputs2Class()

        private string OutputToCode(string text)
        {
            if (text == null)
                return "";

            var sb = new StringBuilder();
            string code;
            //break the text apart into text and code sections
            //test needs to be written to Console.Out
            //and code needs to be written as is
            while (text != "")
            {
                var openTagPos = text.IndexOf(openTag, StringComparison.Ordinal);
                while (openTagPos != -1 && TextToolbox.IsEscaped(text, openTagPos))
                {
                    text = text.Remove(openTagPos - 1, 1);
                    openTagPos = text.IndexOf(openTag, openTagPos, StringComparison.Ordinal);
                }

                if (openTagPos != 0) //we have uncode to process
                {
                    string uncode;
                    if (openTagPos == -1) //it's all uncode
                    {
                        uncode = text;
                        text = "";
                    }
                    else //it's uncode + code
                    {
                        uncode = text.Substring(0, openTagPos);
                        text = text.Substring(openTagPos);
                    }

                    sb.Append("Console.Write(\"");
                    sb.Append(escape(uncode));
                    sb.Append("\");\r\n");
                }
                else //we have code to process (open is at the beginning)
                {
                    var closeTagPos = text.IndexOf(closeTag, StringComparison.Ordinal);
                    while (closeTagPos != -1 && TextToolbox.IsEscaped(text, closeTagPos))
                    {
                        text = text.Remove(closeTagPos - 1, 1);
                        closeTagPos = text.IndexOf(closeTag, closeTagPos, StringComparison.Ordinal);
                    }

                    if (closeTagPos == -1)
                        closeTagPos = text.Length;
                    code = text.Substring(openTag.Length, closeTagPos - openTag.Length).Trim();
                    if (code != "")
                        sb.Append(code);
                    if (closeTagPos + closeTag.Length < text.Length)
                        text = text.Substring(closeTagPos + closeTag.Length);
                    else
                        text = "";
                }
            } //while(text != "")

            return sb.ToString();
        } //outputToCode(string text)

        private string escape(string text)
        {
            //replaces \ with \\, and " with \"
            text = text.Replace("\\", "\\\\"); //backslash
            text = text.Replace("\"", "\\\""); //quote
            text = text.Replace("\r", "\\r"); //return
            text = text.Replace("\n", "\\n"); //new line
            text = text.Replace("\t", "\\t"); //tab
            text = text.Replace("\f", "\\f"); //line feed
            return text;
        }

        private string getLineSubstring(string text, int lineNum)
        {
            var splits = text.Split('\n');
            if (splits.Length >= lineNum)
                return splits[lineNum - 1].Trim();
            return "";
        } //getLine(string text, int lineNum)
    } //public class CSharpToolbox

    public class StringTable : Hashtable
    {
        public StringTable(Hashtable table) : base(table)
        {
        }

        public string this[string key]
        {
            get
            {
                key = key.ToLower();
                if (base[key] == null)
                    return (string) base[key]; //return null string
                return base[key].ToString();
            }
            set
            {
                key = key.ToLower();
                base[key] = value;
            }
        }
    } //class StringTable : Hashtable
} //namespace Verbot4Library