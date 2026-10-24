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
        private static readonly Color PanelBg = Color.FromArgb(30, 30, 30);
        private static readonly Color TitleColor = Color.White;
        private static readonly Color SubColor = Color.FromArgb(160, 160, 160);
        private static readonly Color Accent = Color.FromArgb(229, 9, 20);

        private const int Columns = 5;
        private const int TileHeight = 290;

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
                Padding = new Padding(10, 10, 0, 0),
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

            var header = new Label
            {
                Text = "All Showing",
                Dock = DockStyle.Top,
                Height = 50,
                ForeColor = Color.White,
                BackColor = PanelBg,
                Font = new Font("Segoe UI Semibold", 16f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };
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
        private readonly Font _titleFont = new Font("Segoe UI Semibold", 9f);
        private readonly Font _subFont = new Font("Segoe UI", 8f);
        private const int PosterW = 150, PosterH = 225, PosterTop = 6;

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

            // ----- poster -----
            var pr = new Rectangle((Width - PosterW) / 2, PosterTop, PosterW, PosterH);
            using (var path = RoundRect(pr, 4))
            {
                if (_poster != null)
                {
                    var old = g.Clip;
                    g.SetClip(path, CombineMode.Intersect);
                    DrawCover(g, _poster, pr);
                    g.Clip = old;
                }
                else   // placeholder while loading / if the URL fails
                {
                    using (var b = new SolidBrush(Color.FromArgb(55, 55, 55))) g.FillPath(b, path);
                    string initial = string.IsNullOrEmpty(Movie.Title) ? "?" : Movie.Title.Substring(0, 1).ToUpper();
                    using (var f = new Font("Segoe UI", 26f, FontStyle.Bold))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString(initial, f, Brushes.Gray, pr, sf);
                }

                if (_hover)
                    using (var pen = new Pen(_accent, 2f)) g.DrawPath(pen, path);
            }

            // ----- title (max 2 lines) -----
            var titleRect = new Rectangle(4, PosterTop + PosterH + 4, Width - 8, 30);
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
            var subRect = new Rectangle(4, titleRect.Bottom, Width - 8, 14);
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
            if (disposing) { _titleFont.Dispose(); _subFont.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
