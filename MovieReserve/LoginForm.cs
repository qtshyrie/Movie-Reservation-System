using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;

namespace MovieReserve
{

    public class MovieItem
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string PosterUrl { get; set; }
        public string Genre { get; internal set; }
        public string Synopsis { get; internal set; }
        public string Duration { get; internal set; }
    }

    public static class MovieRepository
    {
        public static string ConnectionString =
            "Server=127.0.0.1;Port=3306;Database=movie_reserve_db;Uid=root;Pwd=;";

        public static List<MovieItem> GetMovies()
        {
            var list = new List<MovieItem>();
            using (var con = new MySqlConnection(ConnectionString))
            using (var cmd = new MySqlCommand(
                "SELECT movie_id, movie_title, genre, synopsis, duration, poster_url FROM movieslist", con))
            {
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new MovieItem
                        {
                            MovieId = Convert.ToInt32(r["movie_id"]),
                            Title = r["movie_title"].ToString(),
                            Genre = r["genre"] == DBNull.Value ? "" : r["genre"].ToString(),
                            Synopsis = r["synopsis"] == DBNull.Value ? "" : r["synopsis"].ToString(),
                            Duration = r["duration"] == DBNull.Value ? "" : r["duration"].ToString(),
                            PosterUrl = r["poster_url"] == DBNull.Value ? "" : r["poster_url"].ToString()
                        });
                    }
                }
            }
            return list;
        }
    }



    // ===================== Who is logged in =====================
    public class UserSession
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public bool IsAdmin => string.Equals(Role, "admin", StringComparison.OrdinalIgnoreCase);

        public string Initials
        {
            get
            {
                var parts = (Name ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return "?";
                if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
                return (parts[0][0].ToString() + parts[1][0]).ToUpper();
            }
        }
    }

    // ===================== Database =====================
    public static class AccountRepository
    {

        private const string Colrole = "role";
        private const string Coluser = "username";
        private const string ColPass = "password";


        public static UserSession Authenticate(string login, string password)
        {
            string sql =
                $"SELECT id, `{Colrole}`, `{Coluser}` FROM accounts " +
                $"WHERE `{Colrole}` = @u AND BINARY `{ColPass}` = @p LIMIT 1";

            using (var con = new MySqlConnection(MovieRepository.ConnectionString))
            using (var cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@u", login);
                cmd.Parameters.AddWithValue("@p", password);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;
                    return new UserSession
                    {
                        Id = Convert.ToInt32(r.GetValue(0)),
                        Name = r.GetValue(1).ToString(),
                        Role = r.GetValue(2).ToString().Trim()
                    };
                }
            }
        }
    }

    // ===================== The login form =====================
    public class LoginForm : Form
    {
        public UserSession User { get; private set; }

        private static readonly Color Red = Color.FromArgb(229, 9, 20);
        private static readonly Color RedHover = Color.FromArgb(193, 17, 25);
        private static readonly Color FieldBg = Color.FromArgb(51, 51, 51);
        private static readonly Color FieldBgFocus = Color.FromArgb(74, 74, 74);
        private static readonly string RememberFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MovieReserve", "remember.txt");

        private readonly float _scale;
        private Rectangle _card;
        private Bitmap _bg;
        private readonly List<Image> _posters = new List<Image>();

        private Label _logo, _title, _error, _help, _footer;
        private Panel _userBox, _passBox;
        private TextBox _txtUser, _txtPass;
        private Label _hintUser, _hintPass;
        private CheckBox _chkRemember;
        private Button _btnSignIn;

        private static readonly HttpClient _http = CreateClient();
        private static HttpClient CreateClient()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var c = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            return c;
        }

        private int S(int v) { return (int)Math.Round(v * _scale); }

        public LoginForm()
        {
            using (var g = CreateGraphics()) _scale = g.DpiX / 96f;

            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Bounds = Screen.PrimaryScreen.WorkingArea;   // fills the screen above the taskbar
            BackColor = Color.Black;
            Text = "MovieReserve - Sign In";
            DoubleBuffered = true;
            ResizeRedraw = true;

            BuildControls();
            AcceptButton = _btnSignIn;

            LayoutUi();          // place controls before the first paint
            BuildBackground();

            Load += (s, e) =>
            {
                Bounds = Screen.PrimaryScreen.WorkingArea;   // re-applied every time the form opens
                LayoutUi();
                BuildBackground();
            };
            Resize += (s, e) => { LayoutUi(); BuildBackground(); };
            Shown += async (s, e) =>
            {
                LoadRemembered();
                (_txtUser.Text.Length > 0 ? _txtPass : _txtUser).Focus();
                await LoadPosterBackgroundAsync();
            };
        }

        // ---------- controls ----------
        private void BuildControls()
        {
            _logo = MakeLabel("MOVIERESERVE", new Font("Segoe UI Black", 24f), Red);

            _title = MakeLabel("Sign In", new Font("Segoe UI", 24f, FontStyle.Bold), Color.White);

            CreateField("Name", false, out _userBox, out _txtUser, out _hintUser);
            CreateField("Password", true, out _passBox, out _txtPass, out _hintPass);

            _error = MakeLabel("", new Font("Segoe UI", 9.5f), Color.FromArgb(229, 80, 0));
            _error.AutoSize = false;

            _btnSignIn = new Button
            {
                Text = "Sign In",
                FlatStyle = FlatStyle.Flat,
                BackColor = Red,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11f),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            _btnSignIn.FlatAppearance.BorderSize = 0;
            _btnSignIn.MouseEnter += (s, e) => { if (_btnSignIn.Enabled) _btnSignIn.BackColor = RedHover; };
            _btnSignIn.MouseLeave += (s, e) => _btnSignIn.BackColor = Red;
            _btnSignIn.Click += BtnSignIn_Click;

            _chkRemember = new CheckBox
            {
                Text = "Remember me",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(179, 179, 179),
                Font = new Font("Segoe UI", 9.5f),
                Checked = true
            };

            _help = MakeLabel("Need help?", new Font("Segoe UI", 9.5f), Color.FromArgb(179, 179, 179));
            _help.Cursor = Cursors.Hand;
            _help.MouseEnter += (s, e) => _help.ForeColor = Color.White;
            _help.MouseLeave += (s, e) => _help.ForeColor = Color.FromArgb(179, 179, 179);
            _help.Click += (s, e) => MessageBox.Show(
                "Forgot your password? Please ask your administrator to reset it for you.",
                "Need help?", MessageBoxButtons.OK, MessageBoxIcon.Information);

            _footer = MakeLabel("Staff and admin accounts only.\r\nNeed access? Ask your administrator.",
                                new Font("Segoe UI", 9.5f), Color.FromArgb(140, 140, 140));
            _footer.AutoSize = false;

            Controls.AddRange(new Control[]
            {
                _logo, _title, _userBox, _passBox, _error, _btnSignIn, _chkRemember, _help, _footer
            });
        }

        private static Label MakeLabel(string text, Font font, Color color)
        {
            return new Label { Text = text, Font = font, ForeColor = color, BackColor = Color.Transparent, AutoSize = true };
        }

        private void CreateField(string hint, bool password, out Panel box, out TextBox tb, out Label hintLabel)
        {
            var panel = new Panel { BackColor = FieldBg };
            var text = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = FieldBg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f),
                UseSystemPasswordChar = password
            };
            var hintLbl = new Label
            {
                Text = hint,
                AutoSize = true,
                BackColor = FieldBg,
                ForeColor = Color.FromArgb(160, 160, 160),
                Font = new Font("Segoe UI", 11.5f),
                Cursor = Cursors.IBeam
            };

            hintLbl.Click += (s, e) => text.Focus();
            panel.Click += (s, e) => text.Focus();
            text.TextChanged += (s, e) => hintLbl.Visible = text.TextLength == 0;
            text.Enter += (s, e) => SetFieldColor(panel, text, hintLbl, FieldBgFocus);
            text.Leave += (s, e) => SetFieldColor(panel, text, hintLbl, FieldBg);

            panel.Controls.Add(hintLbl);
            panel.Controls.Add(text);
            hintLbl.BringToFront();

            box = panel; tb = text; hintLabel = hintLbl;
        }

        private static void SetFieldColor(Panel p, TextBox t, Label h, Color c)
        {
            p.BackColor = c; t.BackColor = c; h.BackColor = c;
        }

        // ---------- layout ----------
        private void LayoutUi()
        {
            if (_userBox == null) return;

            int cw = S(460), ch = S(538);
            _card = new Rectangle((ClientSize.Width - cw) / 2,
                                  (ClientSize.Height - ch) / 2, cw, ch);
            int pad = S(56), x = _card.Left + pad, w = cw - pad * 2, y = _card.Top;

            _logo.Location = new Point(S(56), S(28));
            _title.Location = new Point(x, y + S(44));

            PlaceField(_userBox, _txtUser, _hintUser, x, y + S(124), w, S(50));
            PlaceField(_passBox, _txtPass, _hintPass, x, y + S(190), w, S(50));

            _error.SetBounds(x, y + S(246), w, S(38));
            _btnSignIn.SetBounds(x, y + S(292), w, S(44));
            Round(_btnSignIn, S(4));

            _chkRemember.Location = new Point(x - S(2), y + S(352));
            _help.Location = new Point(x + w - _help.Width, y + S(354));
            _footer.SetBounds(x, y + S(424), w, S(70));

            Invalidate(true);
        }

        private void PlaceField(Panel box, TextBox tb, Label hint, int x, int y, int w, int h)
        {
            box.SetBounds(x, y, w, h);
            tb.Left = S(16);
            tb.Width = w - S(32);
            tb.Top = (h - tb.Height) / 2;
            hint.Left = S(16);
            hint.Top = (h - hint.Height) / 2;
            Round(box, S(4));
        }

        private static void Round(Control c, int radius)
        {
            if (c.Width <= 0 || c.Height <= 0) return;
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

        // ---------- background (tilted poster wall, like Netflix) ----------
        private async Task LoadPosterBackgroundAsync()
        {
            try
            {
                List<MovieItem> movies = await Task.Run(() => MovieRepository.GetMovies());
                Debug.WriteLine("Login background: " + movies.Count + " movies in DB");

                var tasks = new List<Task<Bitmap>>();
                foreach (var m in movies)
                    if (!string.IsNullOrWhiteSpace(m.PosterUrl)) tasks.Add(GetPosterAsync(m));

                Bitmap[] done = await Task.WhenAll(tasks);
                foreach (var b in done)
                {
                    if (b == null) continue;
                    if (IsDisposed) { b.Dispose(); continue; }
                    _posters.Add(b);
                }
                Debug.WriteLine("Login background: " + _posters.Count + " of " + tasks.Count + " posters loaded");
                if (!IsDisposed && _posters.Count > 0) { BuildBackground(); Invalidate(); }
            }
            catch (Exception ex) { Debug.WriteLine("Login background: " + ex.Message); }
        }

        // Posters are cached on disk (%AppData%\MovieReserve\posters) so the next start is instant.
        // If you change a movie's poster_url, delete that folder.
        private static readonly string PosterCacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MovieReserve", "posters");

        private static async Task<Bitmap> GetPosterAsync(MovieItem m)
        {
            string file = Path.Combine(PosterCacheDir, "poster_" + m.MovieId + ".img");
            try
            {
                byte[] data;
                bool fromCache = File.Exists(file);
                if (fromCache) data = File.ReadAllBytes(file);
                else
                {
                    using (var req = new HttpRequestMessage(HttpMethod.Get, m.PosterUrl))
                    {
                        // Ask for JPG/PNG (GDI+ can't read WebP) and look like a normal browser request
                        req.Headers.Accept.ParseAdd("image/jpeg,image/png,image/*;q=0.8");
                        Uri u;
                        if (Uri.TryCreate(m.PosterUrl, UriKind.Absolute, out u))
                            req.Headers.Referrer = new Uri(u.GetLeftPart(UriPartial.Authority) + "/");
                        using (var resp = await _http.SendAsync(req))
                        {
                            resp.EnsureSuccessStatusCode();
                            data = await resp.Content.ReadAsByteArrayAsync();
                        }
                    }
                }

                using (var ms = new MemoryStream(data))
                using (var img = Image.FromStream(ms))
                {
                    var bmp = new Bitmap(img);
                    if (!fromCache)
                    {
                        Directory.CreateDirectory(PosterCacheDir);
                        File.WriteAllBytes(file, data);   // only cache images that decoded fine
                    }
                    return bmp;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Poster failed: " + m.Title + " -> " + ex.Message + "  (" + m.PosterUrl + ")");
                return null;
            }
        }

        private void BuildBackground()
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            if (w < 10 || h < 10) return;

            var old = _bg;
            _bg = new Bitmap(w, h);
            using (var g = Graphics.FromImage(_bg))
            {
                g.Clear(Color.Black);

                if (_posters.Count > 0)
                {
                    g.InterpolationMode = InterpolationMode.Bilinear;
                    int tw = S(150), th = S(225), gap = S(12);
                    float diag = (float)Math.Sqrt((double)w * w + (double)h * h);
                    int cols = (int)(diag / (tw + gap)) + 2, rows = (int)(diag / (th + gap)) + 2;
                    float x0 = -cols * (tw + gap) / 2f, y0 = -rows * (th + gap) / 2f;

                    g.TranslateTransform(w / 2f, h / 2f);
                    g.RotateTransform(-14f);
                    for (int r = 0; r < rows; r++)
                        for (int c = 0; c < cols; c++)
                        {
                            Image img = _posters[(r * 3 + c) % _posters.Count];
                            var dest = new RectangleF(x0 + c * (tw + gap),
                                                      y0 + r * (th + gap) + (c % 2 == 0 ? 0 : S(40)), tw, th);
                            DrawCover(g, img, dest);
                        }
                    g.ResetTransform();
                }

                if (_posters.Count == 0)   // no posters yet: dark red glow instead of flat black
                    using (var lg = new LinearGradientBrush(new Rectangle(0, 0, w, h),
                               Color.FromArgb(75, 0, 0), Color.Black, 45f))
                        g.FillRectangle(lg, 0, 0, w, h);

                // dark overlay + top/bottom fade
                using (var b = new SolidBrush(Color.FromArgb(150, 0, 0, 0))) g.FillRectangle(b, 0, 0, w, h);
                int fade = S(220);
                using (var lg = new LinearGradientBrush(new Rectangle(0, 0, w, fade),
                           Color.FromArgb(235, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), 90f))
                    g.FillRectangle(lg, 0, 0, w, fade);
                using (var lg = new LinearGradientBrush(new Rectangle(0, h - fade, w, fade),
                           Color.FromArgb(0, 0, 0, 0), Color.FromArgb(235, 0, 0, 0), 90f))
                    g.FillRectangle(lg, 0, h - fade, w, fade);
            }
            if (old != null) old.Dispose();
            Invalidate();
        }

        private static void DrawCover(Graphics g, Image img, RectangleF dest)
        {
            float scale = Math.Max(dest.Width / img.Width, dest.Height / img.Height);
            float sw = dest.Width / scale, sh = dest.Height / scale;
            var src = new RectangleF((img.Width - sw) / 2f, (img.Height - sh) / 2f, sw, sh);
            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);
        }

        protected override void OnPaintBackground(PaintEventArgs e) { /* painted in OnPaint */ }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            if (_bg != null) g.DrawImageUnscaled(_bg, 0, 0); else g.Clear(Color.Black);

            if (_card.Width > 0)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var p = RoundPath(_card, S(8)))
                using (var b = new SolidBrush(Color.FromArgb(190, 0, 0, 0)))
                    g.FillPath(b, p);
            }
        }

        // ---------- sign in ----------
        private async void BtnSignIn_Click(object sender, EventArgs e)
        {
            string login = _txtUser.Text.Trim();
            string pass = _txtPass.Text;

            if (login.Length == 0 || pass.Length == 0)
            {
                ShowError("Please enter your name and password.");
                return;
            }

            SetBusy(true);
            try
            {
                UserSession u = await Task.Run(() => AccountRepository.Authenticate(login, pass));
                if (u == null)
                {
                    ShowError("Incorrect name or password. Please try again.");
                    _txtPass.Clear();
                    _txtPass.Focus();
                }
                else
                {
                    SaveRemembered(u.Name);
                    User = u;
                    DialogResult = DialogResult.OK;
                    Close();   // close the login form after a successful sign-in
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Login failed: " + ex.Message);
                ShowError("Can't reach the database. Is MySQL (XAMPP) running?");
            }
            finally { if (!IsDisposed) SetBusy(false); }
        }

        private void ShowError(string message) { _error.Text = message; }

        private void SetBusy(bool busy)
        {
            _btnSignIn.Enabled = !busy;
            _btnSignIn.Text = busy ? "Signing in..." : "Sign In";
            _btnSignIn.BackColor = busy ? RedHover : Red;
            if (busy) _error.Text = "";
        }

        // ---------- remember me ----------
        private void LoadRemembered()
        {
            try { if (File.Exists(RememberFile)) _txtUser.Text = File.ReadAllText(RememberFile).Trim(); }
            catch { }
        }

        private void SaveRemembered(string name)
        {
            try
            {
                if (_chkRemember.Checked)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(RememberFile));
                    File.WriteAllText(RememberFile, name);
                }
                else if (File.Exists(RememberFile)) File.Delete(RememberFile);
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var p in _posters) p.Dispose();
                if (_bg != null) _bg.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
