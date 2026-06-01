using System.Globalization;
using System.IO.Ports;
using System.Media;
using System.Net.Http;
using System.Speech.Synthesis;

namespace ZOYI
{
    public partial class MainWindow : Form
    {
        public enum PARSE_MODE
        {
            STD,
            EXT,
            LUA,
            RAW
        }

        FrameDecoder frame_dec;
        WebServer webServer;

        COMx comx;
        COMx dq02Comx;
        StandardDisplayPanel standardDisplayPanel;
        AdancedDisplayPanel advancedDisplayPanel;

        private readonly string[] measurementModes = new string[]
        {
            "FUNC:AUTO",
            "FUNCtion:R",
            "FUNCtion:C",
            "FUNCtion:L",
            "FUNCtion:Z",
            "FUNCtion:ECAP",
            "FUNCtion:BATT"
        };
        private int currentModeIndex = 0;

        MLua mLua;
        string luaPath = "FrameDecoder\\parse_std.lua";
        Tools tools;
        string toolsPath = "Tools\\Tools.txt";

        ShortcutManager shortcutManager;

        List<string[]> csvData = new List<string[]>();
        object csvLock = new object();

        SpeechSynthesizer tts;
        bool ttsEnabled = false;
        string lastSpokenValue = "";
        DateTime lastSpokenTime = DateTime.MinValue;
        string stableValue = "";
        DateTime stableSince = DateTime.MinValue;

        /*
         * Move window section
         */
        bool bMouseDown = false;
        Point mousePosDown = Point.Empty;
        Point currentFormLocation = Point.Empty;

        /*
         * Alarm section
         */
        bool bAlarmEnable = false;
        float fAlarmOverValue = 0.0f;
        float fAlarmUnderValue = 0.0f;
        Thread alarmSoundThread;
        bool bBeepPlaying = false;
        System.Windows.Forms.Timer _updateTimer;

