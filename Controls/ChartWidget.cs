using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ZOYI
{
    public class Sample
    {
        public double Time { get; set; }
        public double V { get; set; }
        public double I { get; set; }
        public double P { get; set; }
    }

    public class ChartWidget : UserControl
    {
        private const int ChartSeconds = 60;
        private const int MaxSamples = 3000;
        private readonly LinkedList<Sample> _data = new LinkedList<Sample>();
        private int? _hoverX;

        public bool ShowV { get; set; } = true;
        public bool ShowI { get; set; } = true;
        public bool ShowP { get; set; } = true;

        public ChartWidget()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(23, 23, 23);
            MinimumSize = new Size(0, 160);
            Cursor = Cursors.Cross;

            MouseMove += (_, e) => { _hoverX = e.X; Invalidate(); };
            MouseLeave += (_, _) => { _hoverX = null; Invalidate(); };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Zapisz jako PNG", null, (_, _) => SavePng());
            ContextMenuStrip = menu;
        }

        private void SavePng()
        {
            using var sfd = new SaveFileDialog { Filter = "PNG (*.png)|*.png", FileName = "wykres_riden.png" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using var bmp = new Bitmap(Width, Height);
                DrawToBitmap(bmp, new Rectangle(0, 0, Width, Height));
                bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        public void AddSample(double t, double v, double i, double p)
        {
            _data.AddLast(new Sample { Time = t, V = v, I = i, P = p });
            while (_data.Count > MaxSamples)
                _data.RemoveFirst();
            Invalidate();
        }

        public void ClearData()
        {
            _data.Clear();
            Invalidate();
        }

        public void SetToggle(string name, bool state)
        {
            if (name == "v") ShowV = state;
            else if (name == "i") ShowI = state;
            else if (name == "p") ShowP = state;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = Width, h = Height;
            int lMargin = 10, rMargin = 10, tMargin = 55, bMargin = 40;
            int pw = w - lMargin - rMargin;
            int ph = h - tMargin - bMargin;
            int plotLeft = lMargin, plotRight = w - rMargin;

            // ---- Grid: 5 horizontal + vertical time lines ----
            using (var gridPen = new Pen(Color.FromArgb(55, 55, 55)))
            using (var timePen = new Pen(Color.FromArgb(40, 40, 40)))
            {
                // Horizontal
                for (int i = 0; i < 5; i++)
                {
                    float y = tMargin + ph * i / 4f;
                    g.DrawLine(gridPen, plotLeft, y, plotRight, y);
                }

                // Vertical time lines every 10s (0=now, 10,20,30,40,50,60)
                if (_data.Count >= 2)
                {
                    double now = _data.Last.Value.Time;
                    for (int sec = 0; sec <= 60; sec += 10)
                    {
                        double t = now - sec;
                        float x = plotLeft + pw * (float)(1.0 - sec / (double)ChartSeconds);
                        if (x >= plotLeft && x <= plotRight)
                            g.DrawLine(timePen, x, tMargin, x, tMargin + ph);
                    }
                }
            }

            // ---- Time labels at bottom ----
            using (var timeFont = new Font("Consolas", 9))
            {
                double now = _data.Count >= 2 ? _data.Last.Value.Time : 0;
                for (int sec = 0; sec <= 60; sec += 10)
                {
                    double t = now - sec;
                    float x = plotLeft + pw * (float)(1.0 - sec / (double)ChartSeconds);
                    if (x < plotLeft || x > plotRight) continue;
                    string lbl = sec == 0 ? "teraz" : $"-{sec}s";
                    var sz = g.MeasureString(lbl, timeFont);
                    g.DrawString(lbl, timeFont, Brushes.Gray, x - sz.Width / 2, h - 18);
                }
            }

            if (_data.Count < 2) return;

            double nowTime = _data.Last.Value.Time;
            double t0 = nowTime - ChartSeconds;

            var visible = _data.Where(s => s.Time >= t0).ToList();
            if (visible.Count < 2) return;

            float ToX(double t) => plotLeft + pw * (float)(1.0 - (nowTime - t) / ChartSeconds);
            float ToY(double val, double lo, double hi)
            {
                if (hi - lo < 1e-9) return tMargin + ph / 2f;
                return (float)(tMargin + ph * (1.0 - (val - lo) / (hi - lo)));
            }

            void DrawTrace(List<(double t, double val)> pts, Color color, double lo, double hi)
            {
                if (pts.Count < 2) return;
                using var pen = new Pen(color, 1.5f);
                using var path = new GraphicsPath();
                bool first = true;
                foreach (var pt in pts)
                {
                    float x = ToX(pt.t);
                    float y = ToY(pt.val, lo, hi);
                    if (first)
                    {
                        path.StartFigure();
                        path.AddLine(x, y, x, y);
                        first = false;
                    }
                    else
                    {
                        path.AddLine(path.GetLastPoint().X, path.GetLastPoint().Y, x, y);
                    }
                }
                g.DrawPath(pen, path);
            }

            void DrawYScale(Color color, double lo, double hi, string fmt, int nTicks = 4)
            {
                using var tickPen = new Pen(color);
                using var brush = new SolidBrush(color);
                using var font = new Font("Consolas", 8);
                for (int i = 0; i < nTicks; i++)
                {
                    double frac = nTicks > 1 ? (double)i / (nTicks - 1) : 0.5;
                    double val = lo + (hi - lo) * frac;
                    float y = ToY(val, lo, hi);
                    g.DrawLine(tickPen, plotLeft, y, plotLeft + 5, y);
                    string txt = val.ToString(fmt);
                    var rect = new RectangleF(0, y - 7, plotLeft - 2, 14);
                    using var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                    g.DrawString(txt, font, brush, rect, sf);
                    using var dashPen = new Pen(Color.FromArgb(40, color)) { DashStyle = DashStyle.Dash };
                    g.DrawLine(dashPen, plotLeft + 6, y, plotRight, y);
                }
            }

            if (ShowV)
            {
                var vs = visible.Select(s => s.V).ToList();
                double vMin = vs.Min(), vMax = vs.Max();
                if (vMax - vMin < 0.001) { vMin -= 0.1; vMax += 0.1; }
                var vPts = visible.Select(s => (s.Time, s.V)).ToList();
                DrawYScale(Color.FromArgb(0, 255, 85), vMin, vMax, "F2");
                DrawTrace(vPts, Color.FromArgb(0, 255, 85), vMin, vMax);
            }

            if (ShowI)
            {
                var iVals = visible.Select(s => s.I).ToList();
                double iMin = iVals.Min(), iMax = iVals.Max();
                if (iMax - iMin < 0.001) { iMin -= 0.01; iMax += 0.01; }
                var iPts = visible.Select(s => (s.Time, s.I)).ToList();
                DrawYScale(Color.FromArgb(255, 204, 0), iMin, iMax, "F3");
                DrawTrace(iPts, Color.FromArgb(255, 204, 0), iMin, iMax);
            }

            if (ShowP)
            {
                var ps = visible.Select(s => s.P).ToList();
                double pMin = ps.Min(), pMax = ps.Max();
                if (pMax - pMin < 0.001) { pMin -= 0.01; pMax += 0.01; }
                var pPts = visible.Select(s => (s.Time, s.P)).ToList();
                DrawYScale(Color.FromArgb(255, 136, 0), pMin, pMax, "F3");
                DrawTrace(pPts, Color.FromArgb(255, 136, 0), pMin, pMax);
            }

            // ---- Cursor / hover ----
            if (_hoverX.HasValue && _hoverX >= plotLeft && _hoverX <= plotRight)
            {
                double tAtCursor = nowTime - ChartSeconds * (1.0 - (_hoverX.Value - plotLeft) / (double)pw);
                var closest = visible.OrderBy(s => Math.Abs(s.Time - tAtCursor)).FirstOrDefault();
                if (closest != null)
                {
                    float cx = ToX(closest.Time);
                    using var cursorPen = new Pen(Color.FromArgb(140, 140, 140)) { DashStyle = DashStyle.Dot };
                    g.DrawLine(cursorPen, cx, tMargin, cx, tMargin + ph);

                    var lines = new List<string>();
                    if (ShowV) lines.Add($"V: {closest.V:F3}V");
                    if (ShowI) lines.Add($"I: {closest.I:F3}A");
                    if (ShowP) lines.Add($"P: {closest.P:F3}W");
                    string text = string.Join("\n", lines);

                    using var tipFont = new Font("Consolas", 11, FontStyle.Bold);
                    using var bgBrush = new SolidBrush(Color.FromArgb(200, 30, 30, 30));
                    var sz = g.MeasureString(text, tipFont);
                    float tipX = cx + 12 < plotRight ? cx + 12 : cx - sz.Width - 12;
                    float tipY = tMargin + 4;
                    var bgRect = new RectangleF(tipX - 4, tipY - 2, sz.Width + 8, sz.Height + 4);
                    g.FillRectangle(bgBrush, bgRect);
                    g.DrawString(text, tipFont, Brushes.White, tipX, tipY);
                }
            }
        }
    }
}
