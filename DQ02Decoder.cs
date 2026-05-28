using System;
using System.Globalization;

namespace ZOYI
{
    public class DQ02Data
    {
        // Raw CSV fields
        public string[] RawFields { get; set; } = Array.Empty<string>();
        public string RawLine { get; set; } = "";

        // Parsed values
        public double PrimaryValue { get; set; }
        public double SecondaryValue { get; set; }
        public string Function { get; set; } = "";      // C, L, R, Z, V, etc.
        public string SecondaryParam { get; set; } = "";  // D, Q, X, P, R (ESR)
        public string RangeMode { get; set; } = "";
        public string ModelMode { get; set; } = "";       // AUTO, SER, PAR (user's circuit model selection)
        public string CircuitMode { get; set; } = "";    // SER, PAR (actual applied circuit)
        public string Speed { get; set; } = "";
        public string Trigger { get; set; } = "";
        public string RangeValue { get; set; } = "";     // AUTO or numeric impedance in ohms (from field 8)
        public double Frequency { get; set; }
        public double Level { get; set; }
        public double Nominal { get; set; }               // reference value (field 14)
        public double Tolerance { get; set; }              // tolerance % (field 15)
        public double Output { get; set; }                 // output level/type (field 16)
        public double MinLimit { get; set; }
        public double MaxLimit { get; set; }

        // Formatted display
        public string DisplayPrefix { get; set; } = "";   // Cp, Cs, Lp, Ls, R, Z, V
        public string DisplayValue { get; set; } = "—";
        public string DisplayUnit { get; set; } = "";
        public string DisplaySecondary { get; set; } = "";
        public string DisplayStatus { get; set; } = "";

        public bool IsValid { get; set; }

        private static bool IsOverload(string val)
        {
            return !string.IsNullOrEmpty(val) && val.Trim().ToUpperInvariant() == "0L";
        }

        private static string FormatValue(double val, string func)
        {
            if (double.IsInfinity(val) || double.IsNaN(val))
                return "OL";

            double av = Math.Abs(val);
            if (func == "C")
            {
                if (av < 1e-9) return (val * 1e12).ToString("F3") + " pF";
                if (av < 1e-6) return (val * 1e9).ToString("F3") + " nF";
                if (av < 1e-3) return (val * 1e6).ToString("F3") + " µF";
                if (av < 1e0)  return (val * 1e3).ToString("F3") + " mF";
                return val.ToString("F6") + " F";
            }
            if (func == "L")
            {
                if (av < 1e-6) return (val * 1e9).ToString("F3") + " nH";
                if (av < 1e-3) return (val * 1e6).ToString("F3") + " µH";
                if (av < 1e0)  return (val * 1e3).ToString("F3") + " mH";
                return val.ToString("F6") + " H";
            }
            if (func == "R" || func == "Z")
            {
                if (av < 1e3)  return val.ToString("F4") + " Ω";
                if (av < 1e6)  return (val / 1e3).ToString("F3") + " kΩ";
                return (val / 1e6).ToString("F3") + " MΩ";
            }
            if (func == "V")
            {
                if (av < 1)    return (val * 1000).ToString("F1") + " mV";
                return val.ToString("F4") + " V";
            }
            return val.ToString("G6");
        }

        private static string FormatSecondary(double val, string param, string function = "")
        {
            if (double.IsNaN(val)) return $"{param}: OL";

            // In BAT mode (function=V), "R" means internal resistance, not ESR
            if (param == "R" && function == "V")
                return $"R: {FormatValue(val, "R")}";

            return param switch
            {
                "D"   => $"D: {val:F4}",            // dissipation factor (C mode)
                "Q"   => $"Q: {val:F2}",            // quality factor (L mode)
                "X"   => $"X: {FormatValue(val, "R")}",   // reactance (R mode)
                "P"   => $"θ: {val:F2}°",           // phase angle (Z mode)
                "R"   => $"ESR: {FormatValue(val, "R")}", // equivalent series resistance
                "ESR" => $"ESR: {FormatValue(val, "R")}",
                _     => $"{param}: {val:G4}"
            };
        }

        public static string FormatNominal(double val, string func)
        {
            if (val <= 0) return "—";
            return FormatValue(val, func);
        }

