using System;
using System.Collections.Generic;
using BioInfo_Terminal.Methods.Dialog_Handling;

namespace BioInfo_Terminal.Methods.Skills
{
    internal class SkillTemplate
    {
        private int _dialogTarget;
        private int _targetOperation;

        internal SkillTemplate()
        {
            Id = string.Empty;
            OperationIds = new List<string>();
            SkillIds = new List<string>();
            _dialogTarget = 0;
            FillOpertionsFromSkills(SkillIds);
        }

        internal bool Recognized { get; set; } // bool for recognition 
        internal string Id { get; set; } // this skills id
        internal List<string> OperationIds { get; set; } // list of all operation ids
        internal List<string> SkillIds { get; set; } // list of all skill ids
        internal List<SkillTemplate> Skills { get; set; } // list of lab skill 
        internal List<IOperation> Operations { get; set; } // list of skill operations 

        public void RecognizeSkill(Dictionary<string, string> values)
        {
            Recognized = Convert.ToBoolean(values[Id]);
        }

        // ReSharper disable once RedundantAssignment
        public bool RunSkill(ref string text, ref string response)
        {
            if (Recognized || _dialogTarget != 0)
            {
                response = string.Empty;
                var i = 0;
                foreach (var op in Operations)
                foreach (var id in OperationIds)
                {
                    if (i >= _targetOperation)
                    {
                        response = op.RunOperations(id, text, ref _dialogTarget);
                        if (!string.IsNullOrEmpty(response))
                        {
                            _targetOperation++;
                            if (_targetOperation >= OperationIds.Count) _targetOperation = 0;
                            return true;
                        }
                    }

                    i++;
                }
            }

            return false;
        }

        public List<IOperation> AddOperations(List<IOperation> operations)
        {
            operations.AddRange(Operations);
            return operations;
        }

        private void FillOpertionsFromSkills(List<string> skillIds)
        {
            if (Skills == null) return;
            foreach (var id in skillIds)
            foreach (var skill in Skills)
                if (skill.Id == id)
                    Operations = skill.AddOperations(Operations);
        }
    }
}