using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZOYI
{
    public class RidenChartForm : Form
    {
        private readonly Riden6012 _riden;
        private readonly System.Windows.Forms.Timer _sampleTimer = new System.Windows.Forms.Timer { Interval = 50 };
        private readonly System.Windows.Forms.Timer _refreshTimer = new System.Windows.Forms.Timer { Interval = 50 };
        private readonly LinkedList<(double t, float v, float i, float p)> _data = new();
        private const int MaxSamples = 600;
        private const double WindowSeconds = 60;
        private int _samples;
        private float _vMin = 99f, _vMax;
        private float _iMin = 99f, _iMax;
        private float _pMin = 99f, _pMax;

        public RidenChartForm(Riden6012 riden)
        {
            _riden = riden;
            Text = "RIDEN — wykres";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            ShowInTaskbar = true;
            Size = new Size(700, 400);
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.FromArgb(200, 200, 200);
            DoubleBuffered = true;

            _sampleTimer.Tick += (_, _) => Sample();
            _sampleTimer.Start();

            _refreshTimer.Tick += (_, _) => Invalidate();
            _refreshTimer.Start();
        }

        private void Sample()
        {
            float v = _riden.Vout;
            float i = _riden.Iset;
            float p = _riden.Power;
            double now = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

            _data.AddLast((now, v, i, p));
            while (_data.Count > MaxSamples)
                _data.RemoveFirst();

            _samples++;
            if (v < _vMin) _vMin = v;
            if (v > _vMax) _vMax = v;
            if (i < _iMin) _iMin = i;
            if (i > _iMax) _iMax = i;
            if (p < _pMin) _pMin = p;
            if (p > _pMax) _pMax = p;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_data.Count < 2) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int lMargin = 55, rMargin = 10, tMargin = 10, bMargin = 22;
            int pw = ClientSize.Width - lMargin - rMargin;
            int ph = ClientSize.Height - tMargin - bMargin;
            int plotLeft = lMargin;
            int plotRight = ClientSize.Width - rMargin;

            double now = _data.Last.Value.t;
            double t0 = now - WindowSeconds;

            // build visible list
            var visible = new List<(double t, float v, float i, float p)>();
            foreach (var d in _data)
                if (d.t >= t0) visible.Add(d);

            if (visible.Count < 2) return;

            // helpers
            float ToX(double t) => plotLeft + pw * (float)(1.0 - (now - t) / WindowSeconds);
            float ToY(float val, float lo, float hi)
            {
                if (hi - lo < 1e-9f) return tMargin + ph / 2f;
                return tMargin + ph * (1f - (val - lo) / (hi - lo));
            }

            // grid
            using var gridPen = new Pen(Color.FromArgb(42, 42, 42));
            for (int i = 0; i < 5; i++)
            {
                float y = tMargin + ph * i / 4f;
                g.DrawLine(gridPen, plotLeft, y, plotRight, y);
            }

            // time labels
            using var timeFont = new Font("Consolas", 8);
            for (int i = 0; i < 5; i++)
            {
                double sec = WindowSeconds * i / 4;
                float x = ToX(now - sec);
                string lbl = sec > 0 ? $"-{sec:F0}s" : "teraz";
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(lbl, timeFont, Brushes.Gray, x, ClientSize.Height - bMargin + 4, sf);
            }

            void DrawTrace(List<(double t, float val)> pts, Color color, float lo, float hi)
            {
                if (pts.Count < 2) return;
                using var pen = new Pen(color, 1.5f);
                var points = new PointF[pts.Count];
                for (int i = 0; i < pts.Count; i++)
                    points[i] = new PointF(ToX(pts[i].t), ToY(pts[i].val, lo, hi));
                g.DrawLines(pen, points);
            }

            // Y scale helper
            void DrawYScale(Color color, float lo, float hi, string fmt)
            {
                using var tickPen = new Pen(color);
                using var labelBrush = new SolidBrush(color);
                using var font = new Font("Consolas", 8);
                int nTicks = 4;
                for (int i = 0; i < nTicks; i++)
                {
                    float frac = i / (float)(nTicks - 1);
                    float val = lo + (hi - lo) * frac;
                    float y = ToY(val, lo, hi);
                    g.DrawLine(tickPen, plotLeft, y, plotLeft + 5, y);
                    string txt = val.ToString(fmt);
                    var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                    g.DrawString(txt, font, labelBrush, plotLeft - 4, y, sf);
                    // dashed guide
                    using var dashPen = new Pen(Color.FromArgb(40, color)) { DashStyle = DashStyle.Dash };
                    g.DrawLine(dashPen, plotLeft + 6, y, plotRight, y);
                }
            }

            // draw traces
            if (visible.Count >= 2)
            {
                // Vout (green)
                {
                    var pts = new List<(double, float)>();
                    foreach (var d in visible) pts.Add((d.t, d.v));
                    float lo = _vMin, hi = _vMax;
                    if (hi - lo < 0.001f) { lo -= 0.1f; hi += 0.1f; }
                    DrawYScale(Color.FromArgb(0, 255, 85), lo, hi, "F2");
                    DrawTrace(pts, Color.FromArgb(0, 255, 85), lo, hi);
                }

                // Iset (yellow)
                {
                    var pts = new List<(double, float)>();
                    foreach (var d in visible) pts.Add((d.t, d.i));
                    float lo = _iMin, hi = _iMax;
                    if (hi - lo < 0.001f) { lo -= 0.01f; hi += 0.01f; }
                    DrawYScale(Color.FromArgb(255, 204, 0), lo, hi, "F3");
                    DrawTrace(pts, Color.FromArgb(255, 204, 0), lo, hi);
                }

                // Power (orange)
                {
                    var pts = new List<(double, float)>();
                    foreach (var d in visible) pts.Add((d.t, d.p));
                    float lo = _pMin, hi = _pMax;
                    if (hi - lo < 0.001f) { lo -= 0.01f; hi += 0.01f; }
                    DrawYScale(Color.FromArgb(255, 136, 0), lo, hi, "F3");
                    DrawTrace(pts, Color.FromArgb(255, 136, 0), lo, hi);
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _sampleTimer.Stop();
            _refreshTimer.Stop();
            base.OnFormClosed(e);
        }
    }
}
