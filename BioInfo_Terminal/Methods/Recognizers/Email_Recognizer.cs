using System.Collections.Generic;
using BioInfo_Terminal.Methods.Dialog_Handling;

namespace BioInfo_Terminal.Methods.Recognizers
{
    internal class EmailRecognizer : IRecognizers
    {
        public Dictionary<string, string> ParseInfo(string text)
        {
            var user = string.Empty;

            var addWords = false;
            var items = text.Split(' ');
            for (var i = 0; i < items.Length; i++)
            {
                if (addWords)
                    user += " " + items[i];

                if ((items[i] == "user" || items[i] == "email" || items[i] == "to") && i != items.Length - 1)
                {
                    user = items[i + 1];
                    i++;
                    addWords = true;
                }
            }

            var values = new Dictionary<string, string>();
            values.Add("Users", user);
            return values;
        }

        public bool IsSikllIdRec()
        {
            return false;
        }
    }
}