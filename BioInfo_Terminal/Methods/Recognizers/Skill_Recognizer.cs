using System.Collections.Generic;
using BioInfo_Terminal.Methods.Dialog_Handling;

namespace BioInfo_Terminal.Methods.Recognizers
{
    internal class SkillRecognizer : IRecognizers
    {
        public bool IsSikllIdRec()
        {
            return true;
        }

        public Dictionary<string, string> ParseInfo(string text)
        {
            var values = new Dictionary<string, string>();
            var conversionSkill = false;
            var mobilePhaseSkill = false;
            var emailskill = false;
            var molaritySkill = IdentifyMolaritySkill(text);
            var volumeSkill = IdentifyVolumeSkill(text);
            var weightSkill = IdentifyWeightSkill(text);
            var compoundSkill = IdentifyCompoundSkill(text);

            if (text.Contains("convert") || text.Contains("conversion")) conversionSkill = true;
            if (text.Contains("mobile phase")) mobilePhaseSkill = true;
            if (text.Contains("email") || text.Contains("email user") || text.Contains("message user"))
                emailskill = true;

            values.Add("conversionSkill", conversionSkill.ToString());
            values.Add("mobilePhaseSkill", mobilePhaseSkill.ToString());
            values.Add("molaritySkill", molaritySkill.ToString());
            values.Add("volumeSkill", volumeSkill.ToString());
            values.Add("weightSkill", weightSkill.ToString());
            values.Add("compoundSkill", compoundSkill.ToString());
            values.Add("emailSkill", emailskill.ToString());

            return values;
        }

        private bool IdentifyCompoundSkill(string text)
        {
            if (text == "##-skill1") return true;
            return false;
        }

        private bool IdentifyVolumeSkill(string text)
        {
            if (text == "##-skill2") return true;
            return false;
        }

        private bool IdentifyWeightSkill(string text)
        {
            if (text == "##-skill3")
                return true;

            return false;
        }

        private bool IdentifyMolaritySkill(string text)
        {
            if (text == "##-skill4")
                return true;

            return false;
        }
    }
}