using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZOYI
{
    class ChartCanvas : Panel
    {
        public ChartCanvas() { DoubleBuffered = true; }
        protected override void OnResize(EventArgs e) { base.OnResize(e); Refresh(); }
    }

    public class RidenChartForm : Form
    {
        private readonly Riden6012 _riden;
        private readonly System.Windows.Forms.Timer _sampleTimer = new System.Windows.Forms.Timer { Interval = 50 };
        private readonly System.Windows.Forms.Timer _refreshTimer = new System.Windows.Forms.Timer { Interval = 50 };
        private readonly LinkedList<(double t, float v, float i, float p)> _data = new();
        private const int MaxSamples = 2000;
        private const double WindowSeconds = 60;
        private readonly ChartCanvas _canvas;
        private bool _showV = true, _showI = true, _showP = true;

        public RidenChartForm(Riden6012 riden)
        {
            _riden = riden;
            Text = "RIDEN — wykres";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            ShowInTaskbar = true;
            Size = new Size(700, 430);
            BackColor = Color.FromArgb(30, 30, 30);
            ResizeRedraw = true;

            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 28,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            Controls.Add(topBar);

            int x = 8;
            topBar.Controls.Add(MakeToggle("V", Color.FromArgb(0, 255, 85), ref x, v => { _showV = v; _canvas.Invalidate(); }));
            topBar.Controls.Add(MakeToggle("A", Color.FromArgb(255, 204, 0), ref x, i => { _showI = i; _canvas.Invalidate(); }));
            topBar.Controls.Add(MakeToggle("W", Color.FromArgb(255, 136, 0), ref x, p => { _showP = p; _canvas.Invalidate(); }));

            _canvas = new ChartCanvas
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30)
            };
            _canvas.Paint += Canvas_Paint;
            _canvas.ContextMenuStrip = MakeContextMenu();
            Controls.Add(_canvas);

            _sampleTimer.Tick += (_, _) => Sample();
            _sampleTimer.Start();

            _refreshTimer.Tick += (_, _) => _canvas.Invalidate();
            _refreshTimer.Start();
        }

        private ContextMenuStrip MakeContextMenu()
        {
            var m = new ContextMenuStrip();
            m.Items.Add("Zapisz jako PNG", null, (_, _) => ZapiszJakoPNG());
            return m;
        }

        private void ZapiszJakoPNG()
        {
            using var bmp = new Bitmap(_canvas.Width, _canvas.Height);
            _canvas.DrawToBitmap(bmp, new Rectangle(0, 0, _canvas.Width, _canvas.Height));
            var dlg = new SaveFileDialog { Filter = "PNG (*.png)|*.png", FileName = "wykres_riden.png" };
            if (dlg.ShowDialog() == DialogResult.OK)
                bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
        }

        private CheckBox MakeToggle(string label, Color color, ref int x, Action<bool> onToggle)
        {
            var cb = new CheckBox
            {
                Text = label,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10, FontStyle.Bold),
                ForeColor = color,
                Checked = true,
                Size = new Size(32, 24),
                Location = new Point(x, 2),
                Cursor = Cursors.Hand
            };
            x += 38;
            cb.CheckedChanged += (_, _) => onToggle(cb.Checked);
            return cb;
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
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            if (_data.Count < 2) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = _canvas.ClientSize.Width;
            int h = _canvas.ClientSize.Height;
            int lMargin = 55, rMargin = 10, tMargin = 10, bMargin = 22;
            int pw = w - lMargin - rMargin;
            int ph = h - tMargin - bMargin;
            int plotLeft = lMargin;
            int plotRight = w - rMargin;

            double now = _data.Last.Value.t;
            double t0 = now - WindowSeconds;

            var visible = new List<(double t, float v, float i, float p)>();
            foreach (var d in _data)
                if (d.t >= t0) visible.Add(d);

            if (visible.Count < 2) return;

            float ToX(double t) => plotLeft + pw * (float)(1.0 - (now - t) / WindowSeconds);
            float ToY(float val, float lo, float hi)
            {
                if (hi - lo < 1e-9f) return tMargin + ph / 2f;
                return tMargin + ph * (1f - (val - lo) / (hi - lo));
            }

            using var gridPen = new Pen(Color.FromArgb(42, 42, 42));
            for (int i = 0; i < 5; i++)
            {
                float y = tMargin + ph * i / 4f;
                g.DrawLine(gridPen, plotLeft, y, plotRight, y);
            }

            using var timeFont = new Font("Consolas", 8);
            for (int i = 0; i < 5; i++)
            {
                double sec = WindowSeconds * i / 4;
                float x = ToX(now - sec);
                string lbl = sec > 0 ? $"-{sec:F0}s" : "teraz";
                using var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(lbl, timeFont, Brushes.Gray, x, h - bMargin + 4, sf);
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

            void DrawYScale(Color color, float lo, float hi, string fmt, int nTicks)
            {
                using var tickPen = new Pen(color);
                using var labelBrush = new SolidBrush(color);
                using var font = new Font("Consolas", 8);
                for (int i = 0; i < nTicks; i++)
                {
                    float frac = i / (float)(nTicks - 1);
                    float val = lo + (hi - lo) * frac;
                    float y = ToY(val, lo, hi);
                    g.DrawLine(tickPen, plotLeft, y, plotLeft + 5, y);
                    string txt = val.ToString(fmt);
                    using var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                    g.DrawString(txt, font, labelBrush, plotLeft - 4, y, sf);
                    using var dashPen = new Pen(Color.FromArgb(40, color)) { DashStyle = DashStyle.Dash };
                    g.DrawLine(dashPen, plotLeft + 6, y, plotRight, y);
                }
            }

            // Vout (green)
            if (_showV)
            {
                var vs = new List<float>();
                foreach (var d in visible) vs.Add(d.v);
                float vLo = float.MaxValue, vHi = float.MinValue;
                foreach (var val in vs) { if (val < vLo) vLo = val; if (val > vHi) vHi = val; }
                if (vHi - vLo < 0.001f) { vLo -= 0.1f; vHi += 0.1f; }
                var vPts = new List<(double, float)>();
                foreach (var d in visible) vPts.Add((d.t, d.v));
                DrawYScale(Color.FromArgb(0, 255, 85), vLo, vHi, "F2", 5);
                DrawTrace(vPts, Color.FromArgb(0, 255, 85), vLo, vHi);
            }

            // Iset (yellow)
            if (_showI)
            {
                var iVals = new List<float>();
                foreach (var d in visible) iVals.Add(d.i);
                float iLo = float.MaxValue, iHi = float.MinValue;
                foreach (var val in iVals) { if (val < iLo) iLo = val; if (val > iHi) iHi = val; }
                if (iHi - iLo < 0.001f) { iLo -= 0.01f; iHi += 0.01f; }
                var iPts = new List<(double, float)>();
                foreach (var d in visible) iPts.Add((d.t, d.i));
                DrawYScale(Color.FromArgb(255, 204, 0), iLo, iHi, "F3", 5);
                DrawTrace(iPts, Color.FromArgb(255, 204, 0), iLo, iHi);
            }

            // Power (orange)
            if (_showP)
            {
                var pVals = new List<float>();
                foreach (var d in visible) pVals.Add(d.p);
                float pLo = float.MaxValue, pHi = float.MinValue;
                foreach (var val in pVals) { if (val < pLo) pLo = val; if (val > pHi) pHi = val; }
                if (pHi - pLo < 0.001f) { pLo -= 0.01f; pHi += 0.01f; }
                var pPts = new List<(double, float)>();
                foreach (var d in visible) pPts.Add((d.t, d.p));
                DrawYScale(Color.FromArgb(255, 136, 0), pLo, pHi, "F3", 5);
                DrawTrace(pPts, Color.FromArgb(255, 136, 0), pLo, pHi);
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
