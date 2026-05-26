namespace ZOYI
{
    partial class AdancedDisplayPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdancedDisplayPanel));
            lblLabel = new Label();
            lblValue = new Label();
            panelResize = new Panel();
            arcProgressBar1 = new ArcProgressBar();
            contextMenuAdvDisp = new ContextMenuStrip(components);
            toolStripMenuItemArcBar = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripMenuItemValue = new ToolStripMenuItem();
            toolStripMenuItemValueFont = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripMenuItemLabel = new ToolStripMenuItem();
            toolStripMenuItemLabelFont = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            toolStripMenuItemBarColor = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            zamknijPanelToolStripMenuItem = new ToolStripMenuItem();
            fontDialog = new FontDialog();
            lblFreq = new Label();
            lblMode = new Label();
            contextMenuAdvDisp.SuspendLayout();
            SuspendLayout();
            // 
            // lblLabel
            // 
            lblLabel.Anchor = AnchorStyles.None;
            lblLabel.BackColor = Color.Transparent;
            lblLabel.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblLabel.ForeColor = SystemColors.HighlightText;
            lblLabel.Location = new Point(26, 248);
            lblLabel.Margin = new Padding(2, 0, 2, 0);
            lblLabel.Name = "lblLabel";
            lblLabel.Size = new Size(442, 57);
            lblLabel.TabIndex = 0;
            lblLabel.Text = "ZOYI 703S";
            lblLabel.TextAlign = ContentAlignment.MiddleCenter;
            lblLabel.MouseDown += displayPanel_MouseDown;
            lblLabel.MouseMove += displayPanel_MouseMove;
            lblLabel.MouseUp += displayPanel_MouseUp;
            // 
            // lblValue
            // 
            lblValue.Anchor = AnchorStyles.None;
            lblValue.BackColor = Color.FromArgb(0, 0, 19);
            lblValue.BorderStyle = BorderStyle.FixedSingle;
            lblValue.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblValue.ForeColor = SystemColors.HighlightText;
            lblValue.Location = new Point(26, 130);
            lblValue.Margin = new Padding(2, 0, 2, 0);
            lblValue.Name = "lblValue";
            lblValue.Size = new Size(442, 68);
            lblValue.TabIndex = 1;
            lblValue.Text = "---------";
            lblValue.TextAlign = ContentAlignment.MiddleCenter;
            lblValue.MouseDown += displayPanel_MouseDown;
            lblValue.MouseMove += displayPanel_MouseMove;
            lblValue.MouseUp += displayPanel_MouseUp;
            // 
            // panelResize
            // 
            panelResize.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panelResize.BackColor = Color.FromArgb(64, 64, 64);
            panelResize.Cursor = Cursors.SizeNWSE;
            panelResize.Enabled = false;
            panelResize.Location = new Point(487, 306);
            panelResize.Margin = new Padding(2);
            panelResize.Name = "panelResize";
            panelResize.Size = new Size(23, 19);
            panelResize.TabIndex = 2;
            panelResize.MouseDown += panelResize_MouseDown;
            panelResize.MouseMove += panelResize_MouseMove;
            panelResize.MouseUp += panelResize_MouseUp;
            // 
            // arcProgressBar1
            // 
            arcProgressBar1.BackColor = Color.Transparent;
            arcProgressBar1.BarColor1 = Color.Orange;
            arcProgressBar1.BarColor2 = Color.Red;
            arcProgressBar1.BarGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
            arcProgressBar1.BarWidth = 7F;
            arcProgressBar1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            arcProgressBar1.ForeColor = Color.DimGray;
            arcProgressBar1.LineColor = Color.FromArgb(34, 34, 34);
            arcProgressBar1.LineWidth = 1;
            arcProgressBar1.Location = new Point(13, 11);
            arcProgressBar1.MajorThicksCount = 5L;
            arcProgressBar1.Margin = new Padding(2);
            arcProgressBar1.MaximumTick = 25F;
            arcProgressBar1.MinimumSize = new Size(70, 24);
            arcProgressBar1.Name = "arcProgressBar1";
            arcProgressBar1.ProgressShape = ArcProgressBar._ProgressShape.Flat;
            arcProgressBar1.Size = new Size(472, 235);
            arcProgressBar1.TabIndex = 4;
            arcProgressBar1.Text = "arcProgressBar1";
            arcProgressBar1.TextMode = ArcProgressBar._TextMode.None;
            arcProgressBar1.Value = 57F;
            arcProgressBar1.MouseDown += displayPanel_MouseDown;
            arcProgressBar1.MouseMove += displayPanel_MouseMove;
            arcProgressBar1.MouseUp += displayPanel_MouseUp;
            // 
            // contextMenuAdvDisp
            // 
            contextMenuAdvDisp.BackColor = Color.Black;
            contextMenuAdvDisp.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            contextMenuAdvDisp.ImageScalingSize = new Size(24, 24);
            contextMenuAdvDisp.Items.AddRange(new ToolStripItem[] { toolStripMenuItemArcBar, toolStripSeparator2, toolStripMenuItemValue, toolStripMenuItemValueFont, toolStripSeparator3, toolStripMenuItemLabel, toolStripMenuItemLabelFont, toolStripSeparator4, toolStripMenuItemBarColor, toolStripSeparator1, zamknijPanelToolStripMenuItem });
            contextMenuAdvDisp.Name = "contextMenuAdvDisp";
            contextMenuAdvDisp.RenderMode = ToolStripRenderMode.System;
            contextMenuAdvDisp.ShowCheckMargin = true;
            contextMenuAdvDisp.Size = new Size(187, 154);
            // 
            // toolStripMenuItemArcBar
            // 
            toolStripMenuItemArcBar.BackColor = Color.FromArgb(64, 64, 64);
            toolStripMenuItemArcBar.Checked = true;
            toolStripMenuItemArcBar.CheckOnClick = true;
            toolStripMenuItemArcBar.CheckState = CheckState.Checked;
            toolStripMenuItemArcBar.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            toolStripMenuItemArcBar.ForeColor = Color.LemonChiffon;
            toolStripMenuItemArcBar.Name = "toolStripMenuItemArcBar";
            toolStripMenuItemArcBar.Size = new Size(186, 22);
            toolStripMenuItemArcBar.Text = "BARGRAF";
            toolStripMenuItemArcBar.CheckedChanged += toolStripMenuItem_CheckedChanged;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(183, 6);
            // 
            // toolStripMenuItemValue
            // 
            toolStripMenuItemValue.BackColor = Color.FromArgb(64, 64, 64);
            toolStripMenuItemValue.Checked = true;
            toolStripMenuItemValue.CheckOnClick = true;
            toolStripMenuItemValue.CheckState = CheckState.Checked;
            toolStripMenuItemValue.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            toolStripMenuItemValue.ForeColor = Color.LemonChiffon;
            toolStripMenuItemValue.Name = "toolStripMenuItemValue";
            toolStripMenuItemValue.Size = new Size(186, 22);
            toolStripMenuItemValue.Text = "WARTOŚĆ";
            toolStripMenuItemValue.CheckedChanged += toolStripMenuItem_CheckedChanged;
            // 
            // toolStripMenuItemValueFont
            // 
            toolStripMenuItemValueFont.BackColor = Color.FromArgb(64, 64, 64);
            toolStripMenuItemValueFont.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            toolStripMenuItemValueFont.ForeColor = Color.LemonChiffon;
            toolStripMenuItemValueFont.Name = "toolStripMenuItemValueFont";
            toolStripMenuItemValueFont.Size = new Size(186, 22);
            toolStripMenuItemValueFont.Text = "Czcionka";
            toolStripMenuItemValueFont.Click += toolStripMenuItemValueFont_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(183, 6);
            // 
            // toolStripMenuItemLabel
            // 
            toolStripMenuItemLabel.BackColor = Color.FromArgb(64, 64, 64);
            toolStripMenuItemLabel.Checked = true;
            toolStripMenuItemLabel.CheckOnClick = true;
            toolStripMenuItemLabel.CheckState = CheckState.Checked;
            toolStripMenuItemLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            toolStripMenuItemLabel.ForeColor = Color.LemonChiffon;
            toolStripMenuItemLabel.Name = "toolStripMenuItemLabel";
            toolStripMenuItemLabel.Size = new Size(186, 22);
            toolStripMenuItemLabel.Text = "OPIS";
            toolStripMenuItemLabel.CheckedChanged += toolStripMenuItem_CheckedChanged;
            // 
            // toolStripMenuItemLabelFont
            // 
            toolStripMenuItemLabelFont.BackColor = Color.FromArgb(64, 64, 64);
            toolStripMenuItemLabelFont.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            toolStripMenuItemLabelFont.ForeColor = Color.LemonChiffon;
            toolStripMenuItemLabelFont.Name = "toolStripMenuItemLabelFont";
            toolStripMenuItemLabelFont.Size = new Size(186, 22);
            toolStripMenuItemLabelFont.Text = "Czcionka";
            toolStripMenuItemLabelFont.Click += toolStripMenuItemLabelFont_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(183, 6);
            // 
            // toolStripMenuItemBarColor
            // 
            toolStripMenuItemBarColor.BackColor = Color.FromArgb(64, 64, 64);
            toolStripMenuItemBarColor.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            toolStripMenuItemBarColor.ForeColor = Color.LemonChiffon;
            toolStripMenuItemBarColor.Name = "toolStripMenuItemBarColor";
            toolStripMenuItemBarColor.Size = new Size(186, 22);
            toolStripMenuItemBarColor.Text = "Kolor paska";
            toolStripMenuItemBarColor.Click += toolStripMenuItemBarColor_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(183, 6);
            // 
            // zamknijPanelToolStripMenuItem
            // 
            zamknijPanelToolStripMenuItem.BackColor = Color.FromArgb(0, 0, 64);
            zamknijPanelToolStripMenuItem.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            zamknijPanelToolStripMenuItem.ForeColor = Color.LemonChiffon;
            zamknijPanelToolStripMenuItem.Name = "zamknijPanelToolStripMenuItem";
            zamknijPanelToolStripMenuItem.Size = new Size(186, 22);
            zamknijPanelToolStripMenuItem.Text = "Zamknij panel";
            zamknijPanelToolStripMenuItem.Click += zamknijPanelToolStripMenuItem_Click;
            // 
            // lblFreq
            // 
            lblFreq.AutoSize = true;
            lblFreq.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 238);
            lblFreq.Location = new Point(109, 212);
            lblFreq.Margin = new Padding(2, 0, 2, 0);
            lblFreq.Name = "lblFreq";
            lblFreq.Size = new Size(33, 20);
            lblFreq.TabIndex = 5;
            lblFreq.Text = "----";
            // 
            // lblMode
            // 
            lblMode.AutoSize = true;
            lblMode.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 238);
            lblMode.Location = new Point(302, 212);
            lblMode.Margin = new Padding(2, 0, 2, 0);
            lblMode.Name = "lblMode";
            lblMode.Size = new Size(33, 20);
            lblMode.TabIndex = 6;
            lblMode.Text = "----";
            // 
            // AdancedDisplayPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Desktop;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(496, 314);
            ContextMenuStrip = contextMenuAdvDisp;
            Controls.Add(lblMode);
            Controls.Add(lblFreq);
            Controls.Add(lblLabel);
            Controls.Add(lblValue);
            Controls.Add(arcProgressBar1);
            Controls.Add(panelResize);
            DoubleBuffered = true;
            ForeColor = SystemColors.HighlightText;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2);
            Name = "AdancedDisplayPanel";
            StartPosition = FormStartPosition.Manual;
            Text = "displayPanel";
            FormClosed += AdancedDisplayPanel_FormClosed;
            MouseDown += displayPanel_MouseDown;
            MouseMove += displayPanel_MouseMove;
            MouseUp += displayPanel_MouseUp;
            contextMenuAdvDisp.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblLabel;
        private Label lblValue;
        private Panel panelResize;
        private ArcProgressBar arcProgressBar1;
        private ContextMenuStrip contextMenuAdvDisp;
        private ToolStripMenuItem toolStripMenuItemArcBar;
        private ToolStripMenuItem toolStripMenuItemLabel;
        private ToolStripMenuItem toolStripMenuItemValue;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem zamknijPanelToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem toolStripMenuItemValueFont;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem toolStripMenuItemLabelFont;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem toolStripMenuItemBarColor;
        private FontDialog fontDialog;
        private Label lblFreq;
        private Label lblMode;
    }
}