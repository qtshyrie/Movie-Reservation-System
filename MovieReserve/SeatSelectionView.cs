using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace MovieReserve
{
    // =====================================================================
    //  Draws the screen curve, the rows of seats and the legend.
    //  Seat positions come from SeatLayout, so Admin edits show up live.
    // =====================================================================
    internal class SeatCanvas : Control
    {
        private const float SeatW = 36, SeatH = 32, Gap = 8, RowGap = 10, LabelW = 22, ScreenH = 62, LegendH = 44;

        private readonly ReservationSession _s;
        private readonly Dictionary<string, Rectangle> _rects = new Dictionary<string, Rectangle>();
        private readonly ToolTip _tip = new ToolTip();
        private string _hover;
        private float _scale = 1f, _ox, _oy, _labelLeft, _labelRight, _screenX1, _screenX2;
        private bool _edit;

        public event EventHandler<string> SeatClicked;

        public bool EditMode
        {
            get { return _edit; }
            set { _edit = value; Invalidate(); }
        }

        public SeatCanvas(ReservationSession s)
        {
            _s = s;
            BackColor = Color.White;
            Font = UiKit.FontBody;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        private bool CanShow
        {
            get { return _s.Layout != null && (_s.Showtime != null || _edit); }
        }

        // ---------- geometry ----------
        private void Compute()
        {
            _rects.Clear();
            var L = _s.Layout;
            if (L == null || Width < 50 || Height < 50) return;

            int cols = L.Cols, side = L.SeatsPerSide;
            float w0 = 2 * LabelW + 2 * L.SideMargin + cols * SeatW + (cols - 2) * Gap + L.AisleWidth;
            float h0 = ScreenH + L.Rows * SeatH + (L.Rows - 1) * RowGap;
            float availW = Width - 24f, availH = Height - LegendH - 12f;

            _scale = Math.Min(1f, Math.Min(availW / w0, availH / h0));
            if (_scale < 0.3f) _scale = 0.3f;
            _ox = (Width - w0 * _scale) / 2f;
            _oy = 10f;

            float seatsLeft = LabelW + L.SideMargin;
            float blockW = side * SeatW + (side - 1) * Gap;
            float rightBlockX = seatsLeft + blockW + L.AisleWidth;

            for (int r = 0; r < L.Rows; r++)
            {
                for (int i = 0; i < cols; i++)
                {
                    bool right = i >= side;
                    int within = i % side;
                    float x = (right ? rightBlockX : seatsLeft) + within * (SeatW + Gap);
                    float y = ScreenH + r * (SeatH + RowGap);
                    _rects[SeatLayout.Code(r, i + 1)] = new Rectangle(
                        (int)Math.Round(_ox + x * _scale), (int)Math.Round(_oy + y * _scale),
                        (int)Math.Round(SeatW * _scale), (int)Math.Round(SeatH * _scale));
                }
            }

            _labelLeft = _ox;
            _labelRight = _ox + (w0 - LabelW) * _scale;
            _screenX1 = _ox + seatsLeft * _scale;
            _screenX2 = _ox + (rightBlockX + blockW) * _scale;
        }

        // ---------- painting ----------
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            if (!CanShow)
            {
                _rects.Clear();
                string msg = _s.Movie == null
                    ? "Pick a movie on the Movies page to start a reservation."
                    : "Pick a screening time to see the seats.";
                TextRenderer.DrawText(g, msg, UiKit.FontBody, ClientRectangle, UiKit.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            Compute();
            DrawScreen(g);
            DrawRowLabels(g);
            DrawSeats(g);
            DrawLegend(g);
        }

        private void DrawScreen(Graphics g)
        {
            float y = _oy + 14 * _scale;
            float x1 = _screenX1, x2 = _screenX2, w = x2 - x1;
            using (var p = new GraphicsPath())
            using (var pen = new Pen(Color.FromArgb(214, 208, 196), 3f))
            {
                p.AddBezier(x1, y + 22 * _scale, x1 + w * 0.25f, y - 6 * _scale,
                            x1 + w * 0.75f, y - 6 * _scale, x2, y + 22 * _scale);
                g.DrawPath(pen, p);
            }
            using (var f = new Font("Segoe UI", 7f))
            using (var b = new SolidBrush(Color.FromArgb(160, 156, 148)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString("S C R E E N", f, b, (x1 + x2) / 2f, y + 24 * _scale, sf);
        }

        private void DrawRowLabels(Graphics g)
        {
            var L = _s.Layout;
            using (var f = new Font("Segoe UI", Math.Max(7f, 9f * _scale)))
            using (var b = new SolidBrush(UiKit.Muted))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int r = 0; r < L.Rows; r++)
                {
                    float cy = _oy + (ScreenH + r * (SeatH + RowGap) + SeatH / 2f) * _scale;
                    var left = new RectangleF(_labelLeft, cy - 10, LabelW * _scale, 20);
                    var right = new RectangleF(_labelRight, cy - 10, LabelW * _scale, 20);
                    g.DrawString(SeatLayout.RowLabel(r), f, b, left, sf);
                    g.DrawString(SeatLayout.RowLabel(r), f, b, right, sf);
                }
            }
        }

        private void DrawSeats(Graphics g)
        {
            var L = _s.Layout;
            using (var numFont = new Font("Segoe UI", Math.Max(6.5f, 8.5f * _scale)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                foreach (var kv in _rects)
                {
                    string code = kv.Key;
                    Rectangle r = kv.Value;
                    SeatType t = L.TypeOf(code);
                    bool taken = !_edit && _s.Occupied.Contains(code);
                    bool chosen = !_edit && _s.SelectedSeats.Contains(code);

                    Color fill, border, text;
                    if (taken) { fill = UiKit.SeatTaken; border = UiKit.SeatTaken; text = UiKit.SeatTakenText; }
                    else if (chosen) { fill = UiKit.Red; border = UiKit.Red; text = Color.White; }
                    else if (t == SeatType.Pwd) { fill = UiKit.SeatPwd; border = UiKit.SeatPwdBorder; text = UiKit.SeatPwdText; }
                    else if (t == SeatType.Elderly) { fill = UiKit.SeatElder; border = UiKit.SeatElderBorder; text = UiKit.SeatElderText; }
                    else { fill = Color.White; border = UiKit.SeatFreeBorder; text = UiKit.Muted; }

                    bool hot = code == _hover && !taken;
                    using (var path = UiKit.RoundPath(r, Math.Max(3, (int)Math.Round(6 * _scale))))
                    using (var fb = new SolidBrush(fill))
                    using (var pen = new Pen(hot ? UiKit.Red : border, hot ? 2f : 1f))
                    {
                        g.FillPath(fb, path);
                        g.DrawPath(pen, path);
                    }
                    using (var tb = new SolidBrush(text))
                        g.DrawString(code.Substring(1), numFont, tb, r, sf);
                }
            }
        }

        private class LegendItem
        {
            public string Text; public Color Fill; public Color Border;
            public LegendItem(string text, Color fill, Color border) { Text = text; Fill = fill; Border = border; }
        }

        private void DrawLegend(Graphics g)
        {
            var items = new List<LegendItem>();
            if (!_edit)
            {
                items.Add(new LegendItem("Available", Color.White, UiKit.SeatFreeBorder));
                items.Add(new LegendItem("Selected", UiKit.Red, UiKit.Red));
                items.Add(new LegendItem("Occupied", UiKit.SeatTaken, UiKit.SeatTaken));
            }
            else
            {
                items.Add(new LegendItem("Regular", Color.White, UiKit.SeatFreeBorder));
            }
            items.Add(new LegendItem("PWD", UiKit.SeatPwd, UiKit.SeatPwdBorder));
            items.Add(new LegendItem("Elderly", UiKit.SeatElder, UiKit.SeatElderBorder));

            float total = 0;
            var widths = new List<float>();
            foreach (var it in items)
            {
                float tw = g.MeasureString(it.Text, UiKit.FontSmall).Width;
                widths.Add(tw);
                total += 14 + 6 + tw + 18;
            }

            float x = (Width - total) / 2f, y = Height - 30;
            using (var sep = new Pen(UiKit.Line))
                g.DrawLine(sep, 0, Height - LegendH, Width, Height - LegendH);

            for (int i = 0; i < items.Count; i++)
            {
                var r = new Rectangle((int)x, (int)y, 14, 14);
                using (var path = UiKit.RoundPath(r, 3))
                using (var fb = new SolidBrush(items[i].Fill))
                using (var pen = new Pen(items[i].Border))
                {
                    g.FillPath(fb, path);
                    g.DrawPath(pen, path);
                }
                using (var tb = new SolidBrush(UiKit.Muted))
                    g.DrawString(items[i].Text, UiKit.FontSmall, tb, x + 20, y - 1);
                x += 14 + 6 + widths[i] + 18;
            }
        }

        // ---------- mouse ----------
        private string HitTest(Point p)
        {
            foreach (var kv in _rects)
                if (kv.Value.Contains(p)) return kv.Key;
            return null;
        }

        private string Describe(string code)
        {
            string s = code;
            SeatType t = _s.Layout.TypeOf(code);
            if (t == SeatType.Pwd) s += "  \u00B7  PWD seat";
            else if (t == SeatType.Elderly) s += "  \u00B7  Elderly seat";
            if (!_edit && _s.Occupied.Contains(code)) s += "  \u00B7  Taken";
            return s;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!CanShow) return;
            string hit = HitTest(e.Location);
            if (hit == _hover) return;
            _hover = hit;
            Cursor = hit != null ? Cursors.Hand : Cursors.Default;
            _tip.SetToolTip(this, hit == null ? "" : Describe(hit));
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hover != null) { _hover = null; Invalidate(); }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (!CanShow || e.Button != MouseButtons.Left) return;
            string hit = HitTest(e.Location);
            if (hit != null && SeatClicked != null) SeatClicked(this, hit);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tip.Dispose();
            base.Dispose(disposing);
        }
    }

    // =====================================================================
    //  SeatPanel content.
    //  Staff : seat picking + "Change room" button.
    //  Admin : layout editor (paint PWD / Elderly seats, middle aisle, side
    //          margins, rows, seats per side) with Save / Revert / Reset.
    // =====================================================================
    internal class SeatSelectionView : UserControl
    {
        private readonly ReservationSession _s;
        private readonly SeatCanvas _canvas;
        private Panel _header, _editor;
        private Button _btnRoom;
        private RadioButton _rbRegular, _rbPwd, _rbElder;
        private CheckBox _chkEdit;
        private ComboBox _cmbRoom;
        private TrackBar _tbAisle, _tbSide;
        private Label _lblAisle, _lblSide;
        private NumericUpDown _numRows, _numSide;
        private bool _syncing;

        public SeatSelectionView(ReservationSession s)
        {
            _s = s;
            BackColor = Color.White;

            _canvas = new SeatCanvas(s) { Dock = DockStyle.Fill };
            _canvas.SeatClicked += OnSeatClicked;
            Controls.Add(_canvas);                 // Fill first, then Bottom / Top

            if (s.IsAdmin)
            {
                BuildEditor();
                _canvas.EditMode = true;
            }
            BuildHeader();

            s.Changed += c => OnChanged();
            OnChanged();
        }

        // ---------- header ----------
        private void BuildHeader()
        {
            _header = new Panel { Dock = DockStyle.Top, Height = 66, BackColor = Color.White };
            _header.Paint += PaintHeader;
            _header.Resize += (a, b) => _header.Invalidate();

            if (!_s.IsAdmin)
            {
                _btnRoom = UiKit.MakeButton("Change room", false);
                _btnRoom.Size = new Size(124, 36);
                _btnRoom.Click += (a, b) => ShowRoomMenu();
                _header.Controls.Add(_btnRoom);
                _header.Resize += (a, b) => _btnRoom.Location = new Point(_header.Width - _btnRoom.Width - 18, 15);
            }
            Controls.Add(_header);
        }

        private void PaintHeader(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var flags = TextFormatFlags.NoPadding;
            bool editing = _s.IsAdmin && _chkEdit != null && _chkEdit.Checked;
            TextRenderer.DrawText(g, editing ? "Edit seat layout" : "Select seats",
                UiKit.FontTitle, new Point(18, 10), UiKit.Ink, flags);

            const int y = 38;
            if (_s.Showtime == null || _s.Movie == null)
            {
                string t = (_s.IsAdmin && _s.Room != null) ? _s.Room.Name : "No screening selected";
                TextRenderer.DrawText(g, t, UiKit.FontSmall, new Point(18, y), UiKit.Muted, flags);
            }
            else
            {
                string part1 = _s.Movie.Title + "  \u00B7  " + _s.Showtime.Start.ToString("MMM d, h:mm tt") + "  \u00B7  ";
                TextRenderer.DrawText(g, part1, UiKit.FontSmall, new Point(18, y), UiKit.Muted, flags);
                Size sz = TextRenderer.MeasureText(g, part1, UiKit.FontSmall, new Size(int.MaxValue, int.MaxValue), flags);
                string room = _s.Room == null ? "" : _s.Room.Name;
                TextRenderer.DrawText(g, room, UiKit.FontSmall, new Point(18 + sz.Width, y), UiKit.Red, flags);
            }
            using (var p = new Pen(UiKit.Line))
                g.DrawLine(p, 0, _header.Height - 1, _header.Width, _header.Height - 1);
        }

        private void ShowRoomMenu()
        {
            var menu = new ContextMenuStrip();
            foreach (ScreenItem sc in _s.Screens)
            {
                ScreenItem captured = sc;
                var item = new ToolStripMenuItem(sc.Name)
                {
                    Checked = _s.Room != null && _s.Room.ScreenId == sc.ScreenId
                };
                item.Click += (a, b) => _s.ChangeRoom(captured);
                menu.Items.Add(item);
            }
            if (menu.Items.Count == 0)
                menu.Items.Add(new ToolStripMenuItem("No rooms found") { Enabled = false });
            menu.Closed += (a, b) => menu.Dispose();
            menu.Show(_btnRoom, new Point(0, _btnRoom.Height));
        }

        // ---------- admin editor ----------
        private Label Cap(string text)
        {
            return new Label
            {
                Text = text, AutoSize = true, Font = UiKit.FontBody, ForeColor = UiKit.Muted,
                Margin = new Padding(3, 8, 3, 3)
            };
        }

        private RadioButton Radio(string text, bool check)
        {
            return new RadioButton
            {
                Text = text, AutoSize = true, Checked = check, Font = UiKit.FontBody,
                Margin = new Padding(3, 5, 3, 3)
            };
        }

        private TrackBar Track(int max, int value)
        {
            return new TrackBar
            {
                AutoSize = false, Width = 130, Height = 30, Minimum = 0, Maximum = max, Value = value,
                TickStyle = TickStyle.None, Margin = new Padding(3, 2, 3, 3)
            };
        }

        private Label ValueLabel()
        {
            return new Label
            {
                AutoSize = false, Width = 50, Height = 22, Font = UiKit.FontBold, ForeColor = UiKit.Ink,
                Margin = new Padding(0, 8, 14, 3)
            };
        }

        private void BuildEditor()
        {
            _editor = new Panel { Dock = DockStyle.Bottom, Height = 128, BackColor = Color.FromArgb(250, 250, 248) };
            _editor.Paint += (a, e) =>
            {
                using (var p = new Pen(UiKit.Line)) e.Graphics.DrawLine(p, 0, 0, _editor.Width, 0);
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = true, FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(12, 6, 8, 4), BackColor = Color.Transparent
            };
            _editor.Controls.Add(flow);

            // ---- row 1: brush, edit toggle, room ----
            flow.Controls.Add(Cap("Paint seats as"));
            _rbRegular = Radio("Regular", false);
            _rbPwd = Radio("PWD", true);
            _rbElder = Radio("Elderly (60+)", false);
            flow.Controls.AddRange(new Control[] { _rbRegular, _rbPwd, _rbElder });

            _chkEdit = new CheckBox
            {
                Text = "Edit layout", Checked = true, AutoSize = true, Font = UiKit.FontBody,
                Margin = new Padding(18, 6, 3, 3)
            };
            _chkEdit.CheckedChanged += (a, b) =>
            {
                _canvas.EditMode = _chkEdit.Checked;
                _header.Invalidate();
            };
            flow.Controls.Add(_chkEdit);

            flow.Controls.Add(Cap("Room"));
            _cmbRoom = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList, Width = 120, Font = UiKit.FontBody,
                Margin = new Padding(3, 4, 3, 3)
            };
            _cmbRoom.SelectedIndexChanged += (a, b) =>
            {
                if (_syncing) return;
                var sc = _cmbRoom.SelectedItem as ScreenItem;
                if (sc != null) _s.ChangeRoom(sc);
            };
            flow.Controls.Add(_cmbRoom);
            flow.SetFlowBreak(_cmbRoom, true);

            // ---- row 2: spacing ----
            flow.Controls.Add(Cap("Middle aisle"));
            _tbAisle = Track(SeatLayout.MaxAisle, 60);
            _lblAisle = ValueLabel();
            _tbAisle.ValueChanged += (a, b) =>
            {
                _lblAisle.Text = _tbAisle.Value + " px";
                if (_syncing || _s.Layout == null) return;
                _s.Layout.AisleWidth = _tbAisle.Value;
                _canvas.Invalidate();
            };
            flow.Controls.Add(_tbAisle);
            flow.Controls.Add(_lblAisle);

            flow.Controls.Add(Cap("Side margin"));
            _tbSide = Track(SeatLayout.MaxMargin, 40);
            _lblSide = ValueLabel();
            _tbSide.ValueChanged += (a, b) =>
            {
                _lblSide.Text = _tbSide.Value + " px";
                if (_syncing || _s.Layout == null) return;
                _s.Layout.SideMargin = _tbSide.Value;
                _canvas.Invalidate();
            };
            flow.Controls.Add(_tbSide);
            flow.Controls.Add(_lblSide);
            flow.SetFlowBreak(_lblSide, true);

            // ---- row 3: size + buttons ----
            flow.Controls.Add(Cap("Rows"));
            _numRows = new NumericUpDown
            {
                Minimum = SeatLayout.MinRows, Maximum = SeatLayout.MaxRows, Value = 6, Width = 56,
                Font = UiKit.FontBody, Margin = new Padding(3, 4, 12, 3)
            };
            _numRows.ValueChanged += (a, b) =>
            {
                if (_syncing || _s.Layout == null) return;
                _s.Layout.Rows = (int)_numRows.Value;
                _s.NotifyLayoutEdited();
            };
            flow.Controls.Add(_numRows);

            flow.Controls.Add(Cap("Seats per side"));
            _numSide = new NumericUpDown
            {
                Minimum = SeatLayout.MinSide, Maximum = SeatLayout.MaxSide, Value = 6, Width = 56,
                Font = UiKit.FontBody, Margin = new Padding(3, 4, 18, 3)
            };
            _numSide.ValueChanged += (a, b) =>
            {
                if (_syncing || _s.Layout == null) return;
                _s.Layout.SeatsPerSide = (int)_numSide.Value;
                _s.NotifyLayoutEdited();
            };
            flow.Controls.Add(_numSide);

            var save = UiKit.MakeButton("Save layout", true);
            save.Size = new Size(110, 30);
            save.Margin = new Padding(3, 2, 3, 3);
            save.Click += (a, b) =>
            {
                if (_s.Room == null) return;
                try
                {
                    _s.SaveLayout();
                    MessageBox.Show("Layout saved for " + _s.Room.Name + ".\nStaff will see it the next time they open this room.",
                        "MovieReserve", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) { MessageBox.Show("Could not save layout:\n" + ex.Message, "MovieReserve"); }
            };

            var revert = UiKit.MakeButton("Revert", false);
            revert.Size = new Size(80, 30);
            revert.Margin = new Padding(3, 2, 3, 3);
            revert.Click += (a, b) => _s.ReloadLayout();

            var reset = UiKit.MakeButton("Default", false);
            reset.Size = new Size(80, 30);
            reset.Margin = new Padding(3, 2, 3, 3);
            reset.Click += (a, b) => _s.ResetLayoutToDefault();

            flow.Controls.AddRange(new Control[] { save, revert, reset });
            Controls.Add(_editor);
        }

        private SeatType CurrentBrush()
        {
            if (_rbPwd != null && _rbPwd.Checked) return SeatType.Pwd;
            if (_rbElder != null && _rbElder.Checked) return SeatType.Elderly;
            return SeatType.Regular;
        }

        private void SyncEditor()
        {
            if (_editor == null) return;
            _syncing = true;
            try
            {
                var L = _s.Layout;
                if (L != null)
                {
                    _tbAisle.Value = Math.Max(_tbAisle.Minimum, Math.Min(_tbAisle.Maximum, L.AisleWidth));
                    _tbSide.Value = Math.Max(_tbSide.Minimum, Math.Min(_tbSide.Maximum, L.SideMargin));
                    _numRows.Value = Math.Max(_numRows.Minimum, Math.Min(_numRows.Maximum, L.Rows));
                    _numSide.Value = Math.Max(_numSide.Minimum, Math.Min(_numSide.Maximum, L.SeatsPerSide));
                    _lblAisle.Text = _tbAisle.Value + " px";
                    _lblSide.Text = _tbSide.Value + " px";
                }

                if (_cmbRoom.Items.Count != _s.Screens.Count)
                {
                    _cmbRoom.Items.Clear();
                    foreach (ScreenItem sc in _s.Screens) _cmbRoom.Items.Add(sc);
                }
                int idx = -1;
                for (int i = 0; i < _cmbRoom.Items.Count; i++)
                    if (_s.Room != null && ((ScreenItem)_cmbRoom.Items[i]).ScreenId == _s.Room.ScreenId) idx = i;
                _cmbRoom.SelectedIndex = idx;
            }
            finally { _syncing = false; }
        }

        // ---------- events ----------
        private void OnSeatClicked(object sender, string code)
        {
            if (_s.IsAdmin && _chkEdit != null && _chkEdit.Checked)
            {
                _s.Layout.SetType(code, CurrentBrush());
                _canvas.Invalidate();
            }
            else
            {
                _s.ToggleSeat(code);
            }
        }

        private void OnChanged()
        {
            if (_btnRoom != null) _btnRoom.Enabled = _s.Showtime != null;
            SyncEditor();
            _header.Invalidate();
            _canvas.Invalidate();
        }
    }
}
