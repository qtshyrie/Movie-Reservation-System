using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

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

    // ===================== Input rules (letters + numbers only, with max length) =====================
    public static class InputRules
    {
        public const int MaxName = 30;
        public const int MaxPassword = 20;

        public static bool IsAllowed(char c, bool allowSpace)
        {
            if (c >= 'a' && c <= 'z') return true;
            if (c >= 'A' && c <= 'Z') return true;
            if (c >= '0' && c <= '9') return true;
            return allowSpace && c == ' ';
        }

        public static string Clean(string s, int max, bool allowSpace)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (!IsAllowed(c, allowSpace)) continue;
                if (sb.Length >= max) break;
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static bool IsValid(string s, bool allowSpace)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s)
                if (!IsAllowed(c, allowSpace)) return false;
            return true;
        }

        // Blocks typing AND pasting of anything that is not a letter/number (space optional) and enforces max length.
        public static void Attach(TextBox tb, int maxLength, bool allowSpace)
        {
            tb.MaxLength = maxLength;

            tb.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !IsAllowed(e.KeyChar, allowSpace))
                    e.Handled = true;
            };

            tb.TextChanged += (s, e) =>
            {
                string clean = Clean(tb.Text, maxLength, allowSpace);
                if (clean != tb.Text)
                {
                    int pos = tb.SelectionStart;
                    tb.Text = clean;
                    tb.SelectionStart = Math.Min(pos, clean.Length);
                }
            };
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

    public enum LoginStatus { Success, WrongPassword, NotFound, Locked }

    public class LoginResult
    {
        public LoginStatus Status { get; set; }
        public UserSession User { get; set; }
        public int AttemptsLeft { get; set; }   // -1 = not applicable (admin)
    }

    // ===================== Database =====================
    public static class AccountRepository
    {
        // Same columns your original code used:
        //   ColName = the column the typed "Name" is matched against
        //   ColType = the column that holds "admin" / "staff"

        private const string ColName = "firstname";
        private const string ColLogin = "username";    // or "firstname", see above
        private const string ColType = "role";
        private const string ColPass = "password";
        public const int MaxAttempts = 3;

        public static LoginResult Authenticate(string login, string password)
        {
            string sql =
                $"SELECT id, `{ColType}`, `{ColName}`, " +
                $"IFNULL(BINARY TRIM(`{ColPass}`) = BINARY @p, 0) AS pass_ok, " +
                $"IFNULL(failed_attempts, 0) AS fails, IFNULL(is_locked, 0) AS locked " +
                $"FROM accounts WHERE `{ColLogin}` = @u OR `{ColName}` = @u " +
                $"ORDER BY pass_ok DESC LIMIT 1";

            using (var con = new MySqlConnection(MovieRepository.ConnectionString))
            {
                con.Open();

                int id = 0, attempts = 0;
                string name = "", type = "";
                bool passOk = false, locked = false;

                using (var cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@u", login);
                    cmd.Parameters.AddWithValue("@p", password);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return new LoginResult { Status = LoginStatus.NotFound };
                        id = Convert.ToInt32(r["id"]);
                        name = r[ColName].ToString();
                        type = r[ColType].ToString().Trim();
                        passOk = Convert.ToInt32(r["pass_ok"]) == 1;
                        attempts = Convert.ToInt32(r["fails"]);
                        locked = Convert.ToInt32(r["locked"]) != 0;
                    }
                }

                bool isAdmin = string.Equals(type, "admin", StringComparison.OrdinalIgnoreCase);

                // Locked staff cannot sign in at all (even with the right password) until an admin unlocks them.
                if (!isAdmin && locked)
                    return new LoginResult { Status = LoginStatus.Locked, AttemptsLeft = 0 };

                if (passOk)
                {
                    if (!isAdmin && attempts != 0)
                    {
                        using (var up = new MySqlCommand("UPDATE accounts SET failed_attempts = 0 WHERE id = @id", con))
                        {
                            up.Parameters.AddWithValue("@id", id);
                            up.ExecuteNonQuery();
                        }
                    }
                    return new LoginResult
                    {
                        Status = LoginStatus.Success,
                        User = new UserSession { Id = id, Name = name, Role = type }
                    };
                }

                // Wrong password
                if (isAdmin)   // admins are never locked out
                    return new LoginResult { Status = LoginStatus.WrongPassword, AttemptsLeft = -1 };

                attempts++;
                bool nowLocked = attempts >= MaxAttempts;
                using (var up = new MySqlCommand(
                    "UPDATE accounts SET failed_attempts = @f, is_locked = @l WHERE id = @id", con))
                {
                    up.Parameters.AddWithValue("@f", attempts);
                    up.Parameters.AddWithValue("@l", nowLocked ? 1 : 0);
                    up.Parameters.AddWithValue("@id", id);
                    up.ExecuteNonQuery();
                }

                if (nowLocked)
                    return new LoginResult { Status = LoginStatus.Locked, AttemptsLeft = 0 };

                return new LoginResult
                {
                    Status = LoginStatus.WrongPassword,
                    AttemptsLeft = MaxAttempts - attempts
                };
            }
        }

        // Admin-only: verifies the admin's credentials, then unlocks the staff account and resets its attempts.
        // You can also call this logic from your admin panel (or use UnlockById below).
        public static bool UnlockStaff(string adminName, string adminPass, string staffName, out string message)
        {
            using (var con = new MySqlConnection(MovieRepository.ConnectionString))
            {
                con.Open();

                string chk =
                    $"SELECT COUNT(*) FROM accounts WHERE `{ColLogin}` = @a AND BINARY `{ColPass}` = @p " +
                    $"AND `{ColType}` = 'admin'";
                using (var cmd = new MySqlCommand(chk, con))
                {
                    cmd.Parameters.AddWithValue("@a", adminName);
                    cmd.Parameters.AddWithValue("@p", adminPass);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        message = "Admin name or password is incorrect.";
                        return false;
                    }
                }

                string upd =
                    $"UPDATE accounts SET failed_attempts = 0, is_locked = 0 " +
                    $"WHERE `{ColLogin}` = @s AND `{ColType}` <> 'admin'";
                using (var cmd = new MySqlCommand(upd, con))
                {
                    cmd.Parameters.AddWithValue("@s", staffName);
                    int n = cmd.ExecuteNonQuery();
                    if (n == 0)
                    {
                        message = "No staff account found with that name.";
                        return false;
                    }
                }

                message = "Account unlocked. The staff can sign in again.";
                return true;
            }
        }

        // For your admin panel: call this after you've confirmed the current user IsAdmin.
        public static void UnlockById(int accountId)
        {
            using (var con = new MySqlConnection(MovieRepository.ConnectionString))
            using (var cmd = new MySqlCommand(
                "UPDATE accounts SET failed_attempts = 0, is_locked = 0 WHERE id = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", accountId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void ChangePassword(int accountId, string newPassword)
        {
            using (var con = new MySqlConnection(MovieRepository.ConnectionString))
            using (var cmd = new MySqlCommand(
                $"UPDATE accounts SET `{ColPass}` = @p WHERE id = @id", con))
            {
                cmd.Parameters.AddWithValue("@p", newPassword);
                cmd.Parameters.AddWithValue("@id", accountId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }


    // ===================== Eye button (show / hide password) =====================
    public class EyeButton : Control
    {
        private bool _revealed;
        private bool _hover;

        // true = password is visible
        public bool Revealed
        {
            get { return _revealed; }
            set { _revealed = value; Invalidate(); }
        }

        public EyeButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color c = _hover ? Color.White : Color.FromArgb(170, 170, 170);
            float size = Math.Min(Width, Height);
            float cx = Width / 2f, cy = Height / 2f;
            float hw = size * 0.34f, hh = size * 0.16f;
            float pw = Math.Max(1.6f, size / 26f);

            using (var pen = new Pen(c, pw))
            using (var path = new GraphicsPath())
            {
                // eye outline
                path.AddBezier(cx - hw, cy, cx - hw / 2f, cy - hh * 2f, cx + hw / 2f, cy - hh * 2f, cx + hw, cy);
                path.AddBezier(cx + hw, cy, cx + hw / 2f, cy + hh * 2f, cx - hw / 2f, cy + hh * 2f, cx - hw, cy);
                g.DrawPath(pen, path);

                // pupil
                float pr = size * 0.10f;
                g.DrawEllipse(pen, cx - pr, cy - pr, pr * 2f, pr * 2f);

                // slash when the password is hidden
                if (!_revealed)
                {
                    float sx = hw * 0.95f, sy = hw * 0.85f;
                    using (var cut = new Pen(BackColor, pw * 3f))
                        g.DrawLine(cut, cx - sx + pw * 2f, cy + sy + pw * 2f, cx + sx + pw * 2f, cy - sy + pw * 2f);
                    g.DrawLine(pen, cx - sx, cy + sy, cx + sx, cy - sy);
                }
            }
        }
    }

    // ===================== Admin unlock dialog =====================
    public class AdminUnlockForm : Form
    {
        private readonly TextBox _admin = new TextBox();
        private readonly TextBox _pass = new TextBox();
        private readonly TextBox _staff = new TextBox();
        private readonly Label _msg = new Label();
        private readonly Button _ok = new Button();

        public AdminUnlockForm()
        {
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Unlock Staff Account (Admin only)";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = new Font("Segoe UI", 9.5f);
            ClientSize = new Size(360, 290);

            AddRow("Admin name", _admin, 14);
            AddRow("Admin password", _pass, 74);
            AddRow("Staff name to unlock", _staff, 134);
            _pass.UseSystemPasswordChar = true;

            InputRules.Attach(_admin, InputRules.MaxName, true);
            InputRules.Attach(_pass, InputRules.MaxPassword, false);
            InputRules.Attach(_staff, InputRules.MaxName, true);

            _msg.SetBounds(20, 196, 320, 40);
            _msg.ForeColor = Color.Firebrick;

            _ok.Text = "Unlock";
            _ok.SetBounds(150, 244, 90, 32);
            _ok.Click += OkClick;

            var close = new Button { Text = "Close", DialogResult = DialogResult.Cancel };
            close.SetBounds(250, 244, 90, 32);

            AcceptButton = _ok;
            CancelButton = close;
            Controls.Add(_msg);
            Controls.Add(_ok);
            Controls.Add(close);
        }

        private void AddRow(string caption, TextBox tb, int y)
        {
            Controls.Add(new Label { Text = caption, AutoSize = true, Left = 20, Top = y });
            tb.SetBounds(20, y + 22, 320, 26);
            Controls.Add(tb);
        }

        private async void OkClick(object sender, EventArgs e)
        {
            string a = _admin.Text.Trim(), p = _pass.Text, s = _staff.Text.Trim();
            _msg.ForeColor = Color.Firebrick;

            if (a.Length == 0 || p.Length == 0 || s.Length == 0)
            {
                _msg.Text = "Please fill in all the fields.";
                return;
            }

            _ok.Enabled = false;
            try
            {
                string message = "";
                bool ok = await Task.Run(() => AccountRepository.UnlockStaff(a, p, s, out message));
                _msg.ForeColor = ok ? Color.SeaGreen : Color.Firebrick;
                _msg.Text = message;
                if (ok) { _staff.Clear(); _pass.Clear(); }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unlock failed: " + ex.Message);
                _msg.Text = "Can't reach the database. Is MySQL (XAMPP) running?";
            }
            finally { if (!IsDisposed) _ok.Enabled = true; }
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

        private Label _logo, _title, _error, _help;
        private Panel _userBox, _passBox;
        private TextBox _txtUser, _txtPass;
        private Label _hintUser, _hintPass;
        private CheckBox _chkRemember;
        private Button _btnSignIn;
        private EyeButton _eye;
        private ToolTip _tip;

        // Failed attempts with a name that doesn't exist: 3 tries, then a 30-second wait.
        // static = survives closing/reopening the login form while the app is running.
        private const int UnknownMaxAttempts = 3;
        private const int CooldownSeconds = 30;
        private static int _unknownFails;
        private static DateTime _lockUntil = DateTime.MinValue;
        private System.Windows.Forms.Timer _cooldownTimer;

        private static bool InCooldown { get { return DateTime.UtcNow < _lockUntil; } }
        private static int RemainingSeconds
        {
            get
            {
                int sec = (int)Math.Ceiling((_lockUntil - DateTime.UtcNow).TotalSeconds);
                return sec < 0 ? 0 : sec;
            }
        }

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
                if (InCooldown) { ApplyCooldownUi(); _cooldownTimer.Start(); }
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

            // letters + numbers only, with a maximum length
            InputRules.Attach(_txtUser, InputRules.MaxName, true);
            InputRules.Attach(_txtPass, InputRules.MaxPassword, false);

            // eye button inside the password box
            _eye = new EyeButton { BackColor = FieldBg };
            _eye.Click += (s, e) => TogglePassword();
            _passBox.Controls.Add(_eye);
            _eye.BringToFront();
            _tip = new ToolTip();
            _tip.SetToolTip(_eye, "Show / hide password");

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
            _btnSignIn.MouseLeave += (s, e) => _btnSignIn.BackColor = _btnSignIn.Enabled ? Red : RedHover;
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

            Controls.AddRange(new Control[]
            {
                _logo, _title, _userBox, _passBox, _error, _btnSignIn, _chkRemember, _help
            });

            _cooldownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _cooldownTimer.Tick += CooldownTick;
        }

        private void TogglePassword()
        {
            _eye.Revealed = !_eye.Revealed;
            _txtPass.UseSystemPasswordChar = !_eye.Revealed;
            _txtPass.Focus();
            _txtPass.SelectionStart = _txtPass.TextLength;
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
            text.Enter += (s, e) => SetFieldColor(panel, FieldBgFocus);
            text.Leave += (s, e) => SetFieldColor(panel, FieldBg);

            panel.Controls.Add(hintLbl);
            panel.Controls.Add(text);
            hintLbl.BringToFront();

            box = panel; tb = text; hintLabel = hintLbl;
        }

        // colors the panel and everything inside it (textbox, hint, eye button)
        private static void SetFieldColor(Panel p, Color c)
        {
            p.BackColor = c;
            foreach (Control ch in p.Controls) ch.BackColor = c;
        }

        // ---------- layout ----------
        private void LayoutUi()
        {
            if (_userBox == null) return;

            int cw = S(460), ch = S(416);
            _card = new Rectangle((ClientSize.Width - cw) / 2,
                                  (ClientSize.Height - ch) / 2, cw, ch);
            int pad = S(56), x = _card.Left + pad, w = cw - pad * 2, y = _card.Top;

            _logo.Location = new Point(S(56), S(28));
            _title.Location = new Point(x, y + S(44));

            PlaceField(_userBox, _txtUser, _hintUser, x, y + S(124), w, S(50), 0);
            PlaceField(_passBox, _txtPass, _hintPass, x, y + S(190), w, S(50), S(44));
            _eye.SetBounds(w - S(46), 0, S(42), S(50));

            _error.SetBounds(x, y + S(246), w, S(38));
            _btnSignIn.SetBounds(x, y + S(292), w, S(44));
            Round(_btnSignIn, S(4));

            _chkRemember.Location = new Point(x - S(2), y + S(352));
            _help.Location = new Point(x + w - _help.Width, y + S(354));

            Invalidate(true);
        }

        private void PlaceField(Panel box, TextBox tb, Label hint, int x, int y, int w, int h, int reserveRight)
        {
            box.SetBounds(x, y, w, h);
            tb.Left = S(16);
            tb.Width = w - S(32) - reserveRight;
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
            if (InCooldown) { ApplyCooldownUi(); return; }

            string login = _txtUser.Text.Trim();
            string pass = _txtPass.Text;

            if (login.Length == 0 || pass.Length == 0)
            {
                ShowError("Please enter your name and password.");
                return;
            }

            if (!InputRules.IsValid(login, true) || !InputRules.IsValid(pass, false))
            {
                ShowError("Only letters and numbers are allowed.");
                return;
            }

            SetBusy(true);
            try
            {
                LoginResult res = await Task.Run(() => AccountRepository.Authenticate(login, pass));

                switch (res.Status)
                {
                    case LoginStatus.Success:
                        _unknownFails = 0;
                        SaveRemembered(login);
                        User = res.User;
                        DialogResult = DialogResult.OK;
                        Close();   // close the login form after a successful sign-in
                        break;

                    case LoginStatus.Locked:
                        ShowError("This account is locked after " + AccountRepository.MaxAttempts +
                                  " failed attempts. Please ask your administrator to unlock it.");
                        _txtPass.Clear();
                        break;

                    case LoginStatus.WrongPassword:
                        if (res.AttemptsLeft > 0)
                            ShowError("Incorrect name or password. " + res.AttemptsLeft +
                                      (res.AttemptsLeft == 1 ? " attempt" : " attempts") + " left before this account is locked.");
                        else
                            ShowError("Incorrect name or password. Please try again.");
                        _txtPass.Clear();
                        _txtPass.Focus();
                        break;

                    default:   // NotFound (name doesn't exist)
                        _unknownFails++;
                        if (_unknownFails >= UnknownMaxAttempts)
                        {
                            StartCooldown();
                        }
                        else
                        {
                            int left = UnknownMaxAttempts - _unknownFails;
                            ShowError("Incorrect name or password. " + left +
                                      (left == 1 ? " attempt" : " attempts") + " left.");
                        }
                        _txtPass.Clear();
                        if (!InCooldown) _txtPass.Focus();
                        break;
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
            if (!busy && InCooldown) { ApplyCooldownUi(); return; }   // stay disabled during the countdown

            _btnSignIn.Enabled = !busy;
            _btnSignIn.Text = busy ? "Signing in..." : "Sign In";
            _btnSignIn.BackColor = busy ? RedHover : Red;
            if (busy) _error.Text = "";
        }

        // ---------- 30-second countdown ----------
        private void StartCooldown()
        {
            _lockUntil = DateTime.UtcNow.AddSeconds(CooldownSeconds);
            ApplyCooldownUi();
            _cooldownTimer.Start();
        }

        private void CooldownTick(object sender, EventArgs e)
        {
            if (!InCooldown)
            {
                _cooldownTimer.Stop();
                _unknownFails = 0;
                _error.Text = "";
                SetBusy(false);
                _txtUser.Focus();
                return;
            }
            ApplyCooldownUi();
        }

        private void ApplyCooldownUi()
        {
            int sec = RemainingSeconds;
            _btnSignIn.Enabled = false;
            _btnSignIn.Text = "Try again in " + sec + "s";
            _btnSignIn.BackColor = RedHover;
            _error.Text = "Too many failed attempts. Please wait " + sec +
                          (sec == 1 ? " second" : " seconds") + " before trying again.";
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
                if (_tip != null) _tip.Dispose();
                if (_cooldownTimer != null) _cooldownTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}