using System.Collections.Generic;

namespace BioInfo_Terminal.Data
{
    internal class Context
    {
        internal Context()
        {
            DialogTargetSkill = 0;
            Values = new Dictionary<string, string>();
        }

        internal int DialogTargetSkill { get; set; } //Current Skill State 

        //new dictionary storage tool
        internal Dictionary<string, string> Values { get; set; }
    }
}