        /// <summary>
        /// Parse CSV line from ZOYI DQ02.
        /// Fields: primary, secondary, range, function, loss, range2, circuit, speed, trigger, freq, level, min, max, suffix, nominal, tolerance, output
        /// </summary>
        public static DQ02Data Parse(string line)
        {
            var result = new DQ02Data();
            result.RawLine = line;

            if (string.IsNullOrWhiteSpace(line))
                return result;

            var parts = line.TrimEnd('\r', '\n').Split(',');
            result.RawFields = parts;

            if (parts.Length < 15)
                return result;

            result.PrimaryValue   = IsOverload(parts[0]) ? double.NaN : SafeParseDouble(parts[0]);
            result.SecondaryValue = IsOverload(parts[1]) ? double.NaN : SafeParseDouble(parts[1]);
            result.RangeMode      = parts[2];
            result.Function       = parts[3];
            result.SecondaryParam = parts[4];

            if (parts.Length > 5) result.ModelMode    = parts[5];
            if (parts.Length > 6) result.CircuitMode  = parts[6];
            if (parts.Length > 7) result.Speed       = parts[7];
            if (parts.Length > 8) { result.Trigger = parts[8]; result.RangeValue = parts[8]; }

            if (parts.Length > 9)
                result.Frequency = SafeParseDouble(parts[9]);
            if (parts.Length > 10)
                result.Level = SafeParseDouble(parts[10]);
            if (parts.Length > 11)
                result.MinLimit = SafeParseDouble(parts[11]);
            if (parts.Length > 12)
                result.MaxLimit = SafeParseDouble(parts[12]);

            // Field 14: Nominal value (reference)
            if (parts.Length > 14)
                result.Nominal = SafeParseDouble(parts[14]);

            // Field 15: Tolerance (%)
            if (parts.Length > 15)
                result.Tolerance = SafeParseDouble(parts[15]);

            // Field 16: Output
            if (parts.Length > 16)
                result.Output = SafeParseDouble(parts[16]);

            // Display prefix (Cp, Cs, Lp, Ls, R, Z, V)
            result.DisplayPrefix = result.Function switch
            {
                "V" => "V",  // BAT mode — battery voltage
                _ => (result.Function, result.CircuitMode) switch
                {
                    ("C", "PAR") => "Cp",
                    ("C", "SER") => "Cs",
                    ("C", _)     => "C",
                    ("L", "PAR") => "Lp",
                    ("L", "SER") => "Ls",
                    ("L", _)     => "L",
                    ("R", _)     => "R",
                    ("Z", _)     => "Z",
                    (var f, _)   => f
                }
            };

            result.DisplayValue     = FormatValue(result.PrimaryValue, result.Function);
            result.DisplaySecondary = FormatSecondary(result.SecondaryValue, result.SecondaryParam, result.Function);

            string freqStr = result.Frequency >= 1000
                ? (result.Frequency / 1000).ToString("F1") + " kHz"
                : result.Frequency.ToString("F0") + " Hz";
            string levelStr = result.Level >= 1
                ? result.Level.ToString("F0") + " V"
                : (result.Level * 1000).ToString("F0") + " mV";

            result.DisplayStatus = $"{freqStr} | {levelStr} | {result.CircuitMode} | {result.Speed}";

            result.IsValid = true;

            return result;
        }

        private static double SafeParseDouble(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var val);
            return val;
        }

        public static string FormatRange(string rangeValue)
        {
            if (string.IsNullOrEmpty(rangeValue) || rangeValue.Trim().ToUpperInvariant() == "AUTO")
                return "AUTO";

            if (double.TryParse(rangeValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double ohms) && ohms > 0)
            {
                if (ohms >= 1_000_000) return (ohms / 1_000_000).ToString("F0") + " MΩ";
                if (ohms >= 1000) return (ohms / 1000).ToString("F0") + " kΩ";
                return ohms.ToString("F0") + " Ω";
            }

            return rangeValue;
        }

        public override string ToString()
        {
            if (!IsValid) return "—";
            return $"{DisplayPrefix} {DisplayValue}  {DisplaySecondary}  ({DisplayStatus})";
        }
    }
}
