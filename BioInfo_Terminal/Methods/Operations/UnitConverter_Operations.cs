using System;
using System.Collections.Generic;
using BioInfo_Terminal.Methods.Dialog_Handling;
using UnitsNet;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace BioInfo_Terminal.Methods.Operations
{
    internal class UnitConverterOperations : IOperation
    {
        private int _dialougeTarget;
        private int _multiDialougeTarget;

        internal UnitConverterOperations()
        {
            _dialougeTarget = 0;
            _multiDialougeTarget = 0;
            Size = 0;
            UnitA = string.Empty;
            UnitB = string.Empty;
        }

        internal double Size { get; set; } // mass of the current measurement to be converted 
        internal string UnitA { get; set; } // Type of measurement to convert
        internal string UnitB { get; set; } // Type of measurement to return after conversion 

        public void FillValues(Dictionary<string, string> values)
        {
            if (values["Size"] != string.Empty && values["Size"] != "0")
                Size = Convert.ToDouble(values["Size"]);
            if (values["UnitA"] != string.Empty)
                UnitA = values["UnitA"];
            if (values["UnitB"] != string.Empty)
                UnitB = values["UnitB"];
        }

        public string RunOperations(string id, string text, ref int dialougeTarget)
        {
            if (id == "Convert_Units")
            {
                var response = Calculate_UnitConversion(text);
                dialougeTarget = _dialougeTarget;
                return response;
            }

            return null;
        }

        internal double ConvertUnits(string unitA, string unitB, double size)
        {
            try
            {
                string a = FormatForConversion(unitA);
                string b = FormatForConversion(unitB);
                return UnitConverter.ConvertByName(size, "Length", a, b);
            }
            catch 
            {
                string a = FormatForConversion(unitA);
                string b = FormatForConversion(unitB);
                return UnitConverter.ConvertByName(size, "Mass", a, b);
            }
        }

        internal double ConvertDistance_ToMeters(string type, double size)
        {
            //https://stackoverflow.com/questions/14015675/unit-converter-c-sharp

            switch (type)
            {
                case "millimeters":
                    return size / 1000;
                case "centimeters":
                    return size / 100;
                case "decimeters":
                    return size / 10;
                case "yards":
                    return size / 1.094;
                case "kilometers":
                    return size *1000;
                case "micrometers":
                    return size / 1000000;
                case "nanometers":
                    return size / 1000000000;
                case "miles":
                    return size * 1609.344;
                case "foot":
                    return size / 3.281;
                case "inchs":
                    return size / 39.37;
                case "nautical miles":
                    return size * 1852;
            }
            return -1;
        }

        internal double ConvertMass_ToGrams(string type, double mass)
        {
            switch (type)
            {
                case "ounces":
                    return mass * 28.35;
                case "pounds":
                    return mass * 453.592;
                case "stones":
                    return mass * 6350.293;
                case "micrograms":
                    return mass / 1000000;
                case "nanograms":
                    return mass / 1000000000;
                case "milligrams":
                    return mass /1000;
                case "kilograms":
                    return mass * 1000;
                case "ton":
                    return mass * 1000000;
            }
            return -1;
        }

        internal void ClearData()
        {
            Size = 0;
            UnitA = string.Empty;
            UnitB = string.Empty;
        }

        public static string FormatForConversion(string s)
        {
            string r = string.Empty;
            // Check for empty string.  
            if (string.IsNullOrEmpty(s))
            {
                return r;
            }
            // Return char and concat substring.  
            r = char.ToUpper(s[0]) + s.Substring(1);
            if (r[r.Length - 1] == 's')
            {
                r = r.TrimEnd('s');
            }
            return r;
        }

        internal void SetData(double size, string unitA, string unitB)
        {
            Size = size;
            UnitA = unitA;
            UnitB = unitB;
        }

        #region Calculate Unit Conversion

        public string Calculate_UnitConversion(string text)
        {
            try
            {
                var response = string.Empty;
                var result = CheckProgress_UnitConversion(text, ref response, UnitA,
                    UnitB, Size);
                if (result) return response;

                var unitA = UnitA;
                var unitB = UnitB;
                var size = Size;
                var conversion = ConvertUnits(unitA, unitB, size);
                response = "Conversion Complete " + size + " " + unitA + " equals " + conversion + " " + unitB;
                response += ". Data Saved";
                if (conversion > -1.1 && conversion < -0.9)
                    response = "Conversion FAILED. " + size + " " + unitA + " to " + unitB +
                               " did not return a valid result. please try again";
                _dialougeTarget = 0;
                _multiDialougeTarget = 0;
                ClearData();
                return response;
            }
            catch (Exception ex)
            {
                _dialougeTarget = 0;
                _multiDialougeTarget = 0;
                ClearData();
                var response = "Error, there was an issue with the calculation attempt   - " + ex.Message;
                return response;
            }
        }

        internal bool CheckProgress_UnitConversion(string text, ref string response, string unitA, string unitB,
            double size)
        {
            if (size ==0 && _dialougeTarget == 3 && _multiDialougeTarget ==1)
            {
                var check = double.TryParse(text, out _);
                if (check) Size = Convert.ToDouble(text);
            }

            if (unitA == string.Empty && size > 0 && _dialougeTarget == 3 && _multiDialougeTarget == 2)
            {
                UnitA = text;
            }

            if (!string.IsNullOrEmpty(unitA) && string.IsNullOrEmpty(unitB) && _dialougeTarget == 3 && _multiDialougeTarget == 3)
                UnitB = text;

            if (Size > -0.01 && Size < 0.01)
            {
                _dialougeTarget = 3;
                response = "Ok please state the target conversion size";
                _multiDialougeTarget = 1;
                return true;
            }
            if (UnitA == string.Empty)
            {
                _dialougeTarget = 3;
                response = "Ok please state the original Unit of measurement";
                _multiDialougeTarget = 2;
                return true;
            }
            if (UnitB == string.Empty)
            {
                _dialougeTarget = 3;
                response = "Ok What is the second Unit of measurement?";
                _multiDialougeTarget = 3;
                return true;
            }

            return false;
        }

        #endregion
    }
}