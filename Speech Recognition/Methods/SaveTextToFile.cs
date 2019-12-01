using System;
using System.Collections.Generic;
using System.IO;

namespace SpeechRecognition.Methods
{
    public class SaveTextToFile
    {

        public void SaveText(string text)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Replace(@"BioInfo_Terminal\bin\Debug\", "");
            path += @"Documentation\Saved Text Logs\Log1.txt";

            using (var file = new StreamWriter(path, true))
            {
                file.WriteLine(text);
            }
        }

        public void SaveFequencyDictionary()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Replace(@"BioInfo_Terminal\bin\Debug\", "");
            path += @"Documentation\Saved Text Logs\Log1.txt";

            var path2 = AppDomain.CurrentDomain.BaseDirectory;
            path2 = path2.Replace(@"BioInfo_Terminal\bin\Debug\", "");
            path2 += @"Documentation\Saved Text Logs\FrequencyDictionary.txt";

            List<string> allWords = new List<string>();
            List<string> searchedWords = new List<string>();
            List<int> searchedWordCount = new List<int>();

            foreach (string s in File.ReadAllLines(path))
            {
                allWords.AddRange(s.Split(' '));
            }
            foreach (string s in allWords)
            {
                bool search = false;
                foreach (string st in searchedWords)
                {
                    if (s == st)
                    {
                        search = false;
                        break;
                    }
                    else search = true;
                }

                if (searchedWords.Count == 0) search = true;
                if (search)
                {
                    searchedWords.Add(s);
                    int count = 0;
                    foreach (string y in allWords)
                    {
                        if (s == y) count++;
                    }
                    searchedWordCount.Add(count);
                }
            }

            using (StreamWriter sw = File.CreateText(path2))
            {
                for(int i=0; i<searchedWords.Count;i++)
                {
                    sw.WriteLine(searchedWords[i] + " ## "+ searchedWordCount[i] );
                }
                
            }
        }

    }
}
