using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ZOYI
{
    public class ShortcutConfigForm : Form
    {
        private readonly ShortcutManager shortcutManager;
        private readonly TableLayoutPanel tableLayoutPanel;
        private readonly Button btnSave;
        private readonly Button btnCancel;
        private readonly List<ShortcutRow> rows = new List<ShortcutRow>();

        public ShortcutConfigForm(ShortcutManager manager)
        {
            shortcutManager = manager;

            Text = "Konfiguracja skrótów klawiaturowych";
            Size = new Size(550, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(24, 24, 24);

            tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(15, 15, 15, 5),
                BackColor = Color.FromArgb(24, 24, 24)
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            var shortcuts = manager.GetShortcuts();
            foreach (var s in shortcuts)
            {
                var row = new ShortcutRow(s.Id, s.DisplayName, s.Keys, this);
                rows.Add(row);
                tableLayoutPanel.Controls.Add(row.lblName, 0, rows.Count - 1);
                tableLayoutPanel.Controls.Add(row.btnKey, 1, rows.Count - 1);
            }

            var flowButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10, 10, 10, 10),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            btnCancel = new Button
            {
                Text = "ANULUJ",
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(64, 0, 0),
                ForeColor = Color.LightCoral,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            btnSave = new Button
            {
                Text = "ZAPISZ",
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 64, 0),
                ForeColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => SaveAndClose();

            var btnClear = new Button
            {
                Text = "WYCZYŚĆ",
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(64, 64, 0),
                ForeColor = Color.Yellow,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => ClearSelected();

            flowButtons.Controls.Add(btnSave);
            flowButtons.Controls.Add(btnCancel);
            flowButtons.Controls.Add(btnClear);

            Controls.Add(tableLayoutPanel);
            Controls.Add(flowButtons);

            var lblHint = new Label
            {
                Text = "Kliknij przycisk obok akcji i naciśnij kombinację klawiszy. ESC = wyczyść skrót.",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8F),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(24, 24, 24)
            };
            Controls.Add(lblHint);
            lblHint.BringToFront();
        }

        public void StartListening(ShortcutRow row)
        {
            foreach (var r in rows)
                r.IsListening = false;

            row.IsListening = true;
            row.btnKey.BackColor = Color.FromArgb(0, 64, 64);
            row.btnKey.Text = "... naciśnij klawisz ...";
            row.btnKey.Focus();
        }

        public void StopListening(ShortcutRow row)
        {
            row.IsListening = false;
            row.btnKey.BackColor = Color.FromArgb(30, 30, 30);
        }

        private void SaveAndClose()
        {
            foreach (var row in rows)
            {
                shortcutManager.SetShortcut(row.Id, row.CurrentKeys);
            }
            shortcutManager.SaveToSettings();
            DialogResult = DialogResult.OK;
        }

        private void ClearSelected()
        {
            var focusedRow = rows.Find(r => r.btnKey.Focused);
            if (focusedRow != null)
            {
                focusedRow.CurrentKeys = Keys.None;
                focusedRow.btnKey.Text = "Brak";
            }
        }
    }

    public class ShortcutRow
    {
        public ShortcutManager.ActionId Id;
        public Label lblName;
        public Button btnKey;
        public Keys CurrentKeys;
        public bool IsListening;
        private readonly ShortcutConfigForm parentForm;

        public ShortcutRow(ShortcutManager.ActionId id, string name, Keys keys, ShortcutConfigForm parent)
        {
            Id = id;
            CurrentKeys = keys;
            IsListening = false;
            parentForm = parent;

            lblName = new Label
            {
                Text = name,
                ForeColor = Color.Cyan,
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Margin = new Padding(5, 8, 5, 8)
            };

            btnKey = new Button
            {
                Text = ShortcutManager.KeysToString(keys),
                Font = new Font("Consolas", 10F, FontStyle.Bold),
                ForeColor = Color.Yellow,
                BackColor = Color.FromArgb(30, 30, 30),
                FlatStyle = FlatStyle.Flat,
                MinimumSize = new Size(180, 30),
                Margin = new Padding(5, 5, 5, 5),
                Cursor = Cursors.Hand
            };
            btnKey.FlatAppearance.BorderSize = 1;
            btnKey.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btnKey.Click += (s, e) => parentForm.StartListening(this);
            btnKey.PreviewKeyDown += (s, e) =>
            {
                if (IsListening)
                    e.IsInputKey = true;
            };
            btnKey.KeyDown += (s, e) =>
            {
                if (!IsListening) return;

                e.SuppressKeyPress = true;

                Keys newKeys = Keys.None;
                if (e.Control) newKeys |= Keys.Control;
                if (e.Alt) newKeys |= Keys.Alt;
                if (e.Shift) newKeys |= Keys.Shift;
                newKeys |= e.KeyCode;

                if (e.KeyCode == Keys.Escape)
                {
                    newKeys = Keys.None;
                }
                else if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.Menu || e.KeyCode == Keys.ShiftKey)
                {
                    return;
                }

                CurrentKeys = newKeys;
                btnKey.Text = ShortcutManager.KeysToString(newKeys);
                parentForm.StopListening(this);
            };
        }
    }
}
