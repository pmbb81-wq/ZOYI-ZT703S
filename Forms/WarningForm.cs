using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ZOYI
{
    public partial class WarningForm : Form
    {
        public bool DontShowAgain { get; private set; }
        private System.Windows.Forms.Timer _imgTimer;
        private int _imgIndex = 0;
        private List<Image> _images = new List<Image>();

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // WarningForm
            // 
            BackColor = SystemColors.ControlDarkDark;
            ClientSize = new Size(284, 261);
            Name = "WarningForm";
            StartPosition = FormStartPosition.CenterScreen;
            ResumeLayout(false);

        }

        public WarningForm()
        {
            this.Text = "UWAGA!";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = true;
            this.Size = new Size(640, 600);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.FromArgb(220, 220, 220);

            var imgDir = Path.Combine(Application.StartupPath, "Images");
            foreach (string name in new[] { "danger.png", "danger2.png", "danger3.png" })
            {
                string path = Path.Combine(imgDir, name);
                if (File.Exists(path))
                    _images.Add(Image.FromFile(path));
            }

            var pb = new PictureBox
            {
                Image = _images.Count > 0 ? _images[0] : null,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(10, 10),
                Width = 600,
                Height = 350,
                BackColor = Color.FromArgb(30, 30, 30)
            };
            if (pb.Image != null)
                pb.Height = Math.Min(350, pb.Image.Height * 600 / pb.Image.Width);

            if (_images.Count > 1)
            {
                _imgTimer = new System.Windows.Forms.Timer { Interval = 1250 };
                _imgTimer.Tick += (_, _) =>
                {
                    _imgIndex = (_imgIndex + 1) % _images.Count;
                    pb.Image = _images[_imgIndex];
                };
                _imgTimer.Start();
            }

            var lblTitle = new Label
            {
                Text = "OSTRZEŻENIE!",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 60, 60),
                BackColor = Color.FromArgb(30, 30, 30),
                Location = new Point(10, pb.Bottom + 10),
                Width = 600,
                Height = 40
            };

            var lbl = new Label
            {
                Text = "Nie podłączaj miernika do USB podczas pomiaru wysokiego napięcia!\n\n"
                     + "Grozi to uszkodzeniem multimetru oraz portu USB komputera.\n"
                     + "Przed podłączeniem do USB upewnij się, że układ jest odłączony\n"
                     + "od jakiegokolwiek źródła napięcia.\n\n"
                     + "Nie używaj miernika także pod wpływem :)",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.GreenYellow,
                BackColor = Color.FromArgb(30, 30, 30),
                Location = new Point(10, lblTitle.Bottom + 5),
                Width = 600,
                Height = 140
            };

            var cb = new CheckBox
            {
                Text = "Nie pokazuj ponownie",
                Location = new Point(40, lbl.Bottom + 10),
                Width = 300,
                ForeColor = Color.FromArgb(220, 220, 220),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            var btn = new Button
            {
                Text = "OK",
                Location = new Point(500, lbl.Bottom + 5),
                Width = 90,
                Height = 30,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.FromArgb(220, 220, 220),
                FlatStyle = FlatStyle.Standard,
                FlatAppearance = { BorderColor = Color.FromArgb(80, 80, 80) }
            };

            this.Controls.Add(pb);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lbl);
            this.Controls.Add(cb);
            this.Controls.Add(btn);
            this.AcceptButton = btn;
            this.ClientSize = new Size(620, btn.Bottom + 20);

            btn.Click += (s, e) =>
            {
                DontShowAgain = cb.Checked;
                this.Close();
            };

            this.FormClosing += (_, _) =>
            {
                _imgTimer?.Stop();
                _imgTimer?.Dispose();
                foreach (var img in _images)
                    img.Dispose();
            };
        }
    }
}
