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

namespace Conversive.Verbot4
{
    /// <summary>
    ///     Logs events to a text file.
    /// </summary>
    public class ChatTextLogger
    {
        private readonly string _logFilename;

        public ChatTextLogger(string logDirectory)
        {
            var dt = DateTime.Now;
            var count = 1;
            _logFilename = logDirectory + dt.ToString("yyyy-MM-dd") + "-" + count + ".txt";
            while (File.Exists(_logFilename))
            {
                count++;
                _logFilename = logDirectory + dt.ToString("yyyy-MM-dd") + "-" + count + ".txt";
            }

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);
            var fs = new FileStream(_logFilename, FileMode.Create);
            fs.Close();
        } 
        public void SetKnowledgeBases(ArrayList kbNames)
        {
            string output;

            var fs = new FileStream(_logFilename, FileMode.Append);

            output = "\r\n";
            foreach (string kbName in kbNames) output += "using: " + kbName + "\r\n";
            output += "\r\n";

            var data = Encoding.UTF8.GetBytes(output);
            fs.Write(data, 0, data.Length);

            fs.Flush();
            fs.Close();
        } //SetKnowledgeBases(Arraylist kbNames)

        public void Log(string input, string reply)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(_logFilename, FileMode.Append);
                var dt = DateTime.Now;
                var line = dt.ToString("HH:MM:ss") + "\tUser:\t" + input + "\r\n";
                line += dt.ToString("HH:MM:ss") + "\tVerbot:\t" + reply + "\r\n";
                var data = Encoding.UTF8.GetBytes(line);
                fs.Write(data, 0, data.Length);
            }
            finally
            {
                if (fs != null)
                    try
                    {
                        fs.Flush();
                        fs.Close();
                    }
                    catch
                    {
                        // ignored
                    }
            }
        } //Log(string input, string reply)
    } //ChatTextLogger
} //namespace Conversive.Verbot4