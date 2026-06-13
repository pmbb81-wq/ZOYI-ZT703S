using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZOYI
{
    public class RidenChartForm : Form
    {
        private readonly Riden6012 _riden;
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer { Interval = 20 };
        private readonly ChartWidget _chart;

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

            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 28,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            Controls.Add(topBar);

            int x = 8;
            topBar.Controls.Add(MakeToggle("V", Color.FromArgb(0, 255, 85), ref x, "v", true));
            topBar.Controls.Add(MakeToggle("A", Color.FromArgb(255, 204, 0), ref x, "i", false));
            topBar.Controls.Add(MakeToggle("W", Color.FromArgb(136, 136, 136), ref x, "p", false));

            _chart = new ChartWidget
            {
                Dock = DockStyle.Fill,
                ShowV = true,
                ShowI = false,
                ShowP = false
            };
            Controls.Add(_chart);

            _timer.Tick += (_, _) =>
            {
                double now = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
                _chart.AddSample(now, _riden.Vout, _riden.Iout, _riden.Power);
            };
            _timer.Start();
        }

        private CheckBox MakeToggle(string label, Color color, ref int x, string trace, bool chk)
        {
            var cb = new CheckBox
            {
                Text = label,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 10, FontStyle.Bold),
                ForeColor = color,
                Checked = chk,
                Size = new Size(32, 24),
                Location = new Point(x, 2),
                Cursor = Cursors.Hand
            };
            x += 38;
            cb.CheckedChanged += (_, _) => _chart.SetToggle(trace, cb.Checked);
            return cb;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            base.OnFormClosed(e);
        }
    }
}
