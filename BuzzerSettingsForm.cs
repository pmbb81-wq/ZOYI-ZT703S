namespace ZOYI
{
    public class BuzzerSettingsForm : Form
    {
        private CheckBox chbAlarmEnabled;
        private NumericUpDown nudAlarmBelow;
        private NumericUpDown nudAlarmAbove;
        private NumericUpDown nudAlarmFreq;
        private CheckBox chbDiodeEnabled;
        private NumericUpDown nudDiodeFreq;
        private NumericUpDown nudDiodeInterval;
        private NumericUpDown nudContinuityThreshold;
        private NumericUpDown nudDiodeShortThreshold;

        public BuzzerSettingsForm()
        {
            Text = "Ustawienia brzęczyka / alarmu";
            Size = new Size(420, 480);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            TopMost = true;
            ShowInTaskbar = false;
            Font = new Font("Segoe UI", 10);

            int y = 15;
            int labelW = 200;
            int inputX = 220;
            int rowH = 32;

            var lblAlarm = new Label { Text = "ALARM (próg值owy)", Location = new Point(12, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Cyan };
            y += 28;

            chbAlarmEnabled = new CheckBox { Text = "Włącz alarm", Location = new Point(12, y), Checked = Properties.Settings.Default.buzzer_alarm_enabled, ForeColor = Color.White };
            y += rowH;

            AddLabel("Poniżej:", 12, y);
            nudAlarmBelow = AddNumeric(inputX, y, 0, 10000, 2, (decimal)Properties.Settings.Default.buzzer_alarm_below);
            y += rowH;

            AddLabel("Powyżej:", 12, y);
            nudAlarmAbove = AddNumeric(inputX, y, 0, 10000, 2, (decimal)Properties.Settings.Default.buzzer_alarm_above);
            y += rowH;

            AddLabel("Częstotliwość (Hz):", 12, y);
            nudAlarmFreq = AddIntNumeric(inputX, y, 100, 15000, Properties.Settings.Default.buzzer_alarm_freq);
            y += rowH + 10;

            var lblDiode = new Label { Text = "DIODA / CIĄGŁOŚĆ", Location = new Point(12, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Gold };
            y += 28;

            chbDiodeEnabled = new CheckBox { Text = "Włącz brzęczyk diody/ciągłości", Location = new Point(12, y), Checked = Properties.Settings.Default.buzzer_diode_enabled, ForeColor = Color.White };
            y += rowH;

            AddLabel("Częstotliwość (Hz):", 12, y);
            nudDiodeFreq = AddIntNumeric(inputX, y, 100, 15000, Properties.Settings.Default.buzzer_diode_freq);
            y += rowH;

            AddLabel("Interwał powtarzania (ms):", 12, y);
            nudDiodeInterval = AddIntNumeric(inputX, y, 100, 5000, Properties.Settings.Default.buzzer_diode_interval);
            y += rowH;

            AddLabel("Próg ciągłości (Ω):", 12, y);
            nudContinuityThreshold = AddIntNumeric(inputX, y, 1, 1000, Properties.Settings.Default.buzzer_continuity_threshold);
            y += rowH;

            AddLabel("Próg zwarcia diody (mV):", 12, y);
            nudDiodeShortThreshold = AddIntNumeric(inputX, y, 0, 1000, Properties.Settings.Default.buzzer_diode_short_threshold);
            y += rowH + 15;

            var btnSave = new Button
            {
                Text = "Zapisz",
                Location = new Point(12, y),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Anuluj",
                Location = new Point(150, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };

            Controls.AddRange(new Control[] {
                lblAlarm, chbAlarmEnabled, nudAlarmBelow, nudAlarmAbove, nudAlarmFreq,
                lblDiode, chbDiodeEnabled, nudDiodeFreq, nudDiodeInterval,
                nudContinuityThreshold, nudDiodeShortThreshold,
                btnSave, btnCancel
            });
            Controls.Add(chbAlarmEnabled);
        }

        private Label AddLabel(string text, int x, int y)
        {
            var lbl = new Label { Text = text, Location = new Point(x, y), AutoSize = true, ForeColor = Color.LightGray };
            Controls.Add(lbl);
            return lbl;
        }

        private NumericUpDown AddNumeric(int x, int y, decimal min, decimal max, decimal decimals, decimal value)
        {
            var nud = new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(120, 27),
                Minimum = min,
                Maximum = max,
                DecimalPlaces = (int)decimals,
                Increment = decimals > 0 ? 0.01m : 1,
                Value = value,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            Controls.Add(nud);
            return nud;
        }

        private NumericUpDown AddIntNumeric(int x, int y, int min, int max, int value)
        {
            var nud = new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(120, 27),
                Minimum = min,
                Maximum = max,
                Value = value,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            Controls.Add(nud);
            return nud;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            Properties.Settings.Default.buzzer_alarm_enabled = chbAlarmEnabled.Checked;
            Properties.Settings.Default.buzzer_alarm_below = (double)nudAlarmBelow.Value;
            Properties.Settings.Default.buzzer_alarm_above = (double)nudAlarmAbove.Value;
            Properties.Settings.Default.buzzer_alarm_freq = (int)nudAlarmFreq.Value;
            Properties.Settings.Default.buzzer_diode_enabled = chbDiodeEnabled.Checked;
            Properties.Settings.Default.buzzer_diode_freq = (int)nudDiodeFreq.Value;
            Properties.Settings.Default.buzzer_diode_interval = (int)nudDiodeInterval.Value;
            Properties.Settings.Default.buzzer_continuity_threshold = (int)nudContinuityThreshold.Value;
            Properties.Settings.Default.buzzer_diode_short_threshold = (int)nudDiodeShortThreshold.Value;
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