        public MainWindow()
        {
            InitializeComponent();
            this.Text = $"ZOYI ZT-703S — v{Application.ProductVersion}";
            frame_dec = new FrameDecoder();
            comx = new COMx();
            dq02Comx = new COMx();
            webServer = new WebServer(frame_dec);
            refreshDQ02portlist();

            // 6. Narzędzia
            tools = new Tools(toolsPath, panelTools);
            tools.refreshTools();

            // 7. Lista dostępnych portów COM
            refreshCOMlist();

            // 8. Załadowanie ustawień suwaków
            LoadAdvancedPanelSettings(GetTbarArcTicks());

            // 9. Inicjalizacja paneli wyświetlania
            InitializeDisplayPanels();

            // 10. Inicjalizacja TTS
            InitializeTTS();

            // 10. Inicjalizacja menedżera skrótów klawiaturowych
            InitializeShortcuts();

            // 11. Ostrzeżenie startowe
            if (Properties.Settings.Default.show_startup_warning)
            {
                var wf = new WarningForm();
                wf.ShowDialog(this);
                if (wf.DontShowAgain)
                    Properties.Settings.Default.show_startup_warning = false;
            }

            // 12. Sprawdzenie aktualizacji
            _updateTimer = new System.Windows.Forms.Timer { Interval = 300000 };
            _updateTimer.Tick += async (_, _) => await CheckForUpdateAsync();
            this.Shown += async (s, e) =>
            {
                await CheckForUpdateAsync();
                _updateTimer.Start();
            };

            // 13. Umożliw przeciąganie wszystkich etykiet DQ02
            EnableDQ02LabelDragging();
            RestoreControlPositions();

            tbDQ02UserTolerance.TextChanged += (s, e) =>
            {
                string txt = tbDQ02UserTolerance.Text?.Replace("%", "").Trim();
                lblDQ02Tolerance.Text = string.IsNullOrWhiteSpace(txt)
                    ? "Tolerance: —"
                    : $"Tolerance: {txt}%";
            };

            var ctxTan = new ContextMenuStrip();
            ctxTan.Items.Add("Wpisz tanδ", null, (_, _) =>
            {
                var frm = new Form { Text = "tanδ", Size = new Size(300, 130), FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent, MinimizeBox = false, MaximizeBox = false };
                var tb = new TextBox { Left = 20, Top = 20, Width = 240 };
                var btn = new Button { Text = "OK", Left = 100, Top = 60, Width = 80, DialogResult = DialogResult.OK };
                frm.Controls.Add(tb);
                frm.Controls.Add(btn);
                frm.AcceptButton = btn;
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    string input = tb.Text.Trim().Replace(",", ".");
                    if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double val) && val > 0)
                    {
                        _customTanDelta = val;
                        label6.Text = $"tanδ = {val:F4} (RĘCZNIE)";
                    }
                }
            });
            ctxTan.Items.Add("Auto (z bazy)", null, (_, _) =>
            {
                _customTanDelta = null;
                label6.Text = "tanδ: —";
            });
            label6.ContextMenuStrip = ctxTan;

            toolTip1.SetToolTip(checkBox1, "Ukrywa okno podgladu logu.");
            toolTip1.SetToolTip(checkBox2, "Blokuje i odblokowuje przesuwanie wszytkimi elementami na formatcce.");

            toolTip1.SetToolTip(button3, "Zmienia czestotliwosc pomiaru w hercach.");
            toolTip1.SetToolTip(btnDQ02Connect, "Laczy lub rozlacza z wybranym portem COM."); 
            toolTip1.SetToolTip(btnDQ02Refresh, "Odswierza porty COM ktore sa dostepne w komputerze.");
            toolTip1.SetToolTip(button7, "Zmienia tryby poboczne  miernika. X/D/Q/0/ESR.");
            toolTip1.SetToolTip(button5, "Zmienia level napiecia 100mv/300mv/600mv");
            toolTip1.SetToolTip(button4, "AI HELP. Pomaga w szybkim streszczeniu badanego kondensatora a takze umozliwia wyszukanie kondensatora w sklepach nad ktorym prowadzimy diagnoze");
            toolTip1.SetToolTip(button8, "Zapisuje plik CSV z wszystkimi wartosciami jakie sa mierzone.");
            toolTip1.SetToolTip(tbDQ02UserNominal, "Tutaj wpisujemy wartosc kondensatora ktory mierzymy. 1000uF to np 1mF.");
            toolTip1.SetToolTip(tbDQ02UserTolerance, "Tutaj wpisujemy wartosc tolerancji pojemnosci w procentach ale tylko sama liczbe bez znaku procenta.");
            toolTip1.SetToolTip(textBoxvoltage, "Tutaj wpisujemy napiecie naszego mierzonego kondensatora.");
            toolTip1.SetToolTip(textBoxtemperature, "Tutaj wpisujemy wartosc temperatury jaka jest napisana na kondensatorze.");
            toolTip1.SetToolTip(button6, "Zmienia tryby pracy miernika AUTO/R/L/Z/ECAP/BATT.");
            toolTip1.SetToolTip(label6, "Klikniecie prawym myszki pozwala zmienic tanges na swoj lub z bazy csv.");
            toolTip1.SetToolTip(lblDQ02PassFail, "Jezeli wartosc pojemnosci nominalnej w stosunku do wartosci mierzonej\n rozni sie w wpisanej tolerancji to pokaze FAIL lub PASS.");
            toolTip1.SetToolTip(lblDQ02Deviation, "Pokazuje w czasie rzeczywistym roznice tolerancji pojemnosci kondensatora wzgledem nominalnej.");
            //toolTip1.SetToolTip(button3, "Zmienia czestotliwosc pomiaru w hercach.");

            LoadESRImage();
        }

        private void SetWindowLocation()
        {
            try
            {
                int x = Properties.Settings.Default.main_form_pos_x;
                int y = Properties.Settings.Default.main_form_pos_y;
                var pt = new Point(x, y);
                bool onScreen = false;
                foreach (var s in Screen.AllScreens)
                {
                    if (s.Bounds.Contains(pt)) { onScreen = true; break; }
                }
                if (onScreen)
                    this.Location = pt;
                else
                    StartPosition = FormStartPosition.CenterScreen;
            }
            catch
            {
                StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private void InitializeDisplayPanels()
        {
            standardDisplayPanel = new StandardDisplayPanel(chbStandardPanel);
            standardDisplayPanel.Show();

            advancedDisplayPanel = new AdancedDisplayPanel(chbAdvancedPanel);
            var _ = advancedDisplayPanel.Handle;
            if (chbAdvancedPanel.Checked)
                advancedDisplayPanel.Show();
        }

        private TrackBar GetTbarArcTicks()
        {
            return tbarArcTicks;
        }

        private void LoadAdvancedPanelSettings(TrackBar tbarArcTicks)
        {
            tbarArcTicks.Value = ClampSetting(
                Properties.Settings.Default.panel_adv_ticks,
                tbarArcTicks.Minimum,
                tbarArcTicks.Maximum
            );
            lblArcTicks.Text = tbarArcTicks.Value.ToString();

            tbarThicksCount.Value = ClampSetting(
                Properties.Settings.Default.panel_adv_thicks_count,
                tbarThicksCount.Minimum,
                tbarThicksCount.Maximum
            );
            lblThicksCount.Text = tbarThicksCount.Value.ToString();
        }

        private int ClampSetting(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            webServer.Stop();
            comx.disconnect();
            standardDisplayPanel.Close();

            SaveControlPositions();
            Properties.Settings.Default.main_form_pos_x = this.Location.X;
            Properties.Settings.Default.main_form_pos_y = this.Location.Y;
            Properties.Settings.Default.Save();
        }

        private void btnWebServerStart_Click(object sender, EventArgs e)
        {
            int port = int.Parse(tbWebServerPort.Text);
            webServer.Start(port);

            if (webServer.IsRunning)
            {
                llWebAddress.Text = webServer.getURI();
                llWebAddress.Links.Clear();
                llWebAddress.Links.Add(0, webServer.getURI().Length, webServer.getURI());

                tbWebServerPort.Enabled = false;
                btnWebServerStop.Enabled = true;
                btnWebServerStart.Enabled = false;
            }
        }

        private void btnWebServerStop_Click(object sender, EventArgs e)
        {
            if (webServer.IsRunning)
            {
                webServer.Stop();
                tbWebServerPort.Enabled = true;
                btnWebServerStop.Enabled = false;
                btnWebServerStart.Enabled = true;
                llWebAddress.Text = "http://IP:port/";
                llWebAddress.Links.Clear();
            }
        }

        private void llWebAddress_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string? url = e.Link!.LinkData as string;
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

        private void chbShowPanel_CheckedChanged(object sender, EventArgs e)
        {
            if (standardDisplayPanel == null) return;
            if (chbStandardPanel.Checked)
                standardDisplayPanel.Show();
            else
                standardDisplayPanel.Hide();
        }

        private void chbAdvancedPanel_CheckedChanged(object sender, EventArgs e)
        {
            if (advancedDisplayPanel == null) return;
            if (chbAdvancedPanel.Checked)
                advancedDisplayPanel.Show();
            else
                advancedDisplayPanel.Hide();
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            tbComOutput.Text = "";
        }

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Pliki CSV|*.csv";
                sfd.FileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
                sfd.InitialDirectory = "logs";
                sfd.Title = "Zapisz pomiary jako CSV";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    int count = 0;
                    using (var writer = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine("Czas;Wartosc;Jednostka;Tryb;DC/AC;Czestotliwosc;Jedn.Czest.;Status");
                        lock (csvLock)
                        {
                            foreach (var row in csvData)
                            {
                                writer.WriteLine(string.Join(";", row));
                                count++;
                            }
                        }
                    }
                    MessageBox.Show("Zapisano " + count + " pomiarow do pliku:\n" + sfd.FileName, "Zapis CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            standardDisplayPanel.Close();
            advancedDisplayPanel.Close();
            System.Windows.Forms.Application.Exit();
        }

        /*
         * 
         * Move window section
         * 
         */
        private void MainWindow_MouseDown(object sender, MouseEventArgs e)
        {
            bMouseDown = true;
            mousePosDown = Control.MousePosition;
            currentFormLocation = Location;
        }

        private void MainWindow_MouseUp(object sender, MouseEventArgs e)
        {
            bMouseDown = false;
            mousePosDown = Point.Empty;
            currentFormLocation = Point.Empty;
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (bMouseDown)
            {
                var currentPos = Control.MousePosition;
                var distX = currentPos.X - mousePosDown.X;
                var distY = currentPos.Y - mousePosDown.Y;
                Location = new Point(currentFormLocation.X + distX, currentFormLocation.Y + distY);
            }
        }

        /*
         * DQ02 label dragging
         */
        private void EnableDQ02LabelDragging()
        {
            foreach (Control c in tabDQ02.Controls)
            {
                if (c is Label lbl && lbl.Name != null && (lbl.Name.StartsWith("lblDQ02") || lbl.Name == "lblStatus" || lbl.Name == "lblTargetESR" || lbl.Name == "label6"))
                    MakeDraggable(lbl, () => checkBox2.Checked);
            }

            MakeDraggable(button3, () => checkBox2.Checked);
            MakeDraggable(button4, () => checkBox2.Checked);
            MakeDraggable(button5, () => checkBox2.Checked);
            MakeDraggable(button6, () => checkBox2.Checked);
            MakeDraggable(button7, () => checkBox2.Checked);
            MakeDraggable(button8, () => checkBox2.Checked);
            MakeDraggable(tbDQ02UserNominal, () => checkBox2.Checked);
            MakeDraggable(tbDQ02UserTolerance, () => checkBox2.Checked);
            MakeDraggable(textBoxvoltage, () => checkBox2.Checked);
            MakeDraggable(textBoxtemperature, () => checkBox2.Checked);

            SetupDQ02ValueContextMenu();
        }

        private void SetupDQ02ValueContextMenu()
        {
            lblDQ02Value.ContextMenuStrip = MakeFontColorMenu(lblDQ02Value,
                () => Properties.Settings.Default.dq02_value_font,
                v => Properties.Settings.Default.dq02_value_font = v,
                () => Properties.Settings.Default.dq02_value_color,
                v => Properties.Settings.Default.dq02_value_color = v,
                new Font("Segoe UI", 28F, FontStyle.Bold), Color.Gold);

            lblDQ02Prefix.ContextMenuStrip = MakeFontColorMenu(lblDQ02Prefix,
                () => Properties.Settings.Default.dq02_prefix_font,
                v => Properties.Settings.Default.dq02_prefix_font = v,
                () => Properties.Settings.Default.dq02_prefix_color,
                v => Properties.Settings.Default.dq02_prefix_color = v,
                new Font("Segoe UI", 28F, FontStyle.Bold), Color.Gold);

            lblDQ02Secondary.ContextMenuStrip = MakeFontColorMenu(lblDQ02Secondary,
                () => Properties.Settings.Default.dq02_secondary_font,
                v => Properties.Settings.Default.dq02_secondary_font = v,
                () => Properties.Settings.Default.dq02_secondary_color,
                v => Properties.Settings.Default.dq02_secondary_color = v,
                new Font("Segoe UI", 18F, FontStyle.Bold), Color.Cyan);

            lblDQ02PassFail.ContextMenuStrip = MakeFontOnlyMenu(lblDQ02PassFail,
                () => Properties.Settings.Default.dq02_passfail_font,
                v => Properties.Settings.Default.dq02_passfail_font = v,
                new Font("Segoe UI", 9F, FontStyle.Bold));

            lblStatus.ContextMenuStrip = MakeFontOnlyMenu(lblStatus,
                () => Properties.Settings.Default.lbl_status_font,
                v => Properties.Settings.Default.lbl_status_font = v,
                new Font("Segoe UI", 9F));

            lblTargetESR.ContextMenuStrip = MakeFontOnlyMenu(lblTargetESR,
                () => Properties.Settings.Default.lbl_target_esr_font,
                v => Properties.Settings.Default.lbl_target_esr_font = v,
                new Font("Segoe UI", 9F));
        }

        private ContextMenuStrip MakeFontColorMenu(Label lbl,
            Func<Font> getFont, Action<Font> setFont,
            Func<string> getColor, Action<string> setColor,
            Font defaultFont, Color defaultColor)
        {
            // Load saved
            if (getFont() != null) lbl.Font = getFont();
            try { lbl.ForeColor = ColorTranslator.FromHtml(getColor()); } catch { }

            var ctx = new ContextMenuStrip();
            ctx.Items.Add("Change Font...", null, (s, e) =>
            {
                using (var fd = new FontDialog())
                {
                    fd.Font = lbl.Font;
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        lbl.Font = fd.Font;
                        setFont(fd.Font);
                        Properties.Settings.Default.Save();
                    }
                }
            });
            ctx.Items.Add("Change Color...", null, (s, e) =>
            {
                using (var cd = new ColorDialog())
                {
                    cd.Color = lbl.ForeColor;
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        lbl.ForeColor = cd.Color;
                        setColor(ColorTranslator.ToHtml(cd.Color));
                        Properties.Settings.Default.Save();
                    }
                }
            });
            ctx.Items.Add("Reset", null, (s, e) =>
            {
                lbl.Font = defaultFont;
                lbl.ForeColor = defaultColor;
                setFont(null);
                setColor(ColorTranslator.ToHtml(defaultColor));
                Properties.Settings.Default.Save();
            });
            return ctx;
        }

        private ContextMenuStrip MakeFontOnlyMenu(Label lbl,
            Func<Font> getFont, Action<Font> setFont,
            Font defaultFont)
        {
            if (getFont() != null) lbl.Font = getFont();

            var ctx = new ContextMenuStrip();
            ctx.Items.Add("Change Font...", null, (s, e) =>
            {
                using (var fd = new FontDialog())
                {
                    fd.Font = lbl.Font;
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        lbl.Font = fd.Font;
                        setFont(fd.Font);
                        Properties.Settings.Default.Save();
                    }
                }
            });
            ctx.Items.Add("Reset Font", null, (s, e) =>
            {
                lbl.Font = defaultFont;
                setFont(null);
                Properties.Settings.Default.Save();
            });
            return ctx;
        }

        private void MakeDraggable(Control ctrl, Func<bool> blockCheck = null)
        {
            bool dragging = false;
            Point startPoint = Point.Empty;
            Point originalLocation = Point.Empty;

            ctrl.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && (blockCheck == null || !blockCheck()))
                {
                    dragging = true;
                    startPoint = Control.MousePosition;
                    originalLocation = ctrl.Location;
                    ctrl.BringToFront();
                }
            };

            ctrl.MouseMove += (s, e) =>
            {
                if (dragging)
                {
                    var currentPos = Control.MousePosition;
                    var dx = currentPos.X - startPoint.X;
                    var dy = currentPos.Y - startPoint.Y;
                    ctrl.Location = new Point(originalLocation.X + dx, originalLocation.Y + dy);
                }
            };

            ctrl.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    dragging = false;
                    SaveControlPositions();
                }
            };
        }

        private void SaveControlPositions()
        {
            var draggableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "__checkBox1"
            };
            foreach (Control c in tabDQ02.Controls)
            {
                if (!string.IsNullOrEmpty(c.Name))
                {
                    if (c is Label lbl && (lbl.Name.StartsWith("lblDQ02") || lbl.Name == "lblStatus" || lbl.Name == "lblTargetESR" || lbl.Name == "label6"))
                        draggableNames.Add(lbl.Name);
                    else if (c is Button btn && (btn == button3 || btn == button4 || btn == button5 || btn == button6 || btn == button7 || btn == button8))
                        draggableNames.Add(btn.Name);
                    else if (c is TextBox tb && (tb.Name == "tbDQ02UserNominal" || tb.Name == "tbDQ02UserTolerance" || tb.Name == "textBoxvoltage" || tb.Name == "textBoxtemperature"))
                        draggableNames.Add(tb.Name);
                }
            }

            string data = "__checkBox1=" + (checkBox1.Checked ? "1" : "0") + "|__checkBox2=" + (checkBox2.Checked ? "1" : "0");
            foreach (Control c in tabDQ02.Controls)
            {
                if (!string.IsNullOrEmpty(c.Name) && draggableNames.Contains(c.Name) && c != checkBox1)
                {
                    data += "|" + c.Name + "=" + c.Left + "," + c.Top;
                }
            }
            Properties.Settings.Default.saved_control_positions = data;
            Properties.Settings.Default.Save();
        }

        private void RestoreControlPositions()
        {
            string data = Properties.Settings.Default.saved_control_positions;
            if (string.IsNullOrWhiteSpace(data)) return;

            var draggableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "__checkBox1"
            };
            foreach (Control c in tabDQ02.Controls)
            {
                if (!string.IsNullOrEmpty(c.Name))
                {
                    if (c is Label lbl && (lbl.Name.StartsWith("lblDQ02") || lbl.Name == "lblStatus" || lbl.Name == "lblTargetESR" || lbl.Name == "label6"))
                        draggableNames.Add(lbl.Name);
                    else if (c is Button btn && (btn == button3 || btn == button4 || btn == button5 || btn == button6 || btn == button7 || btn == button8))
                        draggableNames.Add(btn.Name);
                    else if (c is TextBox tb && (tb.Name == "tbDQ02UserNominal" || tb.Name == "tbDQ02UserTolerance" || tb.Name == "textBoxvoltage" || tb.Name == "textBoxtemperature"))
                        draggableNames.Add(tb.Name);
                }
            }

            foreach (string entry in data.Split('|'))
            {
                string[] parts = entry.Split('=');
                if (parts.Length != 2) continue;
                string name = parts[0].Trim();

                if (name == "__checkBox1")
                {
                    checkBox1.Checked = (parts[1].Trim() == "1");
                    continue;
                }

                if (name == "__checkBox2")
                {
                    checkBox2.Checked = (parts[1].Trim() == "1");
                    continue;
                }

                if (!draggableNames.Contains(name)) continue;

                string[] coords = parts[1].Split(',');
                if (coords.Length != 2) continue;

                Control ctrl = tabDQ02.Controls[name];
                if (ctrl == null) continue;

                if (int.TryParse(coords[0].Trim(), out int left) &&
                    int.TryParse(coords[1].Trim(), out int top))
                {
                    ctrl.Left = left;
                    ctrl.Top = top;
                }
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnShortcuts_Click(object sender, EventArgs e)
        {
            var configForm = new ShortcutConfigForm(shortcutManager);
            configForm.ShowDialog(this);
        }

        private void pbESR_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Obrazy|*.png;*.jpg;*.jpeg;*.bmp";
                ofd.Title = "Wybierz obraz tabeli ESR";
                ofd.InitialDirectory = "Images";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string ext = Path.GetExtension(ofd.FileName).ToLower();
                        string destPath = "Images\\esr_table" + ext;
                        Directory.CreateDirectory("Images");
                        File.Copy(ofd.FileName, destPath, true);
                        pbESR.Image = Image.FromFile(destPath);
                        MessageBox.Show("Zmieniono obraz tabeli ESR.", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Blad: " + ex.Message, "Blad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadESRImage()
        {
            string imgDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            string esrPath = null;
            string[] extensions = { ".png", ".jpg", ".jpeg", ".bmp" };

            foreach (var ext in extensions)
            {
                string path = Path.Combine(imgDir, "esr_table" + ext);
                if (File.Exists(path))
                {
                    esrPath = path;
                    break;
                }
            }

            if (esrPath != null)
            {
                pbESR.Image = Image.FromFile(esrPath);
            }
        }

        /*
         * 
         * Colors section
         * 
         */

        private void btnColorBg_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.AllowFullOpen = false;

            if (cd.ShowDialog() == DialogResult.OK)
                standardDisplayPanel.setBackgroundColor(cd.Color);
        }

        private void btnColorLabel_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.AllowFullOpen = false;

            if (cd.ShowDialog() == DialogResult.OK)
                standardDisplayPanel.setLabelFontColor(cd.Color);
        }

        private void btnColorValue_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.AllowFullOpen = false;

            if (cd.ShowDialog() == DialogResult.OK)
                standardDisplayPanel.setValueFontColor(cd.Color);
        }

        /*
         * 
         * Alarm section
         * 
         */


        private void tbPanelOpacity_Scroll(object sender, EventArgs e)
        {
            standardDisplayPanel.changeOpacity(tbarPanelOpacity.Value);
            Properties.Settings.Default.panel_opacity = tbarPanelOpacity.Value;
            Properties.Settings.Default.Save();
        }

        /*
         * Tools
         */
        private async void btnToolsEdit_Click(object sender, EventArgs e)
        {
            RichEditor re = new RichEditor(tools);
            re.loadFile(toolsPath, RichEditor.ENCODING.TOOLS);

            string result = await re.AsyncEdit();

            if (result == "save")
                tools.refreshTools();
        }

        private void btnToolsRefresh_Click(object sender, EventArgs e)
        {
            tools.refreshTools();
        }

        private void tbarArcTicks_Scroll(object sender, EventArgs e)
        {
            if (tbarArcTicks.Value < tbarThicksCount.Value)
            {
                tbarThicksCount.Value = tbarArcTicks.Value;
                lblThicksCount.Text = tbarThicksCount.Value.ToString();
                advancedDisplayPanel.setArcProgressBarThicksCount(tbarThicksCount.Value);
            }

            advancedDisplayPanel.setArcProgressBarTicks(tbarArcTicks.Value);
            lblArcTicks.Text = tbarArcTicks.Value.ToString();
        }

        private void tbarThicksCount_Scroll(object sender, EventArgs e)
        {
            if (tbarThicksCount.Value > tbarArcTicks.Value)
            {
                tbarArcTicks.Value = tbarThicksCount.Value;
                lblArcTicks.Text = tbarArcTicks.Value.ToString();
                advancedDisplayPanel.setArcProgressBarTicks(tbarArcTicks.Value);
            }

            advancedDisplayPanel.setArcProgressBarThicksCount(tbarThicksCount.Value);
            lblThicksCount.Text = tbarThicksCount.Value.ToString();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void tbComOutput_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnChartPause_Click(object sender, EventArgs e)
        {
            chartPanel.TogglePause();
            btnChartPause.Text = chartPanel.IsPaused ? "WZNÓW" : "PAUZA";
            btnChartPause.ForeColor = chartPanel.IsPaused ? Color.LightCoral : Color.Yellow;
        }

        private void btnChartClear_Click(object sender, EventArgs e)
        {
            chartPanel.Clear();
        }

        private void btnTimeWindow_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int seconds = 60;

            btnTime10s.BackColor = Color.FromArgb(0, 64, 64);
            btnTime10s.ForeColor = Color.Cyan;
            btnTime30s.BackColor = Color.FromArgb(0, 64, 64);
            btnTime30s.ForeColor = Color.Cyan;
            btnTime60s.BackColor = Color.FromArgb(0, 64, 64);
            btnTime60s.ForeColor = Color.Cyan;
            btnTime5min.BackColor = Color.FromArgb(0, 64, 64);
            btnTime5min.ForeColor = Color.Cyan;

            switch (btn.Name)
            {
                case "btnTime10s":
                    seconds = 10;
                    btn.BackColor = Color.FromArgb(0, 80, 80);
                    btn.ForeColor = Color.White;
                    break;
                case "btnTime30s":
                    seconds = 30;
                    btn.BackColor = Color.FromArgb(0, 80, 80);
                    btn.ForeColor = Color.White;
                    break;
                case "btnTime60s":
                    seconds = 60;
                    btn.BackColor = Color.FromArgb(0, 80, 0);
                    btn.ForeColor = Color.White;
                    break;
                case "btnTime5min":
                    seconds = 300;
                    btn.BackColor = Color.FromArgb(0, 80, 80);
                    btn.ForeColor = Color.White;
                    break;
            }

            chartPanel.SetTimeWindow(seconds);
        }

        private void InitializeTTS()
        {
            tts = new SpeechSynthesizer();
            tts.Rate = 0;
            tts.Volume = 100;

            cbTTSVoice.Items.Clear();
            foreach (var voice in tts.GetInstalledVoices())
            {
                if (voice.Enabled)
                {
                    cbTTSVoice.Items.Add(voice.VoiceInfo.Name);
                }
            }

            if (cbTTSVoice.Items.Count > 0)
            {
                cbTTSVoice.SelectedIndex = 0;
            }

            ttsEnabled = false;
            chbTTSSwitch.Checked = false;
            chbTTSSwitch.BackColor = Color.FromArgb(80, 0, 0);
            chbTTSSwitch.Text = "OFF";
            chbTTSSwitch.ForeColor = Color.LightCoral;
            lblTTSStatus.Text = "TTS OFF";
            lblTTSStatus.ForeColor = Color.LightCoral;

            if (Properties.Settings.Default.tts_voice != null && cbTTSVoice.Items.Contains(Properties.Settings.Default.tts_voice))
            {
                cbTTSVoice.SelectedItem = Properties.Settings.Default.tts_voice;
            }

            Properties.Settings.Default.tts_enabled = false;
            Properties.Settings.Default.Save();
        }

        public void SpeakMeasurement(string value, string unit)
        {
            if (!ttsEnabled || string.IsNullOrEmpty(value)) return;

            float numVal;
            if (!float.TryParse(value.Trim(), System.Globalization.CultureInfo.InvariantCulture, out numVal)) return;
            if (Math.Abs(numVal) < 0.09f) return;

            bool isNegative = numVal < 0;
            string cleanValue = Math.Abs(numVal).ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            var now = DateTime.Now;

            if (cleanValue != stableValue)
            {
                stableValue = cleanValue;
                stableSince = now;
                return;
            }

            if ((now - stableSince).TotalSeconds < 0.5) return;
            if ((now - lastSpokenTime).TotalSeconds < 3) return;

            lastSpokenValue = cleanValue;
            lastSpokenTime = now;

            cleanValue = cleanValue.Replace('.', ',');

            string prefix = isNegative ? "minus " : "";
            string textToSpeak = $"{prefix}{cleanValue} {unit}"
                .Replace("mV", "mili wolta")
                .Replace("kV", "kilo wolta")
                .Replace("uV", "mikrowolta")
                .Replace("V", "wolta")
                .Replace("mA", "mili ampera")
                .Replace("uA", "mikroampera")
                .Replace("A", "ampera")
                .Replace("pF", "pikofarada")
                .Replace("nF", "nanofarada")
                .Replace("uF", "mikrofarada")
                .Replace("mF", "milifarada")
                .Replace("F", "farada")
                .Replace("kΩ", "kilooma")
                .Replace("MΩ", "megaoma")
                .Replace("Ω", " oma")
                .Replace("kHz", "kiloherca")
                .Replace("Hz", "herca");

            try
            {
                if (tts.State == SynthesizerState.Speaking)
                {
                    tts.SpeakAsyncCancelAll();
                }
                tts.SpeakAsync(textToSpeak);
            }
            catch { }
        }

        private void chbTTSSwitch_CheckedChanged(object sender, EventArgs e)
        {
            ttsEnabled = chbTTSSwitch.Checked;
            Properties.Settings.Default.tts_enabled = ttsEnabled;
            Properties.Settings.Default.Save();

            if (ttsEnabled)
            {
                chbTTSSwitch.BackColor = Color.FromArgb(0, 128, 0);
                chbTTSSwitch.Text = "ON";
                chbTTSSwitch.ForeColor = Color.White;
                lblTTSStatus.Text = "TTS ON";
                lblTTSStatus.ForeColor = Color.LightGreen;
            }
            else
            {
                chbTTSSwitch.BackColor = Color.FromArgb(80, 0, 0);
                chbTTSSwitch.Text = "OFF";
                chbTTSSwitch.ForeColor = Color.LightCoral;
                lblTTSStatus.Text = "TTS OFF";
                lblTTSStatus.ForeColor = Color.LightCoral;
            }
        }

        private void cbTTSVoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbTTSVoice.SelectedItem != null)
            {
                string voiceName = cbTTSVoice.SelectedItem.ToString();
                tts.SelectVoice(voiceName);
                Properties.Settings.Default.tts_voice = voiceName;
                Properties.Settings.Default.Save();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Form infoForm = new Form();
            infoForm.Text = "Pomiar tranzystorów SMD — Poradnik KRIS®";
            infoForm.BackColor = Color.FromArgb(24, 24, 24);
            infoForm.ForeColor = Color.LightGreen;
            infoForm.Font = new Font("Consolas", 9F);
            infoForm.StartPosition = FormStartPosition.CenterParent;
            infoForm.Size = new Size(820, 720);
            infoForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            infoForm.MaximizeBox = false;
            infoForm.MinimizeBox = false;

            TabControl tabCtrl = new TabControl();
            tabCtrl.Dock = DockStyle.Fill;
            tabCtrl.Appearance = TabAppearance.Buttons;
            tabCtrl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            TabPage tabText = new TabPage("PORADNIK");
            tabText.BackColor = Color.FromArgb(24, 24, 24);

            Panel guideScroll = new Panel();
            guideScroll.AutoScroll = true;
            guideScroll.Dock = DockStyle.Fill;
            guideScroll.BackColor = Color.FromArgb(24, 24, 24);
            tabText.Controls.Add(guideScroll);

            int gx = 15, gy = 15, gw = 740;

            Panel hdr = CreateCard(gw, 60, Color.FromArgb(0, 64, 64));
            hdr.Location = new Point(gx, gy);
            Label hdrTitle = new Label();
            hdrTitle.Text = "POMIAR TRANZYSTORÓW SMD";
            hdrTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            hdrTitle.ForeColor = Color.Cyan;
            hdrTitle.Location = new Point(15, 5);
            hdrTitle.AutoSize = true;
            hdr.Controls.Add(hdrTitle);
            Label hdrSub = new Label();
            hdrSub.Text = "Poradnik pomiaru multimetrem — ZT-703S";
            hdrSub.Font = new Font("Segoe UI", 10F);
            hdrSub.ForeColor = Color.Gray;
            hdrSub.Location = new Point(15, 35);
            hdrSub.AutoSize = true;
            hdr.Controls.Add(hdrSub);
            guideScroll.Controls.Add(hdr);
            gy += 75;

            gy = AddGuideCard(guideScroll, gx, gy, gw,
                "BEZPIECZENSTWO", Color.FromArgb(128, 0, 0), Color.LightCoral,
                new[] {
                    "Odłącz zasilanie urządzenia (wyjmij baterię!)",
                    "Rozładuj kondensatory (zwieranie pinów pęsetą)",
                    "Wyjmij mierzony element z płyty — pomiar w układzie daje błędne wyniki!",
                    "Nigdy nie mierz elementów pod napięciem!"
                });

            gy = AddGuideCard(guideScroll, gx, gy, gw,
                "ROZPOZNAWANIE ELEMENTU", Color.FromArgb(0, 64, 64), Color.Cyan,
                new[] {
                    "NPN     — kod na korpusie zaczyna się od '1'  (np. 1A, 1AM)",
                    "PNP     — kod zaczyna się od '2'  (np. 2A, 2AM)",
                    "MOSFET N — kod zaczyna się od '7'  (np. 702, 7002)",
                    "MOSFET P — kod zaczyna się od '8'  (np. 802, 8002)",
                    "Zawsze sprawdzaj datasheet po oznaczeniu na korpusie!"
                });

            gy = AddGuideCard(guideScroll, gx, gy, gw,
                "POMIAR BJT — NPN", Color.FromArgb(0, 80, 0), Color.LightGreen,
                new[] {
                    "Ustaw multimetr na DIODE TEST (symbol diody)",
                    "CZERWONA sonda na BAZĘ (B), CZARNA na EMITER (E) → 0.5–0.8V = OK",
                    "CZERWONA sonda na BAZĘ (B), CZARNA na KOLEKTOR (C) → 0.5–0.8V = OK",
                    "Odwróć sondy (czarna na B) → powinno być OL (brak przewodzenia)",
                    "Pomiar E↔C w obie strony = OL (brak zwarcia)"
                });

            gy = AddGuideCard(guideScroll, gx, gy, gw,
                "POMIAR BJT — PNP", Color.FromArgb(80, 64, 0), Color.Yellow,
                new[] {
                    "Ustaw multimetr na DIODE TEST",
                    "CZARNA sonda na BAZĘ (B), CZERWONA na EMITER (E) → 0.5–0.8V = OK",
                    "CZARNA sonda na BAZĘ (B), CZERWONA na KOLEKTOR (C) → 0.5–0.8V = OK",
                    "Odwróć sondy (czerwona na B) → powinno być OL",
                    "Pomiar E↔C w obie strony = OL"
                });

            gy = AddGuideCard(guideScroll, gx, gy, gw,
                "POMIAR MOSFET N-CH", Color.FromArgb(0, 48, 80), Color.Cyan,
                new[] {
                    "Ustaw multimetr na DIODE TEST",
                    "CZERWONA na SOURCE (S), CZARNA na DRAIN (D) → 0.4–0.9V (dioda wewnętrzna) = OK",
                    "CZERWONA na DRAIN (D), CZARNA na SOURCE (S) → OL = OK",
                    "GATE jest izolowany — OL w obie strony (G↔D, G↔S)",
                    "Przewodzenie G↔D lub G↔S = USZKODZONY"
                });

            gy = AddGuideCard(guideScroll, gx, gy, gw,
                "POMIAR MOSFET P-CH", Color.FromArgb(64, 0, 80), Color.Violet,
                new[] {
                    "Ustaw multimetr na DIODE TEST",
                    "CZARNA na SOURCE (S), CZERWONA na DRAIN (D) → 0.4–0.9V = OK",
                    "CZARNA na DRAIN (D), CZERWONA na SOURCE (S) → OL = OK",
                    "GATE izolowany — OL w obie strony",
                    "Przewodzenie G↔D lub G↔S = USZKODZONY"
                });

            gy = AddGuideCard(guideScroll, gx, gy, gw,
                "INTERPRETACJA WYNIKÓW", Color.FromArgb(48, 48, 48), Color.White,
                new[] {
                    "0.400V – 0.900V  →  element SPRAWNY",
                    "0.000V (zwarcie)  →  element USZKODZONY",
                    "OL wszędzie       →  element PRZERWA (uszkodzony)",
                    "Różne wyniki w różne strony → element MOŻE BYĆ SPRAWNY (sprawdź datasheet)"
                });

            gy = AddGuideCard(guideScroll, gx, gy, gw,
                "WAZNE UWAGI", Color.FromArgb(128, 80, 0), Color.Orange,
                new[] {
                    "Multimetr musi mieć sprawne baterie!",
                    "Sondy muszą dobrze kontaktować z pinami",
                    "Oczyść piny izopropanolem przed pomiarem",
                    "Niektóre tranzystory mają inny pinout — zawsze sprawdzaj datasheet!",
                    "Pomiar w układzie (bez wylutowania) może dać błędne wyniki"
                });

            Panel footer = CreateCard(gw, 35, Color.FromArgb(34, 34, 34));
            footer.Location = new Point(gx, gy);
            Label ftLbl = new Label();
            ftLbl.Text = "KRIS® ZT-703S — Poradnik pomiaru tranzystorów SMD";
            ftLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            ftLbl.ForeColor = Color.Gray;
            ftLbl.Location = new Point(15, 8);
            ftLbl.AutoSize = true;
            footer.Controls.Add(ftLbl);
            guideScroll.Controls.Add(footer);

            TabPage tabDiag = new TabPage("DIAGRAMY / NOŻKI");
            tabDiag.BackColor = Color.FromArgb(24, 24, 24);

            TabPage tabDatasheet = new TabPage("DATASHEET / WYSZUKAJ");
            tabDatasheet.BackColor = Color.FromArgb(24, 24, 24);

            Panel diagPanel = new Panel();
            diagPanel.AutoScroll = true;
            diagPanel.Dock = DockStyle.Fill;
            diagPanel.BackColor = Color.FromArgb(24, 24, 24);

            int y = 10;
            int rowH = 260;
            int imgW = 380;
            int imgH = 240;

            PictureBox npnPb = new PictureBox();
            npnPb.Location = new Point(10, y);
            npnPb.Size = new Size(imgW, imgH);
            npnPb.BackColor = Color.FromArgb(13, 13, 13);
            npnPb.BorderStyle = BorderStyle.FixedSingle;
            npnPb.Image = DrawBJTNPN();
            npnPb.SizeMode = PictureBoxSizeMode.Zoom;
            diagPanel.Controls.Add(npnPb);

            Label npnLbl = new Label();
            npnLbl.Location = new Point(10, y + imgH + 2);
            npnLbl.Size = new Size(imgW, 16);
            npnLbl.Text = "NPN — czerwona na B, czarna na E/C -> 0.5-0.8V";
            npnLbl.ForeColor = Color.Yellow;
            npnLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            npnLbl.TextAlign = ContentAlignment.MiddleCenter;
            diagPanel.Controls.Add(npnLbl);

            PictureBox pnpPb = new PictureBox();
            pnpPb.Location = new Point(imgW + 20, y);
            pnpPb.Size = new Size(imgW, imgH);
            pnpPb.BackColor = Color.FromArgb(13, 13, 13);
            pnpPb.BorderStyle = BorderStyle.FixedSingle;
            pnpPb.Image = DrawBJTPNP();
            pnpPb.SizeMode = PictureBoxSizeMode.Zoom;
            diagPanel.Controls.Add(pnpPb);

            Label pnpLbl = new Label();
            pnpLbl.Location = new Point(imgW + 20, y + imgH + 2);
            pnpLbl.Size = new Size(imgW, 16);
            pnpLbl.Text = "PNP — czarna na B, czerwona na E/C -> 0.5-0.8V";
            pnpLbl.ForeColor = Color.Yellow;
            pnpLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pnpLbl.TextAlign = ContentAlignment.MiddleCenter;
            diagPanel.Controls.Add(pnpLbl);

            y += rowH;

            PictureBox nMosPb = new PictureBox();
            nMosPb.Location = new Point(10, y);
            nMosPb.Size = new Size(imgW, imgH);
            nMosPb.BackColor = Color.FromArgb(13, 13, 13);
            nMosPb.BorderStyle = BorderStyle.FixedSingle;
            nMosPb.Image = DrawMOSFETN();
            nMosPb.SizeMode = PictureBoxSizeMode.Zoom;
            diagPanel.Controls.Add(nMosPb);

            Label nMosLbl = new Label();
            nMosLbl.Location = new Point(10, y + imgH + 2);
            nMosLbl.Size = new Size(imgW, 16);
            nMosLbl.Text = "MOSFET N — czerwona na S, czarna na D -> 0.4-0.9V";
            nMosLbl.ForeColor = Color.Cyan;
            nMosLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            nMosLbl.TextAlign = ContentAlignment.MiddleCenter;
            diagPanel.Controls.Add(nMosLbl);

            PictureBox pMosPb = new PictureBox();
            pMosPb.Location = new Point(imgW + 20, y);
            pMosPb.Size = new Size(imgW, imgH);
            pMosPb.BackColor = Color.FromArgb(13, 13, 13);
            pMosPb.BorderStyle = BorderStyle.FixedSingle;
            pMosPb.Image = DrawMOSFETP();
            pMosPb.SizeMode = PictureBoxSizeMode.Zoom;
            diagPanel.Controls.Add(pMosPb);

            Label pMosLbl = new Label();
            pMosLbl.Location = new Point(imgW + 20, y + imgH + 2);
            pMosLbl.Size = new Size(imgW, 16);
            pMosLbl.Text = "MOSFET P — czarna na S, czerwona na D -> 0.4-0.9V";
            pMosLbl.ForeColor = Color.Cyan;
            pMosLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pMosLbl.TextAlign = ContentAlignment.MiddleCenter;
            diagPanel.Controls.Add(pMosLbl);

            y += rowH;

            PictureBox measPb = new PictureBox();
            measPb.Location = new Point(10, y);
            measPb.Size = new Size(imgW * 2 + 10, imgH);
            measPb.BackColor = Color.FromArgb(13, 13, 13);
            measPb.BorderStyle = BorderStyle.FixedSingle;
            measPb.Image = DrawMeasurementGuide();
            measPb.SizeMode = PictureBoxSizeMode.Zoom;
            diagPanel.Controls.Add(measPb);

            Label measLbl = new Label();
            measLbl.Location = new Point(10, y + imgH + 2);
            measLbl.Size = new Size(imgW * 2 + 10, 16);
            measLbl.Text = "SCHEMAT POMIARU — multimetr w trybie diode test, element WYLUROWANY";
            measLbl.ForeColor = Color.LightGreen;
            measLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            measLbl.TextAlign = ContentAlignment.MiddleCenter;
            diagPanel.Controls.Add(measLbl);

            y += rowH;

            PictureBox smdPb = new PictureBox();
            smdPb.Location = new Point(10, y);
            smdPb.Size = new Size(imgW * 2 + 10, imgH);
            smdPb.BackColor = Color.FromArgb(13, 13, 13);
            smdPb.BorderStyle = BorderStyle.FixedSingle;
            smdPb.Image = DrawSMDPackages();
            smdPb.SizeMode = PictureBoxSizeMode.Zoom;
            diagPanel.Controls.Add(smdPb);

            Label smdLbl = new Label();
            smdLbl.Location = new Point(10, y + imgH + 2);
            smdLbl.Size = new Size(imgW * 2 + 10, 16);
            smdLbl.Text = "POPULARNE OBUDOWY SMD — SOT-23, SOT-323, SOT-89 — pinout od gory (kropka/pinek 1)";
            smdLbl.ForeColor = Color.LightGreen;
            smdLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            smdLbl.TextAlign = ContentAlignment.MiddleCenter;
            diagPanel.Controls.Add(smdLbl);

            diagPanel.Size = new Size(imgW * 2 + 30, y + rowH + 30);

            tabDiag.Controls.Add(diagPanel);

            tabCtrl.Controls.Add(tabText);
            tabCtrl.Controls.Add(tabDiag);
            tabCtrl.Controls.Add(tabDatasheet);

            tabDatasheet.BackColor = Color.FromArgb(13, 13, 13);

            Panel dsPanel = new Panel();
            dsPanel.Dock = DockStyle.Fill;
            dsPanel.BackColor = Color.FromArgb(13, 13, 13);

            Panel dsSearch = new Panel();
            dsSearch.Dock = DockStyle.Top;
            dsSearch.Height = 55;
            dsSearch.BackColor = Color.FromArgb(34, 34, 34);
            dsSearch.Padding = new Padding(10, 8, 10, 8);

            Label dsLbl = new Label();
            dsLbl.Text = "Wpisz kod elementu (np. MMBT3904, 2N7002, AO3401):";
            dsLbl.ForeColor = Color.Yellow;
            dsLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dsLbl.Location = new Point(10, 4);
            dsLbl.AutoSize = true;
            dsSearch.Controls.Add(dsLbl);

            TextBox dsInput = new TextBox();
            dsInput.Location = new Point(10, 24);
            dsInput.Size = new Size(250, 25);
            dsInput.Font = new Font("Consolas", 11F, FontStyle.Bold);
            dsInput.BackColor = Color.FromArgb(13, 13, 13);
            dsInput.ForeColor = Color.LightGreen;
            dsInput.BorderStyle = BorderStyle.FixedSingle;
            dsSearch.Controls.Add(dsInput);

            ComboBox cbDsEngine = new ComboBox();
            cbDsEngine.Location = new Point(270, 24);
            cbDsEngine.Size = new Size(150, 25);
            cbDsEngine.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cbDsEngine.BackColor = Color.FromArgb(13, 13, 13);
            cbDsEngine.ForeColor = Color.Cyan;
            cbDsEngine.DropDownStyle = ComboBoxStyle.DropDownList;
            cbDsEngine.FlatStyle = FlatStyle.Flat;
            cbDsEngine.Items.Add("Google");
            cbDsEngine.Items.Add("AllDatasheet");
            cbDsEngine.Items.Add("Octopart");
            cbDsEngine.Items.Add("LCSC");
            cbDsEngine.Items.Add("FindChips");
            cbDsEngine.Items.Add("DigiKey");
            cbDsEngine.Items.Add("Mouser");
            cbDsEngine.Items.Add("SnapEDA");
            cbDsEngine.Items.Add("AllTransistors");
            string savedEngine = Properties.Settings.Default.ds_search_engine;
            if (!string.IsNullOrEmpty(savedEngine) && cbDsEngine.Items.Contains(savedEngine))
                cbDsEngine.SelectedItem = savedEngine;
            else
                cbDsEngine.SelectedIndex = 0;
            dsSearch.Controls.Add(cbDsEngine);

            Button dsBtn = new Button();
            dsBtn.Text = "SZUKAJ";
            dsBtn.Location = new Point(430, 23);
            dsBtn.Size = new Size(95, 27);
            dsBtn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dsBtn.BackColor = Color.FromArgb(0, 64, 64);
            dsBtn.ForeColor = Color.Cyan;
            dsBtn.Cursor = Cursors.Hand;
            dsBtn.FlatStyle = FlatStyle.Flat;
            dsBtn.FlatAppearance.BorderSize = 0;
            dsSearch.Controls.Add(dsBtn);

            Label dsHint = new Label();
            dsHint.Text = "Enter = otworz w przegladarce";
            dsHint.ForeColor = Color.Gray;
            dsHint.Font = new Font("Segoe UI", 8F);
            dsHint.Location = new Point(540, 28);
            dsHint.AutoSize = true;
            dsSearch.Controls.Add(dsHint);

            Panel dsInfo = new Panel();
            dsInfo.Dock = DockStyle.Fill;
            dsInfo.BackColor = Color.FromArgb(13, 13, 13);

            Label dsInfoIcon = new Label();
            dsInfoIcon.Text = "SZUKAJ";
            dsInfoIcon.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            dsInfoIcon.ForeColor = Color.FromArgb(60, 60, 60);
            dsInfoIcon.TextAlign = ContentAlignment.MiddleCenter;
            dsInfoIcon.Dock = DockStyle.Top;
            dsInfoIcon.Height = 50;
            dsInfo.Controls.Add(dsInfoIcon);

            Label dsInfoText = new Label();
            dsInfoText.Text = "Wpisz kod elementu i nacisnij SZUKAJ lub Enter.\nWyniki otwarta sie w domyslnej przegladarce.";
            dsInfoText.Font = new Font("Segoe UI", 11F);
            dsInfoText.ForeColor = Color.FromArgb(80, 80, 80);
            dsInfoText.TextAlign = ContentAlignment.MiddleCenter;
            dsInfoText.Dock = DockStyle.Top;
            dsInfoText.Height = 45;
            dsInfoText.AutoSize = false;
            dsInfo.Controls.Add(dsInfoText);

            void DoSearch()
            {
                string q = dsInput.Text.Trim();
                if (string.IsNullOrEmpty(q)) return;

                string engine = cbDsEngine.SelectedItem.ToString();
                string url = "";

                switch (engine)
                {
                    case "Google":
                        url = $"https://www.google.com/search?q={Uri.EscapeDataString(q + " datasheet pdf")}";
                        break;
                    case "AllDatasheet":
                        url = $"https://www.alldatasheet.com/view.jsp?Searchword={Uri.EscapeDataString(q)}";
                        break;
                    case "Octopart":
                        url = $"https://octopart.com/search?q={Uri.EscapeDataString(q)}";
                        break;
                    case "LCSC":
                        url = $"https://lcsc.com/search?q={Uri.EscapeDataString(q)}";
                        break;
                    case "FindChips":
                        url = $"https://www.findchips.com/search/{Uri.EscapeDataString(q)}";
                        break;
                    case "DigiKey":
                        url = $"https://www.digikey.pl/en/products/result?keywords={Uri.EscapeDataString(q)}";
                        break;
                    case "Mouser":
                        url = $"https://www.mouser.pl/c/?q={Uri.EscapeDataString(q)}";
                        break;
                    case "SnapEDA":
                        url = $"https://www.snapeda.com/search?q={Uri.EscapeDataString(q)}";
                        break;
                    case "AllTransistors":
                        url = $"https://alltransistors.com/search.php?search={Uri.EscapeDataString(q)}";
                        break;
                }

                Properties.Settings.Default.ds_search_engine = engine;
                Properties.Settings.Default.Save();

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }

            dsBtn.Click += (s, e) => DoSearch();

            dsInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    DoSearch();
            };

            dsPanel.Controls.Add(dsInfo);
            dsPanel.Controls.Add(dsSearch);

            tabDatasheet.Controls.Add(dsPanel);

            Button btnClose = new Button();
            btnClose.Text = "ZAMKNIJ";
            btnClose.BackColor = Color.FromArgb(64, 0, 0);
            btnClose.ForeColor = Color.LightCoral;
            btnClose.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnClose.Dock = DockStyle.Bottom;
            btnClose.Height = 40;
            btnClose.Click += (s, ev) => infoForm.Close();

            infoForm.Controls.Add(tabCtrl);
            infoForm.Controls.Add(btnClose);
            infoForm.ShowDialog(this);
        }

        private void InitializeShortcuts()
        {
            shortcutManager = new ShortcutManager();

            shortcutManager.RegisterAction(ShortcutManager.ActionId.ToggleStandardPanel, () =>
            {
                chbStandardPanel.Checked = !chbStandardPanel.Checked;
            });

            shortcutManager.RegisterAction(ShortcutManager.ActionId.ToggleAdvancedPanel, () =>
            {
                chbAdvancedPanel.Checked = !chbAdvancedPanel.Checked;
            });

            shortcutManager.RegisterAction(ShortcutManager.ActionId.ClearLog, () =>
            {
                tbComOutput.Text = "";
            });

            shortcutManager.RegisterAction(ShortcutManager.ActionId.SaveCSV, () =>
            {
                btnSaveLog.PerformClick();
            });

            shortcutManager.RegisterAction(ShortcutManager.ActionId.ToggleChartPause, () =>
            {
                if (tabControl1.SelectedTab == tabPageWYKRES)
                {
                    btnChartPause.PerformClick();
                }
            });

            shortcutManager.RegisterAction(ShortcutManager.ActionId.ClearChart, () =>
            {
                if (tabControl1.SelectedTab == tabPageWYKRES)
                {
                    btnChartClear.PerformClick();
                }
            });

            shortcutManager.RegisterAction(ShortcutManager.ActionId.MinimizeWindow, () =>
            {
                this.WindowState = FormWindowState.Minimized;
            });

            shortcutManager.RegisterAction(ShortcutManager.ActionId.ToggleTTS, () =>
            {
                chbTTSSwitch.Checked = !chbTTSSwitch.Checked;
            });

            shortcutManager.RegisterAction(ShortcutManager.ActionId.CycleTimeWindow, () =>
            {
                if (tabControl1.SelectedTab == tabPageWYKRES)
                {
                    Button[] timeButtons = { btnTime10s, btnTime30s, btnTime60s, btnTime5min };
                    int currentIndex = Array.FindIndex(timeButtons, b => b.ForeColor == Color.White || b.ForeColor == Color.LightGreen);
                    if (currentIndex < 0) currentIndex = 2;
                    int nextIndex = (currentIndex + 1) % timeButtons.Length;
                    btnTimeWindow_Click(timeButtons[nextIndex], EventArgs.Empty);
                }
            });

            shortcutManager.RegisterAction(ShortcutManager.ActionId.OpenShortcutConfig, () =>
            {
                var configForm = new ShortcutConfigForm(shortcutManager);
                if (configForm.ShowDialog(this) == DialogResult.OK)
                {
                }
            });
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (shortcutManager != null && shortcutManager.ProcessKey(keyData))
            {
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async Task CheckForUpdateAsync()
        {
            try
            {
                string url = "https://raw.githubusercontent.com/pmbb81-wq/ZOYI-ZT703S/main/aktualizacja.md";
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                // pierwsze zapytanie (odrzucamy – omija cache)
                await http.GetStringAsync(url);
                await Task.Delay(1000);
                // drugie zapytanie (bierzemy pod uwagę)
                string body = await http.GetStringAsync(url);
                string firstLine = body.Trim().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                string remoteVersion = firstLine.TrimStart('v', 'V').Trim();

                var asmVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string currentVersion = $"{asmVersion.Major}.{asmVersion.Minor}";

                if (!Version.TryParse(remoteVersion, out var rv) || !Version.TryParse(currentVersion, out var cv))
                    return;
                if (rv <= cv) return;

                var dlg = new Form
                {
                    Text = "Aktualizacja",
                    Size = new Size(400, 180),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = Color.FromArgb(30, 30, 30),
                    ForeColor = Color.White
                };

                var lbl = new Label
                {
                    Text = $"Dostępna jest nowa wersja {remoteVersion}!",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.Cyan,
                    BackColor = Color.FromArgb(30, 30, 30),
                    Location = new Point(10, 20),
                    Width = 364,
                    Height = 40
                };

                var btnPobierz = new Button
                {
                    Text = "POBIERZ",
                    Location = new Point(10, 90),
                    Width = 110,
                    Height = 35,
                    BackColor = Color.FromArgb(0, 64, 64),
                    ForeColor = Color.Cyan,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat
                };
                btnPobierz.Click += (_, _) =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://github.com/pmbb81-wq/ZOYI-ZT703S/releases",
                        UseShellExecute = true
                    });
                    dlg.Close();
                };

                var btnSprawdzPozniej = new Button
                {
                    Text = "Sprawdź później",
                    Location = new Point(140, 90),
                    Width = 110,
                    Height = 35,
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel
                };

                var btnAnuluj = new Button
                {
                    Text = "Anuluj",
                    Location = new Point(270, 90),
                    Width = 110,
                    Height = 35,
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel
                };

                dlg.Controls.Add(lbl);
                dlg.Controls.Add(btnPobierz);
                dlg.Controls.Add(btnSprawdzPozniej);
                dlg.Controls.Add(btnAnuluj);
                dlg.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd sprawdzania aktualizacji:\n{ex.Message}", "Update Error");
            }
        }

        private Bitmap DrawBJTNPN()
        {
            Bitmap bmp = new Bitmap(380, 260);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(13, 13, 13));

                using (Font fTitle = new Font("Segoe UI", 11F, FontStyle.Bold))
                using (Font f = new Font("Segoe UI", 10F, FontStyle.Bold))
                using (Font fSmall = new Font("Segoe UI", 9F))
                using (Font fPin = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (Pen penCyan = new Pen(Color.Cyan, 2))
                using (Pen penThin = new Pen(Color.FromArgb(80, 80, 80), 1))
                using (Pen penYellow = new Pen(Color.Yellow, 2))
                using (SolidBrush brushCyan = new SolidBrush(Color.Cyan))
                using (SolidBrush brushYellow = new SolidBrush(Color.Yellow))
                using (SolidBrush brushWhite = new SolidBrush(Color.White))
                using (SolidBrush brushGray = new SolidBrush(Color.Gray))
                {
                    g.DrawString("NPN Transistor — SOT-23 (MMBT3904)", fTitle, brushCyan, 10, 5);

                    g.DrawLine(penThin, 10, 28, 370, 28);

                    int cx = 150, cy = 130;

                    g.DrawEllipse(penCyan, cx - 30, cy - 30, 60, 60);

                    g.DrawLine(penCyan, cx - 30, cy, cx - 8, cy);
                    g.DrawLine(penCyan, cx - 8, cy - 16, cx - 8, cy + 16);

                    g.DrawLine(penCyan, cx - 8, cy - 7, cx + 16, cy - 22);
                    g.DrawLine(penCyan, cx - 8, cy + 7, cx + 16, cy + 22);

                    g.DrawLine(penCyan, cx + 16, cy - 22, cx + 35, cy - 22);
                    g.DrawLine(penCyan, cx + 16, cy + 22, cx + 35, cy + 22);
                    g.DrawLine(penCyan, cx - 30, cy, cx - 50, cy);

                    Pen arrowPen = new Pen(Color.Yellow, 2);
                    arrowPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);
                    g.DrawLine(arrowPen, cx - 8, cy + 7, cx + 13, cy + 20);
                    arrowPen.Dispose();

                    g.DrawString("B", f, brushWhite, cx - 70, cy - 8);
                    g.DrawString("C", f, brushCyan, cx + 37, cy - 32);
                    g.DrawString("E", f, brushYellow, cx + 37, cy + 18);

                    int px = 280, py = 100;

                    g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), px - 20, py - 15, 40, 30);
                    g.DrawRectangle(penCyan, px - 20, py - 15, 40, 30);

                    g.FillEllipse(new SolidBrush(Color.Yellow), px - 18, py - 13, 4, 4);

                    g.DrawLine(penCyan, px - 20, py + 15, px - 20, py + 30);
                    g.DrawLine(penCyan, px, py + 15, px, py + 30);
                    g.DrawLine(penCyan, px + 20, py + 15, px + 20, py + 30);

                    g.DrawString("1", fPin, brushCyan, px - 28, py + 32);
                    g.DrawString("2", fPin, brushCyan, px - 4, py + 32);
                    g.DrawString("3", fPin, brushCyan, px + 18, py + 32);

                    g.DrawString("TOP VIEW", fSmall, brushGray, px - 22, py - 30);

                    g.DrawString("Pin 1 = Base     Pin 2 = Emitter     Pin 3 = Collector", fSmall, brushWhite, 10, 200);
                    g.DrawString("Kod: 1AM | Vceo=40V | Ic=200mA | fT=300MHz", fSmall, brushCyan, 10, 220);
                    g.DrawString("Pomiar: czerwona na B, czarna na E/C -> 0.5-0.8V = OK", fSmall, brushYellow, 10, 240);
                }
            }
            return bmp;
        }

        private Bitmap DrawBJTPNP()
        {
            Bitmap bmp = new Bitmap(380, 260);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(13, 13, 13));

                using (Font fTitle = new Font("Segoe UI", 11F, FontStyle.Bold))
                using (Font f = new Font("Segoe UI", 10F, FontStyle.Bold))
                using (Font fSmall = new Font("Segoe UI", 9F))
                using (Font fPin = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (Pen penCyan = new Pen(Color.Cyan, 2))
                using (Pen penThin = new Pen(Color.FromArgb(80, 80, 80), 1))
                using (Pen penYellow = new Pen(Color.Yellow, 2))
                using (SolidBrush brushCyan = new SolidBrush(Color.Cyan))
                using (SolidBrush brushYellow = new SolidBrush(Color.Yellow))
                using (SolidBrush brushWhite = new SolidBrush(Color.White))
                using (SolidBrush brushGray = new SolidBrush(Color.Gray))
                {
                    g.DrawString("PNP Transistor — SOT-23 (MMBT3906)", fTitle, brushCyan, 10, 5);

                    g.DrawLine(penThin, 10, 28, 370, 28);

                    int cx = 150, cy = 130;

                    g.DrawEllipse(penCyan, cx - 30, cy - 30, 60, 60);

                    g.DrawLine(penCyan, cx - 30, cy, cx - 8, cy);
                    g.DrawLine(penCyan, cx - 8, cy - 16, cx - 8, cy + 16);

                    g.DrawLine(penCyan, cx - 8, cy - 7, cx + 16, cy - 22);
                    g.DrawLine(penCyan, cx - 8, cy + 7, cx + 16, cy + 22);

                    g.DrawLine(penCyan, cx + 16, cy - 22, cx + 35, cy - 22);
                    g.DrawLine(penCyan, cx + 16, cy + 22, cx + 35, cy + 22);
                    g.DrawLine(penCyan, cx - 30, cy, cx - 50, cy);

                    Pen arrowPen = new Pen(Color.Yellow, 2);
                    arrowPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);
                    g.DrawLine(arrowPen, cx + 13, cy - 20, cx - 8, cy - 7);
                    arrowPen.Dispose();

                    g.DrawString("B", f, brushWhite, cx - 70, cy - 8);
                    g.DrawString("E", f, brushYellow, cx + 37, cy - 32);
                    g.DrawString("C", f, brushCyan, cx + 37, cy + 18);

                    int px = 280, py = 100;

                    g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), px - 20, py - 15, 40, 30);
                    g.DrawRectangle(penCyan, px - 20, py - 15, 40, 30);

                    g.FillEllipse(new SolidBrush(Color.Yellow), px - 18, py - 13, 4, 4);

                    g.DrawLine(penCyan, px - 20, py + 15, px - 20, py + 30);
                    g.DrawLine(penCyan, px, py + 15, px, py + 30);
                    g.DrawLine(penCyan, px + 20, py + 15, px + 20, py + 30);

                    g.DrawString("1", fPin, brushCyan, px - 28, py + 32);
                    g.DrawString("2", fPin, brushCyan, px - 4, py + 32);
                    g.DrawString("3", fPin, brushCyan, px + 18, py + 32);

                    g.DrawString("TOP VIEW", fSmall, brushGray, px - 22, py - 30);

                    g.DrawString("Pin 1 = Base     Pin 2 = Emitter     Pin 3 = Collector", fSmall, brushWhite, 10, 200);
                    g.DrawString("Kod: 2A | Vceo=40V | Ic=200mA | fT=250MHz", fSmall, brushCyan, 10, 220);
                    g.DrawString("Pomiar: czarna na B, czerwona na E/C -> 0.5-0.8V = OK", fSmall, brushYellow, 10, 240);
                }
            }
            return bmp;
        }

        private Bitmap DrawMOSFETN()
        {
            Bitmap bmp = new Bitmap(380, 260);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(13, 13, 13));

                using (Font fTitle = new Font("Segoe UI", 11F, FontStyle.Bold))
                using (Font f = new Font("Segoe UI", 10F, FontStyle.Bold))
                using (Font fSmall = new Font("Segoe UI", 9F))
                using (Font fPin = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (Pen penCyan = new Pen(Color.Cyan, 2))
                using (Pen penThin = new Pen(Color.FromArgb(80, 80, 80), 1))
                using (Pen penYellow = new Pen(Color.Yellow, 2))
                using (SolidBrush brushCyan = new SolidBrush(Color.Cyan))
                using (SolidBrush brushYellow = new SolidBrush(Color.Yellow))
                using (SolidBrush brushWhite = new SolidBrush(Color.White))
                using (SolidBrush brushGray = new SolidBrush(Color.Gray))
                {
                    g.DrawString("N-CH MOSFET — SOT-23 (2N7002)", fTitle, brushCyan, 10, 5);

                    g.DrawLine(penThin, 10, 28, 370, 28);

                    int cx = 150, cy = 130;

                    g.DrawEllipse(penCyan, cx - 30, cy - 30, 60, 60);

                    g.DrawLine(penCyan, cx - 50, cy - 18, cx - 12, cy - 18);
                    g.DrawLine(penCyan, cx - 12, cy - 28, cx - 12, cy - 8);

                    g.DrawLine(penCyan, cx - 50, cy + 18, cx - 12, cy + 18);
                    g.DrawLine(penCyan, cx - 12, cy + 8, cx - 12, cy + 28);

                    g.DrawLine(penCyan, cx - 12, cy - 4, cx - 12, cy + 4);

                    g.DrawLine(penCyan, cx + 4, cy - 22, cx + 4, cy + 22);

                    g.DrawLine(penCyan, cx - 12, cy - 18, cx + 4, cy - 22);
                    g.DrawLine(penCyan, cx - 12, cy + 18, cx + 4, cy + 22);

                    g.DrawLine(penCyan, cx + 4, cy - 22, cx + 35, cy - 22);
                    g.DrawLine(penCyan, cx + 4, cy + 22, cx + 35, cy + 22);
                    g.DrawLine(penCyan, cx - 50, cy - 18, cx - 65, cy - 18);

                    Pen arrowPen = new Pen(Color.Yellow, 2);
                    arrowPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);
                    g.DrawLine(arrowPen, cx - 12, cy + 18, cx + 2, cy + 20);
                    arrowPen.Dispose();

                    g.DrawLine(penThin, cx + 4, cy + 22, cx + 14, cy + 22);
                    g.DrawLine(penThin, cx + 14, cy + 14, cx + 14, cy + 30);
                    g.DrawLine(penThin, cx + 9, cy + 30, cx + 19, cy + 30);

                    g.DrawString("G", f, brushWhite, cx - 80, cy - 25);
                    g.DrawString("D", f, brushCyan, cx + 37, cy - 32);
                    g.DrawString("S", f, brushYellow, cx + 37, cy + 18);

                    int px = 280, py = 100;

                    g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), px - 20, py - 15, 40, 30);
                    g.DrawRectangle(penCyan, px - 20, py - 15, 40, 30);

                    g.FillEllipse(new SolidBrush(Color.Yellow), px - 18, py - 13, 4, 4);

                    g.DrawLine(penCyan, px - 20, py + 15, px - 20, py + 30);
                    g.DrawLine(penCyan, px, py + 15, px, py + 30);
                    g.DrawLine(penCyan, px + 20, py + 15, px + 20, py + 30);

                    g.DrawString("1", fPin, brushCyan, px - 28, py + 32);
                    g.DrawString("2", fPin, brushCyan, px - 4, py + 32);
                    g.DrawString("3", fPin, brushCyan, px + 18, py + 32);

                    g.DrawString("TOP VIEW", fSmall, brushGray, px - 22, py - 30);

                    g.DrawString("Pin 1 = Gate     Pin 2 = Source     Pin 3 = Drain", fSmall, brushWhite, 10, 200);
                    g.DrawString("Kod: 702 | Vds=60V | Id=300mA | Rds(on)=1.8Ω", fSmall, brushCyan, 10, 220);
                    g.DrawString("Pomiar: czerwona na S, czarna na D -> 0.4-0.9V = OK", fSmall, brushYellow, 10, 240);
                }
            }
            return bmp;
        }

        private Bitmap DrawMOSFETP()
        {
            Bitmap bmp = new Bitmap(380, 260);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(13, 13, 13));

                using (Font fTitle = new Font("Segoe UI", 11F, FontStyle.Bold))
                using (Font f = new Font("Segoe UI", 10F, FontStyle.Bold))
                using (Font fSmall = new Font("Segoe UI", 9F))
                using (Font fPin = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (Pen penCyan = new Pen(Color.Cyan, 2))
                using (Pen penThin = new Pen(Color.FromArgb(80, 80, 80), 1))
                using (Pen penYellow = new Pen(Color.Yellow, 2))
                using (SolidBrush brushCyan = new SolidBrush(Color.Cyan))
                using (SolidBrush brushYellow = new SolidBrush(Color.Yellow))
                using (SolidBrush brushWhite = new SolidBrush(Color.White))
                using (SolidBrush brushGray = new SolidBrush(Color.Gray))
                {
                    g.DrawString("P-CH MOSFET — SOT-23 (AO3401)", fTitle, brushCyan, 10, 5);

                    g.DrawLine(penThin, 10, 28, 370, 28);

                    int cx = 150, cy = 130;

                    g.DrawEllipse(penCyan, cx - 30, cy - 30, 60, 60);

                    g.DrawLine(penCyan, cx - 50, cy - 18, cx - 12, cy - 18);
                    g.DrawLine(penCyan, cx - 12, cy - 28, cx - 12, cy - 8);

                    g.DrawLine(penCyan, cx - 50, cy + 18, cx - 12, cy + 18);
                    g.DrawLine(penCyan, cx - 12, cy + 8, cx - 12, cy + 28);

                    g.DrawLine(penCyan, cx - 12, cy - 4, cx - 12, cy + 4);

                    g.DrawLine(penCyan, cx + 4, cy - 22, cx + 4, cy + 22);

                    g.DrawLine(penCyan, cx - 12, cy - 18, cx + 4, cy - 22);
                    g.DrawLine(penCyan, cx - 12, cy + 18, cx + 4, cy + 22);

                    g.DrawLine(penCyan, cx + 4, cy - 22, cx + 35, cy - 22);
                    g.DrawLine(penCyan, cx + 4, cy + 22, cx + 35, cy + 22);
                    g.DrawLine(penCyan, cx - 50, cy - 18, cx - 65, cy - 18);

                    Pen arrowPen = new Pen(Color.Yellow, 2);
                    arrowPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5);
                    g.DrawLine(arrowPen, cx + 2, cy - 20, cx - 12, cy - 18);
                    arrowPen.Dispose();

                    g.DrawLine(penThin, cx + 4, cy - 22, cx + 14, cy - 22);
                    g.DrawLine(penThin, cx + 14, cy - 30, cx + 14, cy - 14);
                    g.DrawLine(penThin, cx + 9, cy - 30, cx + 19, cy - 30);

                    g.DrawString("G", f, brushWhite, cx - 80, cy - 25);
                    g.DrawString("S", f, brushYellow, cx + 37, cy - 32);
                    g.DrawString("D", f, brushCyan, cx + 37, cy + 18);

                    int px = 280, py = 100;

                    g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), px - 20, py - 15, 40, 30);
                    g.DrawRectangle(penCyan, px - 20, py - 15, 40, 30);

                    g.FillEllipse(new SolidBrush(Color.Yellow), px - 18, py - 13, 4, 4);

                    g.DrawLine(penCyan, px - 20, py + 15, px - 20, py + 30);
                    g.DrawLine(penCyan, px, py + 15, px, py + 30);
                    g.DrawLine(penCyan, px + 20, py + 15, px + 20, py + 30);

                    g.DrawString("1", fPin, brushCyan, px - 28, py + 32);
                    g.DrawString("2", fPin, brushCyan, px - 4, py + 32);
                    g.DrawString("3", fPin, brushCyan, px + 18, py + 32);

                    g.DrawString("TOP VIEW", fSmall, brushGray, px - 22, py - 30);

                    g.DrawString("Pin 1 = Gate     Pin 2 = Source     Pin 3 = Drain", fSmall, brushWhite, 10, 200);
                    g.DrawString("Kod: A1SHB | Vds=30V | Id=4A | Rds(on)=0.05Ω", fSmall, brushCyan, 10, 220);
                    g.DrawString("Pomiar: czarna na S, czerwona na D -> 0.4-0.9V = OK", fSmall, brushYellow, 10, 240);
                }
            }
            return bmp;
        }

        private Bitmap DrawMeasurementGuide()
        {
            Bitmap bmp = new Bitmap(770, 260);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(13, 13, 13));

                using (Font fTitle = new Font("Segoe UI", 12F, FontStyle.Bold))
                using (Font f = new Font("Segoe UI", 10F, FontStyle.Bold))
                using (Font fSmall = new Font("Segoe UI", 9F))
                using (Font fHeader = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (SolidBrush brushCyan = new SolidBrush(Color.Cyan))
                using (SolidBrush brushYellow = new SolidBrush(Color.Yellow))
                using (SolidBrush brushGreen = new SolidBrush(Color.LightGreen))
                using (SolidBrush brushRed = new SolidBrush(Color.LightCoral))
                using (SolidBrush brushWhite = new SolidBrush(Color.White))
                using (SolidBrush brushGray = new SolidBrush(Color.Gray))
                using (Pen penCyan = new Pen(Color.Cyan, 2))
                using (Pen penThin = new Pen(Color.FromArgb(80, 80, 80), 1))
                using (Pen penRed = new Pen(Color.Red, 2))
                using (Pen penBlue = new Pen(Color.Blue, 2))
                {
                    g.DrawString("PROCEDURA POMIARU — Multimetr + Tranzystor SMD", fTitle, brushCyan, 10, 5);

                    g.DrawLine(penThin, 10, 28, 760, 28);

                    int mx = 100, my = 120;

                    g.DrawRectangle(penCyan, mx - 40, my - 30, 80, 60);
                    g.DrawString("DMM", f, brushWhite, mx - 15, my - 8);
                    g.DrawString("DIODE", fSmall, brushCyan, mx - 18, my + 8);

                    g.DrawEllipse(penCyan, mx + 150, my - 25, 50, 50);
                    g.DrawString("SMD", f, brushWhite, mx + 158, my - 6);

                    g.DrawLine(penRed, mx + 40, my - 15, mx + 150, my - 15);
                    g.DrawLine(penBlue, mx + 40, my + 15, mx + 150, my + 15);

                    g.DrawString("CZER (+)", fSmall, brushRed, mx + 55, my - 28);
                    g.DrawString("CZAR (-)", fSmall, brushCyan, mx + 55, my + 22);

                    int sx = 350, sy = 45;

                    g.DrawString("KROK PO KROKU — POMIAR TRANZYSTORA", f, brushWhite, sx, sy);
                    sy += 22;
                    g.DrawLine(penThin, sx, sy, 750, sy);
                    sy += 8;

                    string[,] steps = {
                        { "1.", "Ustaw multimetr na DIODE TEST (symbol diody)" },
                        { "2.", "Wyjmij tranzystor z plyty (hot-air/lutownica)" },
                        { "3.", "Oczysc piny izopropanolem i szczoteczka" },
                        { "4.", "Przyloz sondy do wlasciwych nozek (patrz diagramy)" },
                        { "5.", "Odczytaj spadek napiecia na wyswietlaczu" },
                        { "", "" },
                        { "WYNIKI:", "" },
                        { "OK", "0.4V - 0.9V  ->  element SPRAWNY" },
                        { "X", "0.000V (zwarcie)  ->  element USZKODZONY" },
                        { "X", "OL wszedzie  ->  element PRZERWA (uszkodzony)" }
                    };

                    for (int i = 0; i < 10; i++)
                    {
                        SolidBrush c = brushWhite;
                        if (steps[i, 0] == "WYNIKI:") c = brushWhite;
                        else if (steps[i, 1].Contains("SPRAWNY")) c = brushGreen;
                        else if (steps[i, 1].Contains("USZKODZONY") || steps[i, 1].Contains("PRZERWA")) c = brushRed;

                        if (steps[i, 0] == "WYNIKI:")
                        {
                            g.DrawString(steps[i, 0], fHeader, brushWhite, sx, sy);
                        }
                        else
                        {
                            g.DrawString(steps[i, 0], fHeader, c, sx, sy);
                            g.DrawString(steps[i, 1], fSmall, c, sx + 30, sy);
                        }
                        sy += 16;
                    }

                    g.DrawString("UWAGA: Nigdy nie mierz elementow pod napieciem! Odłącz zasilanie i rozladuj kondensatory.", fSmall, brushRed, 10, 230);

                    g.DrawLine(penThin, 10, 248, 760, 248);
                    g.DrawString("KRIS ZT-703S — Pomiar tranzystorow SMD", fSmall, brushGray, 10, 250);
                }
            }
            return bmp;
        }

        private Bitmap DrawSMDPackages()
        {
            Bitmap bmp = new Bitmap(770, 260);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(13, 13, 13));

                using (Font fTitle = new Font("Segoe UI", 12F, FontStyle.Bold))
                using (Font f = new Font("Segoe UI", 10F, FontStyle.Bold))
                using (Font fSmall = new Font("Segoe UI", 9F))
                using (Font fPin = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (Font fDim = new Font("Segoe UI", 8F))
                using (SolidBrush brushCyan = new SolidBrush(Color.Cyan))
                using (SolidBrush brushYellow = new SolidBrush(Color.Yellow))
                using (SolidBrush brushWhite = new SolidBrush(Color.White))
                using (SolidBrush brushGray = new SolidBrush(Color.Gray))
                using (Pen penCyan = new Pen(Color.Cyan, 2))
                using (Pen penThin = new Pen(Color.FromArgb(80, 80, 80), 1))
                using (SolidBrush brushBody = new SolidBrush(Color.FromArgb(60, 60, 60)))
                using (SolidBrush brushPin = new SolidBrush(Color.FromArgb(150, 150, 150)))
                {
                    g.DrawString("OBUDOWY SMD — Widok od gory (kropka = pin 1)", fTitle, brushCyan, 10, 5);
                    g.DrawLine(penThin, 10, 28, 760, 28);

                    int colW = 240;
                    int[] colX = { 15, 265, 515 };
                    string[] names = { "SOT-23", "SOT-323 (SC-70)", "SOT-89" };

                    int[] bodyX = { 95, 345, 585 };
                    int[] bodyW = { 50, 44, 64 };
                    int[] bodyH = { 30, 25, 45 };
                    int bodyY = 55;

                    int[][] pinX = {
                        new[] { 102, 120, 138 },
                        new[] { 352, 367, 382 },
                        new[] { 595, 617, 639 }
                    };
                    int pinY = 92;

                    string[][] dims = {
                        new[] { "2.9 x 1.3 mm", "0.95 mm pitch" },
                        new[] { "2.0 x 1.25 mm", "0.65 mm pitch" },
                        new[] { "4.5 x 2.5 mm", "1.5 mm pitch" }
                    };
                    string[][] pinout = {
                        new[] { "1 = Base", "2 = Emitter", "3 = Collector" },
                        new[] { "1 = Base", "2 = Emitter", "3 = Collector" },
                        new[] { "1 = Base", "2 = Emitter", "3 = Collector" }
                    };
                    string[][] codes = {
                        new[] { "1AM = MMBT3904 NPN", "2A  = MMBT3906 PNP", "1P  = MMBT2222A NPN" },
                        new[] { "K2N = MMST3904 NPN", "K5N = MMST3906 PNP", "K3P = MMST2222A NPN" },
                        new[] { "702  = 2N7002  N-MOS", "7002 = 2N7002  N-MOS", "A1SH = AO3401 P-MOS" }
                    };

                    for (int t = 0; t < 3; t++)
                    {
                        int cx = colX[t] + colW / 2;

                        g.DrawString(names[t], f, brushCyan, colX[t] + 5, 35);

                        g.FillRectangle(brushBody, bodyX[t], bodyY, bodyW[t], bodyH[t]);
                        g.DrawRectangle(penCyan, bodyX[t], bodyY, bodyW[t], bodyH[t]);

                        g.FillEllipse(new SolidBrush(Color.Yellow), bodyX[t] + 4, bodyY + 4, 6, 6);

                        g.DrawLine(penThin, bodyX[t], bodyY + bodyH[t] + 3, bodyX[t] + bodyW[t], bodyY + bodyH[t] + 3);
                        g.DrawString(dims[t][0], fDim, brushGray, bodyX[t], bodyY + bodyH[t] + 5);
                        g.DrawString(dims[t][1], fDim, brushGray, bodyX[t], bodyY + bodyH[t] + 17);

                        for (int p = 0; p < 3; p++)
                        {
                            g.FillRectangle(brushPin, pinX[t][p] - 3, pinY, 6, 10);
                            g.DrawRectangle(penCyan, pinX[t][p] - 3, pinY, 6, 10);
                            g.DrawString((p + 1).ToString(), fPin, brushYellow, pinX[t][p] - 3, pinY + 13);
                        }

                        int py2 = pinY + 32;
                        for (int i = 0; i < 3; i++)
                        {
                            g.DrawString(pinout[t][i], fSmall, brushWhite, colX[t] + 5, py2);
                            py2 += 16;
                        }

                        py2 += 6;
                        for (int i = 0; i < 3; i++)
                        {
                            g.DrawString(codes[t][i], fSmall, brushCyan, colX[t] + 5, py2);
                            py2 += 14;
                        }
                    }

                    g.DrawLine(penThin, 10, 235, 760, 235);
                    g.DrawString("UWAGA: Pinout rozni sie w zaleznosci od producenta! Zawsze sprawdzaj datasheet po kodzie na obudowie.", fSmall, brushWhite, 10, 238);
                    g.DrawString("SOT-23 NPN: Pin1=Base, Pin2=Emitter, Pin3=Collector. Niektorzy producenci zamieniaja piny 2 i 3!", fSmall, brushCyan, 10, 250);
                }
            }
            return bmp;
        }

        private Panel CreateCard(int width, int height, Color accentColor)
        {
            Panel card = new Panel();
            card.Size = new Size(width, height);
            card.BackColor = Color.FromArgb(34, 34, 34);
            card.Paint += (s, e) =>
            {
                using (SolidBrush b = new SolidBrush(accentColor))
                {
                    e.Graphics.FillRectangle(b, 0, 0, 5, height);
                }
            };
            return card;
        }

        private int AddGuideCard(Panel parent, int x, int y, int width, string title, Color accentColor, Color titleColor, string[] items)
        {
            int itemH = 22;
            int headerH = 32;
            int pad = 15;
            int totalH = headerH + pad + items.Length * itemH + pad;

            Panel card = CreateCard(width, totalH, accentColor);
            card.Location = new Point(x, y);

            Label titleLbl = new Label();
            titleLbl.Text = title;
            titleLbl.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            titleLbl.ForeColor = titleColor;
            titleLbl.Location = new Point(12, 6);
            titleLbl.AutoSize = true;
            card.Controls.Add(titleLbl);

            for (int i = 0; i < items.Length; i++)
            {
                Label itemLbl = new Label();
                itemLbl.Font = new Font("Segoe UI", 9.5F);
                itemLbl.ForeColor = Color.FromArgb(220, 220, 220);
                itemLbl.Location = new Point(15, headerH + pad + i * itemH);
                itemLbl.Width = width - 30;
                itemLbl.Height = itemH;

                if (items[i].Contains("SPRAWNY") || items[i].Contains("OK"))
                    itemLbl.ForeColor = Color.LightGreen;
                else if (items[i].Contains("USZKODZONY") || items[i].Contains("PRZERWA"))
                    itemLbl.ForeColor = Color.LightCoral;

                itemLbl.Text = "▸ " + items[i];
                card.Controls.Add(itemLbl);
            }

            parent.Controls.Add(card);
            return y + totalH + 10;
        }

        private void tabDQ02_Click(object sender, EventArgs e)
        {

        }

        private void lblDQ02Prefix_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked) return;
            if (dq02Comx.isConnected())
            {
                try
                {
                    dq02Comx.write("VOLTage\n"); //VOLTage\n    :FUNC:LCR:R\n
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Port COM jest zamknięty!");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked) return;
            string commandToSend = measurementModes[currentModeIndex];
            if (dq02Comx.isConnected())
            {
                try
                {
                    dq02Comx.write("FUNCtion:IMPedance:MAIN\n"); //VOLTage\n    :FUNC:LCR:R\n
                    string cleanModeName = commandToSend.Replace("FUNCtion:", "").Replace("FUNC:", "");
                    button6.Text = $"Tryb: {cleanModeName}";
                    currentModeIndex = (currentModeIndex + 1) % measurementModes.Length;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Port COM jest zamknięty!");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked) return;
            if (dq02Comx.isConnected())
            {
                try
                {
                    dq02Comx.write("FUNCtion:IMPedance:SUB\n"); //VOLTage\n    FUNCtion:IMPedance:SUB\n
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Port COM jest zamknięty!");
            }


        }

        private void log_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}
