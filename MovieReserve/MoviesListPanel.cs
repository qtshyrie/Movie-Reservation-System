using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieReserve
{
    // =====================================================================
    //  MoviesListPanel  (1070 x 416)
    //  Poster grid like the reference image: poster, title, genre + duration.
    //  Clicking a poster raises MovieSelected so Form1 can open the reservation panel.
    //  Needs: MovieItem and MovieRepository.GetMovies() (from NowShowingPanel.cs).
    // =====================================================================
    public class MoviesListPanel : UserControl
    {
        // ---- raised when a movie is clicked ----
        public event EventHandler<MovieItem> MovieSelected;

        // ---- look (change freely) ----
        private static readonly Color PanelBg = Color.FromArgb(24, 24, 28);
        private static readonly Color TitleColor = Color.White;
        private static readonly Color SubColor = Color.FromArgb(150, 150, 160);
        private static readonly Color Accent = Color.FromArgb(229, 9, 20);

        private const int Columns = 5;
        private const int TileHeight = 336;

        private readonly FlowLayoutPanel _flow;
        private readonly Label _status;
        private readonly ToolTip _tip = new ToolTip { InitialDelay = 400, AutoPopDelay = 8000 };

        // posters are cached so reloading the panel is instant
        private static readonly ConcurrentDictionary<string, Image> PosterCache =
            new ConcurrentDictionary<string, Image>();
        private static readonly HttpClient Http = CreateClient();

        private static HttpClient CreateClient()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var c = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            return c;
        }

        public MoviesListPanel()
        {
            Size = new Size(1070, 416);
            BackColor = PanelBg;
            DoubleBuffered = true;

            _flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = PanelBg,
                Padding = new Padding(14, 4, 0, 12),
                Margin = Padding.Empty
            };
            // mouse wheel scrolls when there are more than two rows
            _flow.MouseEnter += (s, e) => _flow.Focus();

            _status = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = SubColor,
                BackColor = PanelBg,
                Font = new Font("Segoe UI", 11f),
                Text = "Loading movies...",
                Visible = false
            };

            Controls.Add(_status);
            Controls.Add(_flow);
            _status.BringToFront();

            var header = new SectionHeader("All Showing",
                "Select a poster to start a reservation", PanelBg, Accent, SubColor);
            Controls.Add(header);
            header.SendToBack();

        }

        // DesignMode is unreliable for controls nested inside other controls
        private static bool IsInDesigner =>
            LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
            Process.GetCurrentProcess().ProcessName
                   .Equals("devenv", StringComparison.OrdinalIgnoreCase);

        // Call this again after movies are added/edited (e.g. from the admin screen)
        public async Task LoadMoviesAsync()
        {
            ShowStatus("Loading movies...");

            List<MovieItem> movies;
            try
            {
                movies = await Task.Run(() => MovieRepository.GetMovies());   // DB off the UI thread
            }
            catch (Exception ex)
            {
                ShowStatus("Could not load movies:\n" + ex.Message);
                return;
            }

            if (IsDisposed) return;

            if (movies.Count == 0) { ShowStatus("No movies available."); return; }
            _status.Visible = false;

            _flow.SuspendLayout();
            var old = _flow.Controls.Cast<Control>().ToList();
            _flow.Controls.Clear();
            foreach (var c in old) c.Dispose();
            _flow.Controls.Clear();

            int tileW = Math.Max(140,
                (ClientSize.Width - SystemInformation.VerticalScrollBarWidth - _flow.Padding.Horizontal) / Columns);

            var tiles = new List<MovieTile>();
            foreach (var m in movies)
            {
                var tile = new MovieTile(m, tileW, TileHeight, PanelBg, TitleColor, SubColor, Accent);
                tile.Click += (s, e) => MovieSelected?.Invoke(this, ((MovieTile)s).Movie);
                _tip.SetToolTip(tile, string.IsNullOrWhiteSpace(m.Synopsis) ? m.Title : m.Synopsis);
                _flow.Controls.Add(tile);
                tiles.Add(tile);
            }
            _flow.ResumeLayout();

            // cards appear immediately; posters pop in as they download
            foreach (var t in tiles) { var _ = LoadPosterAsync(t); }
        }

        private void ShowStatus(string text)
        {
            _status.Text = text;
            _status.Visible = true;
            _status.BringToFront();
        }

        private async Task LoadPosterAsync(MovieTile tile)
        {
            string url = tile.Movie.PosterUrl;
            if (string.IsNullOrWhiteSpace(url)) return;

            try
            {
                Image img;
                if (!PosterCache.TryGetValue(url, out img))
                {
                    using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        // GDI+ can't read WebP, so ask for JPG/PNG
                        req.Headers.Accept.ParseAdd("image/jpeg,image/png,image/*;q=0.8");
                        Uri u;
                        if (Uri.TryCreate(url, UriKind.Absolute, out u))
                            req.Headers.Referrer = new Uri(u.GetLeftPart(UriPartial.Authority) + "/");

                        using (var resp = await Http.SendAsync(req))
                        {
                            resp.EnsureSuccessStatusCode();
                            byte[] data = await resp.Content.ReadAsByteArrayAsync();
                            using (var ms = new MemoryStream(data))
                            using (var raw = Image.FromStream(ms))
                                img = new Bitmap(raw);
                        }
                    }
                    PosterCache[url] = img;
                }

                if (!IsDisposed && !tile.IsDisposed) tile.SetPoster(img);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Poster failed: " + tile.Movie.Title + " -> " + ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tip.Dispose();   // cached posters are shared, so they are not disposed here
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MoviesListPanel
            // 
            this.Name = "MoviesListPanel";
            this.Load += new System.EventHandler(this.MoviesListPanel_Load);
            this.ResumeLayout(false);

        }

        private void MoviesListPanel_Load(object sender, EventArgs e)
        {

        }
    }

    // =====================================================================
    //  One poster card: poster + title + "Genre  •  110 min"
    // =====================================================================
    internal class MovieTile : Control
    {
        public MovieItem Movie { get; private set; }

        private Image _poster;
        private bool _hover;
        private readonly Color _bg, _title, _sub, _accent;
        private readonly Font _titleFont = new Font("Segoe UI Semibold", 9.5f);
        private readonly Font _subFont = new Font("Segoe UI", 8.25f);
        private readonly Font _pillFont = new Font("Segoe UI Semibold", 8.5f);
        private const int CardInset = 6, CardPad = 10, PosterMaxW = 164, CardRadius = 10;

        public MovieTile(MovieItem movie, int width, int height,
                         Color bg, Color title, Color sub, Color accent)
        {
            Movie = movie;
            _bg = bg; _title = title; _sub = sub; _accent = accent;

            Size = new Size(width, height);
            Margin = Padding.Empty;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        public void SetPoster(Image img) { _poster = img; Invalidate(); }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(_bg);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // ----- card -----
            var card = new Rectangle(CardInset, CardInset, Width - CardInset * 2 - 1, Height - CardInset * 2 - 1);
            using (var path = RoundRect(card, CardRadius))
            using (var fill = new SolidBrush(_hover ? Color.FromArgb(44, 44, 50) : Color.FromArgb(34, 34, 39)))
            using (var pen = new Pen(_hover ? _accent : Color.FromArgb(52, 52, 58), 1f))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }

            // ----- poster (2:3) -----
            int pw = Math.Max(40, Math.Min(PosterMaxW, card.Width - CardPad * 2));
            int ph = pw * 3 / 2;
            var pr = new Rectangle(card.X + (card.Width - pw) / 2, card.Y + CardPad, pw, ph);
            using (var path = RoundRect(pr, 6))
            {
                if (_poster != null)
                {
                    var state = g.Save();
                    g.SetClip(path, CombineMode.Intersect);
                    DrawCover(g, _poster, pr);
                    g.Restore(state);
                }
                else   // placeholder while loading / if the URL fails
                {
                    using (var b = new LinearGradientBrush(pr, Color.FromArgb(64, 64, 72), Color.FromArgb(42, 42, 48), 90f))
                        g.FillPath(b, path);
                    string initial = string.IsNullOrEmpty(Movie.Title) ? "?" : Movie.Title.Substring(0, 1).ToUpper();
                    using (var f = new Font("Segoe UI", 26f, FontStyle.Bold))
                    using (var b = new SolidBrush(Color.FromArgb(120, 120, 130)))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString(initial, f, b, pr, sf);
                }

                if (_hover)
                {
                    // bottom shade + "Reserve" pill
                    var state = g.Save();
                    g.SetClip(path, CombineMode.Intersect);
                    var shade = new Rectangle(pr.X, pr.Y + pr.Height / 2, pr.Width, pr.Height - pr.Height / 2 + 1);
                    using (var b = new LinearGradientBrush(shade, Color.FromArgb(0, 0, 0, 0), Color.FromArgb(210, 0, 0, 0), 90f))
                        g.FillRectangle(b, shade);
                    g.Restore(state);

                    var pill = new Rectangle(pr.X + (pr.Width - 92) / 2, pr.Bottom - 38, 92, 26);
                    using (var pp = RoundRect(pill, 13))
                    using (var b = new SolidBrush(_accent))
                        g.FillPath(b, pp);
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString("Reserve", _pillFont, Brushes.White, pill, sf);
                }
            }

            // ----- title (max 2 lines) -----
            var titleRect = new Rectangle(card.X + 6, pr.Bottom + 8, card.Width - 12, 34);
            using (var b = new SolidBrush(_title))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.EllipsisWord
            })
                g.DrawString(Movie.Title ?? "", _titleFont, b, titleRect, sf);

            // ----- genre + duration -----
            string sub = Movie.Genre ?? "";
            if (!string.IsNullOrWhiteSpace(Movie.Duration))
                sub += (sub.Length > 0 ? "  \u2022  " : "") + Movie.Duration + " min";
            var subRect = new Rectangle(card.X + 6, titleRect.Bottom, card.Width - 12, 16);
            using (var b = new SolidBrush(_sub))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
                g.DrawString(sub, _subFont, b, subRect, sf);
        }

        // fills the rectangle with the image, cropping the overflow (like CSS "cover")
        private static void DrawCover(Graphics g, Image img, Rectangle dest)
        {
            float scale = Math.Max((float)dest.Width / img.Width, (float)dest.Height / img.Height);
            float sw = dest.Width / scale, sh = dest.Height / scale;
            var src = new RectangleF((img.Width - sw) / 2f, (img.Height - sh) / 2f, sw, sh);
            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);
        }

        private static GraphicsPath RoundRect(Rectangle r, int radius)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _titleFont.Dispose(); _subFont.Dispose(); _pillFont.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // =====================================================================
    //  Section heading: accent bar + title + subtitle (paint only)
    // =====================================================================
    internal class SectionHeader : Control
    {
        private readonly string _title, _subtitle;
        private readonly Color _bg, _accent, _sub;
        private readonly Font _titleFont = new Font("Segoe UI Semibold", 17f);
        private readonly Font _subFont = new Font("Segoe UI", 9f);

        public SectionHeader(string title, string subtitle, Color bg, Color accent, Color sub)
        {
            _title = title; _subtitle = subtitle; _bg = bg; _accent = accent; _sub = sub;
            Dock = DockStyle.Top;
            Height = 64;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(_bg);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var b = new SolidBrush(_accent))
                g.FillRectangle(b, 20, 16, 4, 34);
            g.DrawString(_title, _titleFont, Brushes.White, 32, 9);
            using (var b = new SolidBrush(_sub))
                g.DrawString(_subtitle, _subFont, b, 34, 39);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _titleFont.Dispose(); _subFont.Dispose(); }
            base.Dispose(disposing);
        }
    }
}