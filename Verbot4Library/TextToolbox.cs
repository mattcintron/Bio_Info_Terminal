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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Conversive.Verbot4
{
    /// <summary>
    ///     Toolbox of text manipulation functions.
    /// </summary>
    public class TextToolbox
    {
        private static readonly Random Random = new Random();

        public static string GetNewId()
        {
            return Random.Next(int.MinValue, int.MaxValue).ToString("X") +
                   Random.Next(int.MinValue, int.MaxValue).ToString("X");
        }

        public static string ReplaceOnInput(string text, ArrayList replacements)
        {
            //TODO: see if var bIsCapture is needed 
            // ReSharper disable once NotAccessedVariable
            var bIsCapture = false;
            return ReplaceOnInput(text, replacements, out bIsCapture);
        }

        /// <summary>
        ///     Get the extension for the file
        /// </summary>
        /// <param name="stFileNameOrFullPath"></param>
        /// <returns>To Lower of the file extension with the period (i.e. ".ckb")</returns>
        public static string GetFileExtension(string stFileNameOrFullPath)
        {
            var stRet = stFileNameOrFullPath;
            if (stRet.IndexOf(".", StringComparison.Ordinal) != -1)
                stRet = stRet.Substring(stRet.LastIndexOf(".", StringComparison.Ordinal)).ToLower();
            return stRet;
        }

        /// <summary>
        ///     Uses given InputReplacements to change text
        /// </summary>
        /// <param name="text">change text</param>
        /// <param name="replacements"></param>
        /// <param name="bIsCapture"></param>
        /// <returns>resulting replaced text</returns>
        public static string ReplaceOnInput(string text, ArrayList replacements, out bool bIsCapture)
        {
            var sb = new StringBuilder();
            if (text.IndexOf("[", StringComparison.Ordinal) == -1)
            {
                bIsCapture = false;
                if (replacements.Count > 0)
                {
                    var depth = 0;
                    for (var i = 0; i < text.Length; i++)
                    {
                        var replaced = false;
                        if (text[i] == '[')
                            depth++;
                        else if (text[i] == ']')
                            depth--;
                        else if (depth == 0)
                            foreach (InputReplacement ir in replacements)
                                if (i < text.Length - ir.TextToFind.Length + 1
                                    && ir.TextToFind == text.Substring(i, ir.TextToFind.Length))
                                {
                                    replaced = true;
                                    sb.Append(ir.TextToInput);
                                    i += ir.TextToFind.Length - 1;
                                    break; //can only replace with one thing
                                }

                        if (!replaced)
                            sb.Append(text[i]);
                    } //for(int i = 0; i < text.Length; i++)
                }
                else //if we don't have any input replacements, just do the default (remove accents)
                {
                    //TODO: add if(prefs.useDefaultInputReplacements)
                    sb.Append(ApplyDefaultInputReplacements(text));
                }
            }
            else
            {
                bIsCapture = true;
                sb.Append(text); //if it is a capture, we won't replace anything
            }

            return sb.ToString();
        } //ReplaceOnInput(string text, ArrayList replacements)

        public static string ReplaceSynonyms(string input, Hashtable synonyms)
        {
            var start = input.IndexOf("(", StringComparison.Ordinal);
            int end;

            while (start != -1)
            {
                end = FindNextMatchingChar(input, start);
                if (end != -1)
                {
                    var synonymName = input.Substring(start + 1, end - start - 1).Trim().ToLower();
                    if (synonyms[synonymName] != null)
                        input = input.Substring(0, start + 1) + ((Synonym) synonyms[synonymName]).GetPhrases() +
                                input.Substring(end);
                    else
                        input = input.Substring(0, start) + "(" + synonymName + ")" + input.Substring(end + 1);
                    start = input.IndexOf("(", start + 1, StringComparison.Ordinal);
                }
                else //there's an error with this input, it won't compile
                {
                    //TODO: log an error somewhere?
                    return "";
                }
            }

            return input;
        } //replaceSynonyms(string input, Hashtable synonymGroups)

        public static string ReplaceOutputSynonyms(string text, Hashtable synonyms)
        {
            var start = text.IndexOf("(", StringComparison.Ordinal);
            int end;

            while (start != -1)
                if (!IsEscaped(text, start))
                {
                    end = FindNextMatchingChar(text, start);
                    if (end != -1)
                    {
                        var synonymName = text.Substring(start + 1, end - start - 1).Trim().ToLower();
                        if (synonyms[synonymName] != null && !IsInCommand(text, start, end - start))
                        {
                            var syn = (Synonym) synonyms[synonymName];
                            if (syn.Phrases.Count > 0)
                            {
                                var rand = new Random();
                                var pick = rand.Next(syn.Phrases.Count);
                                if (end == text.Length - 1) //the end is the end
                                    text = text.Substring(0, start) + ((Phrase) syn.Phrases[pick]).Text;
                                else
                                    text = text.Substring(0, start) + ((Phrase) syn.Phrases[pick]).Text +
                                           text.Substring(end + 1);
                            }
                            else
                            {
                                text = text.Substring(0, start + 1) + synonymName + text.Substring(end);
                            }
                        }

                        start = text.IndexOf("(", start + 1, StringComparison.Ordinal);
                    }
                    else
                    {
                        //TODO: log an error somewhere?
                        return text;
                    }
                }
                else //it's escaped \(..
                {
                    //remove the '\'
                    text = text.Remove(start - 1, 1);
                    //find the next start
                    if (start >= text.Length - 1) //the old start is now at the end
                        start = -1;
                    else
                        start = text.IndexOf("(", start, StringComparison.Ordinal);
                }

            return text;
        } //replaceSynonyms(string input, Hashtable synonymGroups)

        public static string ReplaceVars(string text, Hashtable vars)
        {
            var start = text.IndexOf("[", StringComparison.Ordinal);
            int end;
            while (start != -1)
                if (!IsEscaped(text, start))
                {
                    end = FindNextMatchingChar(text, start);
                    if (end != -1)
                    {
                        var varName = ReplaceVars(text.Substring(start + 1, end - start - 1), vars);
                        var varDefaultValue = "";
                        if (varName.IndexOf(':') != -1)
                        {
                            varDefaultValue = varName.Substring(varName.IndexOf(':') + 1).Trim();
                            varName = varName.Substring(0, varName.IndexOf(':')).Trim();
                        }

                        varName = varName.Replace(" ", "_");
                        var varValue = (string) vars[varName.ToLower()];
                        if (varValue == null)
                            varValue = varDefaultValue;

                        if (end == text.Length - 1) //var runs to the end
                        {
                            text = text.Substring(0, start) + varValue;
                            start = -1;
                        }
                        else
                        {
                            text = text.Substring(0, start) + varValue + text.Substring(end + 1);
                            start = text.IndexOf("[", start + varValue.Length, StringComparison.Ordinal);
                        }
                    }
                    else //there's an error with this input, it won't compile
                    {
                        //TODO: log an error somewhere?
                        return "";
                    }
                }
                else // [ is escaped
                {
                    //remove the '\'
                    text = text.Remove(start - 1, 1);
                    //find the next start
                    if (start >= text.Length - 1) //the old start is now at the end
                        start = -1;
                    else
                        start = text.IndexOf("[", start, StringComparison.Ordinal);
                }

            var embCmd = "<mem.get ";
            start = text.IndexOf(embCmd, StringComparison.Ordinal);
            while (start != -1)
                if (!IsEscaped(text, start))
                {
                    end = FindNextMatchingChar(text, start);
                    if (end != -1)
                    {
                        var name = text.Substring(start + embCmd.Length, end - start - embCmd.Length);
                        var left = "";
                        if (start > 0)
                            left = text.Substring(0, start);
                        var right = "";
                        if (end + 1 != text.Length)
                            right = text.Substring(end + 1);
                        text = left + vars[name.ToLower()] + right;
                    }

                    start = text.IndexOf(embCmd, StringComparison.Ordinal);
                }
                else // < is escaped
                {
                    //remove the '\'
                    text = text.Remove(start - 1, 1);
                    //find the next start
                    if (start >= text.Length - 1) //the old start is now at the end
                        start = -1;
                    else
                        start = text.IndexOf(embCmd, start, StringComparison.Ordinal);
                }

            embCmd = "<mem.set ";
            start = text.IndexOf(embCmd, StringComparison.Ordinal);
            while (start != -1)
                if (!IsEscaped(text, start))
                {
                    end = FindNextMatchingChar(text, start);
                    if (end != -1)
                    {
                        var nameValue = text.Substring(start + embCmd.Length, end - start - embCmd.Length);
                        var left = "";
                        var right = "";
                        var cmdArgs = SplitOnFirstUnquotedSpace(nameValue);

                        if (cmdArgs.Length > 1)
                        {
                            var name = cmdArgs[0];
                            //remove quotes if they are there
                            if (name.Length > 1 && name[0] == '"')
                                name = name.Substring(1);
                            if (name.Length > 2 && name[name.Length - 1] == '"')
                                name = name.Substring(0, name.Length - 1);
                            var val = cmdArgs[1];
                            vars[name.ToLower()] = val;
                        }

                        if (start > 0)
                            left = text.Substring(0, start);
                        if (end + 1 != text.Length)
                            right = text.Substring(end + 1);
                        text = left + right;
                    }

                    start = text.IndexOf(embCmd, StringComparison.Ordinal);
                }
                else // < is escaped
                {
                    //remove the '\'
                    text = text.Remove(start - 1, 1);
                    //find the next start
                    if (start >= text.Length - 1) //the old start is now at the end
                        start = -1;
                    else
                        start = text.IndexOf(embCmd, start, StringComparison.Ordinal);
                }

            embCmd = "<mem.del ";
            start = text.IndexOf(embCmd, StringComparison.Ordinal);
            while (start != -1)
                if (!IsEscaped(text, start))
                {
                    end = FindNextMatchingChar(text, start);
                    if (end != -1)
                    {
                        var name = text.Substring(start + embCmd.Length, end - start - embCmd.Length);
                        vars.Remove(name.Trim().ToLower());
                        var left = "";
                        var right = "";
                        if (start > 0)
                            left = text.Substring(0, start);
                        if (end + 1 != text.Length)
                            right = text.Substring(end + 1);
                        text = left + right;
                    }

                    start = text.IndexOf(embCmd, StringComparison.Ordinal);
                }
                else // < is escaped
                {
                    //remove the '\'
                    text = text.Remove(start - 1, 1);
                    //find the next start
                    if (start >= text.Length - 1) //the old start is now at the end
                        start = -1;
                    else
                        start = text.IndexOf(embCmd, start, StringComparison.Ordinal);
                }

            return text;
        } //ReplaceVars(string text, Hashtable vars)

        public static string TextToPattern(string stPat)
        {
            /*
                "quoted text" doesn't get changed
                * => .*?
                [.,;:!?] => NOTHING
                \WSPACE\W => .+?
                \WSPACE\w => .+?\b
                \wSPACE\W => \b.+?
                \wSPACE\w => \b.+?\b
                [blaw] => (?<blaw>.+)
                ^| => ^
                ^\w => ^.*?\b
                ^\W => ^.*?
                |$ => $
                \w$ => \b.*?$
                \W$ => .*?$
                            
            */
            var inQuote = false;
            var varDepth = 0;
            var pat = new StringBuilder(stPat);
            //nonWord matches to non-word characters other than parens
            var nonWord = new Regex(@"[^\w\(\)]");

            var regexVar = false;

            //handle special characters
            for (var j = 0; j < pat.Length; j++)
            {
                //ignore quoted sections
                if (pat[j] == '"' && !IsEscaped(pat, j))
                    inQuote = !inQuote;

                if (inQuote || IsEscaped(pat, j))
                    continue;

                if (pat[j] == '*') //wildcard
                {
                    pat.Replace("*", ".*?", j, 1);
                    j += 2;
                }
                else if (pat[j] == '.' || pat[j] == '?' || pat[j] == '+')
                {
                    pat.Insert(j, '\\');
                    j += 1;
                }
                else if (pat[j] == ' ' && (j <= 3 || pat.ToString().Substring(j - 4, 4) != ">.+)") &&
                         (j == pat.Length - 1 || pat[j + 1] != '['))
                {
                    var replacement = "_";
                    if (varDepth == 0)
                    {
                        if (j == 0 || nonWord.IsMatch(pat[j - 1].ToString())) //starts with nothing or a non-word
                        {
                            if (j == pat.Length - 1 || nonWord.IsMatch(pat[j + 1].ToString())
                            ) //ends with nothing or a non-word
                                replacement = @".+?";
                            else //ends with word character
                                replacement = @".+?\b";
                        }
                        else //starts with a word character
                        {
                            if (j == pat.Length - 1 || nonWord.IsMatch(pat[j + 1].ToString())
                            ) //ends with nothing or a non-word
                                replacement = @"\b.+?";
                            else //ends with word character
                                replacement = @"\b.+?\b";
                        }
                    } //end if not in a var

                    if (!regexVar)
                    {
                        pat.Replace(" ", replacement, j, 1);
                        j += replacement.Length - 1;
                    }
                } //end pat[j] == ' '
                else if (pat[j] == '[')
                {
                    regexVar = false;
                    varDepth++;
                    if (varDepth == 1)
                    {
                        pat.Replace("[", "(?<", j, 1);
                        j += 2;
                    }
                    else
                    {
                        pat.Replace("[", "_s_", j, 1);
                        j += 1;
                    }
                }
                else if (pat[j] == '=' && varDepth > 0)
                {
                    pat.Replace("=", ">", j, 1);
                    regexVar = true;
                }
                else if (pat[j] == ']')
                {
                    varDepth--;
                    if (varDepth == 0)
                    {
                        if (regexVar)
                        {
                            pat.Replace("]", ")", j, 1);
                        }
                        else
                        {
                            pat.Replace("]", ">.+)", j, 1);
                            j += 3;
                        }
                    }
                    else
                    {
                        pat.Replace("]", "_e_", j, 1);
                    }

                    regexVar = false;
                }
            }

            //remove quotes
            for (var j = 0; j < pat.Length; j++)
                if (pat[j] == '"')
                {
                    if (IsEscaped(pat, j))
                        pat.Remove(j - 1, 1);
                    else
                        pat.Remove(j, 1);
                    j--;
                }

            //add initial and trailing wildcards
            stPat = pat.ToString();
            if (stPat.IndexOf("(?<", StringComparison.Ordinal) != 0 && stPat.IndexOf("|", StringComparison.Ordinal) != 0
            ) stPat = @"(|.*?\b|.*?\s)" + stPat;
            if (stPat.LastIndexOf(">.*?)", StringComparison.Ordinal) != stPat.Length - 6 &&
                stPat.LastIndexOf("|", StringComparison.Ordinal) != stPat.Length - 1) stPat += @"(|\b.*?|\s.*?)";

            pat = new StringBuilder(stPat);
            //remove the pipes (walls)
            if (pat.Length > 0 && pat[0] == '|')
                pat.Remove(0, 1);
            if (pat.Length > 0 && pat[pat.Length - 1] == '|')
                pat.Remove(pat.Length - 1, 1);

            pat.Insert(0, '^');
            pat.Append('$');

            return pat.ToString();
        } //TextToPattern(string stPat)

        public static string[] SplitOnFirstUnquotedSpace(string text)
        {
            var pieces = new string[2];
            if (text != null)
            {
                var index = text.IndexOf(" ", StringComparison.Ordinal);
                //find the right place to break
                while (index != -1 && IsQuoted(text, index, 1))
                    if (index < text.Length - 1)
                        index = text.IndexOf(" ", index + 1, StringComparison.Ordinal);
                    else
                        index = -1;

                //break up the string
                if (index != -1)
                {
                    pieces[0] = text.Substring(0, index);
                    pieces[1] = text.Substring(index + 1);
                }
                else
                {
                    pieces[0] = text;
                    pieces[1] = "";
                }
            }

            return pieces;
        } //splitOnFirstUnquotedSpace(string text)

        public static bool IsQuoted(string text, int start, int length)
        {
            //returns true if any of the characters from start to start + length are quoted

            var inQuote = false;
            for (var i = 0; i < start + length; i++)
            {
                if (text[i] == '"' && !IsEscaped(text, i))
                    inQuote = !inQuote;
                if (i >= start && inQuote)
                    return true;
            }

            return false;
        }

        public static bool IsInCommand(string text, int start, int length)
        {
            //returns true if any of the characters from start to start + length are in a command

            var startCommand = '<';
            var endCommand = '>';
            var depth = 0;
            for (var i = 0; i < start + length; i++)
            {
                if (text[i] == startCommand && !IsEscaped(text, i))
                    depth++;
                else if (text[i] == endCommand && !IsEscaped(text, i))
                    depth--;
                if (i >= start && depth > 0)
                    return true;
            }

            return false;
        } //IsInCommand(string text, int start, int length)

        public static bool IsEscaped(string text, int index)
        {
            // Examples:
            // IsExcaped( hello , 2) -> false
            // IsEscaped( test "this" , 5) -> false
            // IsEscaped( test \"this\", 6) -> true
            // IsEscaped( test \\"this", 7) -> false
            // IsEscaped( test \\\"this", 8) -> true
            if (index == 0 || text[index - 1] != '\\')
                return false;
            return !IsEscaped(text, index - 1);
        }

        public static bool IsEscaped(StringBuilder text, int index)
        {
            // Examples:
            // IsExcaped( hello , 2) -> false
            // IsEscaped( test "this" , 5) -> false
            // IsEscaped( test \"this\", 6) -> true
            // IsEscaped( test \\"this", 7) -> false
            // IsEscaped( test \\\"this", 8) -> true
            if (index == 0 || text[index - 1] != '\\')
                return false;
            return !IsEscaped(text, index - 1);
        } //IsEscaped(StringBuilder text, int index)

        public static ArrayList GetDefaultInputReplacements()
        {
            var alRet = new ArrayList();
            alRet.Add(new InputReplacement("À", "A"));
            alRet.Add(new InputReplacement("Á", "A"));
            alRet.Add(new InputReplacement("Â", "A"));
            alRet.Add(new InputReplacement("Ã", "A"));
            alRet.Add(new InputReplacement("Ä", "A"));
            alRet.Add(new InputReplacement("Å", "A"));
            alRet.Add(new InputReplacement("à", "a"));
            alRet.Add(new InputReplacement("á", "a"));
            alRet.Add(new InputReplacement("â", "a"));
            alRet.Add(new InputReplacement("ã", "a"));
            alRet.Add(new InputReplacement("ä", "a"));
            alRet.Add(new InputReplacement("å", "a"));
            alRet.Add(new InputReplacement("Æ", "AE"));
            alRet.Add(new InputReplacement("æ", "ae"));
            alRet.Add(new InputReplacement("Ç", "C"));
            alRet.Add(new InputReplacement("ç", "c"));
            alRet.Add(new InputReplacement("È", "E"));
            alRet.Add(new InputReplacement("É", "E"));
            alRet.Add(new InputReplacement("Ê", "E"));
            alRet.Add(new InputReplacement("Ë", "E"));
            alRet.Add(new InputReplacement("è", "e"));
            alRet.Add(new InputReplacement("é", "e"));
            alRet.Add(new InputReplacement("ê", "e"));
            alRet.Add(new InputReplacement("ë", "e"));
            alRet.Add(new InputReplacement("Ì", "I"));
            alRet.Add(new InputReplacement("Í", "I"));
            alRet.Add(new InputReplacement("Î", "I"));
            alRet.Add(new InputReplacement("Ï", "I"));
            alRet.Add(new InputReplacement("ì", "i"));
            alRet.Add(new InputReplacement("í", "i"));
            alRet.Add(new InputReplacement("î", "i"));
            alRet.Add(new InputReplacement("ï", "i"));
            alRet.Add(new InputReplacement("Ñ", "N"));
            alRet.Add(new InputReplacement("ñ", "n"));
            alRet.Add(new InputReplacement("Ò", "O"));
            alRet.Add(new InputReplacement("Ó", "O"));
            alRet.Add(new InputReplacement("Ô", "O"));
            alRet.Add(new InputReplacement("Õ", "O"));
            alRet.Add(new InputReplacement("Ö", "O"));
            alRet.Add(new InputReplacement("ò", "o"));
            alRet.Add(new InputReplacement("ó", "o"));
            alRet.Add(new InputReplacement("ô", "o"));
            alRet.Add(new InputReplacement("õ", "o"));
            alRet.Add(new InputReplacement("ö", "o"));
            alRet.Add(new InputReplacement("Ù", "U"));
            alRet.Add(new InputReplacement("Ú", "U"));
            alRet.Add(new InputReplacement("Û", "U"));
            alRet.Add(new InputReplacement("Ü", "U"));
            alRet.Add(new InputReplacement("ù", "u"));
            alRet.Add(new InputReplacement("ú", "u"));
            alRet.Add(new InputReplacement("û", "u"));
            alRet.Add(new InputReplacement("ü", "u"));
            alRet.Add(new InputReplacement("Ý", "Y"));
            alRet.Add(new InputReplacement("ý", "y"));

            alRet.Add(new InputReplacement(".", ""));
            alRet.Add(new InputReplacement("!", ""));
            alRet.Add(new InputReplacement("?", ""));
            alRet.Add(new InputReplacement(",", ""));
            alRet.Add(new InputReplacement("\"", ""));
            alRet.Add(new InputReplacement("'", ""));
            alRet.Add(new InputReplacement(":", ""));
            alRet.Add(new InputReplacement(";", ""));

            return alRet;
        }

        public static string ApplyDefaultInputReplacements(string text)
        {
            //ÀÁÂÃÄÅàáâãäåÆæÇçÈÉÊËèéêëÌÍÎÏìíîïÑñÒÓÔÕÖòóôõöÙÚÛÜùúûüÝýÿ.!?,"':;
            var sb = new StringBuilder(text);
            var alDefaultInputReplacements = GetDefaultInputReplacements();
            foreach (InputReplacement ir in alDefaultInputReplacements) sb.Replace(ir.TextToFind, ir.TextToInput);

            return sb.ToString();
        } //ApplyDefaultInputReplacements(string text)

        public static int FindNextUnexcapedChar(string text, char ch)
        {
            return FindNextUnescapedChar(text, ch, 0);
        }

        public static int FindNextUnescapedChar(string text, char ch, int start)
        {
            for (var i = start; i < text.Length; i++)
                if (text[i] == ch && !IsEscaped(text, i))
                    return i;
            return -1;
        }

        public static int FindNextMatchingChar(string text, int index)
        {
            if (index > -1 && index < text.Length)
            {
                var depth = 1;
                char openChar;
                char closeChar;
                switch (text[index])
                {
                    case '[':
                        openChar = '[';
                        closeChar = ']';
                        break;
                    case '(':
                        openChar = '(';
                        closeChar = ')';
                        break;
                    case '<':
                        openChar = '<';
                        closeChar = '>';
                        break;
                    case '{':
                        openChar = '{';
                        closeChar = '}';
                        break;
                    default:
                        return -1;
                } //switch

                for (var i = index + 1; i < text.Length; i++)
                    if (text[i] == closeChar)
                    {
                        depth--;
                        if (depth == 0)
                            return i;
                    }
                    else if (text[i] == openChar)
                    {
                        depth++;
                    }

                return -1;
            } //end if within bounds

            return -1;
        } //FindNextMatch(string text, int start)

        public static string AddCarriageReturns(string text)
        {
            var index = text.IndexOf('\n');
            if (index != -1 && (index == 0 || text[index - 1] != '\r'))
                text = text.Replace("\n", "\r\n");
            return text;
        } //AddCarriageReturns(string text)

        /// <summary>
        ///     Generates a list of word permutations from stInput
        /// </summary>
        /// <param name="stInput">stInput to generates inputs from</param>
        /// <returns>list of word permutations</returns>
        public static ArrayList GetWordPermutations(string stInput)
        {
            return GetWordPermutations(stInput, -1);
        } //GetWordPermutations(string stInput)

        /// <summary>
        ///     Generates a list of word permutations from stInput up to strings with maxSize number of words
        /// </summary>
        /// <param name="stInput">stInput to generates inputs from</param>
        /// <param name="maxSize">maximum number of words in each permutation</param>
        /// <returns>list of word permutations</returns>
        public static ArrayList GetWordPermutations(string stInput, int maxSize)
        {
            var alRet = new ArrayList();

            if (stInput == null)
                return alRet;

            //do this replacement so that we can find synonyms on
            //the right side of the = in a capture
            stInput = stInput.Replace("=", "= ");
            stInput = stInput.Replace("]", " ]");

            var stArr = stInput.Split(' ');
            var sb = new StringBuilder();

            if (maxSize == -1 || maxSize > stArr.Length)
                maxSize = stArr.Length;

            for (var size = maxSize; size > 0; size--) //how many words to get
            for (var start = 0; start <= stArr.Length - size; start++) //index to start at
            {
                sb.Remove(0, sb.Length);
                for (var i = start; i < start + size; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(" ");
                    sb.Append(stArr[i]);
                }

                var part = sb.ToString().Trim();
                if (part != "" && !alRet.Contains(part))
                    alRet.Add(part);
            }

            return alRet;
        } //GetWordPermutations(string stInput, int maxSize)

        public static ArrayList GetAllAvailableSynonymNodes(ArrayList splits, Hashtable synWordIndex)
        {
            return GetAllAvailableSynonymNodes(splits, synWordIndex, null);
        } //GetAllAvailableSynonymNodes(ArrayList splits, Hashtable synWordIndex)

        public static ArrayList GetAllAvailableSynonymNodes(ArrayList splits, Hashtable synWordIndex, string idNotToAdd)
        {
            var synonymNodes = new ArrayList();

            foreach (string split in splits)
                if (synWordIndex[split] != null)
                    foreach (SynonymNode sn in (ArrayList) synWordIndex[split])
                    {
                        if (!sn.MatchedPhrases.Contains(split.ToLower()))
                            sn.MatchedPhrases.Add(split.ToLower());
                        if (sn.Id != idNotToAdd && !synonymNodes.Contains(sn))
                            synonymNodes.Add(sn);
                    }

            return synonymNodes;
        } //GetAllAvailableSynonymNodes(ArrayList splits, Hashtable synWordIndex, string idNotToAdd)

        public static string FetchWebPage(string url)
        {
            var page = "";
            try
            {
                var buf = new byte[1024 * 8];

                var request = (HttpWebRequest) WebRequest.Create(url);
                var response = (HttpWebResponse) request.GetResponse();

                var stream = response.GetResponseStream();

                if (stream != null)
                {
                    var count = stream.Read(buf, 0, buf.Length);

                    page = Encoding.UTF8.GetString(buf, 0, count);
                }

                if (stream != null) stream.Close();
            }
            catch (Exception e)
            {
                //TODO: use or remove var
                // ReSharper disable once UnusedVariable
                var st = e.ToString();
            }

            return page;
        } //FetchWebPage(string url)
    } //class TextToolbox

    public class SynonymNode : TreeNode, IComparable
    {
        public readonly string Id;
        public new readonly string Name;

        public ArrayList MatchedPhrases;

        public SynonymNode(string id, string name)
        {
            Id = id;
            Name = name;
            Text = name;
            ImageIndex = 1;
            SelectedImageIndex = 1;

            MatchedPhrases = new ArrayList();
        }


        public int CompareTo(object other)
        {
            var snOther = (SynonymNode) other;
            return string.Compare(Name, snOther.Name, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return Text;
        }

        public override bool Equals(object other)
        {
            var snOther = (SynonymNode) other;
            if (snOther != null && snOther.Id == Id && snOther.Name == Name)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return (Id + Name).GetHashCode();
        }
    } //class SynonymNode
} //namespace Conversive.Verbot4