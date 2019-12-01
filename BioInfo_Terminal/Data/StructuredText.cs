namespace BioInfo_Terminal.Data
{
    internal class StructuredText
    {
        internal StructuredText(string rawText)
        {
            RawText = rawText;
            NeedsRecognition = true;
            StructureText = string.Empty;
        }

        internal StructuredText(string rtext, string stext)
        {
            NeedsRecognition = true;
            RawText = rtext;
            StructureText = stext;
            IdStructure(StructureText);
        }

        internal bool NeedsRecognition { get; set; }

        internal string RawText { get; set; }

        internal string StructureText { get; set; }

        private void IdStructure(string stext)
        {
            NeedsRecognition = stext.Contains("##-skill");
        }

        internal static string PrepareTextForRecognition(string rawText)
        {
            //prep for molarity calculations
            if (rawText.Contains("calculate molarity") ||
                rawText.Contains("calculate volume") ||
                rawText.Contains("calculate weight"))
            {
                rawText = rawText.Replace("calculate molarity", "calculate molarity for");
                rawText = rawText.Replace("calculate volume", "calculate volume for");
                rawText = rawText.Replace("calculate weight", "calculate weight for");
            }

            return rawText;
        }
    }
}