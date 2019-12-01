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
using System.Security.Cryptography;
using System.Text;

namespace Conversive.Verbot4
{
    /// <summary>
    ///     Toolbox of common functions.
    /// </summary>
    public class ConversiveGeneralTextToolbox
    {
        public static string CleanCsvValue(string stValue, char fieldDelimiter, char textDelimiter)
        {
            stValue = stValue.Replace("\n", "");
            stValue = stValue.Replace("\r", "");
            stValue = stValue.Replace(textDelimiter.ToString(), "");
            stValue = stValue.Replace(fieldDelimiter.ToString(), "");
            return stValue;
        }

        public static string MakeCsv(ArrayList data, char fieldDelimiter, char textDelimiter)
        {
            return MakeCsv(data, fieldDelimiter, textDelimiter, true);
        }

        public static string MakeCsv(ArrayList data, char fieldDelimiter, char textDelimiter, bool bAddHeaderRow)
        {
            //data is an array-list of hash-tables, the keys of the first item will be the headings
            var sb = new StringBuilder();

            for (var i = 0; i < data.Count; i++)
            {
                var oRow = data[i];
                if (oRow is Hashtable)
                {
                    var stRow = "";
                    var htRow = (Hashtable) oRow;
                    if (bAddHeaderRow && i == 0) //add header row
                    {
                        foreach (var oKey in htRow.Keys)
                        {
                            if (stRow != "")
                                stRow += fieldDelimiter;
                            stRow += textDelimiter + oKey.ToString() + textDelimiter;
                        }

                        if (stRow != "")
                        {
                            sb.Append(stRow + "\r\n");
                            stRow = "";
                        }
                    }

                    foreach (var oKey in htRow.Keys)
                    {
                        if (stRow != "")
                            stRow += fieldDelimiter;
                        stRow += textDelimiter + CleanCsvValue(htRow[oKey].ToString(), fieldDelimiter, textDelimiter) +
                                 textDelimiter;
                    }

                    if (stRow != "")
                        sb.Append(stRow + "\r\n");
                }
            }

            return sb.ToString();
        }

        public static ArrayList SplitCsv(string data, char fieldDelimiter, char textDelimiter)
        {
            //notes: fields don't need "'s around them unless they have ,'s or \n's
            //reference: http://www.creativyst.com/Doc/Articles/CSV/CSV01.htm
            //sample line => "Last, First",,27,m
            //should become => {"Last, First", "", "27", "m"}

            var lines = new ArrayList();
            var fields = new ArrayList();

            var startIndex = 0;

            while (startIndex < data.Length)
            {
                //skip white space
                while (startIndex < data.Length &&
                       (data[startIndex] == ' ' && fieldDelimiter != ' ' ||
                        data[startIndex] == '\t' && fieldDelimiter != '\t'))
                    startIndex++;

                //check for end of data
                if (startIndex >= data.Length)
                {
                    if (fields.Count > 0)
                        lines.Add(fields);
                    break; //return
                }

                //check for end of line

                if (data[startIndex] == '\n' || data[startIndex] == '\r')
                {
                    if (fields.Count > 0)
                    {
                        lines.Add(fields);
                        fields = new ArrayList();
                    }

                    startIndex++;
                }

                //check for empty field
                else if (data[startIndex] == fieldDelimiter)
                {
                    fields.Add("");
                    startIndex++;
                }

                //check for textDelimiter
                else if (data[startIndex] == textDelimiter)
                {
                    //read quoted text field
                    var field = new StringBuilder();
                    startIndex++;
                    while (startIndex + 1 < data.Length && data[startIndex] != textDelimiter &&
                           data[startIndex] != '\n' && data[startIndex] != '\r')
                    {
                        field.Append(data[startIndex]);
                        startIndex++;
                    }

                    fields.Add(field.ToString());
                    startIndex += 2; //skip the textDelimiter and the fieldDelimiter
                }
                else
                {
                    //read unquoted field
                    var field = new StringBuilder();
                    while (startIndex < data.Length && data[startIndex] != fieldDelimiter && data[startIndex] != '\n' &&
                           data[startIndex] != '\r')
                    {
                        field.Append(data[startIndex]);
                        startIndex++;
                    }

                    fields.Add(field.ToString().Trim());
                    startIndex++;
                } //else it's an unquoted field
            } //while not done

            if (fields.Count > 0)
                lines.Add(fields);
            return lines;
        } //SplitCSVLine(string line, char fieldDelimiter, char textDelimiter)

        public static string GetMd5String(string text)
        {
            var md5 = MD5.Create();
            return ByteArrayToString(md5.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }

        private static string ByteArrayToString(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length);
            for (var i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
            return sb.ToString();
        } //byteArrayToString(byte[] bytes)
    }
}