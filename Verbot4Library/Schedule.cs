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
using System.Text.RegularExpressions;

namespace Conversive.Verbot4
{
    /// <summary>
    ///     Like Cron, let's you schedule inputs.
    /// </summary>
    public class Schedule
    {
        public ArrayList Events;

        public Schedule()
        {
            Events = new ArrayList();
        }

        public Schedule(string filepath)
        {
            Events = new ArrayList();
            Load(filepath);
        }

        public void Load(string filepath)
        {
            try
            {
                var fs = File.OpenRead(filepath);
                var ch = 'a'; //the current character
                var line = new StringBuilder();
                string stLine;
                var inComment = false;
                while (ch != 65535) //! use EOF?
                {
                    ch = (char) fs.ReadByte();
                    if (ch == '\n' || ch == '\r' || ch == 65535)
                    {
                        stLine = line.ToString().Trim();
                        if (stLine != "")
                            ProcessLine(stLine);
                        inComment = false;
                        line.Remove(0, line.Length);
                    }
                    else if (ch == '#')
                    {
                        inComment = true;
                    }
                    else if (!inComment)
                    {
                        line.Append(ch);
                    }
                } //while((int)ch != 65535)
            }
            catch
            {
                // ignored
            }
        } //Load(string filepath)

        private void ProcessLine(string stLine)
        {
            var linePattern =
                new Regex(@"(?<min>\S+)\s+(?<hr>\S+)\s+(?<dom>\S+)\s+(?<mon>\S+)\s+(?<dow>\S+)\s+(?<text>.+)",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            var match = linePattern.Match(stLine);
            var gc = match.Groups;
            if (gc.Count == 7)
            {
                var min = -1;
                try
                {
                    min = int.Parse(gc["min"].Value);
                }
                catch
                {
                    // ignored
                }

                var hr = -1;
                try
                {
                    hr = int.Parse(gc["hr"].Value);
                }
                catch
                {
                    // ignored
                }

                var dom = -1;
                try
                {
                    dom = int.Parse(gc["dom"].Value);
                }
                catch
                {
                    // ignored
                }

                var mon = -1;
                try
                {
                    mon = int.Parse(gc["mon"].Value);
                }
                catch
                {
                    // ignored
                }

                var dow = -1;
                try
                {
                    dow = int.Parse(gc["dow"].Value);
                }
                catch
                {
                    // ignored
                }

                var text = gc["text"].Value;
                {
                    var e = new Event(min, hr, dom, mon, dow, text);
                    Events.Add(e);
                }
            }
        } //processLine(string stLine)

        public void AddEvent(int minute, int hour, int dayOfMonth, int month, int dayOfWeek, string text)
        {
            Events.Add(new Event(minute, hour, dayOfMonth, month, dayOfWeek, text));
        }

        public string GetCurrentEvent()
        {
            foreach (Event e in Events)
                if (e.IsNow)
                    return e.Text;
            return null;
        } //GetCurrentEvent()

        public ArrayList GetCurrentEvents()
        {
            var texts = new ArrayList();
            foreach (Event e in Events)
                if (e.IsNow)
                    texts.Add(e.Text);
            return texts;
        } //GetCurrentEvents()

        public class Event
        {
            public int DayOfMonth;
            public int DayOfWeek;
            public int Hour;
            public int Minute;
            public int Month;
            public string Text;

            public Event(int minute, int hour, int dayOfMonth, int month, int dayOfWeek, string text)
            {
                Minute = minute;
                Hour = hour;
                DayOfMonth = dayOfMonth;
                Month = month;
                DayOfWeek = dayOfWeek;
                Text = text;
            }

            public bool IsNow
            {
                get
                {
                    var now = DateTime.Now;
                    return (Minute == -1 || Minute == now.Minute)
                           && (Hour == -1 || Hour == now.Hour)
                           && (DayOfMonth == -1 || DayOfMonth == now.Day)
                           && (Month == -1 || Month == now.Month) //1 = Jan
                           && (DayOfWeek == -1 || DayOfWeek == (int) now.DayOfWeek); //0 = Sun
                }
            }
        } //class Event
    } //class Schedule
} //namespace Verbot4Library