using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using BioInfo_Terminal.Methods.Dialog_Handling;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace BioInfo_Terminal.Methods.Operations
{
    internal class ChemOperations : IOperation
    {
        // TODO: delegate to Rowan Student

        private int _dialogTarget;
        private int _multiDialougeTarget;

        internal HttpClient HttpClient = new HttpClient(); // the http Client for the program
        //internal string Klog = @"http://rest.kegg.jp"; // -first part of the kegg db
        //internal string Prolog = @"https://pubchem.ncbi.nlm.nih.gov/rest/pug"; // -the first part of the link to the pub chem database

        internal ChemOperations()
        {
            Volume = 0;
            Weight = 0;
            Concentration = 0;
            Chromatography = string.Empty;
            Chemical = string.Empty;
            MolecularWeight = string.Empty;
            ChemicalFormula = string.Empty;
            Synonyms = string.Empty;
            InchiKeys = string.Empty;
            CiDs = string.Empty;
            DataSpecified = false;
            _dialogTarget = 0;
            _multiDialougeTarget = 0;
        }

        //All Chemical properties for calculations
        internal double Volume { get; set; }
        internal double Weight { get; set; }
        internal double Concentration { get; set; }
        internal string Chromatography { get; set; }
        internal string Chemical { get; set; }
        internal string MolecularWeight { get; set; }
        internal string ChemicalFormula { get; set; }
        internal string Synonyms { get; set; }
        internal string InchiKeys { get; set; }
        internal string CiDs { get; set; }

        //identifies if target data has been specified to read back
        internal bool DataSpecified { get; set; }

        public void FillValues(Dictionary<string, string> values)
        {
            if (values["Volume"] != string.Empty && values["Volume"] != "0")
                Volume = Convert.ToDouble(values["Volume"]);
            if (values["Weight"] != string.Empty && values["Weight"] != "0")
                Weight = Convert.ToDouble(values["Weight"]);
            if (values["Concentration"] != string.Empty && values["Concentration"] != "0")
                Concentration = Convert.ToDouble(values["Concentration"]);
            if (values["Chromatography"] != string.Empty)
                Chromatography = values["Chromatography"];
            if (values["Chemical"] != string.Empty)
                Chemical = values["Chemical"];
        }

        public string RunOperations(string id, string text, ref int dialougeTarget)
        {
            if (id == "Get_Compound")
            {
                var response = ProcessChemical(text);
                dialougeTarget = _dialogTarget;
                return response;
            }

            if (id == "Get_Molarity")
            {
                var response = ProcessMolarity(text);
                dialougeTarget = _dialogTarget;
                return response;
            }

            if (id == "Get_Volume")
            {
                var response = CalculateVolume(text);
                dialougeTarget = _dialogTarget;
                return response;
            }

            if (id == "Get_Weight")
            {
                var response = CalculateWeight(text);
                dialougeTarget = _dialogTarget;
                return response;
            }

            if (id == "Get_MobilePhase")
            {
                var response = CalculateMobilePhase(text);
                dialougeTarget = _dialogTarget;
                return response;
            }

            return null;
        }

        internal double Calculate_Chemical_Mol_Concentration(string chemical, double weight, double volume)
        {
            try
            {
                var mw = Convert.ToDouble(GetMw(chemical));
                var mass = weight / mw;
                var molarity = mass / volume;

                return molarity;
            }
            catch
            {
                return -1;
            }
        }

        internal double Calculate_Moles(string chemical, double weight)
        {
            try
            {
                var mw = Convert.ToDouble(GetMw(chemical));
                var moles = weight / mw;

                moles = Math.Round(moles, 3);

                return moles;
            }
            catch
            {
                return -1;
            }
        }

        internal double Convert_Chemical_Volume(double weight, double concentration, string chemical)
        {
            try
            {
                var mw = Convert.ToDouble(GetMw(chemical));
                var mass = weight / mw;
                var volume = mass / concentration;
                return volume;
            }
            catch
            {
                return -1;
            }
        }

        internal double Convert_Chemical_Weight(double volume, double concentration, string chemical)
        {
            try
            {
                var mw = Convert.ToDouble(GetMw(chemical));
                var mass = concentration * volume;
                var weight = mass * mw;

                return weight;
            }
            catch
            {
                return -1;
            }
        }

        private static string PhaseTwo(double volume)
        {
            double d = 2;

            var pa1 = 1900 / d * volume;
            var pa2 = 100 / d * volume;
            var pa3 = 2 / d * volume;

            var pb1 = 1900 / d * volume;
            var pb2 = 100 / d * volume;
            var pb3 = 2 / d * volume;

            var ww1 = 1 * volume;
            var ww2 = 1 * volume;

            var sw1 = 1 * volume;
            var sw2 = 1 * volume;

            var output =
                "For mobile phase A you will need: 1) Formic Acid (liquid); 2) LC/MS grade Acetonitrile (liquid); 3) LC/MS grade Water." +
                "Procedure: Combine " + pa1 + " mL LC/ MS grade Water with " + pa2 +
                " mL of LC / MS grade Acetonitrile. Add " + pa3 + " mL " +
                "Formic Acid.Stir with a stir bar 5 minutes to mix. \n \n" +
                "For mobile phase B you will need: ) Formic Acid (liquid); 2) LC/MS grade Acetonitrile (liquid); 3) LC/MS grade Water." +
                "Procedure: Combine " + pb1 + " mL LC/ MS grade Acetonitrile with " + pb2 +
                " mL of LC / MS grade Water. Add " + pb3 + " mL Formic Acid.Stir with" +
                "a stir bar 5 minutes to mix. \n \n" +
                "For Weak Wash you will need: 1) LC/MS grade Formic Acid (liquid); 2) LC/MS grade Water.Procedure: Add " +
                ww1 + " mL LC/+" +
                " MS grade Formic Acid to a " + ww2 + " L fresh bottle of LC / MS grade water. \n \n" +
                "For Strong Wash you will need: 1) LC/MS grade Acetonitrile (liquid); 2) LC/MS grade Methanol (liquid)." +
                "Procedure: Add " + sw1 + " mL LC/ MS grade Formic Acid to " + sw2 +
                " L of Methanol.Stir with stir bar for 5 minutes to mix.";
            return output;
        }

        private static string PhaseOne(double volume)
        {
            double d = 4;

            var pa1 = 3.08 / d * volume;
            var pa2 = 3800 / d * volume;
            var pa3 = 200 / d * volume;
            var pa4 = 2 / d * volume;

            var pb1 = 2 / d * volume;
            var pb2 = 4 / d * volume;

            var ww1 = 50 * volume;
            var ww2 = 950 * volume;

            var sw1 = 800 * volume;
            var sw2 = 200 * volume;

            var output =
                "For mobile phase A you will need: 1) Ammonium Acetate (solid); 2) Ammonium Hydroxide (liquid); 3) LC/MS grade acetonitrile (liquid); 4) LC/MS grade water" +
                "Procedure: Completely dissolve " + pa1 + "g of Ammonium Acetate into " + pa2 +
                " mL LC/ MS grade water. Add " + pa3 + " mL of LC / MS grade acetonitrile, then add " +
                "" + pa4 + " mL LC/ MS grade Ammonium Hydroxide. \n \n" +
                " For mobile phase B you will need: 1) LC/MS grade acetonitrile (liquid);  2) Ammonium Hydroxide (liquid)." +
                " Procedure: Add " + pb1 + " mL LC/ MS grade Ammonium Hydroxide to a " + pb2 +
                " L fresh bottle of LC / MS grade Acetonitrile \n \n" +
                " For Weak Wash you will need: 1) LC/MS grade Acetonitrile (liquid); 2) LC/MS grade Water." +
                "Procedure: Add " + ww1 + " mL water to " + ww2 +
                "mL of acetonitrile.Stir with stir bar for 5 minutes to mix. \n \n" +
                "For Strong Wash you will need: 1) LC/MS grade Acetonitrile (liquid); 2) LC/MS grade Water" +
                "Procedure: Add " + sw1 + " mL LC/ MS grade Water to " + sw2 +
                " mL of LC / MS grade Acetonitrile.  Stir with stir bar for 5 minutes to mix.";
            return output;
        }

        internal string CleanString(string text)
        {
            text = text.Replace("data", "");
            text = text.Replace("mw", "");
            text = text.Replace(" molecular weight", "");
            text = text.Replace("formula", "");
            text = text.Replace("synonym", "");
            text = text.Replace(" cid", "");
            text = text.Replace("keys", "");
            text = text.Replace("full", "");
            text = text.Replace(",", "");
            return text;
        }

        #region  Process Chemical - Operations

        internal string ProcessChemical(string text)
        {
            var spellChecker = new Spelling();
            var showMw = false;
            var showFormula = false;
            var showSynonyms = false;
            var showCid = false;
            var showKeys = false;
            var response = string.Empty;
            DataSpecified = false;

            var check = CheckProgress_ProcessChemical(text, ref response, ref showMw, ref showFormula,
                ref showSynonyms, ref showCid, ref showKeys, ref _dialogTarget);
            if (check) return response;

            var dataMissing = false;
            ChemicalFormula = GetFormula(Chemical, ref dataMissing);
            if (dataMissing)
            {
                response = Chemical +
                           " was not found in the PubChem database, please specify compound name,   ";
                response += spellChecker.AnalyzeString(Chemical);
                Chemical = string.Empty;
                return response;
            }

            response += "Chemical is " + Chemical + "\n";
            _dialogTarget = 0;

            if (showMw)
            {
                MolecularWeight = GetMw(Chemical);
                response += " Molecular Weight is " + MolecularWeight + " g/mol \n";
            }

            if (showFormula) response += " Formula is " + ChemicalFormula + "\n";
            if (showCid)
            {
                CiDs = GetCiDs(Chemical);
                if (CiDs.Length > 9)
                    response += ". C.I.D. numbers are \n" + CiDs + "\n";
                else
                    response += ". C.I.D. number is " + CiDs + "\n";
            }

            if (showKeys)
            {
                InchiKeys = GetInChIKeys(Chemical);
                response += " InChi Keys = \n" + InchiKeys;
            }

            if (showSynonyms)
            {
                Synonyms = GetSynonyms(Chemical);
                response += " Synonyms are " + Synonyms;
            }

            return response;
        }

        private bool CheckProgress_ProcessChemical(string text, ref string response, ref bool showMw,
            ref bool showFormula,
            ref bool showSynonyms, ref bool showCid, ref bool showKeys, ref int dialougeTargetSkill)
        {
            if (string.IsNullOrEmpty(Chemical) && dialougeTargetSkill != 1)
            {
                response = "Ok please state the target Chemical compound";
                dialougeTargetSkill = 1;
                return true;
            }

            if (!DataSpecified)
            {
                dialougeTargetSkill = 1;
                text = IdentifyData(text, out showMw, out showFormula, out showSynonyms, out showCid, out showKeys);

                if (string.IsNullOrEmpty(Chemical))
                    Chemical = text;

                if (!DataSpecified)
                {
                    response =
                        "Ok please state any specific data you would like to know, for all simply put data, full.";
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Calculate Molarity - Operations

        //continue setting this up for Operations run
        internal string ProcessMolarity(string text)
        {
            try
            {
                var response = string.Empty;
                var spellChecker = new Spelling();
                var result = CheckProgress_Molarity(text, ref response, Chemical, Weight, Volume);
                if (result) return response;

                var molarity = Calculate_Chemical_Mol_Concentration(Chemical, Weight, Volume);
                molarity = Math.Round(molarity, 2);
                var molesCount = Calculate_Moles(Chemical, Weight);
                response = "Conversion complete.  The number of moles for " + Weight +
                           " grams of " + Chemical +
                           " at volume " + Volume + " liters is " + molesCount +
                           " moles and the molar concentration c = "
                           + molarity + " mol/L.";
                if (molarity > -1.1 && molarity < -0.9)
                {
                    response = "Conversion FAILED calculation did not return a valid result. Please try again";
                    response += spellChecker.AnalyzeString(Chemical);
                }

                response += ".  Data Saved";
                ClearData();
                _dialogTarget = 0;
                _multiDialougeTarget = 0;
                return response;
            }
            catch
                (Exception ex)
            {
                ClearData();
                _dialogTarget = 0;
                _multiDialougeTarget = 0;
                var response = "Error, there was an issue with the calculation attempt   - " + ex.Message;
                return response;
            }
        }

        private bool CheckProgress_Molarity(string text, ref string response, string chemical, double weight,
            double volume)
        {
            if (weight == 0 && _dialogTarget == 7 && _multiDialougeTarget == 1)
            {
                var check = double.TryParse(text.Replace("grams", ""), out _);
                if (check)
                {
                    text = text.Replace("grams", "");
                    Weight = Convert.ToDouble(text);
                }
            }

            if (_dialogTarget == 7 && string.IsNullOrEmpty(chemical) && _multiDialougeTarget == 2)
                Chemical = text.Replace("chemical", "");

            if (_dialogTarget == 7 && volume == 0 && _multiDialougeTarget == 3)
            {
                var check = double.TryParse(text.Replace("liters", ""), out _);
                if (check)
                {
                    text = text.Replace("liters", "");
                    Volume = Convert.ToDouble(text);
                }
            }


            if (Weight == 0)
            {
                _dialogTarget = 7;
                response = "Ok please state the current weight in grams";
                _multiDialougeTarget = 1;
                return true;
            }

            if (Chemical == string.Empty)
            {
                _dialogTarget = 7;
                response = "Ok please state the target chemical compound";
                _multiDialougeTarget = 2;
                return true;
            }

            if (Volume == 0)
            {
                _dialogTarget = 7;
                response = "Ok please state the current volume";
                _multiDialougeTarget = 3;
                return true;
            }

            return false;
        }

        #endregion

        #region Calculate Volume - Operations

        public string CalculateVolume(string text)
        {
            try
            {
                var response = string.Empty;
                var spellChecker = new Spelling();
                var result = ChecKProgress_Volume(text, ref response, Chemical, Weight, Concentration);
                if (result) return response;

                var volume = Convert_Chemical_Volume
                    (Weight, Concentration, Chemical);

                var cVolume = $"{volume:0.00}";
                response = "Conversion complete.  The Volume of " + Weight + " grams of " +
                           Chemical +
                           " at Concentration " +
                           Concentration + " mols per liter is " + cVolume + " liters.";
                if (volume < 0)
                {
                    response = "Conversion FAILED calculation did not return a valid result. please try again";
                    response += spellChecker.AnalyzeString(Chemical);
                }

                response += ".  Data Saved";

                ClearData();
                _dialogTarget = 0;
                _multiDialougeTarget = 0;

                return response;
            }
            catch
                (Exception ex)
            {
                ClearData();
                _dialogTarget = 0;
                _multiDialougeTarget = 0;
                var response = "Error, there was an issue with the calculation attempt   - " + ex.Message;

                return response;
            }
        }

        public bool ChecKProgress_Volume(string text, ref string response, string chemical, double weight,
            double concentration)
        {
            if (weight == 0 && _dialogTarget == 5 && _multiDialougeTarget == 1)
            {
                var check = double.TryParse(text.Replace("grams", ""), out _);
                if (check)
                {
                    text = text.Replace("grams", "");
                    Weight = Convert.ToDouble(text);
                }
            }

            if (_dialogTarget == 5 && string.IsNullOrEmpty(chemical) && _multiDialougeTarget == 2)
                Chemical = text.Replace("chemical", "");

            if (_dialogTarget == 5 && concentration == 0 && _multiDialougeTarget == 3)
            {
                var check = double.TryParse(text.Replace("mols", ""), out _);
                if (check)
                {
                    text = text.Replace("mols", "");
                    Concentration = Convert.ToDouble(text);
                }
            }

            if (Weight == 0)
            {
                _dialogTarget = 5;
                response = "Ok please state the current weight in grams";
                _multiDialougeTarget = 1;
                return true;
            }

            if (Chemical == string.Empty)
            {
                _dialogTarget = 5;
                response = "Ok please state the target chemical compound";
                _multiDialougeTarget = 2;
                return true;
            }

            if (Concentration == 0 && Concentration < 0.1)
            {
                _dialogTarget = 5;
                response = "Ok please state the current Concentration";
                _multiDialougeTarget = 3;
                return true;
            }

            return false;
        }

        #endregion

        #region Calculate Weight - Operations

        public string CalculateWeight(string text)
        {
            try
            {
                var response = string.Empty;
                var spellChecker = new Spelling();
                var result = CheckProgress_Weight(text, ref response, Chemical, Concentration, Volume);
                if (result) return response;

                var weight = Convert_Chemical_Weight(Volume, Concentration, Chemical);
                var cWeight = $"{weight:0.0000}";
                response = "Calculation complete.  In order to prepare " + Volume + " liters of " + Chemical +
                           " solution at " + Concentration + " mol/L " +
                           " you will need to measure " + cWeight + " grams of the chemical.";
                if (weight < 0)
                {
                    response = "Conversion FAILED calculation did not return a valid result. Please try again.";
                    response += spellChecker.AnalyzeString(Chemical);
                }

                response += " Data Saved.";
                ClearData();
                _dialogTarget = 0;
                _multiDialougeTarget = 0;
                return response;
            }
            catch
            {
                var response = "Conversion FAILED calculation did not return a valid result. please try again";
                ClearData();
                _dialogTarget = 0;
                _multiDialougeTarget = 0;

                return response;
            }
        }

        public bool CheckProgress_Weight(string text, ref string response, string chemical, double concentration,
            double volume)
        {
            if (concentration == 0 && _dialogTarget == 6 && _multiDialougeTarget == 1)
            {
                text = text.Replace("mol/L", "");
                text = text.Replace("m", "");
                var check = double.TryParse(text, out _);
                if (check) Concentration = Convert.ToDouble(text);
            }

            if (_dialogTarget == 6 && string.IsNullOrEmpty(chemical) && _multiDialougeTarget == 2)
                Chemical = text.Replace("chemical", "");
            if (_dialogTarget == 6 && volume == 0 && _multiDialougeTarget == 3)
            {
                var check = double.TryParse(text.Replace("liters", ""), out _);
                if (check)
                {
                    text = text.Replace("liters", "");
                    Volume = Convert.ToDouble(text);
                }
            }

            if (Concentration == 0)
            {
                _dialogTarget = 6;
                response = "Ok, please input the CONCENTRATION in [mol/L] ?";
                _multiDialougeTarget = 1;
                return true;
            }

            if (Chemical == string.Empty)
            {
                _dialogTarget = 6;
                response = "Ok, please input the CHEMICAL name ?";
                _multiDialougeTarget = 2;
                return true;
            }

            if (Volume == 0)
            {
                _dialogTarget = 6;
                response = "Ok, please input the VOLUME of the solution in liters?";
                _multiDialougeTarget = 3;
                return true;
            }

            return false;
        }

        #endregion

        #region Calculate Mobile Phase- Operations

        public string CalculateMobilePhase(string text)
        {
            try
            {
                var response = string.Empty;
                _dialogTarget = 4;
                var result = CheckProgress_MP(text, ref response, Chromatography, Volume);
                if (result) return response;

                response = Get_resipes(Volume, Chromatography);
                _dialogTarget = 0;
                _multiDialougeTarget = 0;
                ClearData();
                response += ".  Data Saved";

                return response;
            }
            catch
                (Exception ex)
            {
                _dialogTarget = 0;
                _multiDialougeTarget = 0;
                ClearData();
                var response = "Error, there was an issue with the calculation attempt   - " + ex.Message;

                return response;
            }
        }

        internal bool CheckProgress_MP(string text, ref string response, string chromotography, double volume)
        {
            if (volume == 0 && _dialogTarget == 4 && _multiDialougeTarget == 1)
            {
                var check = double.TryParse(text.Replace("liters", ""), out _);
                if (check)
                {
                    text = text.Replace("liters", "");
                    Volume = Convert.ToDouble(text);
                }
            }

            if (_dialogTarget == 4 && string.IsNullOrEmpty(chromotography) && _multiDialougeTarget == 2)
                Chromatography = text;

            if (Volume > -0.1 && Volume < 0.1)
            {
                _dialogTarget = 4;
                response = "Ok please state the current volume you wish to calculate";
                _multiDialougeTarget = 1;
                return true;
            }

            if (Chromatography == string.Empty)
            {
                _dialogTarget = 4;
                response = "Ok please state the current chromatography";
                _multiDialougeTarget = 2;
                return true;
            }

            return false;
        }

        #endregion

        #region Chem Data Methods

        internal void FillChemicalValues(string text)
        {
            Chemical = text;
            var failed = false;
            ChemicalFormula = GetFormula(Chemical, ref failed);
            if (!failed)
            {
                MolecularWeight = GetMw(Chemical);
                CiDs = GetCiDs(Chemical);
                InchiKeys = GetInChIKeys(Chemical);
                Synonyms = GetSynonyms(Chemical);
            }
        }

        internal void ClearData()
        {
            Volume = 0;
            Weight = 0;
            Concentration = 0;
            Chromatography = string.Empty;
            Chemical = string.Empty;
            MolecularWeight = string.Empty;
            ChemicalFormula = string.Empty;
            Synonyms = string.Empty;
            InchiKeys = string.Empty;
            CiDs = string.Empty;
            DataSpecified = false;
        }

        internal string GetFormula(string chemical, ref bool failed)
        {
            try
            {
                try
                {
                    //build the URI and send request
                    var uri = new Uri(
                        $"https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/{chemical}/property/MolecularFormula/TXT");
                    //get BiMessage back
                    var response = HttpClient.GetStringAsync(uri);
                    string[] items;
                    if (response.Result.Contains("\n"))
                    {
                        items = response.Result.Split(new[] {"\n"}, StringSplitOptions.None);
                        return items[0];
                    }

                    // return data
                    return response.Result;
                }
                catch
                {
                    //build the uri and send request
                    var uri = new Uri($"http://rest.kegg.jp/find/compound/{chemical}");
                    //get BiMessage back
                    var response = HttpClient.GetStringAsync(uri);
                    var s = response.Result;
                    var items = s.Split(new[] {"cpd:"}, StringSplitOptions.None);
                    //return data
                    if (items.Length > 0)
                        return items[1];
                    return null;
                }
            }
            catch (Exception ex)
            {
                failed = true;
                return ex.Message;
            }
        }

        internal string GetCiDs(string chemical)
        {
            try
            {
                //build the URI and send request
                var uri = new Uri($"https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/{chemical}/cids/TXT");
                //get BiMessage back
                var response = HttpClient.GetStringAsync(uri);
                //return data
                return response.Result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        internal string GetInChIKeys(string chemical)
        {
            try
            {
                //build the URI and send request
                var uri = new Uri(
                    $"https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/{chemical}/property/InChIKey/TXT");
                //get BiMessage back
                var response = HttpClient.GetStringAsync(uri);
                //return data
                return response.Result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        internal string GetSynonyms(string chemical)
        {
            try
            {
                //build the URI and send request
                var uri = new Uri($"https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/{chemical}/synonyms/TXT");
                //get BiMessage back
                var response = HttpClient.GetStringAsync(uri);
                string[] items;
                if (response.Result.Contains("\n"))
                {
                    items = response.Result.Split(new[] {"\n"}, StringSplitOptions.None);
                    if (items.Length > 5)
                        return items[0] + " " + items[1] + " " + items[2] + " " + items[3] + " " + items[4] + " ";
                    return response.Result;
                }

                //return data
                return response.Result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        internal string GetMw(string chemical)
        {
            try
            {
                var uri = new Uri(
                    $"https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/{chemical}/property/MolecularWeight/TXT");
                var response = HttpClient.GetStringAsync(uri);
                var result = response.Result;
                if (result.Contains("\n"))
                {
                    var items = result.Split(new[] {"\n"}, StringSplitOptions.None);
                    var mw = Convert.ToDouble(items[0]);
                    mw = Math.Round(mw, 3);
                    return mw.ToString(CultureInfo.InvariantCulture);
                }

                return result;
            }
            catch
            {
                try
                {
                    var uri = new Uri(
                        $"https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/{chemical}/property/MolecularWeight/TXT");
                    var response = HttpClient.GetStringAsync(uri);
                    var result = response.Result;
                    if (result.Contains("\n"))
                    {
                        var items = result.Split(new[] {"\n"}, StringSplitOptions.None);
                        var mw = Convert.ToDouble(items[0]);
                        mw = Math.Round(mw, 3);
                        return mw.ToString(CultureInfo.InvariantCulture);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    return "Error : " + ex.Message;
                }
            }
        }

        internal string Get_resipes(double volume, string chromotography)
        {
            try
            {
                var output = string.Empty;
                if (chromotography == "one" || chromotography == "1" || chromotography == "r phase" ||
                    chromotography == "reverse phase") output = PhaseOne(volume);
                if (chromotography == "two" || chromotography == "2" || chromotography == "hilic" ||
                    chromotography == "helic") output = PhaseTwo(volume);
                return "Your Procedure at Volume " + volume + " liters in Chromatogrphy " + chromotography +
                       "  is as follows     - " + output;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        internal double GetDensity(double weight, double volume)
        {
            var mass = weight / 9.8;
            return mass / volume;
        }

        internal string IdentifyData(string text, out bool showMw, out bool showFormula, out bool showSynonyms,
            out bool showCid, out bool showKeys)
        {
            showMw = text.Contains("mw") || text.Contains("molecular weight");
            if (showMw)
                DataSpecified = true;

            showFormula = text.Contains("formula");
            if (showFormula)
                DataSpecified = true;

            showSynonyms = text.Contains("synonym");
            if (showSynonyms)
                DataSpecified = true;

            showCid = text.Contains(" cid");
            if (showCid) DataSpecified = true;

            showKeys = text.Contains("keys");
            if (showKeys) DataSpecified = true;

            if (text.Contains("full"))
            {
                showMw = true;
                showFormula = true;
                showSynonyms = true;
                showCid = true;
                showKeys = true;
                DataSpecified = true;
            }

            text = CleanString(text);
            return text;
        }

        #endregion
    }
}