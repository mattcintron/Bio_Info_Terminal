using System;
using System.Collections.Generic;
using System.Globalization;
using BioInfo_Terminal.Methods.Dialog_Handling;

namespace BioInfo_Terminal.Methods.Recognizers
{
    internal class ChemRecognizer : IRecognizers
    {
        public bool IsSikllIdRec()
        {
            return false;
        }

        //Interface Implementation
        public Dictionary<string, string> ParseInfo(string text)
        {
            double volume = 0;
            double weight = 0;
            var chromatography = string.Empty;
            var chemical = string.Empty;
            double concentration = 0;

            var items = text.Split(' ');
            var addWords = false;

            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] == "weight")
                {
                    if (items.Length <= i + 1) break;
                    if (double.TryParse(items[i + 1], out _))
                        weight = Convert.ToDouble(items[i + 1]);
                    addWords = false;
                }

                if (items[i] == "volume")
                {
                    if (items.Length <= i + 1) break;
                    if (double.TryParse(items[i + 1], out _))
                        volume = Convert.ToDouble(items[i + 1]);
                    addWords = false;
                }

                if (items[i] == "molarity" || items[i] == "concentration" || items[i] == "mols" || items[i] == "moles")
                {
                    if (items.Length <= i + 1) break;
                    if (double.TryParse(items[i + 1], out _))
                        concentration = Convert.ToDouble(items[i + 1]);
                    addWords = false;
                }

                if (items[i] == "for")
                {
                    if (items.Length <= i + 1) break;
                    chromatography = items[i + 1];
                }

                if (items[i] == "chromatography")
                {
                    if (items.Length <= i + 1) break;
                    chromatography = items[i + 1];
                }

                if (!string.IsNullOrEmpty(chemical) && chemical.Contains(items[i]) &&
                    !chemical.Contains("chemical")) continue;

                if (items[i] != "at" && items[i] != "with" && items[i] != "in" && addWords && items[i] != "chemical"
                    && items[i] != "weight" && items[i] != "concentration" && items[i] != "mols" &&
                    items[i] != "molarity" && items[i] != "for" && items[i] != "data" && items[i] != "volume")
                    chemical += " " + items[i];
                else addWords = false;

                if (items.Length <= i + 1) break;

                if ((items[i] == "for" || items[i] == "of") && chemical == string.Empty &&
                    items[i + 1] != "chemical"
                    && items[i + 1] != "weight" && items[i + 1] != "concentration" && items[i + 1] != "mols" &&
                    items[i + 1] != "molarity")
                {
                    chemical = items[i + 1];
                    addWords = true;
                }

                if (items[i] == "chemical" || items[i] == "compound" ||
                    items[i] == "about" && !text.Contains("calculate") && string.IsNullOrEmpty(chemical))
                {
                    chemical = items[i + 1];
                    addWords = true;
                }
            }

            if (chromatography == "reverse" || chromatography == "2")
                chromatography = "reverse phase";
            if (chromatography == "helic" || chromatography == "1")
                chromatography = "hilic";

            var values = new Dictionary<string, string>();
            values.Add("Volume", volume.ToString(CultureInfo.InvariantCulture));
            values.Add("Weight", weight.ToString(CultureInfo.InvariantCulture));
            values.Add("Chromatography", chromatography);
            values.Add("Chemical", chemical);
            values.Add("Concentration", concentration.ToString(CultureInfo.InvariantCulture));
            return values;
        }
    }
}