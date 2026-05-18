namespace ZOYI
{
    partial class RichEditor
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
            rtbEditor = new RichTextBox();
            btnRichEditorSave = new Button();
            btnRichEditorCancel = new Button();
            button1 = new Button();
            button2 = new Button();
            SuspendLayout();
            // 
            // rtbEditor
            // 
            rtbEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbEditor.BackColor = Color.FromArgb(255, 255, 192);
            rtbEditor.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
            rtbEditor.ForeColor = Color.FromArgb(0, 0, 64);
            rtbEditor.Location = new Point(11, 11);
            rtbEditor.Margin = new Padding(2);
            rtbEditor.Name = "rtbEditor";
            rtbEditor.Size = new Size(695, 429);
            rtbEditor.TabIndex = 0;
            rtbEditor.Text = "";
            // 
            // btnRichEditorSave
            // 
            btnRichEditorSave.Anchor = AnchorStyles.Bottom;
            btnRichEditorSave.Location = new Point(201, 528);
            btnRichEditorSave.Margin = new Padding(2);
            btnRichEditorSave.Name = "btnRichEditorSave";
            btnRichEditorSave.Size = new Size(140, 20);
            btnRichEditorSave.TabIndex = 1;
            btnRichEditorSave.Text = "Zapisz";
            btnRichEditorSave.UseVisualStyleBackColor = true;
            btnRichEditorSave.Click += btnRichEditorSave_Click;
            // 
            // btnRichEditorCancel
            // 
            btnRichEditorCancel.Anchor = AnchorStyles.Bottom;
            btnRichEditorCancel.Location = new Point(374, 528);
            btnRichEditorCancel.Margin = new Padding(2);
            btnRichEditorCancel.Name = "btnRichEditorCancel";
            btnRichEditorCancel.Size = new Size(140, 20);
            btnRichEditorCancel.TabIndex = 2;
            btnRichEditorCancel.Text = "Anuluj";
            btnRichEditorCancel.UseVisualStyleBackColor = true;
            btnRichEditorCancel.Click += btnRichEditorCancel_Click;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(0, 64, 64);
            button1.Cursor = Cursors.Hand;
            button1.Font = new Font("Segoe UI", 21.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            button1.ForeColor = Color.Yellow;
            button1.Location = new Point(12, 458);
            button1.Name = "button1";
            button1.Size = new Size(204, 59);
            button1.TabIndex = 3;
            button1.Text = "ZAPIS";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.BackColor = Color.DimGray;
            button2.Cursor = Cursors.Hand;
            button2.Font = new Font("Segoe UI", 21.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
            button2.ForeColor = Color.FromArgb(255, 255, 192);
            button2.Location = new Point(502, 458);
            button2.Name = "button2";
            button2.Size = new Size(204, 59);
            button2.TabIndex = 4;
            button2.Text = "ANULUJ";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // RichEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Blue;
            ClientSize = new Size(717, 529);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(btnRichEditorCancel);
            Controls.Add(btnRichEditorSave);
            Controls.Add(rtbEditor);
            Margin = new Padding(2);
            Name = "RichEditor";
            Text = "Editor";
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox rtbEditor;
        private Button btnRichEditorSave;
        private Button btnRichEditorCancel;
        private Button button1;
        private Button button2;
    }
}