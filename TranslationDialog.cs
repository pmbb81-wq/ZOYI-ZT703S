namespace ZOYI
{
    public class TranslationDialog : Form
    {
        private Label lblOriginal;
        private TextBox txtTranslation;
        private Button btnSave;
        private Button btnCancel;
        private Button btnRemove;

        public string TranslationResult { get; private set; } = string.Empty;
        public bool RemoveRequested { get; private set; }

        public TranslationDialog(string originalText, string? currentTranslation = null)
        {
            Text = "Tłumaczenie";
            Size = new Size(420, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10);
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { btnSave.PerformClick(); e.SuppressKeyPress = true; }
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
            };

            lblOriginal = new Label
            {
                Text = $"Oryginał: {originalText}",
                Location = new Point(12, 15),
                Size = new Size(380, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Cyan,
                AutoEllipsis = true
            };

            var lblNew = new Label
            {
                Text = "Własna nazwa:",
                Location = new Point(12, 50),
                Size = new Size(380, 22),
                ForeColor = Color.LightGray
            };

            txtTranslation = new TextBox
            {
                Location = new Point(12, 78),
                Size = new Size(380, 27),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Text = currentTranslation ?? string.Empty,
                Font = new Font("Segoe UI", 11)
            };
            txtTranslation.SelectAll();

            btnSave = new Button
            {
                Text = "Zapisz",
                Location = new Point(12, 118),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += (s, e) =>
            {
                TranslationResult = txtTranslation.Text;
                DialogResult = DialogResult.OK;
                Close();
            };

            btnRemove = new Button
            {
                Text = "Przywróć oryginał",
                Location = new Point(148, 118),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRemove.Click += (s, e) =>
            {
                RemoveRequested = true;
                TranslationResult = string.Empty;
                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancel = new Button
            {
                Text = "Anuluj",
                Location = new Point(290, 118),
                Size = new Size(102, 35),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[] { lblOriginal, lblNew, txtTranslation, btnSave, btnRemove, btnCancel });
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            if (currentTranslation != null)
            {
                lblOriginal.Text += $"  (tłumaczenie: {currentTranslation})";
            }
        }
    }
}
