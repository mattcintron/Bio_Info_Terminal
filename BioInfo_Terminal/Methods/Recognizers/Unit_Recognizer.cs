using System;
using System.Collections.Generic;
using System.Globalization;
using BioInfo_Terminal.Methods.Dialog_Handling;

namespace BioInfo_Terminal.Methods.Recognizers
{
    internal class UnitRecognizer : IRecognizers
    {
        public bool IsSikllIdRec()
        {
            return false;
        }

        public Dictionary<string, string> ParseInfo(string text)
        {
            double size = 0;
            var unitA = string.Empty;
            var unitB = string.Empty;
            var items = text.Split(' ');
            for (var i = 0; i < items.Length; i++)
            {
                if (i == items.Length - 1) break;
                if (double.TryParse(items[i], out _)) unitA = items[i + 1];

                if (items[i] == "convert" || items[i] == "conversion")
                {
                    if (double.TryParse(items[i + 1], out _))
                        size = Convert.ToDouble(items[i + 1]);
                    else
                        unitA = items[i + 1];
                }

                if (items[i] == "to" || items[i] == "into")
                {
                    unitB = items[i + 1];

                    if (unitA == string.Empty && i != 0) unitA = items[i - 1];
                }
            }

            var values = new Dictionary<string, string>();
            values.Add("Size", size.ToString(CultureInfo.InvariantCulture));
            values.Add("UnitA", unitA);
            values.Add("UnitB", unitB);

            return values;
        }
    }
}