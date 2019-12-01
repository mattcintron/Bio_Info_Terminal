using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NHunspell;

namespace BioInfo_Terminal.Methods
{
    internal class Spelling
    {
        public string AnalyzeString(string text)
        {
            //reference-
            //https://stackoverflow.com/questions/17975103/is-there-a-native-spell-check-method-for-datatype-string

            var options = string.Empty;

            using (var hunspell = new Hunspell("Resources\\en_GB.aff", "Resources\\en_GB.dic"))
            {
                var words = Regex.Split(text, @"\W+", RegexOptions.IgnoreCase);
                IEnumerable<string> misspelledWords;
                // ReSharper disable once AccessToDisposedClosure
                misspelledWords = words.Where(word => !hunspell.Spell(word));

                foreach (var word in misspelledWords)
                {
                    IEnumerable<string> suggestions = hunspell.Suggest(word);
                    options += " Suggested Replacements for " + word + ",         ";
                    for (var i = 0; i < suggestions.Count(); i++)
                        options += i + 1 + " : " + suggestions.ElementAt(i) + ", ";
                }
            }

            return options;
        }
    }
}