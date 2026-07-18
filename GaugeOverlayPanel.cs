using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace ZOYI
{
    public class GaugeOverlayPanel : Form
    {
        // Drag
        private bool _mouseDown;
        private Point _mouseDownPos;
        private Point _formDownPos;

        private readonly CheckBox _chb;
        private readonly System.Windows.Forms.Timer _smoothTimer = new System.Windows.Forms.Timer { Interval = 33 };
        private readonly System.Windows.Forms.Timer _elapsedTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        private DateTime _startTime = DateTime.Now;
        private bool _timerRunning;

        // Measurement data
        private float _targetValue;
        private float _animValue;
        private string _unit = "";
        private string _label = "";
        private string _displayValue;
        private bool _isOl;
        private float _maxValue;

        // Ranges: label, max value
        private readonly (string label, float max)[] _ranges =
        {
            ("10A", 10f),
            ("5V", 5f),
            ("25V", 25f),
            ("50V", 50f),
            ("100V", 100f),
            ("150V", 150f),
            ("200V", 200f),
            ("250V", 250f),
            ("350V", 350f),
            ("500V", 500f),
            ("700V", 700f),
            ("1000V", 1000f)
        };
        private readonly (string label, float max)[] _resistanceRanges =
        {
            ("10mΩ", 0.01f), ("100mΩ", 0.1f), ("250mΩ", 0.25f), ("500mΩ", 0.5f),
            ("1Ω", 1f), ("2Ω", 2f), ("5Ω", 5f),
            ("10Ω", 10f), ("20Ω", 20f), ("50Ω", 50f),
            ("100Ω", 100f), ("200Ω", 200f), ("500Ω", 500f),
            ("1kΩ", 1000f), ("2kΩ", 2000f), ("5kΩ", 5000f),
            ("10kΩ", 10000f), ("20kΩ", 20000f), ("50kΩ", 50000f),
            ("100kΩ", 100000f), ("200kΩ", 200000f), ("500kΩ", 500000f),
            ("1MΩ", 1000000f), ("2MΩ", 2000000f), ("5MΩ", 5000000f),
            ("10MΩ", 10000000f), ("20MΩ", 20000000f), ("60MΩ", 60000000f),
        };
        private readonly (string label, float max)[] _capacitanceRanges =
        {
            ("1nF", 1f), ("2nF", 2f), ("5nF", 5f),
            ("10nF", 10f), ("20nF", 20f), ("50nF", 50f),
            ("100nF", 100f), ("200nF", 200f), ("500nF", 500f),
            ("1µF", 1000f), ("2µF", 2000f), ("5µF", 5000f),
            ("10µF", 10000f), ("20µF", 20000f), ("50µF", 50000f),
            ("100µF", 100000f), ("200µF", 200000f), ("500µF", 500000f),
            ("1mF", 1000000f), ("2mF", 2000000f), ("5mF", 5000000f),
            ("10mF", 10000000f), ("20mF", 20000000f), ("50mF", 50000000f), ("100mF", 100000000f),
        };
        private int _selectedRange = 1; // start on first V range
        private bool _autoRange = true;
        private bool _isVfc;
        private bool _isHold;
        private bool _isRel;
        private bool _isResistance;
        private bool _isCapacitance;
        private bool _isContinuity;
        private bool _isDiode;
        private bool _continuityShorted;
        private int _highlightedRange = -1;
        private string _displayFormat = "F4";

        // Arc angles (degrees)
        private const float ArcStart = -210f;
        private const float ArcEnd = 30f;
        private const float ArcSpan = 240f;

        // Opacity
        private int _opacity = 92;

        // Volume
        private int _volume = 50;

        // Custom font/color
        private Font? _customValueFont;
        private Font? _customUnitFont;
        private Color _customValueColor = Color.White;

        public GaugeOverlayPanel(CheckBox chb)
        {
            _chb = chb;

            Text = "Gauge Overlay";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(560, 460);
            MinimumSize = new Size(400, 350);
            BackColor = Color.FromArgb(20, 20, 20);
            DoubleBuffered = true;
            AllowTransparency = false;
            ShowInTaskbar = false;
            TopMost = true;

            int xPos = Properties.Settings.Default.panel_gauge_pos_x;
            int yPos = Properties.Settings.Default.panel_gauge_pos_y;
            int scrW = Screen.PrimaryScreen.Bounds.Width;
            int scrH = Screen.PrimaryScreen.Bounds.Height;
            if (xPos < 0 || xPos > scrW) xPos = scrW / 4;
            if (yPos < 0 || yPos > scrH) yPos = scrH / 4;
            Location = new Point(xPos, yPos);

            Opacity = Properties.Settings.Default.panel_gauge_opacity / 100.0;
            _opacity = Properties.Settings.Default.panel_gauge_opacity;
            _volume = Properties.Settings.Default.panel_gauge_volume;
            _customValueFont = Properties.Settings.Default.panel_gauge_value_font;
            try { _customValueColor = ColorTranslator.FromHtml(Properties.Settings.Default.panel_gauge_value_color); } catch { _customValueColor = Color.White; }
            _selectedRange = Math.Clamp(Properties.Settings.Default.panel_gauge_range, 0, ActiveRanges.Length - 1);

            _smoothTimer.Tick += (_, _) =>
            {
                float prevAnim = _animValue;
                float diff = _targetValue - _animValue;
                if (Math.Abs(diff) < 0.0005f)
                    _animValue = _targetValue;
                else
                    _animValue += diff * 0.18f;
                if (Math.Abs(_animValue - prevAnim) > 0.0001f || Math.Abs(diff) < 0.0005f)
                Invalidate();
            };
            _smoothTimer.Start();

            _elapsedTimer.Tick += (_, _) => Invalidate();

            var ctx = new ContextMenuStrip();
            ctx.Items.Add("Ustawienia brzęczyka", null, (_, _) =>
            {
                using var dlg = new BuzzerSettingsForm();
                dlg.ShowDialog(this);
                var mw = Application.OpenForms.OfType<MainWindow>().FirstOrDefault();
                mw?.ApplyBuzzerSettings();
            });
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add("Zmień czcionkę...", null, (_, _) =>
            {
                using var fd = new FontDialog { Font = _customValueFont ?? new Font("Segoe UI", 42, FontStyle.Bold), ShowColor = true, Color = _customValueColor };
                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    _customValueFont = fd.Font;
                    _customValueColor = fd.Color;
                    Properties.Settings.Default.panel_gauge_value_font = fd.Font;
                    Properties.Settings.Default.panel_gauge_value_color = ColorTranslator.ToHtml(fd.Color);
                    Properties.Settings.Default.Save();
                    Invalidate();
                }
            });
            ctx.Items.Add("Przywróć domyślną czcionkę", null, (_, _) =>
            {
                _customValueFont = null;
                _customValueColor = Color.White;
                Properties.Settings.Default.panel_gauge_value_font = null;
                Properties.Settings.Default.panel_gauge_value_color = "White";
                Properties.Settings.Default.Save();
                Invalidate();
            });
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add("Resetuj MAX / TIMER", null, (_, _) =>
            {
                _maxValue = 0;
                _startTime = DateTime.Now;
                Invalidate();
            });
            ctx.Items.Add("Zamknij panel", null, (_, _) => { Hide(); _chb.Checked = false; });
            ContextMenuStrip = ctx;

            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseClick += OnMouseClick;
            FormClosed += OnFormClosed;
            Resize += (_, _) => Invalidate();
        }

        public void updatePanel(FrameDecoder fd)
        {
            _label = fd.Label;
            _unit = fd.Unit;
            _isVfc = fd.Label?.Contains("V.F.C") == true;
            _isHold = fd.IsHeld;
            _isRel = fd.IsRel;
            _isResistance = _unit == "Ω" || _unit == "kΩ" || _unit == "MΩ" || _unit == "KΩ";
            _isCapacitance = _unit == "nF" || _unit == "uF" || _unit == "µF" || _unit == "mF" || _unit == "F";
            _isContinuity = fd.IsContinuity;
            _isDiode = fd.IsDiode;

            if (_isContinuity && fd.BaseValue.HasValue)
                _continuityShorted = Math.Abs(fd.BaseValue.Value) <= 30;
            else
                _continuityShorted = false;
            if (_selectedRange >= ActiveRanges.Length) _selectedRange = ActiveRanges.Length - 1;

            string rawVal = fd.Value?.Trim() ?? "0";
            _isOl = rawVal == "OL";
            _highlightedRange = -1;
            if (_isOl)
            {
                _displayValue = "OL";
                _targetValue = 0f;
            }
            else
            {
                if (rawVal.StartsWith("-")) rawVal = rawVal.Substring(1);
                int dot = rawVal.IndexOf('.');
                if (dot > 0)
                {
                    int intDig = dot;
                    int decDig = rawVal.Length - dot - 1;
                    _displayFormat = new string('0', intDig) + "." + new string('0', decDig);
                }
                else
                {
                    _displayFormat = "F4";
                }

                float newVal;
                if (_isResistance && fd.BaseValue.HasValue)
                    newVal = fd.BaseValue.Value;
                else if (float.TryParse(fd.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out newVal))
                { }
                else
                    newVal = 0f;
                _targetValue = newVal;
                if (Math.Abs(newVal) > _maxValue)
                    _maxValue = Math.Abs(newVal);

                if (_isResistance || _isCapacitance)
                {
                    float abs = Math.Abs(newVal);
                    var ranges = ActiveRanges;
                    int best = 0;
                    float bestDist = float.MaxValue;
                    for (int i = 0; i < ranges.Length; i++)
                    {
                        float dist = Math.Abs(abs - ranges[i].max);
                        if (dist < bestDist) { bestDist = dist; best = i; }
                    }
                    _highlightedRange = best;

                    string unit;
                    float scaled;
                    if (_isResistance)
                    {
                        if (abs >= 1000000f) { scaled = abs / 1000000f; unit = "MΩ"; }
                        else if (abs >= 1000f) { scaled = abs / 1000f; unit = "kΩ"; }
                        else { scaled = abs; unit = "Ω"; }
                    }
                    else
                    {
                        if (abs >= 1000000f) { scaled = abs / 1000000f; unit = "mF"; }
                        else if (abs >= 1000f) { scaled = abs / 1000f; unit = "µF"; }
                        else { scaled = abs; unit = "nF"; }
                    }
                    if (scaled < 10f) _displayValue = scaled.ToString("F3") + " " + unit;
                    else if (scaled < 100f) _displayValue = scaled.ToString("F2") + " " + unit;
                    else _displayValue = scaled.ToString("F1") + " " + unit;
                }
                else
                {
                    _displayValue = null;
                }
            }

            if (_autoRange && !_isOl)
                AutoSelectRange();

            if (!_timerRunning)
            {
                _startTime = DateTime.Now;
                _timerRunning = true;
                _elapsedTimer.Start();
            }
        }

        private void AutoSelectRange()
        {
            var ranges = ActiveRanges;
            float abs = Math.Abs(_targetValue);
            if (abs < 0.001f) { _selectedRange = 0; return; }
            for (int i = 0; i < ranges.Length; i++)
            {
                if (abs < ranges[i].max * 0.92f)
                {
                    _selectedRange = i;
                    return;
                }
            }
            _selectedRange = ranges.Length - 1;
        }

        private (string label, float max)[] ActiveRanges => _isResistance ? _resistanceRanges : _isCapacitance ? _capacitanceRanges : _ranges;
        private float GaugeMax
        {
            get
            {
                var ranges = ActiveRanges;
                if (_selectedRange < 0 || _selectedRange >= ranges.Length) _selectedRange = Math.Clamp(_selectedRange, 0, ranges.Length - 1);
                return ranges[_selectedRange].max;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = ClientRectangle;
            int w = rect.Width, h = rect.Height;
            int cx = w / 2;

            // --- Background ---
            using (var bg = new GraphicsPath())
            {
                int r = 16;
                bg.AddArc(rect.X, rect.Y, r, r, 180, 90);
                bg.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
                bg.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
                bg.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
                bg.CloseFigure();
                using (var b = new SolidBrush(Color.FromArgb(26, 26, 26)))
                    g.FillPath(b, bg);
                using (var p = new Pen(Color.FromArgb(50, 50, 50), 1))
                    g.DrawPath(p, bg);
            }

            // --- Range buttons row (AUTO + ranges) ---
            float btnY = 8;
            float btnH = 22;
            float btnX = 10;
            float gap = 4;
            using (var btnFont = new Font("Segoe UI", 8, FontStyle.Bold))
            {
                // AUTO button
                string autoTxt = "AUTO";
                var szAuto = g.MeasureString(autoTxt, btnFont);
                float autoBtnW = szAuto.Width + 12;
                var autoRect = new RectangleF(btnX, btnY, autoBtnW, btnH);
                bool autoHover = _rangeHover == -2;
                using (var b = new SolidBrush(_autoRange ? Color.FromArgb(180, 100, 20) : autoHover ? Color.FromArgb(50, 50, 50) : Color.FromArgb(35, 35, 35)))
                    g.FillRectangle(b, autoRect);
                using (var p = new Pen(_autoRange ? Color.FromArgb(240, 160, 40) : Color.FromArgb(60, 60, 60), 1))
                    g.DrawRectangle(p, autoRect.X, autoRect.Y, autoRect.Width, autoRect.Height);
                using (var b = new SolidBrush(_autoRange ? Color.White : Color.FromArgb(160, 160, 160)))
                    g.DrawString(autoTxt, btnFont, b, btnX + 6, btnY + 3);
                _autoBtnRect = autoRect;
                btnX += autoBtnW + gap;

                var curRanges = ActiveRanges;
                for (int i = 0; i < curRanges.Length; i++)
                {
                    string txt = curRanges[i].label;
                    var sz = g.MeasureString(txt, btnFont);
                    float btnW = sz.Width + 12;

                    if (btnX + btnW > w - 10)
                    {
                        btnX = 10;
                        btnY += btnH + gap;
                    }

                    var btnRect = new RectangleF(btnX, btnY, btnW, btnH);
                    bool hovered = _rangeHover == i;
                    bool selected = !_autoRange && _selectedRange == i;
                    bool highlighted = _autoRange && _highlightedRange == i;

                    Color bgCol = selected ? Color.FromArgb(60, 120, 180) :
                                 highlighted ? Color.FromArgb(50, 90, 50) :
                                 hovered ? Color.FromArgb(50, 50, 50) : Color.FromArgb(35, 35, 35);
                    Color borderCol = selected ? Color.FromArgb(100, 180, 240) :
                                     highlighted ? Color.FromArgb(80, 180, 80) : Color.FromArgb(60, 60, 60);
                    Color textCol = selected || highlighted ? Color.White : Color.FromArgb(160, 160, 160);

                    using (var b = new SolidBrush(bgCol))
                        g.FillRectangle(b, btnRect);
                    using (var p = new Pen(borderCol, 1))
                        g.DrawRectangle(p, btnRect.X, btnRect.Y, btnRect.Width, btnRect.Height);

                    using (var b = new SolidBrush(textCol))
                        g.DrawString(txt, btnFont, b, btnX + 6, btnY + 3);

                    _rangeButtons[i] = btnRect;
                    btnX += btnW + gap;
                }

                // HOLD + REL + CONT + DIOD indicators (right of ranges)
                string[] badges = { "HOLD", "REL" };
                bool[] badgeActive = { _isHold, _isRel };
                if (_isContinuity) { badges = new[] { "HOLD", "REL", "CONT" }; badgeActive = new[] { _isHold, _isRel, true }; }
                else if (_isDiode) { badges = new[] { "HOLD", "REL", "DIOD" }; badgeActive = new[] { _isHold, _isRel, true }; }
                float[] badgeWidths = new float[badges.Length];
                for (int b = 0; b < badges.Length; b++)
                    badgeWidths[b] = g.MeasureString(badges[b], btnFont).Width + 12;
                for (int b = 0; b < badges.Length; b++)
                {
                    if (btnX + badgeWidths[b] > w - 10)
                    { btnX = 10; btnY += btnH + gap; }
                    bool active = badgeActive[b];
                    bool textVisible = !active || DateTime.Now.Millisecond % 1000 < 500;
                    var badgeRect = new RectangleF(btnX, btnY, badgeWidths[b], btnH);
                    Color bg = active ? Color.FromArgb(180, 100, 20) : Color.FromArgb(35, 35, 35);
                    Color border = active ? Color.FromArgb(240, 160, 40) : Color.FromArgb(60, 60, 60);
                    using (var bBg = new SolidBrush(bg))
                        g.FillRectangle(bBg, badgeRect);
                    using (var p = new Pen(border, 1))
                        g.DrawRectangle(p, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height);
                    if (textVisible)
                    {
                        Color textCol = active ? Color.White : Color.FromArgb(160, 160, 160);
                        using (var bTxt = new SolidBrush(textCol))
                            g.DrawString(badges[b], btnFont, bTxt, btnX + 6, btnY + 3);
                    }
                    btnX += badgeWidths[b] + gap;
                }
            }

            float rangeButtonsBottom = btnY + btnH + 6;

            // --- Title ---
            using (var titleFont = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var b = new SolidBrush(Color.FromArgb(160, 160, 160)))
            {
                var sz = g.MeasureString(_label, titleFont);
                g.DrawString(_label, titleFont, b, cx - sz.Width / 2, rangeButtonsBottom);
            }

            // --- Gauge ---
            float gaugeMax = GaugeMax;
            int gaugeAreaTop = (int)rangeButtonsBottom + 20;
            int gaugeAreaHeight = Math.Max(10, h - gaugeAreaTop - 50);
            int gaugeRadius = Math.Max(20, Math.Min(w / 2 - 20, gaugeAreaHeight / 2 - 10));
            int gaugeCenterY = gaugeAreaTop + gaugeRadius + 10;

            float outerR = gaugeRadius;
            float innerR = gaugeRadius - 20;

            // Background arc
            if (outerR >= 20 && outerR < 5000)
            {
                try
                {
                    using (var pen = new Pen(Color.FromArgb(45, 45, 45), Math.Min(20, outerR / 3)))
                    {
                        var arcRect = new RectangleF(cx - outerR, gaugeCenterY - outerR, outerR * 2, outerR * 2);
                        g.DrawArc(pen, arcRect, ArcStart, ArcSpan);
                    }
                }
                catch { }
            }

            // Value arc
            float normalized = gaugeMax > 0 ? (_animValue / gaugeMax) : 0;
            float clamped = Math.Max(0, Math.Min(1, normalized));
            float valueAngle = clamped * ArcSpan;

            Color arcColor;
            if (clamped < 0.3f) arcColor = Color.FromArgb(0, 200, 0);
            else if (clamped < 0.7f) arcColor = Color.FromArgb(200, 200, 0);
            else arcColor = Color.FromArgb(220, 50, 0);

            if (valueAngle > 0.5f && outerR >= 20 && outerR < 5000 && w > 0 && h > 0)
            {
                try
                {
                    using (var pen = new Pen(arcColor, Math.Min(20, outerR / 3)))
                    {
                        var arcRect = new RectangleF(cx - outerR, gaugeCenterY - outerR, outerR * 2, outerR * 2);
                        g.DrawArc(pen, arcRect, ArcStart, valueAngle);
                    }
                }
                catch { }
            }

            // --- Major ticks with dynamic labels ---
            float tickOuterR = outerR - 2;
            float tickInnerR = outerR - 13;
            float labelR = outerR + 8;
            using (var tickPen = new Pen(Color.FromArgb(150, 150, 150), 2))
            using (var tickFont = new Font("Segoe UI", 8, FontStyle.Bold))
            using (var tickBrush = new SolidBrush(Color.FromArgb(170, 170, 170)))
            {
                int numMajor = 6;
                for (int i = 0; i < numMajor; i++)
                {
                    float v = gaugeMax * i / (numMajor - 1);
                    float frac = (v) / gaugeMax;
                    float angleDeg = ArcStart + frac * ArcSpan;
                    float angleRad = angleDeg * MathF.PI / 180f;

                    var outerPt = new PointF(cx + tickOuterR * MathF.Cos(angleRad), gaugeCenterY + tickOuterR * MathF.Sin(angleRad));
                    var innerPt = new PointF(cx + tickInnerR * MathF.Cos(angleRad), gaugeCenterY + tickInnerR * MathF.Sin(angleRad));
                    g.DrawLine(tickPen, outerPt, innerPt);

                    var labelPt = new PointF(cx + labelR * MathF.Cos(angleRad), gaugeCenterY + labelR * MathF.Sin(angleRad));
                    string txt;
                    if (gaugeMax < 10) txt = v.ToString("F1");
                    else if (gaugeMax < 100) txt = v.ToString("F0");
                    else if (gaugeMax < 500) txt = v.ToString("F0");
                    else txt = v.ToString("F0");
                    var sz = g.MeasureString(txt, tickFont);
                    g.DrawString(txt, tickFont, tickBrush, labelPt.X - sz.Width / 2, labelPt.Y - sz.Height / 2);
                }

                // Minor ticks
                using (var minorPen = new Pen(Color.FromArgb(90, 90, 90), 1))
                {
                    float minorStep = gaugeMax / 20;
                    if (minorStep < 0.01f) minorStep = 0.01f;
                    for (float v = minorStep; v <= gaugeMax - 0.001f; v += minorStep)
                    {
                        bool isMajor = false;
                        for (int i = 0; i < numMajor; i++)
                        {
                            float mv = gaugeMax * i / (numMajor - 1);
                            if (Math.Abs(v - mv) < minorStep * 0.5f) { isMajor = true; break; }
                        }
                        if (isMajor) continue;

                        float frac = v / gaugeMax;
                        float angleDeg = ArcStart + frac * ArcSpan;
                        float angleRad = angleDeg * MathF.PI / 180f;

                        var outerPt = new PointF(cx + (outerR - 2) * MathF.Cos(angleRad), gaugeCenterY + (outerR - 2) * MathF.Sin(angleRad));
                        var innerPt = new PointF(cx + (outerR - 7) * MathF.Cos(angleRad), gaugeCenterY + (outerR - 7) * MathF.Sin(angleRad));
                        g.DrawLine(minorPen, outerPt, innerPt);
                    }
                }
            }

            // --- Red needle ---
            {
                float needleAngle = ArcStart + clamped * ArcSpan;
                float angleRad = needleAngle * MathF.PI / 180f;
                float needleLen = outerR - 3;

                var tip = new PointF(cx + needleLen * MathF.Cos(angleRad), gaugeCenterY + needleLen * MathF.Sin(angleRad));
                var base1 = new PointF(cx - 5, gaugeCenterY);
                var base2 = new PointF(cx + 5, gaugeCenterY);

                using (var path = new GraphicsPath())
                {
                    path.AddPolygon(new[] { base1, tip, base2 });
                    using (var b = new SolidBrush(Color.FromArgb(230, 40, 40)))
                        g.FillPath(b, path);
                }

                g.FillEllipse(Brushes.White, cx - 5, gaugeCenterY - 5, 10, 10);
                using (var b = new SolidBrush(Color.FromArgb(230, 40, 40)))
                    g.FillEllipse(b, cx - 2, gaugeCenterY - 2, 4, 4);
            }

            // --- Unit below gauge center ---
            string gaugeUnit;
            if (_isResistance)
            {
                float abs = Math.Abs(_targetValue);
                if (abs >= 1000000f) gaugeUnit = "MΩ";
                else if (abs >= 1000f) gaugeUnit = "kΩ";
                else gaugeUnit = "Ω";
            }
            else if (_isCapacitance)
            {
                float abs = Math.Abs(_targetValue);
                if (abs >= 1000000f) gaugeUnit = "mF";
                else if (abs >= 1000f) gaugeUnit = "µF";
                else gaugeUnit = "nF";
            }
            else gaugeUnit = string.IsNullOrEmpty(_unit) ? "V" : _unit;

            using (var unitFont = new Font("Segoe UI", 13, FontStyle.Bold))
            using (var b = new SolidBrush(Color.FromArgb(110, 110, 110)))
            {
                var sz = g.MeasureString(gaugeUnit, unitFont);
                g.DrawString(gaugeUnit, unitFont, b, cx - sz.Width / 2, gaugeCenterY + 18);
            }

            // --- Large digital value ---
            var valueFont = _customValueFont ?? new Font("Segoe UI", _isOl ? 32 : 42, FontStyle.Bold);
            bool ownFont = _customValueFont == null;
            try
            {
                string valTxt;
                if (_displayValue != null)
                    valTxt = _displayValue;
                else if (_isVfc)
                    valTxt = _animValue.ToString("0000.0");
                else
                    valTxt = _animValue.ToString(_displayFormat);

                Color valColor;
                if (_continuityShorted)
                    valColor = DateTime.Now.Millisecond % 400 < 200 ? Color.LimeGreen : Color.White;
                else if (_isHold)
                    valColor = Color.Orange;
                else if (_isOl)
                    valColor = Color.FromArgb(180, 180, 180);
                else if (_isContinuity)
                    valColor = Color.Cyan;
                else if (_isDiode)
                    valColor = Color.Gold;
                else
                    valColor = _customValueColor;

                using (var valBrush = new SolidBrush(valColor))
                {
                    var sz = g.MeasureString(valTxt, valueFont);
                    g.DrawString(valTxt, valueFont, valBrush, cx - sz.Width / 2, gaugeCenterY + 36);
                }
            }
            finally
            {
                if (ownFont) valueFont.Dispose();
            }

            // --- Bottom info bar ---
            float barY = h - 50;
            float barH = 42;
            using (var barBrush = new SolidBrush(Color.FromArgb(38, 38, 38)))
                g.FillRectangle(barBrush, 0, barY, w, barH);
            using (var barBorder = new Pen(Color.FromArgb(55, 55, 55), 1))
                g.DrawLine(barBorder, 0, barY, w, barY);

            var elapsed = _timerRunning ? DateTime.Now - _startTime : TimeSpan.Zero;
            string timeStr = elapsed.TotalHours >= 1
                ? $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}"
                : $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

            using (var barFont = new Font("Consolas", 10, FontStyle.Bold))
            {
                // Voltage (green)
                string vTxt = _unit == "V" || _unit == "mV" ? $"V:{_targetValue:F4}{_unit}" : $"V:---";
                g.DrawString(vTxt, barFont, Brushes.LimeGreen, 10, barY + 4);

                // Current (yellow)
                string aTxt = _unit == "A" || _unit == "mA" ? $"A:{_targetValue:F4}{_unit.Replace("m","")}" : $"A:---";
                float xCur = w / 4f;
                g.DrawString(aTxt, barFont, Brushes.Gold, xCur, barY + 4);

                // Time (blue)
                g.DrawString($"T:{timeStr}", barFont, Brushes.DodgerBlue, w / 2f, barY + 4);

                // MAX (purple)
                string maxTxt = $"MAX:{_maxValue:F4}";
                g.DrawString(maxTxt, barFont, Brushes.MediumOrchid, w * 0.73f, barY + 4);
            }

            // --- Opacity slider ---
            int sliderY = h - 10;
            int sliderH = 8;
            int sliderW = 120;
            int sliderX = w - sliderW - 16;
            using (var trackBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
                g.FillRectangle(trackBrush, sliderX, sliderY, sliderW, sliderH);
            int thumbX = sliderX + (int)(sliderW * _opacity / 100f);
            int thumbHalf = 5;
            using (var thumbBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                g.FillRectangle(thumbBrush, thumbX - thumbHalf, sliderY - 3, thumbHalf * 2, sliderH + 6);

            // Opacity % text
            using (var opFont = new Font("Segoe UI", 9))
            using (var opBrush = new SolidBrush(Color.FromArgb(140, 140, 140)))
            {
                string opTxt = _opacity + "%";
                var sz = g.MeasureString(opTxt, opFont);
                g.DrawString(opTxt, opFont, opBrush, sliderX + sliderW / 2 - sz.Width / 2, sliderY - 16);
            }

            // --- Volume slider ---
            int volSliderW = 120;
            int volSliderX = sliderX - volSliderW - 20;
            int volSliderY = h - 10;
            int volSliderH = 8;
            using (var volTrackBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
                g.FillRectangle(volTrackBrush, volSliderX, volSliderY, volSliderW, volSliderH);
            int volThumbX = volSliderX + (int)(volSliderW * _volume / 100f);
            using (var volThumbBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                g.FillRectangle(volThumbBrush, volThumbX - thumbHalf, volSliderY - 3, thumbHalf * 2, volSliderH + 6);

            // Volume label + % text
            using (var volFont = new Font("Segoe UI", 9))
            using (var volBrush = new SolidBrush(Color.FromArgb(140, 140, 140)))
            {
                g.DrawString("VOL", volFont, volBrush, volSliderX + volSliderW / 2 - 12, volSliderY - 16);
            }
        }

        private readonly RectangleF[] _rangeButtons = new RectangleF[30];
        private RectangleF _autoBtnRect;
        private int _rangeHover = -1;

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            // Check AUTO button
            if (_autoBtnRect.Contains(e.Location))
            {
                _autoRange = !_autoRange;
                if (_autoRange) AutoSelectRange();
                Invalidate();
                return;
            }

            // Check range button clicks
            for (int i = 0; i < _rangeButtons.Length; i++)
            {
                if (_rangeButtons[i] != RectangleF.Empty && _rangeButtons[i].Contains(e.Location))
                {
                    _autoRange = false;
                    _selectedRange = i;
                    Properties.Settings.Default.panel_gauge_range = i;
                    Properties.Settings.Default.Save();
                    Invalidate();
                    return;
                }
            }

            // Check opacity slider click
            int sliderW = 120;
            int sliderX = Width - sliderW - 16;
            int sliderY = Height - 10;
            if (e.Y >= sliderY - 8 && e.Y <= sliderY + 16 &&
                e.X >= sliderX - 4 && e.X <= sliderX + sliderW + 4)
            {
                _opacity = (int)((e.X - sliderX) * 100f / sliderW);
                if (_opacity < 10) _opacity = 10;
                if (_opacity > 100) _opacity = 100;
                Opacity = _opacity / 100.0;
                Properties.Settings.Default.panel_gauge_opacity = _opacity;
                Properties.Settings.Default.Save();
                Invalidate();
            }

            // Check volume slider click
            int volSliderW = 120;
            int volSliderX = sliderX - volSliderW - 20;
            int volSliderY = Height - 10;
            if (e.Y >= volSliderY - 8 && e.Y <= volSliderY + 16 &&
                e.X >= volSliderX - 4 && e.X <= volSliderX + volSliderW + 4)
            {
                _volume = (int)((e.X - volSliderX) * 100f / volSliderW);
                if (_volume < 0) _volume = 0;
                if (_volume > 100) _volume = 100;
                Properties.Settings.Default.panel_gauge_volume = _volume;
                Properties.Settings.Default.Save();
                ApplyVolumeToBuzzer();
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_mouseDown)
            {
                var cur = Control.MousePosition;
                Location = new Point(
                    _formDownPos.X + cur.X - _mouseDownPos.X,
                    _formDownPos.Y + cur.Y - _mouseDownPos.Y);
                return;
            }

            // Track hover for AUTO + range buttons
            int prevHover = _rangeHover;
            _rangeHover = -1;
            if (_autoBtnRect.Contains(e.Location))
                _rangeHover = -2;
            else for (int i = 0; i < _rangeButtons.Length; i++)
                if (_rangeButtons[i] != RectangleF.Empty && _rangeButtons[i].Contains(e.Location))
                { _rangeHover = i; break; }
            if (prevHover != _rangeHover)
                Invalidate();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mouseDown = true;
                _mouseDownPos = Control.MousePosition;
                _formDownPos = Location;
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            _smoothTimer.Stop();
            _elapsedTimer.Stop();
            Properties.Settings.Default.panel_gauge_pos_x = Location.X;
            Properties.Settings.Default.panel_gauge_pos_y = Location.Y;
            Properties.Settings.Default.panel_gauge_opacity = _opacity;
            Properties.Settings.Default.panel_gauge_volume = _volume;
            Properties.Settings.Default.panel_gauge_range = _selectedRange;
            Properties.Settings.Default.Save();
        }

        private void ApplyVolumeToBuzzer()
        {
            var mainWnd = Application.OpenForms.OfType<MainWindow>().FirstOrDefault();
            mainWnd?.SetAlarmVolume(_volume / 100f);
        }
    }
}
