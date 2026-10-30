using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieReserve
{
    // ===================== Edit Account Profile (name is read-only, password can be changed) =====================
    public class ProfileForm : Form
    {
        private static readonly Color Bg = Color.FromArgb(24, 24, 28);
        private static readonly Color CardBg = Color.FromArgb(32, 32, 38);
        private static readonly Color FieldBg = Color.FromArgb(18, 18, 22);
        private static readonly Color FieldBorder = Color.FromArgb(58, 58, 66);
        private static readonly Color Muted = Color.FromArgb(160, 160, 168);
        private static readonly Color Red = Color.FromArgb(229, 9, 20);
        private static readonly Color RedHover = Color.FromArgb(193, 17, 25);

        private readonly UserSession _user;

        private TextBox _txtName, _txtPass, _txtConfirm;
        private EyeButton _eyePass, _eyeConfirm;
        private Label _error;
        private Button _btnSave, _btnCancel, _btnClose;

        private Point _dragStart;
        private bool _dragging;

        public ProfileForm(UserSession user)
        {
            _user = user;

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Bg;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);
            ClientSize = new Size(450, 530);
            DoubleBuffered = true;
            Text = "Edit Account Profile";

            BuildControls();

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            Load += (s, e) => { RoundForm(); _txtPass.Focus(); };
            Resize += (s, e) => RoundForm();

            // drag the window by its header
            MouseDown += (s, e) => { if (e.Y < 60) { _dragging = true; _dragStart = e.Location; } };
            MouseMove += (s, e) =>
            {
                if (_dragging)
                    Location = new Point(Left + e.X - _dragStart.X, Top + e.Y - _dragStart.Y);
            };
            MouseUp += (s, e) => _dragging = false;
        }

        // ---------- controls ----------
        private void BuildControls()
        {
            var title = new Label
            {
                Text = "Edit Account Profile",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Left = 24,
                Top = 18
            };
            title.MouseDown += (s, e) => { _dragging = true; _dragStart = new Point(e.X + title.Left, e.Y + title.Top); };
            title.MouseMove += (s, e) =>
            {
                if (_dragging)
                    Location = new Point(Left + e.X + title.Left - _dragStart.X, Top + e.Y + title.Top - _dragStart.Y);
            };
            title.MouseUp += (s, e) => _dragging = false;

            _btnClose = new Button
            {
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                BackColor = CardBg,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel,
                TabStop = false
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.SetBounds(450 - 24 - 32, 14, 32, 32);
            _btnClose.Click += (s, e) => Close();

            // name (read-only)
            AddCaption("Staff Name", null, 190);
            _txtName = new TextBox { Text = _user.Name, ReadOnly = true, TabStop = false };
            var nameBox = MakeFieldBox(_txtName, 212, false, null);
            _txtName.ForeColor = Muted;
            AddCaption("Your name can only be changed by an administrator.", null, 258, true);

            // new password
            AddCaption("New Password", "(Leave blank to keep current)", 286);
            _txtPass = new TextBox { UseSystemPasswordChar = true };
            var passBox = MakeFieldBox(_txtPass, 308, true, out _eyePass);
            InputRules.Attach(_txtPass, InputRules.MaxPassword, false);

            // confirm
            AddCaption("Confirm New Password", null, 366);
            _txtConfirm = new TextBox { UseSystemPasswordChar = true };
            var confirmBox = MakeFieldBox(_txtConfirm, 388, true, out _eyeConfirm);
            InputRules.Attach(_txtConfirm, InputRules.MaxPassword, false);

            _eyePass.Click += (s, e) => Toggle(_txtPass, _eyePass);
            _eyeConfirm.Click += (s, e) => Toggle(_txtConfirm, _eyeConfirm);

            _error = new Label
            {
                AutoSize = false,
                ForeColor = Color.FromArgb(229, 80, 0),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9f)
            };
            _error.SetBounds(24, 440, 402, 22);

            _btnCancel = new Button
            {
                Text = "Cancel",
                FlatStyle = FlatStyle.Flat,
                BackColor = Bg,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI Semibold", 9.5f)
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.SetBounds(190, 474, 90, 38);

            _btnSave = new Button
            {
                Text = "✓  Save Changes",
                FlatStyle = FlatStyle.Flat,
                BackColor = Red,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Font = new Font("Segoe UI Semibold", 9.5f)
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.SetBounds(290, 474, 136, 38);
            _btnSave.MouseEnter += (s, e) => { if (_btnSave.Enabled) _btnSave.BackColor = RedHover; };
            _btnSave.MouseLeave += (s, e) => _btnSave.BackColor = Red;
            _btnSave.Click += BtnSave_Click;

            Controls.AddRange(new Control[]
            {
                title, _btnClose, nameBox, passBox, confirmBox, _error, _btnCancel, _btnSave
            });
        }

        private void AddCaption(string text, string sub, int y, bool small = false)
        {
            var l = new Label
            {
                Text = text,
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = small ? Muted : Color.White,
                Font = small ? new Font("Segoe UI", 8.5f) : new Font("Segoe UI Semibold", 9.5f),
                Left = 24,
                Top = y
            };
            Controls.Add(l);

            if (sub != null)
            {
                var s2 = new Label
                {
                    Text = sub,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    ForeColor = Muted,
                    Font = new Font("Segoe UI", 9f),
                    Top = y
                };
                s2.Left = l.Right + 4;
                Controls.Add(s2);
                // Label.Right isn't known until it is measured, so fix the position after layout
                l.SizeChanged += (a, b) => s2.Left = l.Right + 4;
            }
        }

        private Panel MakeFieldBox(TextBox tb, int y, bool withEye, out EyeButton eye)
        {
            const int w = 402, h = 42;
            var box = new Panel { BackColor = FieldBg, Left = 24, Top = y, Width = w, Height = h };
            box.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var p = RoundPath(new Rectangle(0, 0, box.Width - 1, box.Height - 1), 8))
                using (var pen = new Pen(tb.Focused ? Red : FieldBorder))
                    e.Graphics.DrawPath(pen, p);
            };

            tb.BorderStyle = BorderStyle.None;
            tb.BackColor = FieldBg;
            tb.ForeColor = Color.White;
            tb.Font = new Font("Segoe UI", 11f);
            tb.Left = 14;
            tb.Width = w - 28 - (withEye ? 40 : 0);
            tb.Top = (h - tb.Height) / 2;
            tb.Enter += (s, e) => box.Invalidate();
            tb.Leave += (s, e) => box.Invalidate();
            box.Controls.Add(tb);

            eye = null;
            if (withEye)
            {
                eye = new EyeButton { BackColor = FieldBg };
                eye.SetBounds(w - 44, 4, 36, h - 8);
                box.Controls.Add(eye);
                eye.BringToFront();
            }

            box.Click += (s, e) => tb.Focus();
            Round(box, 8);
            return box;
        }

        private Panel MakeFieldBox(TextBox tb, int y, bool withEye, object unused)
        {
            EyeButton dummy;
            return MakeFieldBox(tb, y, withEye, out dummy);
        }

        private static void Toggle(TextBox tb, EyeButton eye)
        {
            eye.Revealed = !eye.Revealed;
            tb.UseSystemPasswordChar = !eye.Revealed;
            tb.Focus();
            tb.SelectionStart = tb.TextLength;
        }

        // ---------- painting (border, header line, profile card, avatar) ----------
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // window border
            using (var p = RoundPath(new Rectangle(0, 0, Width - 1, Height - 1), 14))
            using (var pen = new Pen(FieldBorder))
                g.DrawPath(pen, p);

            // header separator
            using (var pen = new Pen(Color.FromArgb(45, 45, 52)))
                g.DrawLine(pen, 0, 60, Width, 60);

            // profile card
            var card = new Rectangle(24, 76, 402, 92);
            using (var p = RoundPath(card, 10))
            {
                using (var b = new SolidBrush(CardBg)) g.FillPath(b, p);
                using (var pen = new Pen(Color.FromArgb(48, 48, 56))) g.DrawPath(pen, p);
            }

            // avatar
            var av = new Rectangle(card.Left + 16, card.Top + 14, 64, 64);
            using (var b = new SolidBrush(Color.FromArgb(60, 229, 9, 20))) g.FillEllipse(b, av);
            using (var pen = new Pen(Red, 2f)) g.DrawEllipse(pen, av);
            using (var f = new Font("Segoe UI", 15f, FontStyle.Bold))
            using (var br = new SolidBrush(Red))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(_user.Initials, f, br, av, sf);

            // name + role
            int tx = av.Right + 16;
            using (var f = new Font("Segoe UI", 12f, FontStyle.Bold))
                g.DrawString(_user.Name, f, Brushes.White, tx, card.Top + 20);

            string role = (_user.Role ?? "").Trim();
            if (role.Length > 0) role = char.ToUpper(role[0]) + role.Substring(1).ToLower();
            using (var f = new Font("Segoe UI", 9.5f))
            using (var br = new SolidBrush(Muted))
                g.DrawString(role + " Account", f, br, tx, card.Top + 48);
        }

        // ---------- save ----------
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            string p1 = _txtPass.Text, p2 = _txtConfirm.Text;

            // nothing typed = keep the current password
            if (p1.Length == 0 && p2.Length == 0)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            if (p1.Length < 4) { ShowError("Password must be at least 4 characters."); return; }
            if (!InputRules.IsValid(p1, false)) { ShowError("Only letters and numbers are allowed."); return; }
            if (p1 != p2) { ShowError("The two passwords do not match."); _txtConfirm.Focus(); return; }

            _btnSave.Enabled = false;
            _btnSave.Text = "Saving...";
            ShowError("");
            try
            {
                await Task.Run(() => AccountRepository.ChangePassword(_user.Id, p1));

                _error.ForeColor = Color.SeaGreen;
                _error.Text = "Password updated.";
                await Task.Delay(800);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Change password failed: " + ex.Message);
                ShowError("Can't reach the database. Is MySQL (XAMPP) running?");
                if (!IsDisposed) { _btnSave.Enabled = true; _btnSave.Text = "✓  Save Changes"; }
            }
        }

        private void ShowError(string msg)
        {
            _error.ForeColor = Color.FromArgb(229, 80, 0);
            _error.Text = msg;
        }

        // ---------- rounding helpers ----------
        private void RoundForm()
        {
            using (var p = RoundPath(new Rectangle(0, 0, Width, Height), 14))
            {
                if (Region != null) Region.Dispose();
                Region = new Region(p);
            }
        }

        private static void Round(Control c, int radius)
        {
            using (var p = RoundPath(new Rectangle(0, 0, c.Width, c.Height), radius))
            {
                if (c.Region != null) c.Region.Dispose();
                c.Region = new Region(p);
            }
        }

        private static GraphicsPath RoundPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
