namespace ZOYI
{
    partial class StandardDisplayPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StandardDisplayPanel));
            lblLabel = new Label();
            lblValue = new Label();
            panelResize = new Panel();
            tableLayoutPanel = new TableLayoutPanel();
            label3 = new Label();
            label1 = new Label();
            lblFreq = new Label();
            label2 = new Label();
            lblMode = new Label();
            contextMenuStdDisp = new ContextMenuStrip(components);
            toolStripMenuItemLabel = new ToolStripMenuItem();
            toolStripMenuItemFontLabel = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripMenuItemValue = new ToolStripMenuItem();
            toolStripMenuItemFontValue = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripMenuItemFooter = new ToolStripMenuItem();
            toolStripMenuItemFontFooter = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            zamknijPaneToolStripMenuItem = new ToolStripMenuItem();
            fontDialog = new FontDialog();
            tableLayoutPanel1 = new TableLayoutPanel();
            label7 = new Label();
            label6 = new Label();
            label8 = new Label();
            dateTimePicker1 = new DateTimePicker();
            tableLayoutPanel.SuspendLayout();
            contextMenuStdDisp.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblLabel
            // 
            lblLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblLabel.BorderStyle = BorderStyle.FixedSingle;
            lblLabel.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblLabel.ForeColor = SystemColors.HighlightText;
            lblLabel.Location = new Point(293, 0);
            lblLabel.Margin = new Padding(2, 0, 2, 0);
            lblLabel.Name = "lblLabel";
            lblLabel.Size = new Size(282, 91);
            lblLabel.TabIndex = 0;
            lblLabel.Text = "ZOYI ZT- 703S";
            lblLabel.TextAlign = ContentAlignment.MiddleCenter;
            lblLabel.MouseDown += displayPanel_MouseDown;
            lblLabel.MouseMove += displayPanel_MouseMove;
            lblLabel.MouseUp += displayPanel_MouseUp;
            // 
            // lblValue
            // 
            lblValue.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblValue.BorderStyle = BorderStyle.FixedSingle;
            lblValue.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblValue.ForeColor = SystemColors.HighlightText;
            lblValue.Location = new Point(2, 0);
            lblValue.Margin = new Padding(2, 0, 2, 0);
            lblValue.Name = "lblValue";
            lblValue.Size = new Size(287, 91);
            lblValue.TabIndex = 1;
            lblValue.Text = "----------";
            lblValue.TextAlign = ContentAlignment.MiddleCenter;
            lblValue.MouseDown += displayPanel_MouseDown;
            lblValue.MouseMove += displayPanel_MouseMove;
            lblValue.MouseUp += displayPanel_MouseUp;
            // 
            // panelResize
            // 
            panelResize.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panelResize.Cursor = Cursors.SizeNWSE;
            panelResize.Location = new Point(1563, 122);
            panelResize.Margin = new Padding(2);
            panelResize.Name = "panelResize";
            panelResize.Size = new Size(23, 19);
            panelResize.TabIndex = 2;
            panelResize.MouseDown += panelResize_MouseDown;
            panelResize.MouseMove += panelResize_MouseMove;
            panelResize.MouseUp += panelResize_MouseUp;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 7;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 291F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 286F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 147F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 148F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 168F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 8F));
            tableLayoutPanel.Controls.Add(label3, 6, 0);
            tableLayoutPanel.Controls.Add(label1, 4, 0);
            tableLayoutPanel.Controls.Add(lblValue, 0, 0);
            tableLayoutPanel.Controls.Add(lblLabel, 1, 0);
            tableLayoutPanel.Controls.Add(lblFreq, 2, 0);
            tableLayoutPanel.Controls.Add(label2, 5, 0);
            tableLayoutPanel.Controls.Add(lblMode, 3, 0);
            tableLayoutPanel.Location = new Point(2, 45);
            tableLayoutPanel.Margin = new Padding(2);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 1;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel.Size = new Size(1586, 91);
            tableLayoutPanel.TabIndex = 3;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label3.AutoEllipsis = true;
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.BorderStyle = BorderStyle.FixedSingle;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label3.Location = new Point(1212, 0);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(372, 91);
            label3.TabIndex = 8;
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.BorderStyle = BorderStyle.FixedSingle;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label1.Location = new Point(874, 0);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(164, 91);
            label1.TabIndex = 6;
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.Click += label1_Click;
            // 
            // lblFreq
            // 
            lblFreq.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblFreq.AutoSize = true;
            lblFreq.BackColor = Color.Transparent;
            lblFreq.BorderStyle = BorderStyle.FixedSingle;
            lblFreq.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 238);
            lblFreq.Location = new Point(579, 0);
            lblFreq.Margin = new Padding(2, 0, 2, 0);
            lblFreq.Name = "lblFreq";
            lblFreq.Size = new Size(143, 91);
            lblFreq.TabIndex = 4;
            lblFreq.Text = "----";
            lblFreq.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.BorderStyle = BorderStyle.FixedSingle;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label2.Location = new Point(1042, 0);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(166, 91);
            label2.TabIndex = 7;
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblMode
            // 
            lblMode.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblMode.AutoSize = true;
            lblMode.BackColor = Color.Transparent;
            lblMode.BorderStyle = BorderStyle.FixedSingle;
            lblMode.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 238);
            lblMode.Location = new Point(726, 0);
            lblMode.Margin = new Padding(2, 0, 2, 0);
            lblMode.Name = "lblMode";
            lblMode.Size = new Size(144, 91);
            lblMode.TabIndex = 5;
            lblMode.Text = "----";
            lblMode.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // contextMenuStdDisp
            // 
            contextMenuStdDisp.BackColor = Color.Black;
            contextMenuStdDisp.ImageScalingSize = new Size(24, 24);
            contextMenuStdDisp.Items.AddRange(new ToolStripItem[] { toolStripMenuItemLabel, toolStripMenuItemFontLabel, toolStripSeparator2, toolStripMenuItemValue, toolStripMenuItemFontValue, toolStripSeparator1, toolStripMenuItemFooter, toolStripMenuItemFontFooter, toolStripSeparator3, zamknijPaneToolStripMenuItem });
            contextMenuStdDisp.Name = "contextMenuStdDisp";
            contextMenuStdDisp.RenderMode = ToolStripRenderMode.System;
            contextMenuStdDisp.Size = new Size(164, 176);
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
            toolStripMenuItemLabel.Size = new Size(163, 22);
            toolStripMenuItemLabel.Text = "OPIS";
            toolStripMenuItemLabel.CheckedChanged += toolStripMenuItem_CheckedChanged;
            // 
            // toolStripMenuItemFontLabel
            // 
            toolStripMenuItemFontLabel.BackColor = Color.FromArgb(64, 64, 64);
            toolStripMenuItemFontLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            toolStripMenuItemFontLabel.ForeColor = Color.LemonChiffon;
            toolStripMenuItemFontLabel.Name = "toolStripMenuItemFontLabel";
            toolStripMenuItemFontLabel.Size = new Size(163, 22);
            toolStripMenuItemFontLabel.Text = "Czcionka";
            toolStripMenuItemFontLabel.Click += toolStripMenuItemFontLabel_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(160, 6);
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
            toolStripMenuItemValue.Size = new Size(163, 22);
            toolStripMenuItemValue.Text = "WARTOŚC";
            toolStripMenuItemValue.CheckedChanged += toolStripMenuItem_CheckedChanged;
            // 
            // toolStripMenuItemFontValue
            // 
            toolStripMenuItemFontValue.BackColor = Color.FromArgb(64, 64, 64);
            toolStripMenuItemFontValue.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            toolStripMenuItemFontValue.ForeColor = Color.LemonChiffon;
            toolStripMenuItemFontValue.Name = "toolStripMenuItemFontValue";
            toolStripMenuItemFontValue.Size = new Size(163, 22);
            toolStripMenuItemFontValue.Text = "Czcionka";
            toolStripMenuItemFontValue.Click += toolStripMenuItemFontValue_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(160, 6);
            // 
            // toolStripMenuItemFooter
            // 
            toolStripMenuItemFooter.BackColor = Color.FromArgb(64, 64, 64);
            toolStripMenuItemFooter.Checked = true;
            toolStripMenuItemFooter.CheckOnClick = true;
            toolStripMenuItemFooter.CheckState = CheckState.Checked;
            toolStripMenuItemFooter.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            toolStripMenuItemFooter.ForeColor = Color.LemonChiffon;
            toolStripMenuItemFooter.Name = "toolStripMenuItemFooter";
            toolStripMenuItemFooter.Size = new Size(163, 22);
            toolStripMenuItemFooter.Text = "FUNKCJE";
            toolStripMenuItemFooter.CheckedChanged += toolStripMenuItem_CheckedChanged;
            // 
            // toolStripMenuItemFontFooter
            // 
            toolStripMenuItemFontFooter.BackColor = Color.FromArgb(64, 64, 64);
            toolStripMenuItemFontFooter.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            toolStripMenuItemFontFooter.ForeColor = Color.LemonChiffon;
            toolStripMenuItemFontFooter.Name = "toolStripMenuItemFontFooter";
            toolStripMenuItemFontFooter.Size = new Size(163, 22);
            toolStripMenuItemFontFooter.Text = "Czcionka";
            toolStripMenuItemFontFooter.Click += toolStripMenuItemFontFooter_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(160, 6);
            // 
            // zamknijPaneToolStripMenuItem
            // 
            zamknijPaneToolStripMenuItem.BackColor = Color.FromArgb(64, 64, 64);
            zamknijPaneToolStripMenuItem.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            zamknijPaneToolStripMenuItem.ForeColor = Color.Yellow;
            zamknijPaneToolStripMenuItem.Name = "zamknijPaneToolStripMenuItem";
            zamknijPaneToolStripMenuItem.Size = new Size(163, 22);
            zamknijPaneToolStripMenuItem.Text = "Zamknij panel";
            zamknijPaneToolStripMenuItem.Click += zamknijPanelToolStripMenuItem_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = Color.FromArgb(64, 64, 64);
            tableLayoutPanel1.ColumnCount = 5;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 578F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 296F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 166F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 167F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 247F));
            tableLayoutPanel1.Controls.Add(label7, 1, 0);
            tableLayoutPanel1.Controls.Add(label6, 0, 0);
            tableLayoutPanel1.Controls.Add(label8, 2, 0);
            tableLayoutPanel1.Controls.Add(dateTimePicker1, 4, 0);
            tableLayoutPanel1.Location = new Point(2, 2);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(1586, 39);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // label7
            // 
            label7.BackColor = Color.FromArgb(0, 0, 64);
            label7.BorderStyle = BorderStyle.FixedSingle;
            label7.Dock = DockStyle.Top;
            label7.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            label7.ForeColor = Color.Lime;
            label7.Location = new Point(580, 0);
            label7.Margin = new Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new Size(292, 36);
            label7.TabIndex = 15;
            label7.Text = "KRIS® VERSION";
            label7.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.BackColor = Color.FromArgb(34, 34, 34);
            label6.BorderStyle = BorderStyle.FixedSingle;
            label6.Dock = DockStyle.Top;
            label6.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            label6.ForeColor = Color.Yellow;
            label6.Location = new Point(2, 0);
            label6.Margin = new Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new Size(574, 36);
            label6.TabIndex = 13;
            label6.Text = "ZOYI®        ZT-703S         OSCILLOSCOPE MULTIMETER";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            label8.BackColor = Color.Yellow;
            label8.BorderStyle = BorderStyle.FixedSingle;
            label8.Dock = DockStyle.Top;
            label8.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            label8.ForeColor = Color.Navy;
            label8.Location = new Point(876, 0);
            label8.Margin = new Padding(2, 0, 2, 0);
            label8.Name = "label8";
            label8.Size = new Size(162, 36);
            label8.TabIndex = 16;
            label8.Text = "GG 8772666";
            label8.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.CalendarFont = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            dateTimePicker1.Cursor = Cursors.Hand;
            dateTimePicker1.Dock = DockStyle.Top;
            dateTimePicker1.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 238);
            dateTimePicker1.Location = new Point(1210, 3);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(373, 33);
            dateTimePicker1.TabIndex = 17;
            dateTimePicker1.TabStop = false;
            dateTimePicker1.Value = new DateTime(2025, 12, 21, 0, 0, 0, 0);
            dateTimePicker1.ValueChanged += dateTimePicker1_ValueChanged;
            // 
            // StandardDisplayPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Desktop;
            ClientSize = new Size(1586, 140);
            ContextMenuStrip = contextMenuStdDisp;
            Controls.Add(tableLayoutPanel1);
            Controls.Add(tableLayoutPanel);
            Controls.Add(panelResize);
            ForeColor = SystemColors.HighlightText;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2);
            Name = "StandardDisplayPanel";
            StartPosition = FormStartPosition.CenterParent;
            Text = "displayPanel";
            FormClosed += DisplayPanel_FormClosed;
            MouseDown += displayPanel_MouseDown;
            MouseMove += displayPanel_MouseMove;
            MouseUp += displayPanel_MouseUp;
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            contextMenuStdDisp.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Label lblLabel;
        private Label lblValue;
        private Panel panelResize;
        private TableLayoutPanel tableLayoutPanel;
        private ContextMenuStrip contextMenuStdDisp;
        private ToolStripMenuItem toolStripMenuItemValue;
        private ToolStripMenuItem toolStripMenuItemLabel;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem zamknijPaneToolStripMenuItem;
        private Label lblFreq;
        private Label lblMode;
        private ToolStripMenuItem toolStripMenuItemFontLabel;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem toolStripMenuItemFontValue;
        private FontDialog fontDialog;
        private ToolStripMenuItem toolStripMenuItemFooter;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem toolStripMenuItemFontFooter;
        private Label label1;
        private Label label3;
        private Label label2;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label6;
        private Label label7;
        private Label label8;
        private DateTimePicker dateTimePicker1;
    }
}