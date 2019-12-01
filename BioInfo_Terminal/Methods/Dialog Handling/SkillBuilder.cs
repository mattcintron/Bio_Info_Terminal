using System.Collections.Generic;
using BioInfo_Terminal.Methods.Operations;
using BioInfo_Terminal.Methods.Skills;

namespace BioInfo_Terminal.Methods.Dialog_Handling
{
    internal class SkillBuilder
    {
        internal SkillBuilder()
        {
            //Operations
            var chemOperations = new ChemOperations();
            EmailOperations = new EmailOperations(chemOperations);
            var unitConverterOperations = new UnitConverterOperations();

            //Lab Skills
            var getCompound = new SkillTemplate
            {
                Id = "compoundSkill",
                OperationIds = new List<string> {"Get_Compound"},
                SkillIds = new List<string>()
            };
            var getMolarity = new SkillTemplate
            {
                Id = "molaritySkill",
                OperationIds = new List<string> {"Get_Molarity"},
                SkillIds = new List<string>()
            };
            var getVolume = new SkillTemplate
            {
                Id = "volumeSkill",
                OperationIds = new List<string> {"Get_Volume"},
                SkillIds = new List<string>()
            };
            var getWeight = new SkillTemplate
            {
                Id = "weightSkill",
                OperationIds = new List<string> {"Get_Weight"},
                SkillIds = new List<string>()
            };
            var getMobilePhase = new SkillTemplate
            {
                Id = "mobilePhaseSkill",
                OperationIds = new List<string> {"Get_MobilePhase"},
                SkillIds = new List<string>()
            };
            var convertUnits = new SkillTemplate
            {
                Id = "conversionSkill",
                OperationIds = new List<string> {"Convert_Units"},
                SkillIds = new List<string>()
            };
            var sendEmail = new SkillTemplate
            {
                Id = "emailSkill",
                OperationIds = new List<string> {"Send_Email"},
                SkillIds = new List<string>()
            };

            //add to interfaces
            Operations = new List<IOperation>
            {
                EmailOperations,
                chemOperations,
                unitConverterOperations
            };
            Skills = new List<SkillTemplate>
            {
                getCompound,
                getMolarity,
                getVolume,
                getWeight,
                getMobilePhase,
                convertUnits,
                sendEmail
            };
            FillSkillLists();
        }

        internal EmailOperations EmailOperations { get; set; } //tool for handling all email requests

        internal List<SkillTemplate> Skills { get; set; } //list of lab skill 

        internal List<IOperation> Operations { get; set; } //list of skill operations 

        internal void ClearOperations()
        {
            //Operations
            EmailOperations = new EmailOperations();
            var chemOperations = new ChemOperations();
            var unitConverterOperations = new UnitConverterOperations();

            Operations = new List<IOperation>
            {
                EmailOperations,
                chemOperations,
                unitConverterOperations
            };
        }

        private void FillSkillLists()
        {
            foreach (var sk in Skills)
            {
                sk.Skills = Skills;
                for (var i = 0; i < Operations.Count; i++) sk.Operations = Operations;
            }
        }
    }

    internal interface IRecognizers
    {
        bool IsSikllIdRec();

        Dictionary<string, string> ParseInfo(string text);
    }

    internal interface IOperation
    {
        void FillValues(Dictionary<string, string> values);

        string RunOperations(string id, string text, ref int dialougeTarget);
    }
}