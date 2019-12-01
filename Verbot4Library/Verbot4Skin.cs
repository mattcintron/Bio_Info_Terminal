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

using System.Globalization;
using System.Xml.Serialization;

namespace Conversive.Verbot4
{
    /// <summary>
    ///     Defines the look of the player.
    /// </summary>
    public class Verbot4Skin
    {
        public string AgentFileName;
        public string AgentPanelBackgroundColor;
        public int AgentPitch;
        public int AgentSpeed;
        public string AgentTtsMode;
        public bool AllowWindowResize;
        public string AppBackgroundColor;
        public string BackgroundImageFileName;


        [XmlIgnore] public bool Changed;

        public string CharacterFile;
        public int CharacterTtsMode;
        public string InputBackgroundColor;
        public Font InputFont;
        public string InputTextColor;
        public int LanguageId;
        public string OutputBackgroundColor;
        public Font OutputFont;
        public string OutputTextColor;
        public bool UseConversiveCharacter = false;

        public int WindowHeight;
        public int WindowWidth;

        public Verbot4Skin()
        {
            AgentFileName = "merlin.acs";
            LanguageId = 1033;
            AgentTtsMode = "";
            AgentSpeed = 0;
            AgentPitch = 0;

            CharacterFile = "julia.ccs";
            CharacterTtsMode = 0;

            WindowHeight = 0;
            WindowWidth = 0;
            AllowWindowResize = true;
            BackgroundImageFileName = "";
            AppBackgroundColor = "#D4D0C8";
            AgentPanelBackgroundColor = "#D4D0C8";

            InputFont = new Font();
            OutputFont = new Font();

            InputTextColor = "#000000";
            InputBackgroundColor = "#FFFFFF";
            OutputTextColor = "#000000";
            OutputBackgroundColor = "#FFFFFF";
        }

        public string RgbToHex(int rgb)
        {
            var r = ((byte) ((rgb & 0x00FF0000) >> 16)).ToString("X");
            if (r.Length == 1)
                r = "0" + r;
            var g = ((byte) ((rgb & 0x0000FF00) >> 8)).ToString("X");
            if (g.Length == 1)
                g = "0" + g;
            var b = ((byte) ((rgb & 0x000000FF) >> 0)).ToString("X");
            if (b.Length == 1)
                b = "0" + b;
            return "#" + r + g + b;
        }

        public int HexToRgb(string hex, int iDefault)
        {
            var iColor = iDefault;
            if (hex.Length == 7 && hex[0] == '#')
                iColor = int.Parse("FF" + hex[1] + hex[2] + hex[3] + hex[4] + hex[5] + hex[6], NumberStyles.HexNumber);
            return iColor;
        }
    }

    public class Font
    {
        public string FontName;
        public float FontSize;
        public FontStyle FontStyle;

        public Font()
        {
            FontName = "Microsoft Sans Serif";
            FontSize = (float) 8.25;
            FontStyle = FontStyle.Regular;
        }
    } //class Font

    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        BoldItalic = 3,
        Underline = 4,
        BoldUnderline = 5,
        ItalicUnderline = 6,
        BoldItalicUnderline = 7,
        Strikeout = 8,
        BoldStrikeout = 9,
        ItalicStrikeout = 10,
        BoldItalicStrikeout = 11,
        UnderlineStrikeout = 12,
        BoldUnderlineStrikeout = 13,
        ItalicUnderlineStrikeout = 14,
        BoldItalicUnderlineStrikeout = 15
    }
} //namespace Conversive.Verbot4