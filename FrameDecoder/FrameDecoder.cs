using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZOYI
{
    public partial class FrameDecoder
    {
        int FRAME_SIZE = 18;
        byte BIT_4 = 0x10;
        byte[] OL = new byte[] { 0x10, 0x00, 0x61, 0xEB, 0x00 };
        byte[][] OL_PATTERNS = new byte[][] {
            new byte[] { 0x00, 0x00, 0x61, 0xEB, 0x00 },
            new byte[] { 0x00, 0x00, 0x61, 0xDB, 0x00 },
            new byte[] { 0x00, 0x00, 0x61, 0x8B, 0x00 },
            new byte[] { 0x00, 0xEB, 0x61, 0x00, 0x00 },
            new byte[] { 0xEB, 0x00, 0x61, 0x00, 0x00 },
            new byte[] { 0x61, 0xEB, 0x00, 0x00, 0x00 },
            new byte[] { 0x00, 0x61, 0xEB, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x61, 0xEB },
        };

        const float STABILIZATION_THRESHOLD = 0.00001f;

        // STD, EXT
        public string? Value { get; private set; }
        // STD
        public string? Label { get; private set; }
        // EXT
        public string? Cap_unit { get; private set; }
        // STD, EXT
        public string? Unit { get; private set; }
        // EXT
        public string? Mode1 { get; private set; }
        // EXT
        public string? Mode2 { get; private set; }
        // EXT
        public string? Freq { get; private set; }
        // EXT
        public string? Freq_unit { get; private set; }

        public float? BaseValue { get; private set; }
        public string? BaseUnit { get; private set; }

        private bool _prevHold;
        private string _heldValue;
        private string _heldUnit;
        private string _heldFreq;
        private string _heldMode2;
        public bool IsHeld { get; private set; }
        public bool IsRel { get; private set; }
        public bool IsContinuity { get; private set; }
        public bool IsDiode { get; private set; }

        private static readonly Dictionary<string, (string Base, float Factor)> _unitScale = new()
        {
            ["V"] = ("V", 1),
            ["mV"] = ("V", 0.001f),
            ["A"] = ("A", 1),
            ["mA"] = ("A", 0.001f),
            ["Ω"] = ("Ω", 1),
            ["KΩ"] = ("Ω", 1000),
            ["kΩ"] = ("Ω", 1000),
            ["MΩ"] = ("Ω", 1000000),
            ["nF"] = ("nF", 1),
            ["uF"] = ("nF", 1000),
            ["mF"] = ("nF", 1000000),
            ["%"] = ("%", 1),
            ["Hz"] = ("Hz", 1),
            ["HZ"] = ("Hz", 1),
            ["kHz"] = ("Hz", 1000),
        };

        private MLua mLua;

        public FrameDecoder()
        {
            mLua = new MLua();
        }

        /*
         * 
         */
        public void DecodeStdandard(String frame)
        {
            string[] label_value = frame.Split(':');
            // label, unit2
            string[] ret = new string[2];

            Value = label_value[1];

            switch (label_value[0])
            {
                case "Electricity":
                    ret[0] = "Pomiar PRĄDU";
                    ret[1] += "A";
                    break;
                case "AElectricity":
                    ret[0] = "Pomiar PRĄDU";
                    ret[1] = "A";
                    break;
                case "mAElectricity":
                    ret[0] = "Pomiar PRĄDU mA";
                    ret[1] = "mA";
                    break;
                case "MOMResistance":
                    ret[0] = "Pomiar OPORNOŚCI";
                    ret[1] = "MΩ";
                    break;
                case "OMResistance":
                    ret[0] = "Pomiar OPORNOŚCI";
                    ret[1] = "Ω";
                    break;
                case "KOMResistance":
                    ret[0] = "Pomiar OPORNOŚCI";
                    ret[1] = "KΩ";
                    break;
                case "OMbeep":
                    ret[0] = "CIĄGŁOŚĆ";
                    ret[1] = "";
                    break;
                case "VDiode":
                    ret[0] = "DIODA";
                    ret[1] = "mV";
                    if (!ret[1].Contains("0,"))
                        ret[1] = "V";
                    break;
                case "nFCap":
                    ret[0] = "Pomiar POJEMNOŚCI";
                    ret[1] = "nF";
                    break;
                case "uFCap":
                    ret[0] = "Pomiar POJEMNOŚCI";
                    ret[1] = "uF";
                    break;
                case "mFCap":
                    ret[0] = "Pomiar POJEMNOŚCI";
                    ret[1] = "mF";
                    break;
                case "VVoltage":
                    ret[0] = "POMIAR NAPIĘCIA";
                    ret[1] = "V";
                    break;
                case "mVVoltage":
                    ret[0] = "POMIAR NAPIĘCIA mV";
                    ret[1] = "mV";
                    break;
            }

            Label = ret[0];
            Unit = ret[1];
            IsContinuity = label_value[0] == "OMbeep";
            IsDiode = label_value[0] == "VDiode";

            StabilizeValue();
            NormalizeUnit();
        }

        /*
         * Extended
         */
        public void DecodeExtended(byte[] frame)
        {
            DecodeDigits(frame);
            DecodeUnit(frame);
            DecodeMode(frame);
            DecodeFreq(frame);

            if (Value != null && Value.Trim() != "OL" && !float.TryParse(Value.Trim(), System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out float _))
            {
                Value = "OL";
            }

            bool hold = (frame[11] & 0x80) > 0;

            if (hold)
            {
                if (!_prevHold)
                {
                    _heldValue = Value;
                    _heldUnit = Unit;
                    _heldFreq = Freq;
                    _heldMode2 = Mode2;
                    _prevHold = true;
                }
                Value = _heldValue;
                Unit = _heldUnit;
                Freq = _heldFreq;
                Mode2 = _heldMode2;
                IsHeld = true;
            }
            else
            {
                _prevHold = false;
                IsHeld = false;
            }

            Label = Mode2;
            NormalizeUnit();
        }

        private void DecodeDigits(byte[] frame)
        {
            Value = " ";
            bool decimalFound = false;

            bool ol = false;
            foreach (var pat in OL_PATTERNS)
            {
                bool match = true;
                for (int j = 0; j < 5; j++)
                {
                    if ((frame[j + 1] & ~BIT_4) != pat[j]) { match = false; break; }
                }
                if (match) { ol = true; break; }
            }
            if (ol)
            {
                Value = "OL";
                return;
            }

            for (int i = 5; i > 0; i--)
            {
                if ((frame[i] & BIT_4) > 0)
                {
                    if (i == 5)
                    {
                        Value += "-";
                    }
                    else
                    {
                        Value += ".";
                        decimalFound = true;
                    }
                    frame[i] ^= BIT_4;
                }

                Value += DecodeDigit(frame[i]);
            }

            if (!decimalFound && Value.Length > 2)
            {
                Value = Value.Insert(Value.Length - 1, ".");
            }

            StabilizeValue(decimalFound);
        }

        private void DecodeUnit(byte[] frame)
        {
            Cap_unit = "";
            Unit = "";

            byte unit1 = frame[15];
            byte unit2 = frame[16];

            switch (unit1)
            {
                case 0x40:
                    Cap_unit = "n";
                    break;
                case 0x80:
                    Cap_unit = "u";
                    break;
                case 0x0C:
                    Cap_unit = "m";
                    break;
                default:
                    if (unit1 != 0x00)
                        Console.WriteLine($"[DEBUG] Unknown capacitance unit1 byte: 0x{unit1:X2} (frame: {BitConverter.ToString(frame)})");
                    break;
            }

            switch (unit2)
            {
                case 0x01:
                    Unit = "A";
                    break;
                case 0x02:
                    Unit = "V";
                    break;
                case 0x04:
                    Unit = "F";
                    break;
                case 0x09:
                    Unit = "mA";
                    break;
                case 0x0A:
                    Unit = "mV";
                    break;
                case 0x0C:
                    Unit = "mF";
                    break;
                case 0x40:
                    Unit = "Ω";
                    break;
                case 0x50:
                    Unit = "MΩ";
                    break;
                case 0x60:
                    Unit = "kΩ";
                    break;
            }

            if (Unit == "F")
            {
                Unit = Cap_unit + Unit;
            }
        }

        private void DecodeMode(byte[] frame)
        {
            Mode1 = "";
            Mode2 = "";
            Freq_unit = "";
            IsRel = false;
            IsContinuity = false;
            IsDiode = false;

            byte mode1 = frame[10];
            byte mode2 = frame[11];

            byte AUTO = 0x04;
            byte VFC = 0x08;
            byte DC = 0x10;
            byte AC = 0x40;
            byte HOLD = 0x80;

            if ((mode2 & AUTO) > 0)
                Mode2 += " AUTO";
            else
                Mode2 += " MANUAL";

            if ((mode2 & VFC) > 0)
                Mode2 += " AC V.F.C";
            else if ((mode2 & DC) > 0)
                Mode2 += " DC";
            else if ((mode2 & AC) > 0)
                Mode2 += " AC";

            if ((mode2 & HOLD) > 0)
                Mode2 += " HOLD";


            byte Hz = 0x02;
            byte kHz = 0x3;
            byte RELATIVE_M = 0x20;
            byte CONTINUE = 0x40;
            byte DIODE = 0x80;

            if ((mode1 & Hz) > 0)
                Freq_unit = "HZ";
            else if ((mode1 & kHz) > 0)
                Freq_unit = "kHz";
            else if ((mode1 & RELATIVE_M) > 0)
            {
                Mode1 = "RELATIVE";
                IsRel = true;
            }
            else if ((mode1 & CONTINUE) > 0)
            {
                Mode1 = "CONTINUE";
                IsContinuity = true;
            }
            else if ((mode1 & DIODE) > 0)
            {
                Mode1 = "DIODE";
                IsDiode = true;
            }
        }

        private void DecodeFreq(byte[] frame)
        {
            Freq = "";

            for (int i = 9; i >= 6; i--)
            {
                if ((frame[i] & BIT_4) > 0)
                {
                    Freq += ".";
                    frame[i] ^= BIT_4;
                }
                Freq += DecodeDigit(frame[i]);
            }
        }

        private void StabilizeValue(bool decimalFound = true)
        {
            if (string.IsNullOrEmpty(Value)) return;
            if (Value == "OL") return;

            float val;
            if (!float.TryParse(Value, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                return;

            if (decimalFound)
            {
                string trimmed = Value.Trim();
                int dot = trimmed.IndexOf('.');
                int intDigits = dot;
                if (trimmed[0] == '-') intDigits--;
                int dec = trimmed.Length - dot - 1;
                string fmt = new string('0', intDigits) + "." + new string('0', dec);

                if (Math.Abs(val) < STABILIZATION_THRESHOLD)
                    Value = " " + 0f.ToString(fmt, CultureInfo.InvariantCulture);
                else
                    Value = " " + Math.Round(val, dec, MidpointRounding.AwayFromZero).ToString(fmt, CultureInfo.InvariantCulture);
            }
            else
            {
                Value = " " + Value.Trim();
            }
        }

        private void NormalizeUnit()
        {
            if (string.IsNullOrEmpty(Value) || Value.Trim() == "OL" || string.IsNullOrEmpty(Unit))
            {
                BaseValue = null;
                BaseUnit = Unit;
                return;
            }

            if (!float.TryParse(Value, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
            {
                BaseValue = null;
                BaseUnit = Unit;
                return;
            }

            if (_unitScale.TryGetValue(Unit, out var scale))
            {
                BaseValue = val * scale.Factor;
                BaseUnit = scale.Base;
            }
            else
            {
                BaseValue = val;
                BaseUnit = Unit;
            }
        }

        private char DecodeDigit(byte hex)
        {
            switch (hex)
            {
                case 0xEB: return '0';
                case 0x0A: return '1';
                case 0xAD: return '2';
                case 0x8F: return '3';
                case 0x4E: return '4';
                case 0xC7: return '5';
                case 0xE7: return '6';
                case 0x8A: return '7';
                case 0xEF: return '8';
                case 0xCF: return '9';
                default: return ' ';
            }
        }

        /*
         * JSON
         */
        public string JsonSerialize()
        {
            var retMembers = new
            {
                Value = this.Value,
                Label = this.Label,
                Unit = this.Unit,
                Cap_unit = this.Cap_unit,
                Mode1 = this.Mode1,
                Mode2 = this.Mode2,
                Freq = this.Freq,
                Freq_unit = this.Freq_unit
            };

            return JsonSerializer.Serialize(retMembers);
        }


        /*
         * LUA
         */
        public void DecodeLua(String frame)
        {
            string[] ret = mLua.parseLabelValueSuffix_LUA(frame);

            Label = ret[0];
            Value = ret[1];
            Unit = ret[2];

            StabilizeValue();
            NormalizeUnit();
        }

        public void LuaReload(String path)
        {
            mLua.LuaReload(path);
        }

        public void luaHighlightRichTextBox(string code, RichTextBox rtbEditor)
        {
            mLua.luaHighlightRichTextBox(code, rtbEditor);
        }
    }
}
