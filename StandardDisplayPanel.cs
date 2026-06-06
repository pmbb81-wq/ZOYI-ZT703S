using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZOYI
{
    public partial class StandardDisplayPanel : Form
    {
        // Window move
        bool bDispalyMouseDown = false;
        Point pDisplayMousePosDown = Point.Empty;
        Point pDisplayCurrentFormLocation = Point.Empty;

        // Window resize
        bool bResizeMouseDown = false;
        Point pResizeMousePosDown = Point.Empty;
        Size szCurrentFormSize = Size.Empty;
        Size szCurrentTableSize = Size.Empty;

        Font font_label;
        Font font_value;
        Font font_footer;
        Color color_label;
        Color color_value;
        Color color_footer;
        Color color_bg;

        CheckBox chbShowHide;
        System.Windows.Forms.Timer clockTimer;
        System.Windows.Forms.Timer scrollTimer;
        Panel pnlScroll;
        Label lblScroll;
        int scrollX = 0;
        string scrollText = "";

        public StandardDisplayPanel(CheckBox chb)
        {
            InitializeComponent();

            int xPos = Properties.Settings.Default.panel_std_form_pos_x;
            int yPos = Properties.Settings.Default.panel_std_form_pos_y;

            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // prevent display panel outside screen
            if (xPos > screenWidth || xPos < 0)
                xPos = screenWidth / 3;

            if (yPos > screenHeight || yPos < 0)
                yPos = screenHeight / 3;

            this.Location = new Point(xPos, yPos);

            //this.Location = new Point(Properties.Settings.Default.panel_std_form_pos_x, Properties.Settings.Default.panel_std_form_pos_y);

            font_label = Properties.Settings.Default.panel_std_label_font;
            font_value = Properties.Settings.Default.panel_std_value_font;
            font_footer = Properties.Settings.Default.panel_std_footer_font;
            color_label = ColorTranslator.FromHtml(Properties.Settings.Default.panel_std_label_color);
            color_value = ColorTranslator.FromHtml(Properties.Settings.Default.panel_std_value_color);
            color_footer = ColorTranslator.FromHtml(Properties.Settings.Default.panel_std_footer_color);
            color_bg = ColorTranslator.FromHtml(Properties.Settings.Default.panel_std_bg_color);

            lblLabel.Font = font_label;
            lblValue.Font = font_value;
            lblLabel.ForeColor = color_label;
            lblValue.ForeColor = color_value;
            lblFreq.Font = font_footer;
            lblMode.Font = font_footer;
            lblFreq.ForeColor = color_footer;
            lblMode.ForeColor = color_footer;
            this.BackColor = color_bg;

            chbShowHide = chb;

            dateTimePicker1.Value = DateTime.Now;

            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) => { dateTimePicker1.Value = DateTime.Now; };
            clockTimer.Start();

            string savedName = Properties.Settings.Default.panel_std_custom_name;
            if (!string.IsNullOrEmpty(savedName))
                label7.Text = savedName;

            string savedGG = Properties.Settings.Default.panel_std_custom_gg;
            if (!string.IsNullOrEmpty(savedGG))
                label8.Text = savedGG;

            string savedTitle = Properties.Settings.Default.panel_std_custom_title;
            if (!string.IsNullOrEmpty(savedTitle))
                scrollText = savedTitle;
            else
                scrollText = label6.Text;

            float savedFontSize = (float)Properties.Settings.Default.panel_std_title_font_size;
            if (savedFontSize <= 0) savedFontSize = 15.75f;

            pnlScroll = new Panel();
            pnlScroll.Size = new Size(578, 80);
            pnlScroll.BackColor = Color.FromArgb(34, 34, 34);
            pnlScroll.Cursor = Cursors.Hand;
            pnlScroll.Click += label6_Click;
            pnlScroll.Dock = DockStyle.Fill;

            lblScroll = new Label();
            lblScroll.AutoSize = true;
            lblScroll.Font = new Font("Segoe UI", savedFontSize, FontStyle.Bold, GraphicsUnit.Point, 238);
            lblScroll.ForeColor = Color.Yellow;
            lblScroll.BackColor = Color.FromArgb(34, 34, 34);
            lblScroll.Text = scrollText + "          ";
            lblScroll.Location = new Point(0, 0);
            pnlScroll.Controls.Add(lblScroll);

            label6.Visible = false;
            tableLayoutPanel1.Controls.Add(pnlScroll, 0, 0);

            scrollX = pnlScroll.Width;

            scrollTimer = new System.Windows.Forms.Timer();
            scrollTimer.Interval = 30;
            scrollTimer.Tick += ScrollTimer_Tick;
            scrollTimer.Start();

            this.Shown += (s, ev) => { scrollX = pnlScroll.Width; };

            SetupLabel1ContextMenu();
            SetupLabel2ContextMenu();
        }

        private void SetupLabel1ContextMenu()
        {
            if (label1 == null) return;

            if (Properties.Settings.Default.panel_std_dq02_label_font != null)
                label1.Font = Properties.Settings.Default.panel_std_dq02_label_font;
            try { label1.ForeColor = ColorTranslator.FromHtml(Properties.Settings.Default.panel_std_dq02_label_color); } catch { }

            var ctx = new ContextMenuStrip();
            ctx.Items.Add("Change Font...", null, (s, e) =>
            {
                using (var fd = new FontDialog())
                {
                    fd.Font = label1.Font;
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        label1.Font = fd.Font;
                        Properties.Settings.Default.panel_std_dq02_label_font = fd.Font;
                        Properties.Settings.Default.Save();
                    }
                }
            });
            ctx.Items.Add("Change Color...", null, (s, e) =>
            {
                using (var cd = new ColorDialog())
                {
                    cd.Color = label1.ForeColor;
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        label1.ForeColor = cd.Color;
                        Properties.Settings.Default.panel_std_dq02_label_color = ColorTranslator.ToHtml(cd.Color);
                        Properties.Settings.Default.Save();
                    }
                }
            });
            ctx.Items.Add("Reset", null, (s, e) =>
            {
                label1.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                label1.ForeColor = Color.White;
                Properties.Settings.Default.panel_std_dq02_label_font = null;
                Properties.Settings.Default.panel_std_dq02_label_color = "White";
                Properties.Settings.Default.Save();
            });
            label1.ContextMenuStrip = ctx;
        }

        public void SetDQ02Value(string prefix, string value, string secondary)
        {
            if (label1 != null)
                label1.Text = $"{prefix} {value}\n{secondary}";
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(scrollText) || lblScroll == null || pnlScroll == null) return;

            scrollX -= 2;
            if (scrollX < -lblScroll.Width)
                scrollX = pnlScroll.Width;

            lblScroll.Location = new Point(scrollX, 0);
        }

        // update label, value, suffix
        public void updatePanel(FrameDecoder frame_decoder)
        {
            lblLabel.Text = frame_decoder.Label;
            lblValue.Text = frame_decoder.Value + " " + frame_decoder.Unit;
            lblFreq.Text = frame_decoder.Freq + " " + frame_decoder.Freq_unit;
            lblMode.Text = frame_decoder.Mode1;
        }

        /*
         * 
         * Window move section
         * 
         */
        private void displayPanel_MouseDown(object sender, MouseEventArgs e)
        {
            bDispalyMouseDown = true;
            pDisplayMousePosDown = Control.MousePosition;
            pDisplayCurrentFormLocation = Location;
        }

        private void displayPanel_MouseUp(object sender, MouseEventArgs e)
        {
            bDispalyMouseDown = false;
            pDisplayMousePosDown = Point.Empty;
            pDisplayCurrentFormLocation = Point.Empty;
        }

        private void displayPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (bDispalyMouseDown)
            {
                var currentPos = Control.MousePosition;
                var distX = currentPos.X - pDisplayMousePosDown.X;
                var distY = currentPos.Y - pDisplayMousePosDown.Y;
                Location = new Point(pDisplayCurrentFormLocation.X + distX, pDisplayCurrentFormLocation.Y + distY);
            }
        }

        /*
         * 
         * Colors section
         * 
         */
        public void SetRidenData(string vout, string iout)
        {
            if (label2 != null)
                label2.Text = $"V: {vout}\nI: {iout}";
        }

        private void SetupLabel2ContextMenu()
        {
            if (label2 == null) return;

            if (Properties.Settings.Default.panel_std_riden_label_font != null)
                label2.Font = Properties.Settings.Default.panel_std_riden_label_font;
            try { label2.ForeColor = ColorTranslator.FromHtml(Properties.Settings.Default.panel_std_riden_label_color); } catch { }

            var ctx = new ContextMenuStrip();
            ctx.Items.Add("Change Font...", null, (s, e) =>
            {
                using (var fd = new FontDialog())
                {
                    fd.Font = label2.Font;
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        label2.Font = fd.Font;
                        Properties.Settings.Default.panel_std_riden_label_font = fd.Font;
                        Properties.Settings.Default.Save();
                    }
                }
            });
            ctx.Items.Add("Change Color...", null, (s, e) =>
            {
                using (var cd = new ColorDialog())
                {
                    cd.Color = label2.ForeColor;
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        label2.ForeColor = cd.Color;
                        Properties.Settings.Default.panel_std_riden_label_color = ColorTranslator.ToHtml(cd.Color);
                        Properties.Settings.Default.Save();
                    }
                }
            });
            ctx.Items.Add("Reset", null, (s, e) =>
            {
                label2.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                label2.ForeColor = Color.White;
                Properties.Settings.Default.panel_std_riden_label_font = null;
                Properties.Settings.Default.panel_std_riden_label_color = "White";
                Properties.Settings.Default.Save();
            });
            label2.ContextMenuStrip = ctx;
        }

        public void setBackgroundColor(Color color)
        {
            this.BackColor = color;
            Properties.Settings.Default.panel_std_bg_color = ColorTranslator.ToHtml(color);
            Properties.Settings.Default.Save();
        }

        public void setLabelFontColor(Color color)
        {
            lblLabel.ForeColor = color;
            Properties.Settings.Default.panel_std_label_color = ColorTranslator.ToHtml(color);
            Properties.Settings.Default.Save();
        }

        public void setValueFontColor(Color color)
        {
            lblValue.ForeColor = color;
            Properties.Settings.Default.panel_std_value_color = ColorTranslator.ToHtml(color); ;
            Properties.Settings.Default.Save();
        }

        public void changeOpacity(int val)
        {
            double opacity = val / 100.0;
            this.Opacity = opacity;
        }

        /*
         * 
         * Resize panel section
         * 
         */
        private void panelResize_MouseDown(object sender, MouseEventArgs e)
        {
            bResizeMouseDown = true;
            pResizeMousePosDown = Control.MousePosition;
            szCurrentFormSize = this.Size;
            szCurrentTableSize = tableLayoutPanel.Size;
        }

        private void panelResize_MouseUp(object sender, MouseEventArgs e)
        {
            bResizeMouseDown = false;
            pResizeMousePosDown = Point.Empty;
            szCurrentFormSize = Size.Empty;
            szCurrentTableSize = Size.Empty;
        }

        private void panelResize_MouseMove(object sender, MouseEventArgs e)
        {
            if (bResizeMouseDown)
            {
                var currentPos = Control.MousePosition;
                var distX = currentPos.X - pResizeMousePosDown.X;
                var distY = currentPos.Y - pResizeMousePosDown.Y;
                this.Size = new Size(szCurrentFormSize.Width + distX, szCurrentFormSize.Height + distY);
                tableLayoutPanel.Size = new Size(szCurrentTableSize.Width + distX, szCurrentTableSize.Height + distY);

                try
                {
                    Font labelOldFont = lblLabel.Font;
                    //float fontOldSize = labelOldFont.Size;
                    float fontNewSize = lblLabel.Height / 3;
                    Font labelNewFont = new Font(labelOldFont.FontFamily, fontNewSize, labelOldFont.Style);
                    lblLabel.Font = labelNewFont;
                    lblValue.Font = labelNewFont;
                }
                catch { }
            }
        }

        private void DisplayPanel_FormClosed(object sender, FormClosedEventArgs e)
        {
            clockTimer?.Stop();
            clockTimer?.Dispose();

            Properties.Settings.Default.panel_std_form_pos_x = this.Location.X;
            Properties.Settings.Default.panel_std_form_pos_y = this.Location.Y;
            Properties.Settings.Default.Save();
        }

        private void toolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (toolStripMenuItemLabel.Checked)
                lblLabel.Visible = true;
            else
                lblLabel.Visible = false;

            if (toolStripMenuItemValue.Checked)
                lblValue.Visible = true;
            else
                lblValue.Visible = false;

            if (toolStripMenuItemFooter.Checked)
            {
                lblFreq.Visible = true;
                lblMode.Visible = true;
            }
            else
            {
                lblFreq.Visible = false;
                lblMode.Visible = false;
            }
        }

        private void zamknijPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
            chbShowHide.Checked = false;
        }

        private void toolStripMenuItemFontLabel_Click(object sender, EventArgs e)
        {
            fontDialog.ShowColor = true;
            fontDialog.Font = lblLabel.Font;
            fontDialog.Color = lblLabel.ForeColor;

            if (fontDialog.ShowDialog() != DialogResult.Cancel)
            {
                lblLabel.Font = fontDialog.Font;
                lblLabel.ForeColor = fontDialog.Color;

                Properties.Settings.Default.panel_std_label_font = lblLabel.Font;
                Properties.Settings.Default.panel_std_label_color = ColorTranslator.ToHtml(lblLabel.ForeColor);
                Properties.Settings.Default.Save();
            }
        }

        private void toolStripMenuItemFontValue_Click(object sender, EventArgs e)
        {
            fontDialog.ShowColor = true;
            fontDialog.Font = lblValue.Font;
            fontDialog.Color = lblValue.ForeColor;

            if (fontDialog.ShowDialog() != DialogResult.Cancel)
            {
                lblValue.Font = fontDialog.Font;
                lblValue.ForeColor = fontDialog.Color;

                Properties.Settings.Default.panel_std_value_font = lblValue.Font;
                Properties.Settings.Default.panel_std_value_color = ColorTranslator.ToHtml(lblValue.ForeColor);
                Properties.Settings.Default.Save();
            }
        }

        private void toolStripMenuItemFontFooter_Click(object sender, EventArgs e)
        {
            fontDialog.ShowColor = true;
            fontDialog.Font = lblFreq.Font;
            fontDialog.Color = lblFreq.ForeColor;

            if (fontDialog.ShowDialog() != DialogResult.Cancel)
            {
                lblFreq.Font = fontDialog.Font;
                lblFreq.ForeColor = fontDialog.Color;
                lblMode.Font = fontDialog.Font;
                lblMode.ForeColor = fontDialog.Color;

                Properties.Settings.Default.panel_std_footer_font = lblFreq.Font;
                Properties.Settings.Default.panel_std_footer_color = ColorTranslator.ToHtml(lblFreq.ForeColor);
                Properties.Settings.Default.Save();
            }
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {
            scrollTimer.Stop();

            using (var input = new Form())
            {
                input.Text = "Edytuj tekst banera";
                input.Size = new Size(400, 200);
                input.FormBorderStyle = FormBorderStyle.FixedDialog;
                input.MaximizeBox = false;
                input.MinimizeBox = false;
                input.StartPosition = FormStartPosition.CenterParent;
                input.BackColor = Color.FromArgb(24, 24, 24);

                var lblText = new Label();
                lblText.Text = "Tekst:";
                lblText.ForeColor = Color.Yellow;
                lblText.Font = new Font("Segoe UI", 9F);
                lblText.Location = new Point(15, 10);
                lblText.AutoSize = true;
                input.Controls.Add(lblText);

                var txt = new TextBox();
                txt.Location = new Point(15, 30);
                txt.Size = new Size(350, 25);
                txt.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                txt.BackColor = Color.FromArgb(13, 13, 13);
                txt.ForeColor = Color.Yellow;
                txt.BorderStyle = BorderStyle.FixedSingle;
                txt.Text = scrollText;
                input.Controls.Add(txt);

                var lblSize = new Label();
                lblSize.Text = "Rozmiar czcionki:";
                lblSize.ForeColor = Color.Yellow;
                lblSize.Font = new Font("Segoe UI", 9F);
                lblSize.Location = new Point(15, 65);
                lblSize.AutoSize = true;
                input.Controls.Add(lblSize);

                var numSize = new NumericUpDown();
                numSize.Location = new Point(15, 85);
                numSize.Size = new Size(80, 25);
                numSize.Font = new Font("Segoe UI", 11F);
                numSize.BackColor = Color.FromArgb(13, 13, 13);
                numSize.ForeColor = Color.Yellow;
                numSize.Minimum = 8;
                numSize.Maximum = 48;
                numSize.DecimalPlaces = 1;
                numSize.Value = (decimal)lblScroll.Font.Size;
                input.Controls.Add(numSize);

                var btnOk = new Button();
                btnOk.Text = "ZAPISZ";
                btnOk.Location = new Point(140, 125);
                btnOk.Size = new Size(100, 30);
                btnOk.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                btnOk.BackColor = Color.FromArgb(0, 64, 0);
                btnOk.ForeColor = Color.LightGreen;
                btnOk.FlatStyle = FlatStyle.Flat;
                btnOk.FlatAppearance.BorderSize = 0;
                btnOk.Cursor = Cursors.Hand;
                btnOk.Click += (s, ev) => { input.DialogResult = DialogResult.OK; input.Close(); };
                input.Controls.Add(btnOk);

                input.AcceptButton = btnOk;

                if (input.ShowDialog(this) == DialogResult.OK)
                {
                    scrollText = txt.Text;
                    float newSize = (float)numSize.Value;
                    lblScroll.Font = new Font("Segoe UI", newSize, FontStyle.Bold, GraphicsUnit.Point, 238);
                    lblScroll.Text = scrollText + "          ";
                    scrollX = pnlScroll.Width;
                    Properties.Settings.Default.panel_std_custom_title = txt.Text;
                    Properties.Settings.Default.panel_std_title_font_size = (double)newSize;
                    Properties.Settings.Default.Save();
                }
            }

            scrollTimer.Start();
        }

        private void label7_Click(object sender, EventArgs e)
        {
            using (var input = new Form())
            {
                input.Text = "Edytuj nazwe";
                input.Size = new Size(300, 140);
                input.FormBorderStyle = FormBorderStyle.FixedDialog;
                input.MaximizeBox = false;
                input.MinimizeBox = false;
                input.StartPosition = FormStartPosition.CenterParent;
                input.BackColor = Color.FromArgb(24, 24, 24);

                var txt = new TextBox();
                txt.Location = new Point(15, 15);
                txt.Size = new Size(250, 25);
                txt.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                txt.BackColor = Color.FromArgb(13, 13, 13);
                txt.ForeColor = Color.Lime;
                txt.BorderStyle = BorderStyle.FixedSingle;
                txt.Text = label7.Text;
                input.Controls.Add(txt);

                var btnOk = new Button();
                btnOk.Text = "ZAPISZ";
                btnOk.Location = new Point(90, 55);
                btnOk.Size = new Size(100, 30);
                btnOk.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                btnOk.BackColor = Color.FromArgb(0, 64, 0);
                btnOk.ForeColor = Color.LightGreen;
                btnOk.FlatStyle = FlatStyle.Flat;
                btnOk.FlatAppearance.BorderSize = 0;
                btnOk.Cursor = Cursors.Hand;
                btnOk.Click += (s, ev) => { input.DialogResult = DialogResult.OK; input.Close(); };
                input.Controls.Add(btnOk);

                input.AcceptButton = btnOk;

                if (input.ShowDialog(this) == DialogResult.OK)
                {
                    label7.Text = txt.Text;
                    Properties.Settings.Default.panel_std_custom_name = txt.Text;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void label8_Click(object sender, EventArgs e)
        {
            using (var input = new Form())
            {
                input.Text = "Edytuj numer GG";
                input.Size = new Size(300, 140);
                input.FormBorderStyle = FormBorderStyle.FixedDialog;
                input.MaximizeBox = false;
                input.MinimizeBox = false;
                input.StartPosition = FormStartPosition.CenterParent;
                input.BackColor = Color.FromArgb(24, 24, 24);

                var txt = new TextBox();
                txt.Location = new Point(15, 15);
                txt.Size = new Size(250, 25);
                txt.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                txt.BackColor = Color.FromArgb(13, 13, 13);
                txt.ForeColor = Color.Navy;
                txt.BorderStyle = BorderStyle.FixedSingle;
                txt.Text = label8.Text;
                input.Controls.Add(txt);

                var btnOk = new Button();
                btnOk.Text = "ZAPISZ";
                btnOk.Location = new Point(90, 55);
                btnOk.Size = new Size(100, 30);
                btnOk.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                btnOk.BackColor = Color.FromArgb(0, 64, 0);
                btnOk.ForeColor = Color.LightGreen;
                btnOk.FlatStyle = FlatStyle.Flat;
                btnOk.FlatAppearance.BorderSize = 0;
                btnOk.Cursor = Cursors.Hand;
                btnOk.Click += (s, ev) => { input.DialogResult = DialogResult.OK; input.Close(); };
                input.Controls.Add(btnOk);

                input.AcceptButton = btnOk;

                if (input.ShowDialog(this) == DialogResult.OK)
                {
                    label8.Text = txt.Text;
                    Properties.Settings.Default.panel_std_custom_gg = txt.Text;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
