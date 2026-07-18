using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ZOYI
{
    internal class AuthorsForm : Form
    {
        private readonly record struct AuthorInfo(string Name, string Desc, List<LinkInfo>? Links);

        private readonly record struct LinkInfo(string Label, string Url);

        private readonly AuthorInfo[] _authors;

        private readonly string _paypalUrl = "https://paypal.me/kris0725pl";

        public AuthorsForm()
        {
            _authors = new AuthorInfo[]
            {
                new("Marcin Ożóg",
                    "Główny założyciel projektu. Uwielbia programować. Aktualnie pracuje nad aplikacją na ESP32 nazwaną Atlas Cube. Jego portfolio można znaleźć na stronie GitHuba.",
                    new List<LinkInfo> { new("GitHub", "https://github.com/marcinozog") }),
                new("Marian Łopaciński",
                    "Odpowiada za przemeblowanie całego GUI, czyli wyglądu programu. Podobno mieszka gdzieś w Piotrkowie Trybunalskim ;)",
                    new List<LinkInfo> { new("YouTube", "https://www.youtube.com/@Marian_%C5%81opaci%C5%84ski") }),
                new("Krzysztof Nowacki",
                    "Gitarzysta od 35 lat. Kocha elektronikę i programowanie. Główny koordynator projektu :)",
                    new List<LinkInfo>
                    {
                        new("YouTube", "https://www.youtube.com/@kris0725pl"),
                        new("GitHub", "https://github.com/pmbb81-wq"),
                        new("SoundCloud", "https://soundcloud.com/krzysztof-nowacki-264455791"),
                    }),
            };

            Text = "Autorzy";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            ClientSize = new Size(620, 450);
            ShowInTaskbar = false;

            int photoW = 140, photoH = 140;
            int gap = 30;
            int totalW = photoW * 3 + gap * 2;
            int startX = (ClientSize.Width - totalW) / 2;
            int photoY = 25;

            for (int i = 0; i < _authors.Length; i++)
            {
                int idx = i;
                int x = startX + i * (photoW + gap);
                var author = _authors[i];

                var pb = new PictureBox
                {
                    Size = new Size(photoW, photoH),
                    Location = new Point(x, photoY),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.FromArgb(50, 50, 50),
                    BorderStyle = BorderStyle.FixedSingle,
                };
                pb.Paint += PlaceholderPaint;

                string authorsDir = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Authors");
                string authorFile = $"autor{i + 1}";
                foreach (var ext in new[] { ".png", ".jpg", ".jpeg", ".bmp" })
                {
                    string imgPath = System.IO.Path.Combine(authorsDir, authorFile + ext);
                    if (System.IO.File.Exists(imgPath))
                    {
                        pb.Image = Image.FromFile(imgPath);
                        break;
                    }
                }

                if (author.Links != null && author.Links.Count > 0)
                {
                    pb.Cursor = Cursors.Hand;
                    pb.Click += (_, _) => OnAvatarClick(idx);
                }

                Controls.Add(pb);

                var lblName = new Label
                {
                    Text = author.Name,
                    AutoSize = false,
                    Width = photoW,
                    Height = 22,
                    Location = new Point(x, photoY + photoH + 6),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.Cyan,
                    TextAlign = ContentAlignment.TopCenter
                };
                Controls.Add(lblName);

                var lblDesc = new ReadingLabel
                {
                    Text = author.Desc,
                    Width = photoW,
                    Height = 130,
                    Location = new Point(x, photoY + photoH + 28),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(200, 200, 200),
                    HighlightColor = Color.Cyan,
                };
                Controls.Add(lblDesc);
            }

            var btnDonate = new Button
            {
                Text = "♥  DONATE  (PayPal)",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 80, 120),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(200, 40),
                Location = new Point((ClientSize.Width - 200) / 2, ClientSize.Height - 60),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnDonate.Click += (_, _) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = _paypalUrl,
                        UseShellExecute = true
                    });
                }
                catch { }
            };
            Controls.Add(btnDonate);
        }

        private void OnAvatarClick(int authorIndex)
        {
            var links = _authors[authorIndex].Links;
            if (links == null || links.Count == 0) return;

            if (links.Count == 1)
            {
                OpenUrl(links[0].Url);
                return;
            }

            using var dlg = new Form
            {
                Text = _authors[authorIndex].Name,
                Size = new Size(320, 60 + links.Count * 45),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                ShowInTaskbar = false,
            };

            for (int j = 0; j < links.Count; j++)
            {
                var link = links[j];
                var btn = new Button
                {
                    Text = link.Label,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0, 64, 64),
                    ForeColor = Color.Cyan,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Size = new Size(270, 35),
                    Location = new Point((dlg.ClientSize.Width - 270) / 2, 15 + j * 45),
                    Cursor = Cursors.Hand,
                    FlatAppearance = { BorderSize = 0 }
                };
                btn.Click += (_, _) => OpenUrl(link.Url);
                dlg.Controls.Add(btn);
            }

            dlg.ShowDialog(this);
        }

        private static void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void PlaceholderPaint(object? sender, PaintEventArgs e)
        {
            if (sender is not PictureBox pb) return;
            if (pb.Image != null) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(pb.BackColor);

            using var font = new Font("Segoe UI", 48F, FontStyle.Bold);
            using var brush = new SolidBrush(Color.FromArgb(80, 80, 80));
            var sz = g.MeasureString("?", font);
            g.DrawString("?", font, brush,
                (pb.Width - sz.Width) / 2,
                (pb.Height - sz.Height) / 2 - 5);
        }
    }

    internal class ReadingLabel : Control
    {
        private readonly System.Windows.Forms.Timer _timer;
        private string[] _words = Array.Empty<string>();
        private int _currentWord = -1;
        private int _pauseCounter;

        public Color HighlightColor { get; set; } = Color.Cyan;
        public int WordIntervalMs { get; set; } = 400;

        public ReadingLabel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);

            _timer = new System.Windows.Forms.Timer { Interval = WordIntervalMs };
            _timer.Tick += OnTick;
        }

        public new string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                _words = string.IsNullOrWhiteSpace(value)
                    ? Array.Empty<string>()
                    : value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                _currentWord = -1;
                _pauseCounter = 0;
                if (_words.Length > 0) _timer.Start();
                else _timer.Stop();
                Invalidate();
            }
        }

        private void OnTick(object? sender, EventArgs e)
        {
            if (_words.Length == 0) return;

            if (_pauseCounter > 0)
            {
                _pauseCounter--;
                return;
            }

            _currentWord++;
            if (_currentWord >= _words.Length)
            {
                _currentWord = -1;
                _pauseCounter = 4;
                Invalidate();
                return;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_words.Length == 0) return;

            var g = e.Graphics;
            g.SetClip(new Rectangle(0, 0, Width, Height));

            using var normalFont = Font;
            using var boldFont = new Font(Font, FontStyle.Bold);

            int x = 4;
            int y = 2;

            for (int i = 0; i < _words.Length; i++)
            {
                var font = (i == _currentWord) ? boldFont : normalFont;
                var color = (i == _currentWord) ? HighlightColor : ForeColor;

                var sz = TextRenderer.MeasureText(g, _words[i], font, Size.Empty,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

                if (x + sz.Width > Width - 4)
                {
                    x = 4;
                    y += sz.Height + 2;
                }

                if (y + sz.Height > Height) break;

                TextRenderer.DrawText(g, _words[i], font, new Point(x, y), color,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

                x += sz.Width + TextRenderer.MeasureText(g, " ", normalFont,
                    Size.Empty, TextFormatFlags.NoPadding).Width;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
