using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MovieReserve
{
    // =====================================================================
    //  SelectedMoviePanel content: NOW SHOWING, poster, title, genre,
    //  and a ListBox where the user picks the Show_time.
    // =====================================================================
    internal class MovieInfoView : UserControl
    {
        private readonly ReservationSession _s;
        private readonly Label _now, _title, _meta, _screening, _empty;
        private readonly PictureBox _poster;
        private readonly ListBox _list;
        private bool _syncing;

        public MovieInfoView(ReservationSession s)
        {
            _s = s;
            BackColor = Color.White;

            _now = new Label { Text = "NOW SHOWING", AutoSize = true, Font = UiKit.FontCap, ForeColor = UiKit.Red };
            _title = new Label
            {
                AutoSize = false, AutoEllipsis = true, Font = UiKit.FontTitle, ForeColor = UiKit.Ink,
                Text = "No movie selected"
            };
            _meta = new Label { AutoSize = false, AutoEllipsis = true, Font = UiKit.FontSmall, ForeColor = UiKit.Muted };
            _screening = new Label { Text = "Screening time", AutoSize = true, Font = UiKit.FontCap, ForeColor = UiKit.Muted };

            _poster = new PictureBox { BackColor = Color.FromArgb(235, 234, 230), SizeMode = PictureBoxSizeMode.Zoom };

            _list = new ListBox
            {
                DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 34, IntegralHeight = false,
                BorderStyle = BorderStyle.None, BackColor = Color.White, Font = UiKit.FontBody
            };
            _list.DrawItem += DrawTime;
            _list.SelectedIndexChanged += (a, b) =>
            {
                if (_syncing) return;
                var st = _list.SelectedItem as ShowtimeItem;
                if (st != null) _s.SelectShowtime(st);
            };

            _empty = new Label
            {
                AutoSize = false, Text = "Pick a movie to see its screenings.", Font = UiKit.FontSmall,
                ForeColor = UiKit.Muted, BackColor = Color.White, TextAlign = ContentAlignment.MiddleLeft
            };

            Controls.AddRange(new Control[] { _now, _poster, _title, _meta, _screening, _list, _empty });
            _empty.BringToFront();

            Resize += (a, b) => DoLayout();
            DoLayout();

            s.Changed += OnChanged;
        }

        private void DoLayout()
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            if (w < 80) return;
            _now.Location = new Point(16, 14);
            _poster.SetBounds(16, 36, 66, 99);
            int tx = 96, tw = w - tx - 14;
            _title.SetBounds(tx, 36, tw, 46);
            _meta.SetBounds(tx, 84, tw, 20);
            _screening.Location = new Point(16, 148);
            _list.SetBounds(14, 168, w - 28, Math.Max(40, h - 168 - 14));
            _empty.SetBounds(_list.Left + 2, _list.Top, _list.Width - 4, 34);
        }

        private void OnChanged(SessionChange c)
        {
            if (c == SessionChange.Movie) ShowMovie();
            else if (c == SessionChange.Showtimes) FillList();
            else if (c == SessionChange.Showtime) SelectCurrent();
        }

        private async void ShowMovie()
        {
            var m = _s.Movie;
            _poster.Image = null;
            if (m == null)
            {
                _title.Text = "No movie selected";
                _meta.Text = "";
                return;
            }

            _title.Text = m.Title;
            string meta = m.Genre ?? "";
            if (!string.IsNullOrWhiteSpace(m.Duration))
                meta += (meta.Length > 0 ? "  \u00B7  " : "") + m.Duration + " min";
            _meta.Text = meta;

            Image img = await PosterLoader.GetAsync(m.PosterUrl);
            if (img != null && _s.Movie == m && !IsDisposed) _poster.Image = img;   // cached, shared: not disposed here
        }

        private void FillList()
        {
            _syncing = true;
            try
            {
                _list.Items.Clear();
                foreach (ShowtimeItem st in _s.Showtimes) _list.Items.Add(st);
            }
            finally { _syncing = false; }

            _empty.Text = _s.Movie == null ? "Pick a movie to see its screenings." : "No screenings scheduled for this movie.";
            _empty.Visible = _list.Items.Count == 0;
        }

        private void SelectCurrent()
        {
            _syncing = true;
            try { _list.SelectedItem = _s.Showtime; }
            finally { _syncing = false; }
            _list.Invalidate();
        }

        private void DrawTime(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var st = (ShowtimeItem)_list.Items[e.Index];
            bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var bg = new SolidBrush(Color.White)) g.FillRectangle(bg, e.Bounds);

            var r = new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 2, e.Bounds.Width - 3, e.Bounds.Height - 4);
            using (var path = UiKit.RoundPath(r, 6))
            using (var fill = new SolidBrush(sel ? UiKit.Red : Color.FromArgb(246, 245, 242)))
                g.FillPath(fill, path);

            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
            var left = new Rectangle(r.X + 10, r.Y, 118, r.Height);
            var right = new Rectangle(r.X + 130, r.Y, r.Width - 140, r.Height);

            TextRenderer.DrawText(g, st.Start.ToString("MMM d  h:mm tt"), UiKit.FontBold, left,
                sel ? Color.White : UiKit.Ink, flags | TextFormatFlags.Left);
            TextRenderer.DrawText(g, st.ScreenName + "  \u00B7  " + UiKit.Money(st.Price), UiKit.FontSmall, right,
                sel ? Color.FromArgb(255, 226, 226) : UiKit.Muted, flags | TextFormatFlags.Right);
        }
    }
}
