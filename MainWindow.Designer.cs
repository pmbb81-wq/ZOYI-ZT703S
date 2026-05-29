namespace ZOYI
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            button1 = new Button();
            label1 = new Label();
            btnMinimize = new Button();
            btnShortcuts = new Button();
            label4 = new Label();
            pictureBox1 = new PictureBox();
            pbESR = new PictureBox();
            label8 = new Label();
            lblComConnStatus = new Label();
            chbTTSSwitch = new CheckBox();
            cbTTSVoice = new ComboBox();
            lblTTSStatus = new Label();
            labelTTS = new Label();
            tabPage4 = new TabPage();
            tabTools = new TabPage();
            textBox1 = new TextBox();
            btnToolsRefresh = new Button();
            btnToolsEdit = new Button();
            panelTools = new Panel();
            tabPage2 = new TabPage();
            groupBox3 = new GroupBox();
            lblThicksCount = new Label();
            tbarThicksCount = new TrackBar();
            label11 = new Label();
            lblArcTicks = new Label();
            tbarArcTicks = new TrackBar();
            label10 = new Label();
            groupBox1 = new GroupBox();
            label5 = new Label();
            tbarPanelOpacity = new TrackBar();
            btnPanelColorBg = new Button();
            btnPanelColorLabel = new Button();
            btnPanelColorValue = new Button();
            tabPageCOM = new TabPage();
            chbAdvancedPanel = new CheckBox();
            groupBox2 = new GroupBox();
            rbCOMparseExt = new RadioButton();
            button2 = new Button();
            rbCOMparseRaw = new RadioButton();
            rbCOMparseLua = new RadioButton();
            rbCOMparseStd = new RadioButton();
            tbComOutput = new TextBox();
            tbCOMBaudrate = new TextBox();
            btnClearLog = new Button();
            btnSaveLog = new Button();
            chbStandardPanel = new CheckBox();
            btnListCOM = new Button();
            lbListCOMs = new ListBox();
            btnComConnect = new Button();
            lblBaudRate = new Label();
            tabControl1 = new TabControl();
            tabPageWYKRES = new TabPage();
            chartPanel = new ChartPanel();
            btnChartPause = new Button();
            btnChartClear = new Button();
            btnTime10s = new Button();
            btnTime30s = new Button();
            btnTime60s = new Button();
            btnTime5min = new Button();
            tabPage3 = new TabPage();
            tabPage1 = new TabPage();
            tabPage6 = new TabPage();
            tabPage5 = new TabPage();
            tabWebServer = new TabPage();
            btnWebServerStop = new Button();
            label2 = new Label();
            tbWebServerPort = new TextBox();
            label3 = new Label();
            llWebAddress = new LinkLabel();
            btnWebServerStart = new Button();
            tabDQ02 = new TabPage();
            button7 = new Button();
            button6 = new Button();
            button5 = new Button();
            tbDQ02Log = new TextBox();
            btnDQ02Connect = new Button();
            btnDQ02ClearLog = new Button();
            btnDQ02SaveLog = new Button();
            btnDQ02Refresh = new Button();
            button3 = new Button();
            button4 = new Button();
            lbDQ02Ports = new ListBox();
            tbDQ02Baud = new TextBox();
            lblDQ02Functions = new Label();
            lblDQ02Speed = new Label();
            lblDQ02Model = new Label();
            lblDQ02Freq = new Label();
            lblDQ02Level = new Label();
            lblDQ02Nominal = new Label();
            lblDQ02LossParam = new Label();
            lblDQ02Range = new Label();
            lblDQ02Output = new Label();
            lblDQ02Comparison = new Label();
            lblDQ02Bias = new Label();
            lblDQ02Tolerance = new Label();
            lblDQ02Prefix = new Label();
            lblDQ02Value = new Label();
            lblDQ02Secondary = new Label();
            tbDQ02UserNominal = new TextBox();
            tbDQ02UserTolerance = new TextBox();
            lblDQ02Deviation = new Label();
            lblDQ02PassFail = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbESR).BeginInit();
            tabTools.SuspendLayout();
            tabPage2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tbarThicksCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tbarArcTicks).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tbarPanelOpacity).BeginInit();
            tabPageCOM.SuspendLayout();
            groupBox2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPageWYKRES.SuspendLayout();
            tabPage3.SuspendLayout();
            tabWebServer.SuspendLayout();
            tabDQ02.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.BackColor = Color.Brown;
            button1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button1.ForeColor = SystemColors.HighlightText;
            button1.Location = new Point(729, 7);
            button1.Margin = new Padding(2);
            button1.Name = "button1";
            button1.Size = new Size(36, 29);
            button1.TabIndex = 11;
            button1.Text = "X";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            label1.ForeColor = Color.Gold;
            label1.Location = new Point(230, 9);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(418, 25);
            label1.TabIndex = 12;
            label1.Text = "ZOYI® ZT-703S OSCILLOSCOPE MULTIMETER";
            label1.Click += label1_Click;
            label1.MouseDown += MainWindow_MouseDown;
            label1.MouseMove += MainWindow_MouseMove;
            label1.MouseUp += MainWindow_MouseUp;
            // 
            // btnMinimize
            // 
            btnMinimize.BackColor = Color.Brown;
            btnMinimize.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnMinimize.ForeColor = SystemColors.HighlightText;
            btnMinimize.Location = new Point(689, 7);
            btnMinimize.Margin = new Padding(2);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(36, 29);
            btnMinimize.TabIndex = 13;
            btnMinimize.Text = "__";
            btnMinimize.UseVisualStyleBackColor = false;
            btnMinimize.Click += btnMinimize_Click;
            // 
            // btnShortcuts
            // 
            btnShortcuts.BackColor = Color.FromArgb(0, 64, 64);
            btnShortcuts.Cursor = Cursors.Hand;
            btnShortcuts.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnShortcuts.ForeColor = Color.Cyan;
            btnShortcuts.Location = new Point(649, 7);
            btnShortcuts.Margin = new Padding(2);
            btnShortcuts.Name = "btnShortcuts";
            btnShortcuts.Size = new Size(36, 29);
            btnShortcuts.TabIndex = 14;
            btnShortcuts.Text = "SK";
            btnShortcuts.UseVisualStyleBackColor = false;
            btnShortcuts.Click += btnShortcuts_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BorderStyle = BorderStyle.FixedSingle;
            label4.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            label4.ForeColor = Color.Yellow;
            label4.Location = new Point(530, 453);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(238, 22);
            label4.TabIndex = 15;
            label4.Text = "Wersja 2.36          |  KRIS® version";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            label4.Click += label4_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(10, 7);
            pictureBox1.Margin = new Padding(2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(192, 29);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 16;
            pictureBox1.TabStop = false;
            // 
            // pbESR
            // 
            pbESR.BackColor = Color.FromArgb(24, 24, 24);
            pbESR.Cursor = Cursors.Hand;
            pbESR.Dock = DockStyle.Fill;
            pbESR.Location = new Point(0, 0);
            pbESR.Name = "pbESR";
            pbESR.Size = new Size(747, 371);
            pbESR.SizeMode = PictureBoxSizeMode.Zoom;
            pbESR.TabIndex = 0;
            pbESR.TabStop = false;
            pbESR.Click += pbESR_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            label8.Location = new Point(18, 453);
            label8.Margin = new Padding(2, 0, 2, 0);
            label8.Name = "label8";
            label8.Size = new Size(47, 20);
            label8.TabIndex = 17;
            label8.Text = "COM:";
            // 
            // lblComConnStatus
            // 
            lblComConnStatus.AutoSize = true;
            lblComConnStatus.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            lblComConnStatus.ForeColor = Color.LightGreen;
            lblComConnStatus.Location = new Point(69, 453);
            lblComConnStatus.Margin = new Padding(2, 0, 2, 0);
            lblComConnStatus.Name = "lblComConnStatus";
            lblComConnStatus.Size = new Size(105, 20);
            lblComConnStatus.TabIndex = 18;
            lblComConnStatus.Text = "ROZŁĄCZONY";
            // 
            // chbTTSSwitch
            // 
            chbTTSSwitch.Appearance = Appearance.Button;
            chbTTSSwitch.BackColor = Color.FromArgb(80, 0, 0);
            chbTTSSwitch.Cursor = Cursors.Hand;
            chbTTSSwitch.FlatStyle = FlatStyle.Flat;
            chbTTSSwitch.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            chbTTSSwitch.ForeColor = Color.LightCoral;
            chbTTSSwitch.Location = new Point(348, 335);
            chbTTSSwitch.Margin = new Padding(2);
            chbTTSSwitch.Name = "chbTTSSwitch";
            chbTTSSwitch.Size = new Size(90, 30);
            chbTTSSwitch.TabIndex = 19;
            chbTTSSwitch.Text = "OFF";
            chbTTSSwitch.TextAlign = ContentAlignment.MiddleCenter;
            chbTTSSwitch.UseVisualStyleBackColor = false;
            chbTTSSwitch.CheckedChanged += chbTTSSwitch_CheckedChanged;
            // 
            // cbTTSVoice
            // 
            cbTTSVoice.BackColor = Color.FromArgb(34, 34, 34);
            cbTTSVoice.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTTSVoice.FlatStyle = FlatStyle.Flat;
            cbTTSVoice.Font = new Font("Segoe UI", 10F);
            cbTTSVoice.ForeColor = Color.LightGreen;
            cbTTSVoice.FormattingEnabled = true;
            cbTTSVoice.Location = new Point(164, 338);
            cbTTSVoice.Margin = new Padding(2);
            cbTTSVoice.Name = "cbTTSVoice";
            cbTTSVoice.Size = new Size(180, 25);
            cbTTSVoice.TabIndex = 20;
            cbTTSVoice.SelectedIndexChanged += cbTTSVoice_SelectedIndexChanged;
            // 
            // lblTTSStatus
            // 
            lblTTSStatus.AutoSize = true;
            lblTTSStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTTSStatus.ForeColor = Color.LightCoral;
            lblTTSStatus.Location = new Point(442, 346);
            lblTTSStatus.Margin = new Padding(2, 0, 2, 0);
            lblTTSStatus.Name = "lblTTSStatus";
            lblTTSStatus.Size = new Size(62, 19);
            lblTTSStatus.TabIndex = 21;
            lblTTSStatus.Text = "TTS OFF";
            // 
            // labelTTS
            // 
            labelTTS.AutoSize = true;
            labelTTS.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelTTS.ForeColor = Color.FromArgb(200, 200, 200);
            labelTTS.Location = new Point(4, 342);
            labelTTS.Margin = new Padding(2, 0, 2, 0);
            labelTTS.Name = "labelTTS";
            labelTTS.Size = new Size(154, 20);
            labelTTS.TabIndex = 18;
            labelTTS.Text = "LEKTOR POMIARÓW";
            // 
            // tabPage4
            // 
            tabPage4.BackColor = Color.FromArgb(44, 44, 44);
            tabPage4.BackgroundImage = (Image)resources.GetObject("tabPage4.BackgroundImage");
            tabPage4.BackgroundImageLayout = ImageLayout.Zoom;
            tabPage4.Location = new Point(4, 32);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(751, 375);
            tabPage4.TabIndex = 5;
            tabPage4.Text = "ZOYI-INFO";
            // 
            // tabTools
            // 
            tabTools.BackColor = Color.FromArgb(64, 64, 64);
            tabTools.BackgroundImageLayout = ImageLayout.Zoom;
            tabTools.Controls.Add(textBox1);
            tabTools.Controls.Add(btnToolsRefresh);
            tabTools.Controls.Add(btnToolsEdit);
            tabTools.Controls.Add(panelTools);
            tabTools.Location = new Point(4, 32);
            tabTools.Margin = new Padding(2);
            tabTools.Name = "tabTools";
            tabTools.Padding = new Padding(2);
            tabTools.Size = new Size(751, 375);
            tabTools.TabIndex = 3;
            tabTools.Text = "PRZYBORNIK";
            // 
            // textBox1
            // 
            textBox1.BackColor = SystemColors.HotTrack;
            textBox1.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            textBox1.ForeColor = Color.Yellow;
            textBox1.Location = new Point(636, 202);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ShortcutsEnabled = false;
            textBox1.Size = new Size(110, 168);
            textBox1.TabIndex = 3;
            textBox1.Text = "INSTRUKCJA\r\n\r\n[ -- ]\r\nTYTUŁ\r\n\r\n[  |  ]\r\nSEPARATOR\r\nnazwa | link";
            textBox1.TextAlign = HorizontalAlignment.Center;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // btnToolsRefresh
            // 
            btnToolsRefresh.BackColor = Color.FromArgb(64, 0, 0);
            btnToolsRefresh.Cursor = Cursors.Hand;
            btnToolsRefresh.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnToolsRefresh.Location = new Point(636, 98);
            btnToolsRefresh.Margin = new Padding(2);
            btnToolsRefresh.Name = "btnToolsRefresh";
            btnToolsRefresh.Size = new Size(113, 69);
            btnToolsRefresh.TabIndex = 2;
            btnToolsRefresh.Text = "ODŚWIEŻ";
            btnToolsRefresh.UseVisualStyleBackColor = false;
            btnToolsRefresh.Click += btnToolsRefresh_Click;
            // 
            // btnToolsEdit
            // 
            btnToolsEdit.BackColor = Color.FromArgb(0, 64, 64);
            btnToolsEdit.Cursor = Cursors.Hand;
            btnToolsEdit.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnToolsEdit.Location = new Point(634, 13);
            btnToolsEdit.Margin = new Padding(2);
            btnToolsEdit.Name = "btnToolsEdit";
            btnToolsEdit.Size = new Size(113, 69);
            btnToolsEdit.TabIndex = 1;
            btnToolsEdit.Text = "EDYCJA";
            btnToolsEdit.UseVisualStyleBackColor = false;
            btnToolsEdit.Click += btnToolsEdit_Click;
            // 
            // panelTools
            // 
            panelTools.AutoScroll = true;
            panelTools.BackColor = Color.FromArgb(0, 0, 24);
            panelTools.BackgroundImageLayout = ImageLayout.None;
            panelTools.BorderStyle = BorderStyle.Fixed3D;
            panelTools.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
            panelTools.Location = new Point(4, 4);
            panelTools.Margin = new Padding(2);
            panelTools.Name = "panelTools";
            panelTools.Size = new Size(626, 367);
            panelTools.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.BackColor = Color.FromArgb(34, 34, 34);
            tabPage2.BackgroundImage = (Image)resources.GetObject("tabPage2.BackgroundImage");
            tabPage2.BackgroundImageLayout = ImageLayout.Zoom;
            tabPage2.Controls.Add(groupBox3);
            tabPage2.Controls.Add(groupBox1);
            tabPage2.Controls.Add(labelTTS);
            tabPage2.Controls.Add(chbTTSSwitch);
            tabPage2.Controls.Add(cbTTSVoice);
            tabPage2.Controls.Add(lblTTSStatus);
            tabPage2.ForeColor = SystemColors.HighlightText;
            tabPage2.Location = new Point(4, 32);
            tabPage2.Margin = new Padding(2);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(2);
            tabPage2.Size = new Size(751, 375);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "USTAWIENIA";
            // 
            // groupBox3
            // 
            groupBox3.BackColor = Color.Transparent;
            groupBox3.Controls.Add(lblThicksCount);
            groupBox3.Controls.Add(tbarThicksCount);
            groupBox3.Controls.Add(label11);
            groupBox3.Controls.Add(lblArcTicks);
            groupBox3.Controls.Add(tbarArcTicks);
            groupBox3.Controls.Add(label10);
            groupBox3.ForeColor = SystemColors.HighlightText;
            groupBox3.Location = new Point(14, 191);
            groupBox3.Margin = new Padding(2);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(2);
            groupBox3.Size = new Size(721, 140);
            groupBox3.TabIndex = 17;
            groupBox3.TabStop = false;
            groupBox3.Text = "Panel zaawansowany";
            // 
            // lblThicksCount
            // 
            lblThicksCount.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblThicksCount.AutoSize = true;
            lblThicksCount.BorderStyle = BorderStyle.FixedSingle;
            lblThicksCount.Cursor = Cursors.Hand;
            lblThicksCount.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold);
            lblThicksCount.ForeColor = Color.Yellow;
            lblThicksCount.Location = new Point(121, 85);
            lblThicksCount.Margin = new Padding(2, 0, 2, 0);
            lblThicksCount.Name = "lblThicksCount";
            lblThicksCount.Size = new Size(35, 39);
            lblThicksCount.TabIndex = 5;
            lblThicksCount.Text = "5";
            lblThicksCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tbarThicksCount
            // 
            tbarThicksCount.BackColor = Color.FromArgb(34, 34, 34);
            tbarThicksCount.Location = new Point(170, 92);
            tbarThicksCount.Margin = new Padding(2);
            tbarThicksCount.Maximum = 50;
            tbarThicksCount.Minimum = 5;
            tbarThicksCount.Name = "tbarThicksCount";
            tbarThicksCount.Size = new Size(527, 45);
            tbarThicksCount.TabIndex = 4;
            tbarThicksCount.TickStyle = TickStyle.Both;
            tbarThicksCount.Value = 5;
            tbarThicksCount.Scroll += tbarThicksCount_Scroll;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label11.Location = new Point(8, 92);
            label11.Margin = new Padding(2, 0, 2, 0);
            label11.Name = "label11";
            label11.Size = new Size(101, 21);
            label11.TabIndex = 3;
            label11.Text = "Thicks count";
            // 
            // lblArcTicks
            // 
            lblArcTicks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblArcTicks.AutoSize = true;
            lblArcTicks.BorderStyle = BorderStyle.FixedSingle;
            lblArcTicks.Cursor = Cursors.Hand;
            lblArcTicks.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold);
            lblArcTicks.ForeColor = Color.Yellow;
            lblArcTicks.Location = new Point(115, 33);
            lblArcTicks.Margin = new Padding(2, 0, 2, 0);
            lblArcTicks.Name = "lblArcTicks";
            lblArcTicks.Size = new Size(51, 39);
            lblArcTicks.TabIndex = 2;
            lblArcTicks.Text = "25";
            lblArcTicks.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tbarArcTicks
            // 
            tbarArcTicks.BackColor = Color.FromArgb(34, 34, 34);
            tbarArcTicks.Location = new Point(170, 33);
            tbarArcTicks.Margin = new Padding(2);
            tbarArcTicks.Maximum = 250;
            tbarArcTicks.Minimum = 10;
            tbarArcTicks.Name = "tbarArcTicks";
            tbarArcTicks.Size = new Size(527, 45);
            tbarArcTicks.TabIndex = 1;
            tbarArcTicks.TickFrequency = 5;
            tbarArcTicks.TickStyle = TickStyle.Both;
            tbarArcTicks.Value = 25;
            tbarArcTicks.Scroll += tbarArcTicks_Scroll;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            label10.Location = new Point(8, 40);
            label10.Margin = new Padding(2, 0, 2, 0);
            label10.Name = "label10";
            label10.Size = new Size(46, 21);
            label10.TabIndex = 0;
            label10.Text = "Ticks";
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.Transparent;
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(tbarPanelOpacity);
            groupBox1.Controls.Add(btnPanelColorBg);
            groupBox1.Controls.Add(btnPanelColorLabel);
            groupBox1.Controls.Add(btnPanelColorValue);
            groupBox1.ForeColor = SystemColors.HighlightText;
            groupBox1.Location = new Point(14, 6);
            groupBox1.Margin = new Padding(2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(2);
            groupBox1.Size = new Size(721, 179);
            groupBox1.TabIndex = 16;
            groupBox1.TabStop = false;
            groupBox1.Text = "Panel standardowy";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
            label5.ForeColor = Color.LawnGreen;
            label5.Location = new Point(8, 127);
            label5.Margin = new Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new Size(153, 21);
            label5.TabIndex = 13;
            label5.Text = "PRZEŹROCZYSTOŚĆ";
            // 
            // tbarPanelOpacity
            // 
            tbarPanelOpacity.BackColor = Color.FromArgb(34, 34, 34);
            tbarPanelOpacity.Location = new Point(170, 117);
            tbarPanelOpacity.Margin = new Padding(2);
            tbarPanelOpacity.Maximum = 100;
            tbarPanelOpacity.Minimum = 20;
            tbarPanelOpacity.Name = "tbarPanelOpacity";
            tbarPanelOpacity.Size = new Size(537, 45);
            tbarPanelOpacity.TabIndex = 12;
            tbarPanelOpacity.TickFrequency = 5;
            tbarPanelOpacity.TickStyle = TickStyle.Both;
            tbarPanelOpacity.Value = 100;
            tbarPanelOpacity.Scroll += tbPanelOpacity_Scroll;
            // 
            // btnPanelColorBg
            // 
            btnPanelColorBg.BackColor = Color.FromArgb(0, 0, 64);
            btnPanelColorBg.Cursor = Cursors.Hand;
            btnPanelColorBg.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnPanelColorBg.Location = new Point(10, 34);
            btnPanelColorBg.Margin = new Padding(2);
            btnPanelColorBg.Name = "btnPanelColorBg";
            btnPanelColorBg.Size = new Size(193, 56);
            btnPanelColorBg.TabIndex = 8;
            btnPanelColorBg.Text = "KOLOR TŁA";
            btnPanelColorBg.UseVisualStyleBackColor = false;
            btnPanelColorBg.Click += btnColorBg_Click;
            // 
            // btnPanelColorLabel
            // 
            btnPanelColorLabel.BackColor = Color.FromArgb(0, 0, 54);
            btnPanelColorLabel.Cursor = Cursors.Hand;
            btnPanelColorLabel.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnPanelColorLabel.Location = new Point(268, 34);
            btnPanelColorLabel.Margin = new Padding(2);
            btnPanelColorLabel.Name = "btnPanelColorLabel";
            btnPanelColorLabel.Size = new Size(193, 56);
            btnPanelColorLabel.TabIndex = 9;
            btnPanelColorLabel.Text = "KOLOR OPISU";
            btnPanelColorLabel.UseVisualStyleBackColor = false;
            btnPanelColorLabel.Click += btnColorLabel_Click;
            // 
            // btnPanelColorValue
            // 
            btnPanelColorValue.BackColor = Color.FromArgb(0, 0, 44);
            btnPanelColorValue.Cursor = Cursors.Hand;
            btnPanelColorValue.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnPanelColorValue.Location = new Point(514, 34);
            btnPanelColorValue.Margin = new Padding(2);
            btnPanelColorValue.Name = "btnPanelColorValue";
            btnPanelColorValue.Size = new Size(193, 56);
            btnPanelColorValue.TabIndex = 11;
            btnPanelColorValue.Text = "KOLOR WARTOŚCI";
            btnPanelColorValue.UseVisualStyleBackColor = false;
            btnPanelColorValue.Click += btnColorValue_Click;
            // 
            // tabPageCOM
            // 
            tabPageCOM.BackColor = Color.FromArgb(34, 34, 34);
            tabPageCOM.Controls.Add(chbAdvancedPanel);
            tabPageCOM.Controls.Add(groupBox2);
            tabPageCOM.Controls.Add(tbComOutput);
            tabPageCOM.Controls.Add(tbCOMBaudrate);
            tabPageCOM.Controls.Add(btnClearLog);
            tabPageCOM.Controls.Add(btnSaveLog);
            tabPageCOM.Controls.Add(chbStandardPanel);
            tabPageCOM.Controls.Add(btnListCOM);
            tabPageCOM.Controls.Add(lbListCOMs);
            tabPageCOM.Controls.Add(btnComConnect);
            tabPageCOM.Controls.Add(lblBaudRate);
            tabPageCOM.Location = new Point(4, 32);
            tabPageCOM.Margin = new Padding(2);
            tabPageCOM.Name = "tabPageCOM";
            tabPageCOM.Padding = new Padding(2);
            tabPageCOM.Size = new Size(751, 375);
            tabPageCOM.TabIndex = 0;
            tabPageCOM.Text = "START";
            // 
            // chbAdvancedPanel
            // 
            chbAdvancedPanel.AutoSize = true;
            chbAdvancedPanel.Cursor = Cursors.Hand;
            chbAdvancedPanel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            chbAdvancedPanel.Location = new Point(532, 333);
            chbAdvancedPanel.Margin = new Padding(2);
            chbAdvancedPanel.Name = "chbAdvancedPanel";
            chbAdvancedPanel.Size = new Size(188, 25);
            chbAdvancedPanel.TabIndex = 13;
            chbAdvancedPanel.Text = "PANEL ROZSZEŻONY";
            chbAdvancedPanel.UseVisualStyleBackColor = true;
            chbAdvancedPanel.CheckedChanged += chbAdvancedPanel_CheckedChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(rbCOMparseExt);
            groupBox2.Controls.Add(button2);
            groupBox2.Controls.Add(rbCOMparseRaw);
            groupBox2.Controls.Add(rbCOMparseLua);
            groupBox2.Controls.Add(rbCOMparseStd);
            groupBox2.ForeColor = SystemColors.HighlightText;
            groupBox2.Location = new Point(516, 121);
            groupBox2.Margin = new Padding(2);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(2);
            groupBox2.Size = new Size(217, 162);
            groupBox2.TabIndex = 12;
            groupBox2.TabStop = false;
            groupBox2.Text = "Tryb wyświetlania";
            // 
            // rbCOMparseExt
            // 
            rbCOMparseExt.AutoSize = true;
            rbCOMparseExt.Checked = true;
            rbCOMparseExt.Cursor = Cursors.Hand;
            rbCOMparseExt.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            rbCOMparseExt.Location = new Point(16, 58);
            rbCOMparseExt.Margin = new Padding(2);
            rbCOMparseExt.Name = "rbCOMparseExt";
            rbCOMparseExt.Size = new Size(111, 25);
            rbCOMparseExt.TabIndex = 16;
            rbCOMparseExt.TabStop = true;
            rbCOMparseExt.Text = "EXTENDED";
            rbCOMparseExt.UseVisualStyleBackColor = true;
            rbCOMparseExt.CheckedChanged += rbComParse_CheckedChanged;
            // 
            // button2
            // 
            button2.BackColor = Color.FromArgb(64, 64, 64);
            button2.Cursor = Cursors.Hand;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
            button2.ForeColor = Color.Gold;
            button2.Location = new Point(122, 87);
            button2.Margin = new Padding(2);
            button2.Name = "button2";
            button2.Size = new Size(88, 34);
            button2.TabIndex = 15;
            button2.Text = "EDYCJA";
            button2.UseVisualStyleBackColor = false;
            button2.Click += btnCOMluaEdit_Click;
            // 
            // rbCOMparseRaw
            // 
            rbCOMparseRaw.AutoSize = true;
            rbCOMparseRaw.Cursor = Cursors.Hand;
            rbCOMparseRaw.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            rbCOMparseRaw.Location = new Point(16, 121);
            rbCOMparseRaw.Margin = new Padding(2);
            rbCOMparseRaw.Name = "rbCOMparseRaw";
            rbCOMparseRaw.Size = new Size(64, 25);
            rbCOMparseRaw.TabIndex = 14;
            rbCOMparseRaw.Text = "RAW";
            rbCOMparseRaw.UseVisualStyleBackColor = true;
            rbCOMparseRaw.CheckedChanged += rbComParse_CheckedChanged;
            // 
            // rbCOMparseLua
            // 
            rbCOMparseLua.AutoSize = true;
            rbCOMparseLua.Cursor = Cursors.Hand;
            rbCOMparseLua.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            rbCOMparseLua.Location = new Point(16, 92);
            rbCOMparseLua.Margin = new Padding(2);
            rbCOMparseLua.Name = "rbCOMparseLua";
            rbCOMparseLua.Size = new Size(98, 25);
            rbCOMparseLua.TabIndex = 13;
            rbCOMparseLua.Text = "LUA (std)";
            rbCOMparseLua.UseVisualStyleBackColor = true;
            rbCOMparseLua.CheckedChanged += rbComParse_CheckedChanged;
            // 
            // rbCOMparseStd
            // 
            rbCOMparseStd.AutoSize = true;
            rbCOMparseStd.Cursor = Cursors.Hand;
            rbCOMparseStd.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            rbCOMparseStd.Location = new Point(16, 28);
            rbCOMparseStd.Margin = new Padding(2);
            rbCOMparseStd.Name = "rbCOMparseStd";
            rbCOMparseStd.Size = new Size(114, 25);
            rbCOMparseStd.TabIndex = 12;
            rbCOMparseStd.Text = "STANDARD";
            rbCOMparseStd.UseVisualStyleBackColor = true;
            rbCOMparseStd.CheckedChanged += rbComParse_CheckedChanged;
            // 
            // tbComOutput
            // 
            tbComOutput.BackColor = Color.FromArgb(24, 24, 24);
            tbComOutput.ForeColor = SystemColors.HighlightText;
            tbComOutput.Location = new Point(216, 8);
            tbComOutput.Margin = new Padding(2);
            tbComOutput.Multiline = true;
            tbComOutput.Name = "tbComOutput";
            tbComOutput.ScrollBars = ScrollBars.Vertical;
            tbComOutput.Size = new Size(276, 357);
            tbComOutput.TabIndex = 5;
            tbComOutput.TextAlign = HorizontalAlignment.Center;
            tbComOutput.TextChanged += tbComOutput_TextChanged;
            // 
            // tbCOMBaudrate
            // 
            tbCOMBaudrate.BackColor = Color.Navy;
            tbCOMBaudrate.Cursor = Cursors.IBeam;
            tbCOMBaudrate.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            tbCOMBaudrate.ForeColor = Color.Gold;
            tbCOMBaudrate.Location = new Point(101, 228);
            tbCOMBaudrate.Margin = new Padding(2);
            tbCOMBaudrate.Name = "tbCOMBaudrate";
            tbCOMBaudrate.Size = new Size(99, 27);
            tbCOMBaudrate.TabIndex = 8;
            tbCOMBaudrate.Text = "9600";
            tbCOMBaudrate.TextAlign = HorizontalAlignment.Center;
            // 
            // btnClearLog
            // 
            btnClearLog.BackColor = SystemColors.Desktop;
            btnClearLog.Cursor = Cursors.Hand;
            btnClearLog.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnClearLog.ForeColor = Color.FromArgb(128, 255, 128);
            btnClearLog.Location = new Point(516, 12);
            btnClearLog.Margin = new Padding(2);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(217, 44);
            btnClearLog.TabIndex = 9;
            btnClearLog.Text = "WYCZYŚĆ";
            btnClearLog.UseVisualStyleBackColor = false;
            btnClearLog.Click += btnClearLog_Click;
            // 
            // btnSaveLog
            // 
            btnSaveLog.BackColor = SystemColors.Desktop;
            btnSaveLog.Cursor = Cursors.Hand;
            btnSaveLog.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnSaveLog.ForeColor = Color.FromArgb(128, 128, 255);
            btnSaveLog.Location = new Point(516, 60);
            btnSaveLog.Margin = new Padding(2);
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Size = new Size(217, 41);
            btnSaveLog.TabIndex = 10;
            btnSaveLog.Text = "ZAPISZ CSV";
            btnSaveLog.UseVisualStyleBackColor = false;
            btnSaveLog.Click += btnSaveLog_Click;
            // 
            // chbStandardPanel
            // 
            chbStandardPanel.AutoSize = true;
            chbStandardPanel.Checked = true;
            chbStandardPanel.CheckState = CheckState.Checked;
            chbStandardPanel.Cursor = Cursors.Hand;
            chbStandardPanel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            chbStandardPanel.Location = new Point(532, 304);
            chbStandardPanel.Margin = new Padding(2);
            chbStandardPanel.Name = "chbStandardPanel";
            chbStandardPanel.Size = new Size(203, 25);
            chbStandardPanel.TabIndex = 6;
            chbStandardPanel.Text = "PANEL STANDARTOWY";
            chbStandardPanel.UseVisualStyleBackColor = true;
            chbStandardPanel.CheckedChanged += chbShowPanel_CheckedChanged;
            // 
            // btnListCOM
            // 
            btnListCOM.BackColor = Color.FromArgb(24, 24, 24);
            btnListCOM.Cursor = Cursors.Hand;
            btnListCOM.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnListCOM.ForeColor = Color.FromArgb(255, 255, 128);
            btnListCOM.Location = new Point(15, 8);
            btnListCOM.Margin = new Padding(2);
            btnListCOM.Name = "btnListCOM";
            btnListCOM.Size = new Size(185, 68);
            btnListCOM.TabIndex = 1;
            btnListCOM.Text = "ODŚWIEŻ PORTY";
            btnListCOM.UseVisualStyleBackColor = false;
            btnListCOM.Click += btnListCOM_Click;
            // 
            // lbListCOMs
            // 
            lbListCOMs.BackColor = Color.FromArgb(44, 44, 44);
            lbListCOMs.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            lbListCOMs.ForeColor = Color.White;
            lbListCOMs.FormattingEnabled = true;
            lbListCOMs.ItemHeight = 20;
            lbListCOMs.Location = new Point(15, 80);
            lbListCOMs.Margin = new Padding(2);
            lbListCOMs.Name = "lbListCOMs";
            lbListCOMs.Size = new Size(185, 144);
            lbListCOMs.TabIndex = 2;
            // 
            // btnComConnect
            // 
            btnComConnect.BackColor = Color.LightGreen;
            btnComConnect.Cursor = Cursors.Hand;
            btnComConnect.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnComConnect.ForeColor = SystemColors.Desktop;
            btnComConnect.Location = new Point(15, 269);
            btnComConnect.Margin = new Padding(2);
            btnComConnect.Name = "btnComConnect";
            btnComConnect.Size = new Size(185, 96);
            btnComConnect.TabIndex = 3;
            btnComConnect.Text = "POŁĄCZ";
            btnComConnect.UseVisualStyleBackColor = false;
            btnComConnect.Click += btnComConnect_Click;
            // 
            // lblBaudRate
            // 
            lblBaudRate.AutoSize = true;
            lblBaudRate.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            lblBaudRate.ForeColor = Color.Yellow;
            lblBaudRate.Location = new Point(15, 231);
            lblBaudRate.Margin = new Padding(2, 0, 2, 0);
            lblBaudRate.Name = "lblBaudRate";
            lblBaudRate.Size = new Size(82, 20);
            lblBaudRate.TabIndex = 7;
            lblBaudRate.Text = "Baud Rate:";
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Appearance = TabAppearance.Buttons;
            tabControl1.Controls.Add(tabPageCOM);
            tabControl1.Controls.Add(tabPageWYKRES);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabTools);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage6);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabWebServer);
            tabControl1.Controls.Add(tabDQ02);
            tabControl1.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            tabControl1.Location = new Point(10, 40);
            tabControl1.Margin = new Padding(2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(759, 411);
            tabControl1.SizeMode = TabSizeMode.FillToRight;
            tabControl1.TabIndex = 14;
            // 
            // tabPageWYKRES
            // 
            tabPageWYKRES.BackColor = Color.FromArgb(13, 13, 13);
            tabPageWYKRES.Controls.Add(chartPanel);
            tabPageWYKRES.Controls.Add(btnChartPause);
            tabPageWYKRES.Controls.Add(btnChartClear);
            tabPageWYKRES.Controls.Add(btnTime10s);
            tabPageWYKRES.Controls.Add(btnTime30s);
            tabPageWYKRES.Controls.Add(btnTime60s);
            tabPageWYKRES.Controls.Add(btnTime5min);
            tabPageWYKRES.Location = new Point(4, 32);
            tabPageWYKRES.Margin = new Padding(2);
            tabPageWYKRES.Name = "tabPageWYKRES";
            tabPageWYKRES.Padding = new Padding(2);
            tabPageWYKRES.Size = new Size(751, 375);
            tabPageWYKRES.TabIndex = 10;
            tabPageWYKRES.Text = "WYKRES";
            // 
            // chartPanel
            // 
            chartPanel.Location = new Point(0, 40);
            chartPanel.Name = "chartPanel";
            chartPanel.Size = new Size(751, 335);
            chartPanel.TabIndex = 0;
            // 
            // btnChartPause
            // 
            btnChartPause.BackColor = Color.FromArgb(64, 64, 64);
            btnChartPause.Cursor = Cursors.Hand;
            btnChartPause.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnChartPause.ForeColor = Color.Yellow;
            btnChartPause.Location = new Point(8, 6);
            btnChartPause.Margin = new Padding(2);
            btnChartPause.Name = "btnChartPause";
            btnChartPause.Size = new Size(90, 30);
            btnChartPause.TabIndex = 1;
            btnChartPause.Text = "PAUZA";
            btnChartPause.UseVisualStyleBackColor = false;
            btnChartPause.Click += btnChartPause_Click;
            // 
            // btnChartClear
            // 
            btnChartClear.BackColor = Color.FromArgb(64, 0, 0);
            btnChartClear.Cursor = Cursors.Hand;
            btnChartClear.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnChartClear.ForeColor = Color.LightCoral;
            btnChartClear.Location = new Point(102, 6);
            btnChartClear.Margin = new Padding(2);
            btnChartClear.Name = "btnChartClear";
            btnChartClear.Size = new Size(90, 30);
            btnChartClear.TabIndex = 2;
            btnChartClear.Text = "WYCZYŚĆ";
            btnChartClear.UseVisualStyleBackColor = false;
            btnChartClear.Click += btnChartClear_Click;
            // 
            // btnTime10s
            // 
            btnTime10s.BackColor = Color.FromArgb(0, 64, 64);
            btnTime10s.Cursor = Cursors.Hand;
            btnTime10s.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnTime10s.ForeColor = Color.Cyan;
            btnTime10s.Location = new Point(480, 6);
            btnTime10s.Margin = new Padding(2);
            btnTime10s.Name = "btnTime10s";
            btnTime10s.Size = new Size(60, 30);
            btnTime10s.TabIndex = 3;
            btnTime10s.Text = "10s";
            btnTime10s.UseVisualStyleBackColor = false;
            btnTime10s.Click += btnTimeWindow_Click;
            // 
            // btnTime30s
            // 
            btnTime30s.BackColor = Color.FromArgb(0, 64, 64);
            btnTime30s.Cursor = Cursors.Hand;
            btnTime30s.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnTime30s.ForeColor = Color.Cyan;
            btnTime30s.Location = new Point(544, 6);
            btnTime30s.Margin = new Padding(2);
            btnTime30s.Name = "btnTime30s";
            btnTime30s.Size = new Size(60, 30);
            btnTime30s.TabIndex = 4;
            btnTime30s.Text = "30s";
            btnTime30s.UseVisualStyleBackColor = false;
            btnTime30s.Click += btnTimeWindow_Click;
            // 
            // btnTime60s
            // 
            btnTime60s.BackColor = Color.FromArgb(0, 64, 0);
            btnTime60s.Cursor = Cursors.Hand;
            btnTime60s.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnTime60s.ForeColor = Color.LightGreen;
            btnTime60s.Location = new Point(608, 6);
            btnTime60s.Margin = new Padding(2);
            btnTime60s.Name = "btnTime60s";
            btnTime60s.Size = new Size(60, 30);
            btnTime60s.TabIndex = 5;
            btnTime60s.Text = "60s";
            btnTime60s.UseVisualStyleBackColor = false;
            btnTime60s.Click += btnTimeWindow_Click;
            // 
            // btnTime5min
            // 
            btnTime5min.BackColor = Color.FromArgb(0, 64, 64);
            btnTime5min.Cursor = Cursors.Hand;
            btnTime5min.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnTime5min.ForeColor = Color.Cyan;
            btnTime5min.Location = new Point(672, 6);
            btnTime5min.Margin = new Padding(2);
            btnTime5min.Name = "btnTime5min";
            btnTime5min.Size = new Size(70, 30);
            btnTime5min.TabIndex = 6;
            btnTime5min.Text = "5min";
            btnTime5min.UseVisualStyleBackColor = false;
            btnTime5min.Click += btnTimeWindow_Click;
            // 
            // tabPage3
            // 
            tabPage3.BackColor = Color.FromArgb(24, 24, 24);
            tabPage3.BorderStyle = BorderStyle.Fixed3D;
            tabPage3.Controls.Add(pbESR);
            tabPage3.ForeColor = SystemColors.GrayText;
            tabPage3.Location = new Point(4, 32);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(751, 375);
            tabPage3.TabIndex = 9;
            tabPage3.Text = "ESR TABELA";
            // 
            // tabPage1
            // 
            tabPage1.BackColor = Color.FromArgb(24, 24, 24);
            tabPage1.BackgroundImage = (Image)resources.GetObject("tabPage1.BackgroundImage");
            tabPage1.BackgroundImageLayout = ImageLayout.Zoom;
            tabPage1.BorderStyle = BorderStyle.Fixed3D;
            tabPage1.ForeColor = Color.Blue;
            tabPage1.Location = new Point(4, 32);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(751, 375);
            tabPage1.TabIndex = 6;
            tabPage1.Text = "KOD REZYSTORÓW";
            // 
            // tabPage6
            // 
            tabPage6.BackColor = Color.FromArgb(24, 24, 24);
            tabPage6.BackgroundImage = (Image)resources.GetObject("tabPage6.BackgroundImage");
            tabPage6.BackgroundImageLayout = ImageLayout.Stretch;
            tabPage6.Location = new Point(4, 32);
            tabPage6.Name = "tabPage6";
            tabPage6.Padding = new Padding(3);
            tabPage6.Size = new Size(751, 375);
            tabPage6.TabIndex = 8;
            tabPage6.Text = "PODŁĄCZ._STANDARD";
            // 
            // tabPage5
            // 
            tabPage5.BackColor = Color.FromArgb(24, 24, 24);
            tabPage5.BackgroundImage = (Image)resources.GetObject("tabPage5.BackgroundImage");
            tabPage5.BackgroundImageLayout = ImageLayout.Stretch;
            tabPage5.Location = new Point(4, 32);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(751, 375);
            tabPage5.TabIndex = 7;
            tabPage5.Text = "PODŁĄCZ._EXTENDED";
            // 
            // tabWebServer
            // 
            tabWebServer.BackColor = Color.FromArgb(34, 34, 34);
            tabWebServer.Controls.Add(btnWebServerStop);
            tabWebServer.Controls.Add(label2);
            tabWebServer.Controls.Add(tbWebServerPort);
            tabWebServer.Controls.Add(label3);
            tabWebServer.Controls.Add(llWebAddress);
            tabWebServer.Controls.Add(btnWebServerStart);
            tabWebServer.Location = new Point(4, 32);
            tabWebServer.Name = "tabWebServer";
            tabWebServer.Padding = new Padding(3);
            tabWebServer.Size = new Size(751, 375);
            tabWebServer.TabIndex = 11;
            tabWebServer.Text = "WEB SERWER";
            // 
            // btnWebServerStop
            // 
            btnWebServerStop.BackColor = Color.FromArgb(64, 0, 0);
            btnWebServerStop.Cursor = Cursors.Hand;
            btnWebServerStop.Enabled = false;
            btnWebServerStop.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnWebServerStop.ForeColor = Color.LightCoral;
            btnWebServerStop.Location = new Point(180, 16);
            btnWebServerStop.Margin = new Padding(2);
            btnWebServerStop.Name = "btnWebServerStop";
            btnWebServerStop.Size = new Size(160, 44);
            btnWebServerStop.TabIndex = 1;
            btnWebServerStop.Text = "ZATRZYMAJ";
            btnWebServerStop.UseVisualStyleBackColor = false;
            btnWebServerStop.Click += btnWebServerStop_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            label2.ForeColor = Color.Yellow;
            label2.Location = new Point(19, 83);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(41, 20);
            label2.TabIndex = 2;
            label2.Text = "Port:";
            // 
            // tbWebServerPort
            // 
            tbWebServerPort.BackColor = Color.FromArgb(24, 24, 24);
            tbWebServerPort.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold);
            tbWebServerPort.ForeColor = Color.Gold;
            tbWebServerPort.Location = new Point(68, 77);
            tbWebServerPort.Margin = new Padding(2);
            tbWebServerPort.Name = "tbWebServerPort";
            tbWebServerPort.Size = new Size(80, 27);
            tbWebServerPort.TabIndex = 3;
            tbWebServerPort.Text = "8080";
            tbWebServerPort.TextAlign = HorizontalAlignment.Center;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold);
            label3.ForeColor = Color.LightGreen;
            label3.Location = new Point(19, 123);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(52, 20);
            label3.TabIndex = 4;
            label3.Text = "Adres:";
            // 
            // llWebAddress
            // 
            llWebAddress.AutoSize = true;
            llWebAddress.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold);
            llWebAddress.LinkColor = Color.Cyan;
            llWebAddress.Location = new Point(79, 123);
            llWebAddress.Margin = new Padding(2, 0, 2, 0);
            llWebAddress.Name = "llWebAddress";
            llWebAddress.Size = new Size(112, 20);
            llWebAddress.TabIndex = 5;
            llWebAddress.TabStop = true;
            llWebAddress.Text = "http://IP:port/";
            llWebAddress.LinkClicked += llWebAddress_LinkClicked;
            // 
            // btnWebServerStart
            // 
            btnWebServerStart.BackColor = Color.FromArgb(0, 64, 0);
            btnWebServerStart.Cursor = Cursors.Hand;
            btnWebServerStart.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnWebServerStart.ForeColor = Color.LightGreen;
            btnWebServerStart.Location = new Point(16, 16);
            btnWebServerStart.Margin = new Padding(2);
            btnWebServerStart.Name = "btnWebServerStart";
            btnWebServerStart.Size = new Size(160, 44);
            btnWebServerStart.TabIndex = 0;
            btnWebServerStart.Text = "URUCHOM";
            btnWebServerStart.UseVisualStyleBackColor = false;
            btnWebServerStart.Click += btnWebServerStart_Click;
            // 
            // tabDQ02
            // 
            tabDQ02.BackColor = Color.FromArgb(34, 34, 34);
            tabDQ02.Controls.Add(button7);
            tabDQ02.Controls.Add(button6);
            tabDQ02.Controls.Add(button5);
            tabDQ02.Controls.Add(tbDQ02Log);
            tabDQ02.Controls.Add(btnDQ02Connect);
            tabDQ02.Controls.Add(btnDQ02ClearLog);
            tabDQ02.Controls.Add(btnDQ02SaveLog);
            tabDQ02.Controls.Add(btnDQ02Refresh);
            tabDQ02.Controls.Add(button3);
            tabDQ02.Controls.Add(button4);
            tabDQ02.Controls.Add(lbDQ02Ports);
            tabDQ02.Controls.Add(tbDQ02Baud);
            tabDQ02.Controls.Add(lblDQ02Functions);
            tabDQ02.Controls.Add(lblDQ02Speed);
            tabDQ02.Controls.Add(lblDQ02Model);
            tabDQ02.Controls.Add(lblDQ02Freq);
            tabDQ02.Controls.Add(lblDQ02Level);
            tabDQ02.Controls.Add(lblDQ02Nominal);
            tabDQ02.Controls.Add(lblDQ02LossParam);
            tabDQ02.Controls.Add(lblDQ02Range);
            tabDQ02.Controls.Add(lblDQ02Output);
            tabDQ02.Controls.Add(lblDQ02Comparison);
            tabDQ02.Controls.Add(lblDQ02Bias);
            tabDQ02.Controls.Add(lblDQ02Tolerance);
            tabDQ02.Controls.Add(lblDQ02Prefix);
            tabDQ02.Controls.Add(lblDQ02Value);
            tabDQ02.Controls.Add(lblDQ02Secondary);
            tabDQ02.Controls.Add(tbDQ02UserNominal);
            tabDQ02.Controls.Add(tbDQ02UserTolerance);
            tabDQ02.Controls.Add(lblDQ02Deviation);
            tabDQ02.Controls.Add(lblDQ02PassFail);
            tabDQ02.Location = new Point(4, 32);
            tabDQ02.Name = "tabDQ02";
            tabDQ02.Padding = new Padding(3);
            tabDQ02.Size = new Size(751, 375);
            tabDQ02.TabIndex = 12;
            tabDQ02.Text = "ZOYI DQ02";
            // 
            // button7
            // 
            button7.BackColor = Color.FromArgb(0, 64, 64);
            button7.Cursor = Cursors.Hand;
            button7.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button7.ForeColor = Color.Cyan;
            button7.Location = new Point(185, 341);
            button7.Name = "button7";
            button7.Size = new Size(155, 28);
            button7.TabIndex = 33;
            button7.Text = "X D Q 0 R";
            button7.UseVisualStyleBackColor = false;
            button7.Click += button7_Click;
            // 
            // button6
            // 
            button6.BackColor = Color.FromArgb(0, 64, 64);
            button6.Cursor = Cursors.Hand;
            button6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button6.ForeColor = Color.Cyan;
            button6.Location = new Point(18, 341);
            button6.Name = "button6";
            button6.Size = new Size(155, 28);
            button6.TabIndex = 32;
            button6.Text = "Tryb: AUTO";
            button6.UseVisualStyleBackColor = false;
            button6.Click += button6_Click;
            // 
            // button5
            // 
            button5.BackColor = Color.FromArgb(0, 64, 64);
            button5.Cursor = Cursors.Hand;
            button5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button5.ForeColor = Color.Cyan;
            button5.Location = new Point(200, 58);
            button5.Name = "button5";
            button5.Size = new Size(155, 28);
            button5.TabIndex = 30;
            button5.Text = "Change Level";
            button5.UseVisualStyleBackColor = false;
            button5.Click += button5_Click;
            // 
            // tbDQ02Log
            // 
            tbDQ02Log.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tbDQ02Log.BackColor = Color.FromArgb(13, 13, 13);
            tbDQ02Log.Font = new Font("Consolas", 8F);
            tbDQ02Log.ForeColor = Color.FromArgb(0, 200, 0);
            tbDQ02Log.Location = new Point(364, 210);
            tbDQ02Log.Multiline = true;
            tbDQ02Log.Name = "tbDQ02Log";
            tbDQ02Log.ReadOnly = true;
            tbDQ02Log.ScrollBars = ScrollBars.Vertical;
            tbDQ02Log.Size = new Size(371, 128);
            tbDQ02Log.TabIndex = 6;
            tbDQ02Log.WordWrap = false;
            // 
            // btnDQ02Connect
            // 
            btnDQ02Connect.BackColor = Color.FromArgb(0, 64, 0);
            btnDQ02Connect.Cursor = Cursors.Hand;
            btnDQ02Connect.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDQ02Connect.ForeColor = Color.LightGreen;
            btnDQ02Connect.Location = new Point(580, 6);
            btnDQ02Connect.Name = "btnDQ02Connect";
            btnDQ02Connect.Size = new Size(155, 28);
            btnDQ02Connect.TabIndex = 7;
            btnDQ02Connect.Text = "POŁĄCZ";
            btnDQ02Connect.UseVisualStyleBackColor = false;
            btnDQ02Connect.Click += btnDQ02Connect_Click;
            // 
            // btnDQ02ClearLog
            // 
            btnDQ02ClearLog.BackColor = Color.FromArgb(64, 0, 0);
            btnDQ02ClearLog.Cursor = Cursors.Hand;
            btnDQ02ClearLog.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnDQ02ClearLog.ForeColor = Color.LightCoral;
            btnDQ02ClearLog.Location = new Point(564, 344);
            btnDQ02ClearLog.Name = "btnDQ02ClearLog";
            btnDQ02ClearLog.Size = new Size(70, 28);
            btnDQ02ClearLog.TabIndex = 8;
            btnDQ02ClearLog.Text = "CLEAR";
            btnDQ02ClearLog.UseVisualStyleBackColor = false;
            btnDQ02ClearLog.Click += btnDQ02ClearLog_Click;
            // 
            // btnDQ02SaveLog
            // 
            btnDQ02SaveLog.BackColor = Color.FromArgb(0, 0, 64);
            btnDQ02SaveLog.Cursor = Cursors.Hand;
            btnDQ02SaveLog.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnDQ02SaveLog.ForeColor = Color.CornflowerBlue;
            btnDQ02SaveLog.Location = new Point(650, 344);
            btnDQ02SaveLog.Name = "btnDQ02SaveLog";
            btnDQ02SaveLog.Size = new Size(70, 28);
            btnDQ02SaveLog.TabIndex = 9;
            btnDQ02SaveLog.Text = "SAVE";
            btnDQ02SaveLog.UseVisualStyleBackColor = false;
            btnDQ02SaveLog.Click += btnDQ02SaveLog_Click;
            // 
            // btnDQ02Refresh
            // 
            btnDQ02Refresh.BackColor = Color.FromArgb(32, 32, 32);
            btnDQ02Refresh.Cursor = Cursors.Hand;
            btnDQ02Refresh.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnDQ02Refresh.ForeColor = Color.LightGray;
            btnDQ02Refresh.Location = new Point(580, 73);
            btnDQ02Refresh.Name = "btnDQ02Refresh";
            btnDQ02Refresh.Size = new Size(155, 28);
            btnDQ02Refresh.TabIndex = 10;
            btnDQ02Refresh.Text = "REFRESH";
            btnDQ02Refresh.UseVisualStyleBackColor = false;
            btnDQ02Refresh.Click += btnDQ02RefreshPorts_Click;
            // 
            // button3
            // 
            button3.BackColor = Color.FromArgb(0, 64, 64);
            button3.Cursor = Cursors.Hand;
            button3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button3.ForeColor = Color.Cyan;
            button3.Location = new Point(10, 58);
            button3.Name = "button3";
            button3.Size = new Size(155, 28);
            button3.TabIndex = 28;
            button3.Text = "Change Frequency";
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.BackColor = Color.FromArgb(64, 0, 64);
            button4.Cursor = Cursors.Hand;
            button4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button4.ForeColor = Color.Magenta;
            button4.Location = new Point(516, 214);
            button4.Name = "button4";
            button4.Size = new Size(155, 28);
            button4.TabIndex = 29;
            button4.Text = "Change Level";
            button4.UseVisualStyleBackColor = false;
            button4.Click += button4_Click;
            // 
            // lbDQ02Ports
            // 
            lbDQ02Ports.BackColor = Color.FromArgb(24, 24, 24);
            lbDQ02Ports.Font = new Font("Consolas", 10F);
            lbDQ02Ports.ForeColor = Color.Gold;
            lbDQ02Ports.ItemHeight = 15;
            lbDQ02Ports.Location = new Point(580, 107);
            lbDQ02Ports.Name = "lbDQ02Ports";
            lbDQ02Ports.Size = new Size(155, 49);
            lbDQ02Ports.TabIndex = 11;
            // 
            // tbDQ02Baud
            // 
            tbDQ02Baud.BackColor = Color.FromArgb(24, 24, 24);
            tbDQ02Baud.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            tbDQ02Baud.ForeColor = Color.Gold;
            tbDQ02Baud.Location = new Point(580, 40);
            tbDQ02Baud.Name = "tbDQ02Baud";
            tbDQ02Baud.Size = new Size(155, 27);
            tbDQ02Baud.TabIndex = 12;
            tbDQ02Baud.Text = "115200";
            tbDQ02Baud.TextAlign = HorizontalAlignment.Center;
            // 
            // lblDQ02Functions
            // 
            lblDQ02Functions.AutoSize = true;
            lblDQ02Functions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDQ02Functions.ForeColor = Color.FromArgb(180, 220, 255);
            lblDQ02Functions.Location = new Point(10, 3);
            lblDQ02Functions.Name = "lblDQ02Functions";
            lblDQ02Functions.Size = new Size(78, 15);
            lblDQ02Functions.TabIndex = 13;
            lblDQ02Functions.Text = "Functions: —";
            // 
            // lblDQ02Speed
            // 
            lblDQ02Speed.AutoSize = true;
            lblDQ02Speed.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDQ02Speed.ForeColor = Color.FromArgb(180, 220, 255);
            lblDQ02Speed.Location = new Point(200, 6);
            lblDQ02Speed.Name = "lblDQ02Speed";
            lblDQ02Speed.Size = new Size(60, 15);
            lblDQ02Speed.TabIndex = 14;
            lblDQ02Speed.Text = "Speed: —";
            // 
            // lblDQ02Model
            // 
            lblDQ02Model.AutoSize = true;
            lblDQ02Model.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDQ02Model.ForeColor = Color.FromArgb(180, 220, 255);
            lblDQ02Model.Location = new Point(380, 6);
            lblDQ02Model.Name = "lblDQ02Model";
            lblDQ02Model.Size = new Size(60, 15);
            lblDQ02Model.TabIndex = 15;
            lblDQ02Model.Text = "Model: —";
            // 
            // lblDQ02Freq
            // 
            lblDQ02Freq.AutoSize = true;
            lblDQ02Freq.Font = new Font("Segoe UI", 9F);
            lblDQ02Freq.ForeColor = Color.FromArgb(200, 200, 200);
            lblDQ02Freq.Location = new Point(8, 35);
            lblDQ02Freq.Name = "lblDQ02Freq";
            lblDQ02Freq.Size = new Size(80, 15);
            lblDQ02Freq.TabIndex = 16;
            lblDQ02Freq.Text = "Frequency: —";
            // 
            // lblDQ02Level
            // 
            lblDQ02Level.AutoSize = true;
            lblDQ02Level.Font = new Font("Segoe UI", 9F);
            lblDQ02Level.ForeColor = Color.FromArgb(200, 200, 200);
            lblDQ02Level.Location = new Point(200, 34);
            lblDQ02Level.Name = "lblDQ02Level";
            lblDQ02Level.Size = new Size(52, 15);
            lblDQ02Level.TabIndex = 17;
            lblDQ02Level.Text = "Level: —";
            // 
            // lblDQ02Nominal
            // 
            lblDQ02Nominal.AutoSize = true;
            lblDQ02Nominal.Font = new Font("Segoe UI", 9F);
            lblDQ02Nominal.ForeColor = Color.FromArgb(200, 200, 200);
            lblDQ02Nominal.Location = new Point(380, 35);
            lblDQ02Nominal.Name = "lblDQ02Nominal";
            lblDQ02Nominal.Size = new Size(71, 15);
            lblDQ02Nominal.TabIndex = 18;
            lblDQ02Nominal.Text = "Nominal: —";
            // 
            // lblDQ02LossParam
            // 
            lblDQ02LossParam.AutoSize = true;
            lblDQ02LossParam.Font = new Font("Segoe UI", 9F);
            lblDQ02LossParam.ForeColor = Color.FromArgb(200, 200, 200);
            lblDQ02LossParam.Location = new Point(6, 99);
            lblDQ02LossParam.Name = "lblDQ02LossParam";
            lblDQ02LossParam.Size = new Size(113, 15);
            lblDQ02LossParam.TabIndex = 19;
            lblDQ02LossParam.Text = "Parameters: Loss: —";
            // 
            // lblDQ02Range
            // 
            lblDQ02Range.AutoSize = true;
            lblDQ02Range.Font = new Font("Segoe UI", 9F);
            lblDQ02Range.ForeColor = Color.FromArgb(200, 200, 200);
            lblDQ02Range.Location = new Point(200, 99);
            lblDQ02Range.Name = "lblDQ02Range";
            lblDQ02Range.Size = new Size(58, 15);
            lblDQ02Range.TabIndex = 20;
            lblDQ02Range.Text = "Range: —";
            // 
            // lblDQ02Output
            // 
            lblDQ02Output.AutoSize = true;
            lblDQ02Output.Font = new Font("Segoe UI", 9F);
            lblDQ02Output.ForeColor = Color.FromArgb(200, 200, 200);
            lblDQ02Output.Location = new Point(380, 65);
            lblDQ02Output.Name = "lblDQ02Output";
            lblDQ02Output.Size = new Size(63, 15);
            lblDQ02Output.TabIndex = 21;
            lblDQ02Output.Text = "Output: —";
            // 
            // lblDQ02Comparison
            // 
            lblDQ02Comparison.AutoSize = true;
            lblDQ02Comparison.Font = new Font("Segoe UI", 9F);
            lblDQ02Comparison.ForeColor = Color.FromArgb(200, 200, 200);
            lblDQ02Comparison.Location = new Point(10, 141);
            lblDQ02Comparison.Name = "lblDQ02Comparison";
            lblDQ02Comparison.Size = new Size(90, 15);
            lblDQ02Comparison.TabIndex = 22;
            lblDQ02Comparison.Text = "Comparison: —";
            // 
            // lblDQ02Bias
            // 
            lblDQ02Bias.AutoSize = true;
            lblDQ02Bias.Font = new Font("Segoe UI", 9F);
            lblDQ02Bias.ForeColor = Color.FromArgb(200, 200, 200);
            lblDQ02Bias.Location = new Point(200, 136);
            lblDQ02Bias.Name = "lblDQ02Bias";
            lblDQ02Bias.Size = new Size(46, 15);
            lblDQ02Bias.TabIndex = 23;
            lblDQ02Bias.Text = "Bias: —";
            // 
            // lblDQ02Tolerance
            // 
            lblDQ02Tolerance.AutoSize = true;
            lblDQ02Tolerance.Font = new Font("Segoe UI", 9F);
            lblDQ02Tolerance.ForeColor = Color.FromArgb(200, 200, 200);
            lblDQ02Tolerance.Location = new Point(380, 95);
            lblDQ02Tolerance.Name = "lblDQ02Tolerance";
            lblDQ02Tolerance.Size = new Size(76, 15);
            lblDQ02Tolerance.TabIndex = 24;
            lblDQ02Tolerance.Text = "Tolerance: —";
            // 
            // lblDQ02Prefix
            // 
            lblDQ02Prefix.AutoSize = true;
            lblDQ02Prefix.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblDQ02Prefix.ForeColor = Color.Gold;
            lblDQ02Prefix.Location = new Point(40, 227);
            lblDQ02Prefix.Name = "lblDQ02Prefix";
            lblDQ02Prefix.Size = new Size(60, 51);
            lblDQ02Prefix.TabIndex = 25;
            lblDQ02Prefix.Text = "—";
            // 
            // lblDQ02Value
            // 
            lblDQ02Value.AutoSize = true;
            lblDQ02Value.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblDQ02Value.ForeColor = Color.Gold;
            lblDQ02Value.Location = new Point(128, 227);
            lblDQ02Value.Name = "lblDQ02Value";
            lblDQ02Value.Size = new Size(60, 51);
            lblDQ02Value.TabIndex = 26;
            lblDQ02Value.Text = "—";
            // 
            // lblDQ02Secondary
            // 
            lblDQ02Secondary.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblDQ02Secondary.ForeColor = Color.Cyan;
            lblDQ02Secondary.Location = new Point(40, 281);
            lblDQ02Secondary.Name = "lblDQ02Secondary";
            lblDQ02Secondary.Size = new Size(133, 57);
            lblDQ02Secondary.TabIndex = 27;
            lblDQ02Secondary.Text = "0.000";
            // 
            // tbDQ02UserNominal
            // 
            tbDQ02UserNominal.BackColor = Color.FromArgb(24, 24, 24);
            tbDQ02UserNominal.Font = new Font("Segoe UI", 9F);
            tbDQ02UserNominal.ForeColor = Color.Gold;
            tbDQ02UserNominal.Location = new Point(10, 167);
            tbDQ02UserNominal.Name = "tbDQ02UserNominal";
            tbDQ02UserNominal.Size = new Size(80, 23);
            tbDQ02UserNominal.TabIndex = 28;
            tbDQ02UserNominal.TextAlign = HorizontalAlignment.Center;
            // 
            // tbDQ02UserTolerance
            // 
            tbDQ02UserTolerance.BackColor = Color.FromArgb(24, 24, 24);
            tbDQ02UserTolerance.Font = new Font("Segoe UI", 9F);
            tbDQ02UserTolerance.ForeColor = Color.Gold;
            tbDQ02UserTolerance.Location = new Point(115, 167);
            tbDQ02UserTolerance.Name = "tbDQ02UserTolerance";
            tbDQ02UserTolerance.Size = new Size(50, 23);
            tbDQ02UserTolerance.TabIndex = 29;
            tbDQ02UserTolerance.TextAlign = HorizontalAlignment.Center;
            // 
            // lblDQ02Deviation
            // 
            lblDQ02Deviation.AutoSize = true;
            lblDQ02Deviation.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDQ02Deviation.ForeColor = Color.FromArgb(200, 200, 200);
            lblDQ02Deviation.Location = new Point(10, 193);
            lblDQ02Deviation.Name = "lblDQ02Deviation";
            lblDQ02Deviation.Size = new Size(79, 15);
            lblDQ02Deviation.TabIndex = 30;
            lblDQ02Deviation.Text = "Deviation: —";
            // 
            // lblDQ02PassFail
            // 
            lblDQ02PassFail.AutoSize = true;
            lblDQ02PassFail.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDQ02PassFail.ForeColor = Color.FromArgb(200, 200, 200);
            lblDQ02PassFail.Location = new Point(185, 175);
            lblDQ02PassFail.Name = "lblDQ02PassFail";
            lblDQ02PassFail.Size = new Size(19, 15);
            lblDQ02PassFail.TabIndex = 31;
            lblDQ02PassFail.Text = "—";
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(13, 13, 13);
            ClientSize = new Size(778, 479);
            Controls.Add(lblComConnStatus);
            Controls.Add(label8);
            Controls.Add(pictureBox1);
            Controls.Add(label4);
            Controls.Add(tabControl1);
            Controls.Add(btnShortcuts);
            Controls.Add(btnMinimize);
            Controls.Add(label1);
            Controls.Add(button1);
            ForeColor = SystemColors.HighlightText;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2);
            MaximizeBox = false;
            Name = "MainWindow";
            StartPosition = FormStartPosition.Manual;
            Text = "ZOYI Terminal";
            FormClosed += MainWindow_FormClosed;
            MouseDown += MainWindow_MouseDown;
            MouseMove += MainWindow_MouseMove;
            MouseUp += MainWindow_MouseUp;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbESR).EndInit();
            tabTools.ResumeLayout(false);
            tabTools.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tbarThicksCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)tbarArcTicks).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tbarPanelOpacity).EndInit();
            tabPageCOM.ResumeLayout(false);
            tabPageCOM.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPageWYKRES.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabWebServer.ResumeLayout(false);
            tabWebServer.PerformLayout();
            tabDQ02.ResumeLayout(false);
            tabDQ02.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button button1;
        private Label label1;
        private Button btnMinimize;
        private Button btnShortcuts;
        private Label label4;
        private PictureBox pictureBox1;
        private PictureBox pbESR;
        private Label label8;
        private Label lblComConnStatus;
        private TabPage tabPage4;
        private TabPage tabTools;
        private Button btnToolsRefresh;
        private Button btnToolsEdit;
        private Panel panelTools;
        private TabPage tabPage2;
        private GroupBox groupBox3;
        private Label lblThicksCount;
        private TrackBar tbarThicksCount;
        private Label label11;
        private Label lblArcTicks;
        private TrackBar tbarArcTicks;
        private Label label10;
        private GroupBox groupBox1;
        private Label label5;
        private TrackBar tbarPanelOpacity;
        private Button btnPanelColorBg;
        private Button btnPanelColorLabel;
        private Button btnPanelColorValue;
        private TabPage tabPageCOM;
        private CheckBox chbAdvancedPanel;
        private GroupBox groupBox2;
        private RadioButton rbCOMparseExt;
        private Button button2;
        private RadioButton rbCOMparseRaw;
        private RadioButton rbCOMparseLua;
        private RadioButton rbCOMparseStd;
        private TextBox tbComOutput;
        private TextBox tbCOMBaudrate;
        private Button btnClearLog;
        private Button btnSaveLog;
        private CheckBox chbStandardPanel;
        private Button btnListCOM;
        private ListBox lbListCOMs;
        private Button btnComConnect;
        private Label lblBaudRate;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage5;
        private TabPage tabPage6;
        private TabPage tabPage3;
        private TabPage tabPageWYKRES;
        private ChartPanel chartPanel;
        private Button btnChartPause;
        private Button btnChartClear;
        private Button btnTime10s;
        private Button btnTime30s;
        private Button btnTime60s;
        private Button btnTime5min;
        private TextBox textBox1;
        private Label labelTTS;
        private CheckBox chbTTSSwitch;
        private ComboBox cbTTSVoice;
        private Label lblTTSStatus;
        private TabPage tabWebServer;
        private TabPage tabDQ02;
        private Button btnDQ02Connect;
        private Button btnDQ02ClearLog;
        private Button btnDQ02SaveLog;
        private ListBox lbDQ02Ports;
        private TextBox tbDQ02Baud;
        private TextBox tbDQ02Log;
        private Label lblDQ02Functions;
        private Label lblDQ02Speed;
        private Label lblDQ02Model;
        private Button btnDQ02Refresh;
        private Button button3;
        private Button button4;
        private Label lblDQ02Freq;
        private Label lblDQ02Level;
        private Label lblDQ02Nominal;
        private Label lblDQ02LossParam;
        private Label lblDQ02Range;
        private Label lblDQ02Output;
        private Label lblDQ02Comparison;
        private Label lblDQ02Bias;
        private Label lblDQ02Tolerance;
        private Label lblDQ02Prefix;
        private Label lblDQ02Value;
        private Label lblDQ02Secondary;
        private TextBox tbDQ02UserNominal;
        private TextBox tbDQ02UserTolerance;
        private Label lblDQ02Deviation;
        private Label lblDQ02PassFail;
        private Button btnWebServerStart;
        private Button btnWebServerStop;
        private Label label2;
        private TextBox tbWebServerPort;
        private Label label3;
        private LinkLabel llWebAddress;
        private Button button5;
        private Button button6;
        private Button button7;
    }
}
