using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZOYI
{
    public class UpdateDialog : Form
    {
        public UpdateDialog(UpdateChecker.ReleaseInfo release)
        {
            Text = "Aktualizacja dostepna!";
            Size = new Size(450, 380);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(24, 24, 24);
            ShowInTaskbar = false;

            var lblIcon = new Label();
            lblIcon.Text = "📦";
            lblIcon.Font = new Font("Segoe UI", 36F);
            lblIcon.ForeColor = Color.Lime;
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblIcon.Dock = DockStyle.Top;
            lblIcon.Height = 60;
            Controls.Add(lblIcon);

            var lblTitle = new Label();
            lblTitle.Text = "Nowa wersja: " + release.TagName;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Cyan;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 35;
            lblTitle.AutoSize = false;
            Controls.Add(lblTitle);

            var lblDate = new Label();
            try
            {
                var date = DateTime.Parse(release.PublishedAt).ToString("yyyy-MM-dd");
                lblDate.Text = "Data wydania: " + date;
            }
            catch { lblDate.Text = ""; }
            lblDate.Font = new Font("Segoe UI", 9F);
            lblDate.ForeColor = Color.Gray;
            lblDate.TextAlign = ContentAlignment.MiddleCenter;
            lblDate.Dock = DockStyle.Top;
            lblDate.Height = 25;
            lblDate.AutoSize = false;
            Controls.Add(lblDate);

            var txtNotes = new TextBox();
            txtNotes.Text = release.Body ?? "Brak opisu zmian.";
            txtNotes.Multiline = true;
            txtNotes.ReadOnly = true;
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Font = new Font("Consolas", 9F);
            txtNotes.BackColor = Color.FromArgb(13, 13, 13);
            txtNotes.ForeColor = Color.LightGreen;
            txtNotes.BorderStyle = BorderStyle.None;
            txtNotes.Dock = DockStyle.Fill;
            txtNotes.Margin = new Padding(15, 10, 15, 10);
            Controls.Add(txtNotes);

            var pnlBottom = new Panel();
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Height = 60;
            pnlBottom.BackColor = Color.FromArgb(34, 34, 34);
            Controls.Add(pnlBottom);

            var btnDownload = new Button();
            btnDownload.Text = "POBIERZ I URUCHOM";
            btnDownload.Location = new Point(15, 12);
            btnDownload.Size = new Size(200, 36);
            btnDownload.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnDownload.BackColor = Color.FromArgb(0, 64, 0);
            btnDownload.ForeColor = Color.LightGreen;
            btnDownload.FlatStyle = FlatStyle.Flat;
            btnDownload.FlatAppearance.BorderSize = 0;
            btnDownload.Cursor = Cursors.Hand;
            pnlBottom.Controls.Add(btnDownload);

            var btnLater = new Button();
            btnLater.Text = "PRZYPOMNIJ PÓŹNIEJ";
            btnLater.Location = new Point(225, 12);
            btnLater.Size = new Size(200, 36);
            btnLater.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLater.BackColor = Color.FromArgb(64, 64, 0);
            btnLater.ForeColor = Color.Yellow;
            btnLater.FlatStyle = FlatStyle.Flat;
            btnLater.FlatAppearance.BorderSize = 0;
            btnLater.Cursor = Cursors.Hand;
            pnlBottom.Controls.Add(btnLater);

            var btnSkip = new Button();
            btnSkip.Text = "POMIN";
            btnSkip.Location = new Point(15, 12);
            btnSkip.Size = new Size(100, 36);
            btnSkip.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSkip.BackColor = Color.FromArgb(64, 0, 0);
            btnSkip.ForeColor = Color.LightCoral;
            btnSkip.FlatStyle = FlatStyle.Flat;
            btnSkip.FlatAppearance.BorderSize = 0;
            btnSkip.Cursor = Cursors.Hand;
            btnSkip.Visible = false;
            pnlBottom.Controls.Add(btnSkip);

            btnDownload.Click += async (s, e) =>
            {
                btnDownload.Enabled = false;
                btnDownload.Text = "POBIERANIE...";

                var asset = release.Assets.Length > 0 ? release.Assets[0] : null;
                if (asset != null)
                {
                    string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), asset.Name);
                    bool ok = await UpdateChecker.DownloadAndRun(asset, path);
                    if (ok)
                    {
                        btnDownload.Text = "POBRANO!";
                        btnDownload.BackColor = Color.FromArgb(0, 80, 0);
                    }
                    else
                    {
                        btnDownload.Text = "BLAD POBIERANIA";
                        btnDownload.BackColor = Color.FromArgb(80, 0, 0);
                    }
                }
                else
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(release.HtmlUrl) { UseShellExecute = true });
                    DialogResult = DialogResult.OK;
                }
            };

            btnLater.Click += (s, e) => DialogResult = DialogResult.Cancel;

            btnSkip.Click += (s, e) =>
            {
                Properties.Settings.Default.skipped_version = release.TagName;
                Properties.Settings.Default.Save();
                DialogResult = DialogResult.Ignore;
            };
        }
    }
}
