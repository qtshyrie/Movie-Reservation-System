using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MovieReserve
{
    // Cycles through every image in Assets\poster with a smooth crossfade.
    internal class PosterSlideshowView : UserControl
    {
        private readonly List<Image> _imgs = new List<Image>();
        private readonly Timer _slideTimer = new Timer { Interval = 2000 };  // time between slides
        private readonly Timer _fadeTimer = new Timer { Interval = 15 };     // animation frame rate (~60 fps)
        private readonly Stopwatch _sw = new Stopwatch();

        private int _cur = 0, _next = -1;
        private float _t;                      // eased fade progress, 0..1
        private readonly string _folder;

        public int FadeMs { get; set; } = 700;                 // length of the crossfade
        public int SlideMs { get { return _slideTimer.Interval; } set { _slideTimer.Interval = Math.Max(500, value); } }

        public PosterSlideshowView() : this(Path.Combine(Application.StartupPath, "Assets", "poster")) { }

        public PosterSlideshowView(string folder)
        {
            _folder = folder;
            BackColor = Color.White;
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            if (!DesignMode) LoadImages();

            _slideTimer.Tick += (a, e) => StartFade();
            _fadeTimer.Tick += (a, e) => FadeStep();
        }

        // ---------- loading ----------
        private void LoadImages()
        {
            if (!Directory.Exists(_folder)) return;

            var exts = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            var files = Directory.GetFiles(_folder)
                                 .Where(f => exts.Contains(Path.GetExtension(f).ToLowerInvariant()))
                                 .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

            foreach (var f in files)
            {
                try
                {
                    // read into memory so the file is never locked, and shrink big posters
                    using (var ms = new MemoryStream(File.ReadAllBytes(f)))
                    using (var raw = Image.FromStream(ms))
                        _imgs.Add(Downscale(raw, 900));
                }
                catch { /* skip unreadable files */ }
            }
        }

        private static Image Downscale(Image src, int maxSide)
        {
            float k = Math.Min(1f, (float)maxSide / Math.Max(src.Width, src.Height));
            int w = Math.Max(1, (int)(src.Width * k)), h = Math.Max(1, (int)(src.Height * k));
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, 0, 0, w, h);
            }
            return bmp;
        }

        // ---------- animation ----------
        private void StartFade()
        {
            if (_imgs.Count < 2 || _fadeTimer.Enabled) return;
            _next = (_cur + 1) % _imgs.Count;
            _t = 0;
            _sw.Restart();
            _fadeTimer.Start();
        }

        private void FadeStep()
        {
            float linear = Math.Min(1f, (float)_sw.ElapsedMilliseconds / FadeMs);
            _t = linear * linear * (3f - 2f * linear);   // smoothstep easing
            Invalidate();

            if (linear >= 1f)
            {
                _fadeTimer.Stop();
                _cur = _next;
                _next = -1;
                _t = 0;
                Invalidate();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (DesignMode) return;

            if (Visible && _imgs.Count > 1) _slideTimer.Start();
            else { _slideTimer.Stop(); _fadeTimer.Stop(); }
        }

        // ---------- painting ----------
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (_imgs.Count == 0)
            {
                using (var f = new Font("Segoe UI", 9f))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString("No posters found in\n" + _folder, f, Brushes.Gray, ClientRectangle, sf);
                return;
            }

            DrawCover(g, _imgs[_cur], 1f);
            if (_next >= 0) DrawCover(g, _imgs[_next], _t);
        }

        // Fills the whole control (center-crop, keeps aspect ratio) at the given opacity.
        private void DrawCover(Graphics g, Image img, float alpha)
        {
            Rectangle dest = ClientRectangle;
            if (dest.Width < 1 || dest.Height < 1 || alpha <= 0f) return;

            float scale = Math.Max((float)dest.Width / img.Width, (float)dest.Height / img.Height);
            float sw = dest.Width / scale, sh = dest.Height / scale;
            float sx = (img.Width - sw) / 2f, sy = (img.Height - sh) / 2f;

            if (alpha >= 1f)
            {
                g.DrawImage(img, dest, sx, sy, sw, sh, GraphicsUnit.Pixel);
                return;
            }

            using (var ia = new ImageAttributes())
            {
                ia.SetColorMatrix(new ColorMatrix { Matrix33 = alpha });
                g.DrawImage(img, dest, sx, sy, sw, sh, GraphicsUnit.Pixel, ia);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PosterSlideshowView
            // 
            this.Name = "PosterSlideshowView";
            this.Load += new System.EventHandler(this.PosterSlideshowView_Load);
            this.ResumeLayout(false);

        }

        private void PosterSlideshowView_Load(object sender, EventArgs e)
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _slideTimer.Dispose();
                _fadeTimer.Dispose();
                foreach (var i in _imgs) i.Dispose();
                _imgs.Clear();
            }
            base.Dispose(disposing);
        }
    }
}