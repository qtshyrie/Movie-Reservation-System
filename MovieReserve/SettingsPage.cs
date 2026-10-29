using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MovieReserve
{

    public class MovieInput
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public int Duration { get; set; }
        public DateTime Showtime { get; set; }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public int ShowtimeId { get; set; }
        public int ScreenId { get; set; }
        public decimal Price { get; set; }
        public string RoomName { get; set; }
        public DateTime EndTime { get { return Showtime.AddMinutes(Duration); } }
        public string ShowtimeText { get { return ShowtimeId > 0 ? Showtime.ToString("MMM dd, hh:mm tt") : "No showtime"; } }
    }


    public class AccountItem
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public int FailedAttempts { get; set; }
        public bool IsLocked { get; set; }
        public string Status { get { return IsLocked ? "Locked" : "Active"; } }
    }

    public class SettingsPage : UserControl
    {
        // ---- palette ----
        static readonly Color Bg = Color.FromArgb(18, 18, 20);
        static readonly Color Card = Color.FromArgb(30, 30, 34);
        static readonly Color CardLine = Color.FromArgb(52, 52, 58);
        static readonly Color Field = Color.FromArgb(42, 42, 48);
        static readonly Color TextMain = Color.FromArgb(240, 240, 242);
        static readonly Color TextMuted = Color.FromArgb(150, 150, 158);
        static readonly Color Accent = Color.FromArgb(229, 9, 20);
        static readonly Color AccentHover = Color.FromArgb(192, 0, 0);

        // ---- public API ----
        public event EventHandler<MovieInput> MovieAdded;
        public event EventHandler<MovieInput> MovieUpdated;
        public event EventHandler<MovieInput> MovieRemoved;

        public SettingsPage()
        {
            DoubleBuffered = true;
            BackColor = Bg;
            Font = new Font("Segoe UI", 9.5F);
            BuildUi();
            ShowTab(0);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode) return;
            LoadRooms();
            LoadMoviesFromDb();

            // every minute: showtimes that have finished leave the list (and are moved to MoviesHistory)
            if (_refreshTimer == null)
            {
                _refreshTimer = new Timer { Interval = 60000 };
                _refreshTimer.Tick += (s, ev) =>
                {
                    DateTime now = DateTime.Now;
                    foreach (var m in _all)
                        if (m.ShowtimeId > 0 && m.EndTime <= now) { LoadMoviesFromDb(true); break; }
                };
                _refreshTimer.Start();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _refreshTimer != null) { _refreshTimer.Stop(); _refreshTimer.Dispose(); _refreshTimer = null; }
            base.Dispose(disposing);
        }

        public void SetMovies(IEnumerable<MovieInput> movies)
        {
            _all.Clear();
            foreach (var m in movies) _all.Add(m);
            ApplyFilter();
        }


        // ---- state ----
        private readonly BindingList<MovieInput> _movies = new BindingList<MovieInput>();
        private readonly List<MovieInput> _all = new List<MovieInput>();   // everything still showing (before the search filter)
        private bool _suppressSelect;
        private Timer _refreshTimer;
        private Panel _tabMovies, _tabSystem, _tabAccounts, _TabDev;
        private TabButton _btnTabMovies, _btnTabSystem, _btnTabAccounts, _btnTabDev;
        private TextBox txtTitle, txtUrl, txtDescription, txtSearch;
        private DateTimePicker dtpDate;

        // accounts tab
        private readonly BindingList<AccountItem> _accounts = new BindingList<AccountItem>();
        private bool _suppressAccSelect;
        private TextBox txtAccFirst, txtAccUser, txtAccPass;
        private ComboBox cboAccRole;
        private NumericUpDown numAccFailed;
        private CheckBox chkAccLocked;
        private FlatButton btnAccAdd, btnAccUpdate, btnAccUnlock, btnAccDelete, btnAccClear;
        private DataGridView gridAcc;
        private Label lblAccCount;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        private class RuleException : Exception
        {
            public string Title { get; private set; }
            public RuleException(string title, string message) : base(message) { Title = title; }
        }
        private ComboBox cboGenre, cboRoom;
        private NumericUpDown numDuration;
        private DateTimePicker dtpShowtime;
        private FlatButton btnAdd, btnUpdate, btnRemove, btnClear;
        private DataGridView grid;
        private Label lblCount;
        private readonly Dictionary<string, Label> _infoValues = new Dictionary<string, Label>();

        // ================= UI =================
        private void BuildUi()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = Bg };
            header.Controls.Add(new Label
            {
                Text = "Settings",
                ForeColor = TextMain,
                Font = new Font("Segoe UI Semibold", 20F),
                AutoSize = true,
                Location = new Point(32, 16)
            });
            header.Controls.Add(new Label
            {
                Text = "Manage the movie catalogue and review system details.",
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(34, 56)
            });

            var strip = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Bg };

            _btnTabMovies = new TabButton { Text = "Manage Movies", Location = new Point(32, 6), Size = new Size(160, 38) };
            _btnTabSystem = new TabButton { Text = "System Information", Location = new Point(200, 6), Size = new Size(180, 38) };
            _btnTabAccounts = new TabButton { Text = "Manage Accounts", Location = new Point(388, 6), Size = new Size(170, 38) };
            _btnTabDev = new TabButton { Text = "Developers", Location = new Point(566, 6), Size = new Size(140, 38) };

            _btnTabMovies.Click += (s, e) => ShowTab(0);
            _btnTabSystem.Click += (s, e) => ShowTab(1);
            _btnTabAccounts.Click += (s, e) => ShowTab(2);
            _btnTabDev.Click += (s, e) => ShowTab(3);

            strip.Controls.Add(_btnTabMovies);
            strip.Controls.Add(_btnTabSystem);
            strip.Controls.Add(_btnTabAccounts);
            strip.Controls.Add(_btnTabDev);

            _tabMovies = BuildMoviesTab();
            _tabSystem = BuildSystemTab();
            _tabAccounts = BuildAccountsTab();
            _TabDev = BuildDevTab();

            // Docking order: add tabs first, then strips/headers on top
            Controls.Add(_tabMovies);
            Controls.Add(_tabSystem);
            Controls.Add(_tabAccounts);
            Controls.Add(_TabDev);
            Controls.Add(strip);
            Controls.Add(header);
        }

        private void ShowTab(int index)
        {
            _tabMovies.Visible = index == 0;
            _tabSystem.Visible = index == 1;
            _tabAccounts.Visible = index == 2;
            _TabDev.Visible = index == 3;

            _btnTabMovies.Active = index == 0;
            _btnTabSystem.Active = index == 1;
            _btnTabAccounts.Active = index == 2;
            _btnTabDev.Active = index == 3;
            if (index == 1) RefreshSystemInfo();
            if (index == 2 && !DesignMode) LoadAccountsFromDb();
        }

        // ---------- Manage Movies ----------
        private static bool IsSearchChar(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == ' ';
        }
        private Panel BuildMoviesTab()
        {

            var host = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Padding = new Padding(32, 8, 32, 32) };

            var form = new RoundedCard { Dock = DockStyle.Left, Width = 360 };

            int y = 20;
            form.Controls.Add(new Label
            {
                Text = "Movie details",
                ForeColor = TextMain,
                Font = new Font("Segoe UI Semibold", 12F),
                AutoSize = true,
                Location = new Point(24, y)
            });
            y += 36;

            txtTitle = new TextBox { BackColor = Field, ForeColor = TextMain, BorderStyle = BorderStyle.FixedSingle };
            AddField(form, "Title", txtTitle, ref y);

            cboGenre = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Field, ForeColor = TextMain };
            cboGenre.Items.AddRange(new object[] { "Action", "Adventure", "Animation", "Comedy", "Drama", "Fantasy", "Horror", "Romance", "Sci-Fi", "Thriller" });
            cboGenre.SelectedIndex = 0;
            AddFieldAt(form, "Genre", cboGenre, 24, y, 188, 28);

            numDuration = new NumericUpDown { Minimum = 1, Maximum = 400, Value = 120, BackColor = Field, ForeColor = TextMain, BorderStyle = BorderStyle.FixedSingle };
            AddFieldAt(form, "Duration (min)", numDuration, 224, y, 108, 28);
            y += 54;

            // date + time of the showtime
            dtpDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MMM dd, yyyy",
                CalendarMonthBackground = Field,
                CalendarForeColor = TextMain,
                CalendarTitleBackColor = Accent,
                CalendarTitleForeColor = Color.White,
                CalendarTrailingForeColor = TextMuted
            };
            AddFieldAt(form, "Date", dtpDate, 24, y, 148, 28);

            dtpShowtime = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "hh:mm tt", ShowUpDown = true };
            AddFieldAt(form, "Time", dtpShowtime, 184, y, 148, 28);
            y += 54;

            cboRoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Field, ForeColor = TextMain };
            AddFieldAt(form, "Room", cboRoom, 24, y, 308, 28);
            y += 54;

            txtUrl = new TextBox { BackColor = Field, ForeColor = TextMain, BorderStyle = BorderStyle.FixedSingle, MaxLength = 255 };
            AddField(form, "Poster URL", txtUrl, ref y);

            txtDescription = new TextBox { BackColor = Field, ForeColor = TextMain, BorderStyle = BorderStyle.FixedSingle, Multiline = true, ScrollBars = ScrollBars.Vertical };
            AddFieldAt(form, "Description", txtDescription, 24, y, 308, 48);
            y += 82;

            btnAdd = new FlatButton { Text = "Add", Primary = true, Location = new Point(24, y), Size = new Size(148, 40) };
            btnUpdate = new FlatButton { Text = "Update", Location = new Point(184, y), Size = new Size(148, 40) };
            y += 46;
            btnRemove = new FlatButton { Text = "Remove", Location = new Point(24, y), Size = new Size(148, 40) };
            btnClear = new FlatButton { Text = "Clear", Location = new Point(184, y), Size = new Size(148, 40) };
            btnAdd.Click += (s, e) => AddMovie();
            btnUpdate.Click += (s, e) => UpdateMovie();
            btnRemove.Click += (s, e) => RemoveMovie();
            btnClear.Click += (s, e) => ClearForm();
            form.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnRemove, btnClear });
            SetSelectionState(false);

            var list = new RoundedCard { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var listHead = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Card };
            listHead.Controls.Add(new Label
            {
                Text = "Now Showing",
                ForeColor = TextMain,
                Font = new Font("Segoe UI Semibold", 12F),
                AutoSize = true,
                Location = new Point(0, 4)
            });
            lblCount = new Label { ForeColor = TextMuted, AutoSize = true, Location = new Point(2, 28), Font = new Font("Segoe UI", 8.5F) };
            listHead.Controls.Add(lblCount);

            // search bar (right side of the header)
            txtSearch = new TextBox
            {
                BackColor = Field,
                ForeColor = TextMain,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                Width = 270,
                MaxLength = 24
            };
            txtSearch.Location = new Point(Math.Max(0, listHead.Width - txtSearch.Width), 6);
            listHead.Controls.Add(txtSearch);
            listHead.Resize += (s, e) => txtSearch.Left = Math.Max(0, listHead.ClientSize.Width - txtSearch.Width);
            txtSearch.HandleCreated += (s, e) => SendMessage(txtSearch.Handle, EM_SETCUEBANNER, (IntPtr)1, "Search title, genre, room, date...");
            txtSearch.KeyPress += (s, e) =>
            {
                // allow backspace/ctrl keys, block everything that isn't a letter or space
                if (!char.IsControl(e.KeyChar) && !IsSearchChar(e.KeyChar))
                    e.Handled = true;
            };

            txtSearch.TextChanged += (s, e) =>
            {
                // strip anything invalid that arrived via paste / drag-drop
                string clean = new string(txtSearch.Text.Where(IsSearchChar).ToArray());
                if (clean != txtSearch.Text)
                {
                    int pos = txtSearch.SelectionStart - (txtSearch.Text.Length - clean.Length);
                    txtSearch.Text = clean;                       // re-fires TextChanged with clean text
                    txtSearch.SelectionStart = Math.Max(0, pos);
                    return;
                }
                ApplyFilter();
            };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { txtSearch.Clear(); e.SuppressKeyPress = true; } };

            grid = BuildGrid();
            list.Controls.Add(grid);
            list.Controls.Add(listHead);

            var gap = new Panel { Dock = DockStyle.Left, Width = 20, BackColor = Bg };

            host.Controls.Add(list);
            host.Controls.Add(gap);
            host.Controls.Add(form);
            return host;
        }

        private DataGridView BuildGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = CardLine,
                BackgroundColor = Card,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            g.RowTemplate.Height = 38;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersDefaultCellStyle.BackColor = Card;
            g.ColumnHeadersDefaultCellStyle.ForeColor = TextMuted;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Card;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextMuted;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.DefaultCellStyle.BackColor = Card;
            g.DefaultCellStyle.ForeColor = TextMain;
            g.DefaultCellStyle.SelectionBackColor = Accent;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);

            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Title", DataPropertyName = "Title", FillWeight = 30 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Genre", DataPropertyName = "Genre", FillWeight = 17 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Duration", DataPropertyName = "Duration", FillWeight = 12 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Showtime", DataPropertyName = "ShowtimeText", FillWeight = 24 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Room", DataPropertyName = "RoomName", FillWeight = 17 });

            g.DataSource = _movies;
            g.SelectionChanged += (s, e) => LoadSelectedIntoForm();
            _movies.ListChanged += (s, e) => UpdateCount();
            UpdateCount();
            return g;
        }

        private void UpdateCount()
        {
            if (lblCount == null) return;
            var ids = new HashSet<int>();
            int shows = 0;
            foreach (var m in _movies) { ids.Add(m.Id); if (m.ShowtimeId > 0) shows++; }
            string text = ids.Count + (ids.Count == 1 ? " movie, " : " movies, ") + shows + (shows == 1 ? " showtime" : " showtimes");
            if (_all.Count != _movies.Count) text += "   |   showing " + _movies.Count + " of " + _all.Count;
            lblCount.Text = text;
        }

        /// <summary>Rebuilds the grid from _all using the search box (every word must match somewhere).</summary>
        private void ApplyFilter()
        {
            var cur = Selected;
            int keepId = cur != null ? cur.Id : -1, keepShow = cur != null ? cur.ShowtimeId : -1;
            string[] terms = (txtSearch == null ? "" : txtSearch.Text).Trim().ToLowerInvariant()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            _suppressSelect = true;
            try
            {
                _movies.RaiseListChangedEvents = false;
                _movies.Clear();
                foreach (var m in _all) if (Matches(m, terms)) _movies.Add(m);
                _movies.RaiseListChangedEvents = true;
                _movies.ResetBindings();

                if (grid != null)
                {
                    grid.ClearSelection();
                    if (keepId >= 0)
                        foreach (DataGridViewRow row in grid.Rows)
                        {
                            var item = row.DataBoundItem as MovieInput;
                            if (item != null && item.Id == keepId && item.ShowtimeId == keepShow) { row.Selected = true; break; }
                        }
                }
            }
            finally { _suppressSelect = false; }
            if (btnUpdate != null) SetSelectionState(Selected != null);
            UpdateCount();
        }

        private static bool Matches(MovieInput m, string[] terms)
        {
            if (terms.Length == 0) return true;
            string hay = (m.Title + " " + m.Genre + " " + m.RoomName + " " + m.ShowtimeText + " " + m.Duration + " " +
                          (m.ShowtimeId > 0 ? m.Showtime.ToString("dddd MMMM d yyyy") : "")).ToLowerInvariant();
            foreach (var t in terms) if (hay.IndexOf(t, StringComparison.Ordinal) < 0) return false;
            return true;
        }

        private MovieInput Selected
        {
            get
            {
                if (grid == null || grid.SelectedRows.Count == 0) return null;
                return grid.SelectedRows[0].DataBoundItem as MovieInput;
            }
        }

        private void LoadSelectedIntoForm()
        {
            if (_suppressSelect) return;
            var m = Selected;
            SetSelectionState(m != null);
            if (m == null) return;
            txtTitle.Text = m.Title;
            if (!string.IsNullOrEmpty(m.Genre) && !cboGenre.Items.Contains(m.Genre)) cboGenre.Items.Add(m.Genre);
            cboGenre.SelectedItem = m.Genre;
            txtUrl.Text = m.PosterUrl;
            txtDescription.Text = m.Description;
            numDuration.Value = Math.Max(numDuration.Minimum, Math.Min(numDuration.Maximum, m.Duration));
            dtpDate.Value = m.Showtime.Date < dtpDate.MinDate ? dtpDate.MinDate : m.Showtime.Date;
            dtpShowtime.Value = m.Showtime;
            foreach (ScreenItem sc in cboRoom.Items) if (sc.ScreenId == m.ScreenId) { cboRoom.SelectedItem = sc; break; }
        }

        private void SetSelectionState(bool hasSelection)
        {
            btnUpdate.Enabled = hasSelection;
            btnRemove.Enabled = hasSelection;
        }

        private MovieInput ReadForm()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Please enter a movie title.", "Missing title", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtTitle.Focus();
                return null;
            }
            var room = cboRoom.SelectedItem as ScreenItem;
            if (room == null)
            {
                MessageBox.Show("Please choose a room.", "Missing room", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return new MovieInput
            {
                ScreenId = room.ScreenId,
                RoomName = room.Name,
                Title = txtTitle.Text.Trim(),
                Genre = cboGenre.SelectedItem as string ?? "",
                Duration = (int)numDuration.Value,
                Showtime = dtpDate.Value.Date.AddHours(dtpShowtime.Value.Hour).AddMinutes(dtpShowtime.Value.Minute),
                PosterUrl = txtUrl.Text.Trim(),
                Description = txtDescription.Text.Trim()
            };
        }

        private void AddMovie()
        {
            var m = ReadForm();
            if (m == null) return;
            if (m.Showtime < DateTime.Now)
            {
                MessageBox.Show("That showtime is already in the past. Please pick a later date or time.", "Invalid showtime", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dtpDate.Focus();
                return;
            }
            m.Price = DefaultPrice;
            try { DbAdd(m); }
            catch (RuleException rx) { MessageBox.Show(rx.Message, rx.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            catch (Exception ex) { DbError("add", ex); return; }
            LoadMoviesFromDb();
            var h = MovieAdded; if (h != null) h(this, m);
            ClearForm();
        }

        private void UpdateMovie()
        {
            var cur = Selected;
            if (cur == null) return;
            var m = ReadForm();
            if (m == null) return;
            m.Id = cur.Id;
            m.ShowtimeId = cur.ShowtimeId;
            m.Price = cur.ShowtimeId > 0 ? cur.Price : DefaultPrice;
            // only block "past" when the date/time was actually changed (a show that is running right now can still be edited)
            if ((cur.ShowtimeId == 0 || m.Showtime != cur.Showtime) && m.Showtime < DateTime.Now)
            {
                MessageBox.Show("That showtime is already in the past. Please pick a later date or time.", "Invalid showtime", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dtpDate.Focus();
                return;
            }
            try { DbUpdate(m); }
            catch (RuleException rx) { MessageBox.Show(rx.Message, rx.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            catch (Exception ex) { DbError("update", ex); return; }
            LoadMoviesFromDb();
            foreach (DataGridViewRow row in grid.Rows)
            {
                var item = row.DataBoundItem as MovieInput;
                if (item != null && item.Id == m.Id && item.ShowtimeId == m.ShowtimeId) { grid.ClearSelection(); row.Selected = true; break; }
            }
            var h = MovieUpdated; if (h != null) h(this, m);
        }

        /// <summary>"Remove" no longer deletes: the showtime and all its details are copied to MoviesHistory first.</summary>
        private void RemoveMovie()
        {
            var cur = Selected;
            if (cur == null) return;
            string prompt = cur.ShowtimeId > 0
                ? "Move the " + cur.ShowtimeText + " showtime (" + cur.RoomName + ") of \"" + cur.Title + "\" to Movies History?\n\nIt will leave this list, and all of its details are kept in the MoviesHistory table."
                : "Move \"" + cur.Title + "\" to Movies History?";
            if (MessageBox.Show(prompt, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { DbRemove(cur); }
            catch (Exception ex)
            {
                var my = ex as MySqlException;
                if (my != null && my.Number == 1451)
                    MessageBox.Show("This showtime has booked seats, so it can't be removed.", "Cannot remove", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    DbError("remove", ex);
                return;
            }
            LoadMoviesFromDb();
            var h = MovieRemoved; if (h != null) h(this, cur);
            ClearForm();
        }

        private void ClearForm()
        {
            grid.ClearSelection();
            txtTitle.Clear();
            cboGenre.SelectedIndex = 0;
            numDuration.Value = 120;
            dtpDate.Value = DateTime.Today;
            dtpShowtime.Value = DateTime.Now;
            if (cboRoom.Items.Count > 0) cboRoom.SelectedIndex = 0;
            txtUrl.Clear();
            txtDescription.Clear();
            SetSelectionState(false);
            txtTitle.Focus();
        }

        // ---------- Database (movieslist + showtimes) ----------
        private const decimal DefaultPrice = 200m;  // price used for new showtimes

        private static MySqlConnection Open()
        {
            var conn = new MySqlConnection(MovieRepository.ConnectionString);
            conn.Open();
            return conn;
        }

        private static object Nz(string s) { return string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s; }

        private void DbError(string action, Exception ex)
        {
            string hint = "";
            var my = ex as MySqlException;
            if (my != null && my.Number == 1364) hint = "\n\nmovieslist.movie_id is not AUTO_INCREMENT - run movieslist_fix.sql.";
            MessageBox.Show("Could not " + action + " the movie.\n\n" + ex.Message + hint, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void LoadRooms()
        {
            try
            {
                cboRoom.Items.Clear();
                foreach (var sc in ReservationRepository.GetScreens()) cboRoom.Items.Add(sc);
                if (cboRoom.Items.Count > 0) cboRoom.SelectedIndex = 0;
            }
            catch (Exception ex) { DbError("load the rooms for", ex); }
        }

        // ---------- MoviesHistory (archive) ----------
        private static bool _historyReady;
        private static bool _archiveErrorShown;

        private const string CreateHistorySql =
            "CREATE TABLE IF NOT EXISTS MoviesHistory (" +
            " history_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY," +
            " showtime_id INT NULL," +
            " movie_id INT NULL," +
            " movie_title VARCHAR(255) NOT NULL," +
            " genre VARCHAR(100) NULL," +
            " duration INT NULL," +
            " synopsis TEXT NULL," +
            " poster_url VARCHAR(500) NULL," +
            " screen_id INT NULL," +
            " screen_name VARCHAR(100) NULL," +
            " price DECIMAL(10,2) NULL," +
            " start_time DATETIME NULL," +
            " end_time DATETIME NULL," +
            " archive_reason VARCHAR(20) NOT NULL," +
            " archived_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP," +
            " INDEX idx_hist_showtime (showtime_id)," +
            " INDEX idx_hist_movie (movie_id)" +
            ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4";

        private static void EnsureHistoryTable(MySqlConnection conn)
        {
            if (_historyReady) return;
            using (var cmd = new MySqlCommand(CreateHistorySql, conn)) cmd.ExecuteNonQuery();
            _historyReady = true;
        }

        /// <summary>Copies one showtime (with its movie + room details) into MoviesHistory. Does nothing if it is already there.</summary>
        private static void ArchiveShowtimeRow(MySqlConnection conn, MySqlTransaction tx, int showtimeId, string reason)
        {
            using (var cmd = new MySqlCommand(
                "INSERT INTO MoviesHistory (showtime_id, movie_id, movie_title, genre, duration, synopsis, poster_url, screen_id, screen_name, price, start_time, end_time, archive_reason) " +
                "SELECT s.showtime_id, m.movie_id, m.movie_title, m.genre, m.duration, m.synopsis, m.poster_url, s.screen_id, sc.screen_name, s.price, s.start_time, " +
                "       DATE_ADD(s.start_time, INTERVAL IFNULL(m.duration, 0) MINUTE), @reason " +
                "FROM showtimes s " +
                "JOIN movieslist m ON m.movie_id = s.movie_id " +
                "LEFT JOIN screens sc ON sc.screen_id = s.screen_id " +
                "WHERE s.showtime_id = @sid " +
                "AND NOT EXISTS (SELECT 1 FROM MoviesHistory h WHERE h.showtime_id = s.showtime_id AND h.start_time = s.start_time)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@sid", showtimeId);
                cmd.Parameters.AddWithValue("@reason", reason);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Copies a movie that has no showtime into MoviesHistory.</summary>
        private static void ArchiveMovieRow(MySqlConnection conn, MySqlTransaction tx, int movieId, string reason)
        {
            using (var cmd = new MySqlCommand(
                "INSERT INTO MoviesHistory (movie_id, movie_title, genre, duration, synopsis, poster_url, archive_reason) " +
                "SELECT movie_id, movie_title, genre, duration, synopsis, poster_url, @reason FROM movieslist WHERE movie_id = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", movieId);
                cmd.Parameters.AddWithValue("@reason", reason);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Moves every showtime whose end time (start + duration) has passed into MoviesHistory.</summary>
        private static void ArchiveExpired()
        {
            using (var conn = Open())
            {
                var ids = new List<int>();
                using (var cmd = new MySqlCommand(
                    "SELECT s.showtime_id FROM showtimes s JOIN movieslist m ON m.movie_id = s.movie_id " +
                    "WHERE DATE_ADD(s.start_time, INTERVAL IFNULL(m.duration, 0) MINUTE) <= NOW()", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) ids.Add(Convert.ToInt32(r[0]));

                foreach (int id in ids)
                {
                    using (var tx = conn.BeginTransaction())
                    {
                        ArchiveShowtimeRow(conn, tx, id, "Expired");
                        try
                        {
                            using (var del = new MySqlCommand("DELETE FROM showtimes WHERE showtime_id=@sid", conn, tx))
                            {
                                del.Parameters.AddWithValue("@sid", id);
                                del.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            // 1451 = it still has booked seats / reservations. Keep the row (so those bookings stay valid);
                            // it is already copied to MoviesHistory and the list query hides it because it has ended.
                            if (ex.Number != 1451) throw;
                        }
                        tx.Commit();
                    }
                }
            }
        }

        // ---------- Database (movies) ----------

        public void LoadMoviesFromDb() { LoadMoviesFromDb(false); }

        /// <summary>One row per showtime that has not ended yet (a movie that was never scheduled still appears once).</summary>
        private void LoadMoviesFromDb(bool silent)
        {
            try
            {
                using (var conn = Open()) EnsureHistoryTable(conn);
                ArchiveExpired();
            }
            catch (Exception ex)
            {
                if (!silent && !_archiveErrorShown) { _archiveErrorShown = true; DbError("archive the finished showtimes of", ex); }
            }

            const string sql =
                "SELECT m.movie_id, m.movie_title, m.genre, m.duration, m.synopsis, m.poster_url, " +
                "       s.showtime_id, s.screen_id, sc.screen_name, s.price, s.start_time " +
                "FROM movieslist m " +
                "LEFT JOIN showtimes s ON s.movie_id = m.movie_id " +
                "LEFT JOIN screens sc ON sc.screen_id = s.screen_id " +
                "WHERE (s.showtime_id IS NOT NULL AND DATE_ADD(s.start_time, INTERVAL IFNULL(m.duration, 0) MINUTE) > NOW()) " +
                "   OR (s.showtime_id IS NULL AND NOT EXISTS (SELECT 1 FROM MoviesHistory h WHERE h.movie_id = m.movie_id)) " +
                "ORDER BY m.movie_title, s.start_time";
            try
            {
                var rows = new List<MovieInput>();
                using (var conn = Open())
                using (var cmd = new MySqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var m = new MovieInput
                        {
                            Id = Convert.ToInt32(r["movie_id"]),
                            Title = Convert.ToString(r["movie_title"]),
                            Genre = r["genre"] as string ?? "",
                            Duration = r["duration"] == DBNull.Value ? 0 : Convert.ToInt32(r["duration"]),
                            Description = r["synopsis"] as string ?? "",
                            PosterUrl = r["poster_url"] as string ?? "",
                            Showtime = DateTime.Today
                        };
                        if (r["showtime_id"] != DBNull.Value)
                        {
                            m.ShowtimeId = Convert.ToInt32(r["showtime_id"]);
                            m.ScreenId = Convert.ToInt32(r["screen_id"]);
                            m.RoomName = Convert.ToString(r["screen_name"]);
                            m.Price = Convert.ToDecimal(r["price"]);
                            m.Showtime = Convert.ToDateTime(r["start_time"]);
                        }
                        rows.Add(m);
                    }
                }
                SetMovies(rows);
            }
            catch (Exception ex) { if (!silent) DbError("load", ex); }
        }

        private static void BindMovie(MySqlCommand cmd, MovieInput m)
        {
            cmd.Parameters.AddWithValue("@t", m.Title);
            cmd.Parameters.AddWithValue("@g", Nz(m.Genre));
            cmd.Parameters.AddWithValue("@d", m.Duration);
            cmd.Parameters.AddWithValue("@s", Nz(m.Description));
            cmd.Parameters.AddWithValue("@u", Nz(m.PosterUrl));
        }

        /// <summary>
        /// Throws a RuleException if the showtime would overlap another showtime in the same room.
        /// Overlap = new start &lt; other end AND new end &gt; other start (back-to-back shows are fine).
        /// </summary>
        private static void CheckOverlap(MySqlConnection conn, MySqlTransaction tx, MovieInput m, int ignoreShowtimeId)
        {
            DateTime start = m.Showtime, end = m.Showtime.AddMinutes(m.Duration);
            const string sql =
                "SELECT mv.movie_title, s.start_time, " +
                "       DATE_ADD(s.start_time, INTERVAL IF(s.movie_id = @mid, @dur, IFNULL(mv.duration, 0)) MINUTE) AS end_time " +
                "FROM showtimes s JOIN movieslist mv ON mv.movie_id = s.movie_id " +
                "WHERE s.screen_id = @sc AND s.showtime_id <> @sid " +
                "  AND s.start_time < @end " +
                "  AND DATE_ADD(s.start_time, INTERVAL IF(s.movie_id = @mid, @dur, IFNULL(mv.duration, 0)) MINUTE) > @start " +
                "ORDER BY s.start_time LIMIT 1";
            using (var cmd = new MySqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@sc", m.ScreenId);
                cmd.Parameters.AddWithValue("@sid", ignoreShowtimeId);
                cmd.Parameters.AddWithValue("@mid", m.Id);
                cmd.Parameters.AddWithValue("@dur", m.Duration);
                cmd.Parameters.AddWithValue("@start", start);
                cmd.Parameters.AddWithValue("@end", end);
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return;
                    DateTime from = Convert.ToDateTime(r["start_time"]), to = Convert.ToDateTime(r["end_time"]);
                    throw new RuleException("Schedule conflict",
                        "\"" + Convert.ToString(r["movie_title"]) + "\" already uses " + m.RoomName + " from " +
                        from.ToString("hh:mm tt") + " to " + to.ToString("hh:mm tt") + " on " + from.ToString("MMM dd, yyyy") + ".\n\n" +
                        "Your showtime would run " + start.ToString("hh:mm tt") + " - " + end.ToString("hh:mm tt") +
                        ". Please choose another room, date or time.");
                }
            }
        }

        private void DbAdd(MovieInput m)
        {
            using (var conn = Open())
            using (var tx = conn.BeginTransaction())
            {
                // same title -> add another showtime (e.g. another room) to the existing movie
                object found;
                using (var find = new MySqlCommand("SELECT movie_id FROM movieslist WHERE movie_title=@t LIMIT 1", conn, tx))
                {
                    find.Parameters.AddWithValue("@t", m.Title);
                    found = find.ExecuteScalar();
                }

                if (found != null && found != DBNull.Value)
                {
                    m.Id = Convert.ToInt32(found);
                    using (var cmd = new MySqlCommand(
                        "UPDATE movieslist SET genre=@g, duration=@d, synopsis=@s, poster_url=@u WHERE movie_id=@id", conn, tx))
                    {
                        BindMovie(cmd, m);
                        cmd.Parameters.AddWithValue("@id", m.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var cmd = new MySqlCommand(
                        "INSERT INTO movieslist (movie_title, genre, duration, synopsis, poster_url) VALUES (@t, @g, @d, @s, @u)", conn, tx))
                    {
                        BindMovie(cmd, m);
                        cmd.ExecuteNonQuery();
                        if (cmd.LastInsertedId == 0)
                            throw new InvalidOperationException("movieslist.movie_id is not AUTO_INCREMENT - run movieslist_fix.sql.");
                        m.Id = (int)cmd.LastInsertedId;
                    }
                }
                CheckOverlap(conn, tx, m, 0);       // throws -> nothing above is committed
                DbInsertShowtime(conn, tx, m);
                tx.Commit();
            }
        }

        private void DbUpdate(MovieInput m)
        {
            using (var conn = Open())
            using (var tx = conn.BeginTransaction())
            {
                CheckOverlap(conn, tx, m, m.ShowtimeId);   // ignores the showtime being edited
                using (var cmd = new MySqlCommand(
                    "UPDATE movieslist SET movie_title=@t, genre=@g, duration=@d, synopsis=@s, poster_url=@u WHERE movie_id=@id", conn, tx))
                {
                    BindMovie(cmd, m);
                    cmd.Parameters.AddWithValue("@id", m.Id);
                    cmd.ExecuteNonQuery();
                }
                if (m.ShowtimeId > 0)
                {
                    using (var cmd = new MySqlCommand("UPDATE showtimes SET start_time=@st, screen_id=@sc WHERE showtime_id=@sid", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@st", m.Showtime);
                        cmd.Parameters.AddWithValue("@sc", m.ScreenId);
                        cmd.Parameters.AddWithValue("@sid", m.ShowtimeId);
                        cmd.ExecuteNonQuery();
                    }
                }
                else DbInsertShowtime(conn, tx, m);
                tx.Commit();
            }
        }

        /// <summary>Archive instead of delete: copy to MoviesHistory, then take it out of showtimes.</summary>
        private void DbRemove(MovieInput m)
        {
            using (var conn = Open())
            {
                EnsureHistoryTable(conn);
                using (var tx = conn.BeginTransaction())
                {
                    if (m.ShowtimeId > 0)
                    {
                        ArchiveShowtimeRow(conn, tx, m.ShowtimeId, "Removed");
                        using (var cmd = new MySqlCommand("DELETE FROM showtimes WHERE showtime_id=@sid", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@sid", m.ShowtimeId);
                            cmd.ExecuteNonQuery();      // fails (1451) if seats are already booked -> whole thing rolls back
                        }
                        long left;
                        using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM showtimes WHERE movie_id=@id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", m.Id);
                            left = Convert.ToInt64(cmd.ExecuteScalar());
                        }
                        if (left == 0)
                        {
                            try
                            {
                                using (var cmd = new MySqlCommand("DELETE FROM movieslist WHERE movie_id=@id", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@id", m.Id);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (MySqlException ex) { if (ex.Number != 1451) throw; }  // still referenced elsewhere: keep it (hidden, it has history)
                        }
                    }
                    else
                    {
                        ArchiveMovieRow(conn, tx, m.Id, "Removed");
                        using (var cmd = new MySqlCommand("DELETE FROM movieslist WHERE movie_id=@id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", m.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
            }
        }

        private static void DbInsertShowtime(MySqlConnection conn, MySqlTransaction tx, MovieInput m)
        {
            using (var cmd = new MySqlCommand(
                "INSERT INTO showtimes (movie_id, screen_id, start_time, price) VALUES (@id, @sc, @st, @p)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", m.Id);
                cmd.Parameters.AddWithValue("@sc", m.ScreenId);
                cmd.Parameters.AddWithValue("@st", m.Showtime);
                cmd.Parameters.AddWithValue("@p", m.Price);
                cmd.ExecuteNonQuery();
                m.ShowtimeId = (int)cmd.LastInsertedId;
            }
        }

        // ---------- Manage Accounts ----------

        /// <summary>
        /// IMPORTANT: this must produce exactly what your login screen compares against.
        /// It stores the password as typed (plain text). If your login hashes passwords
        /// (SHA-256, BCrypt, ...), call that same hash function here.
        /// </summary>
        private static string EncodePassword(string plain) { return plain; }

        private Panel BuildAccountsTab()
        {
            var host = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Padding = new Padding(32, 8, 32, 32), Visible = false };
            var form = new RoundedCard { Dock = DockStyle.Left, Width = 360 };

            int y = 20;
            form.Controls.Add(new Label
            {
                Text = "Account details",
                ForeColor = TextMain,
                Font = new Font("Segoe UI Semibold", 12F),
                AutoSize = true,
                Location = new Point(24, y)
            });
            y += 36;

            txtAccFirst = new TextBox { BackColor = Field, ForeColor = TextMain, BorderStyle = BorderStyle.FixedSingle, MaxLength = 38 };
            AddField(form, "Full name", txtAccFirst, ref y);

            txtAccUser = new TextBox { BackColor = Field, ForeColor = TextMain, BorderStyle = BorderStyle.FixedSingle, MaxLength = 38 };
            AddField(form, "Username", txtAccUser, ref y);

            txtAccPass = new TextBox { BackColor = Field, ForeColor = TextMain, BorderStyle = BorderStyle.FixedSingle, MaxLength = 38, UseSystemPasswordChar = true };
            AddField(form, "Password (blank = keep current)", txtAccPass, ref y);

            txtAccFirst.KeyPress += (s, e) =>
            {
                char c = e.KeyChar;
                if (!char.IsControl(c) && !((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.'))
                    e.Handled = true;
            };
            txtAccUser.KeyPress += (s, e) =>
            {
                char c = e.KeyChar;
                if (!char.IsControl(c) && !((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')))
                    e.Handled = true;
            };
            txtAccPass.KeyPress += (s, e) =>
            {
                char c = e.KeyChar;
                if (!char.IsControl(c) && (c < 32 || c > 126))
                    e.Handled = true;
            };

            cboAccRole = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Field, ForeColor = TextMain };
            cboAccRole.Items.AddRange(new object[] { "staff", "admin" });
            cboAccRole.SelectedIndex = 0;
            AddFieldAt(form, "Role", cboAccRole, 24, y, 148, 28);

            numAccFailed = new NumericUpDown { Minimum = 0, Maximum = 999, Value = 0, BackColor = Field, ForeColor = TextMain, BorderStyle = BorderStyle.FixedSingle };
            AddFieldAt(form, "Failed attempts", numAccFailed, 184, y, 148, 28);
            y += 54;

            chkAccLocked = new CheckBox
            {
                Text = "Account is locked",
                ForeColor = TextMain,
                BackColor = Card,
                AutoSize = true,
                Cursor = Cursors.Hand,
                Location = new Point(24, y)
            };
            form.Controls.Add(chkAccLocked);
            y += 38;

            btnAccAdd = new FlatButton { Text = "Add", Location = new Point(24, y), Size = new Size(148, 40) };
            btnAccUpdate = new FlatButton { Text = "Update", Location = new Point(184, y), Size = new Size(148, 40) };
            y += 46;
            btnAccUnlock = new FlatButton { Text = "Reset attempts && unlock", Primary = true, Location = new Point(24, y), Size = new Size(308, 40) };
            y += 46;
            btnAccDelete = new FlatButton { Text = "Delete", Location = new Point(24, y), Size = new Size(148, 40) };
            btnAccClear = new FlatButton { Text = "Clear", Location = new Point(184, y), Size = new Size(148, 40) };
            btnAccAdd.Click += (s, e) => AddAccount();
            btnAccUpdate.Click += (s, e) => UpdateAccount();
            btnAccUnlock.Click += (s, e) => UnlockAccount();
            btnAccDelete.Click += (s, e) => DeleteAccount();
            btnAccClear.Click += (s, e) => ClearAccountForm();
            form.Controls.AddRange(new Control[] { btnAccAdd, btnAccUpdate, btnAccUnlock, btnAccDelete, btnAccClear });
            SetAccountSelectionState(false);

            var list = new RoundedCard { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var listHead = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Card };
            listHead.Controls.Add(new Label
            {
                Text = "Accounts",
                ForeColor = TextMain,
                Font = new Font("Segoe UI Semibold", 12F),
                AutoSize = true,
                Location = new Point(0, 4)
            });
            lblAccCount = new Label { ForeColor = TextMuted, AutoSize = true, Location = new Point(2, 28), Font = new Font("Segoe UI", 8.5F) };
            listHead.Controls.Add(lblAccCount);

            gridAcc = BuildAccountsGrid();
            list.Controls.Add(gridAcc);
            list.Controls.Add(listHead);

            var gap = new Panel { Dock = DockStyle.Left, Width = 20, BackColor = Bg };
            host.Controls.Add(list);
            host.Controls.Add(gap);
            host.Controls.Add(form);
            return host;
        }

        private DataGridView BuildAccountsGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = CardLine,
                BackgroundColor = Card,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            g.RowTemplate.Height = 38;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersDefaultCellStyle.BackColor = Card;
            g.ColumnHeadersDefaultCellStyle.ForeColor = TextMuted;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Card;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextMuted;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.DefaultCellStyle.BackColor = Card;
            g.DefaultCellStyle.ForeColor = TextMain;
            g.DefaultCellStyle.SelectionBackColor = Accent;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);

            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", FillWeight = 8 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "First name", DataPropertyName = "FirstName", FillWeight = 24 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Username", DataPropertyName = "Username", FillWeight = 24 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Role", DataPropertyName = "Role", FillWeight = 12 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Failed", DataPropertyName = "FailedAttempts", FillWeight = 10 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status", FillWeight = 14 });

            g.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                var row = g.Rows[e.RowIndex];
                var a = row.DataBoundItem as AccountItem;
                if (a != null && a.IsLocked && !row.Selected && g.Columns[e.ColumnIndex].DataPropertyName == "Status")
                    e.CellStyle.ForeColor = Color.FromArgb(255, 107, 107);
            };

            g.DataSource = _accounts;
            g.SelectionChanged += (s, e) => LoadSelectedAccountIntoForm();
            _accounts.ListChanged += (s, e) => UpdateAccountCount();
            UpdateAccountCount();
            return g;
        }

        private void UpdateAccountCount()
        {
            if (lblAccCount == null) return;
            int locked = 0;
            foreach (var a in _accounts) if (a.IsLocked) locked++;
            lblAccCount.Text = _accounts.Count + (_accounts.Count == 1 ? " account, " : " accounts, ") + locked + " locked";
        }

        private AccountItem SelectedAccount
        {
            get
            {
                if (gridAcc == null || gridAcc.SelectedRows.Count == 0) return null;
                return gridAcc.SelectedRows[0].DataBoundItem as AccountItem;
            }
        }

        private void SetAccountSelectionState(bool hasSelection)
        {
            btnAccUpdate.Enabled = hasSelection;
            btnAccUnlock.Enabled = hasSelection;
            btnAccDelete.Enabled = hasSelection;
        }

        private void LoadSelectedAccountIntoForm()
        {
            if (_suppressAccSelect) return;
            var a = SelectedAccount;
            SetAccountSelectionState(a != null);
            if (a == null) return;
            FillAccountForm(a);
        }

        private void FillAccountForm(AccountItem a)
        {
            txtAccFirst.Text = a.FirstName;
            txtAccUser.Text = a.Username;
            txtAccPass.Clear();
            cboAccRole.SelectedItem = a.Role;
            numAccFailed.Value = Math.Max(numAccFailed.Minimum, Math.Min(numAccFailed.Maximum, a.FailedAttempts));
            chkAccLocked.Checked = a.IsLocked;
        }

        private void ClearAccountForm()
        {
            gridAcc.ClearSelection();
            txtAccFirst.Clear();
            txtAccUser.Clear();
            txtAccPass.Clear();
            cboAccRole.SelectedIndex = 0;
            numAccFailed.Value = 0;
            chkAccLocked.Checked = false;
            SetAccountSelectionState(false);
            txtAccFirst.Focus();
        }

        private AccountItem ReadAccountForm(bool passwordRequired)
        {
            string first = txtAccFirst.Text;
            string user = txtAccUser.Text;
            string pass = txtAccPass.Text;

            if (string.IsNullOrWhiteSpace(first))
            {
                MessageBox.Show("Please enter the first name.", "Missing first name", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtAccFirst.Focus();
                return null;
            }
            if (!IsValidFirstName(first))
            {
                MessageBox.Show("First name may contain only letters (A-Z, a-z) and dots (.).", "Invalid first name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAccFirst.Focus();
                txtAccFirst.SelectAll();
                return null;
            }

            if (string.IsNullOrWhiteSpace(user))
            {
                MessageBox.Show("Please enter a username.", "Missing username", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtAccUser.Focus();
                return null;
            }
            if (!IsValidUsername(user))
            {
                MessageBox.Show("Username may contain only letters (A-Z, a-z) and numbers (0-9), with no spaces or symbols.", "Invalid username", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAccUser.Focus();
                txtAccUser.SelectAll();
                return null;
            }

            if (passwordRequired && pass.Length == 0)
            {
                MessageBox.Show("Please enter a password for the new account.", "Missing password", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtAccPass.Focus();
                return null;
            }
            // blank is allowed on Update (keeps current); anything typed must be valid
            if (pass.Length > 0 && !IsValidPassword(pass))
            {
                MessageBox.Show("Password may contain only printable ASCII characters (letters, numbers, symbols and spaces).", "Invalid password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAccPass.Focus();
                txtAccPass.SelectAll();
                return null;
            }

            return new AccountItem
            {
                FirstName = first,
                Username = user,
                Role = cboAccRole.SelectedItem as string ?? "staff",
                FailedAttempts = (int)numAccFailed.Value,
                IsLocked = chkAccLocked.Checked
            };
        }

        private void AccountError(string action, Exception ex)
        {
            MessageBox.Show("Could not " + action + " the account.\n\n" + ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void LoadAccountsFromDb()
        {
            try
            {
                var rows = new List<AccountItem>();
                using (var conn = Open())
                using (var cmd = new MySqlCommand(
                    "SELECT id, firstname, username, role, failed_attempts, is_locked FROM accounts ORDER BY role DESC, username", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        rows.Add(new AccountItem
                        {
                            Id = Convert.ToInt32(r["id"]),
                            FirstName = r["firstname"] as string ?? "",
                            Username = r["username"] as string ?? "",
                            Role = Convert.ToString(r["role"]),
                            FailedAttempts = r["failed_attempts"] == DBNull.Value ? 0 : Convert.ToInt32(r["failed_attempts"]),
                            IsLocked = r["is_locked"] != DBNull.Value && Convert.ToInt32(r["is_locked"]) != 0
                        });
                    }
                }

                var cur = SelectedAccount;
                int keep = cur != null ? cur.Id : -1;
                AccountItem again = null;

                _suppressAccSelect = true;
                try
                {
                    _accounts.RaiseListChangedEvents = false;
                    _accounts.Clear();
                    foreach (var a in rows) _accounts.Add(a);
                    _accounts.RaiseListChangedEvents = true;
                    _accounts.ResetBindings();

                    gridAcc.ClearSelection();
                    if (keep >= 0)
                        foreach (DataGridViewRow row in gridAcc.Rows)
                        {
                            var item = row.DataBoundItem as AccountItem;
                            if (item != null && item.Id == keep) { row.Selected = true; again = item; break; }
                        }
                }
                finally { _suppressAccSelect = false; }

                SetAccountSelectionState(again != null);
                if (again != null) FillAccountForm(again);   // show the fresh values (e.g. after an unlock)
                UpdateAccountCount();
            }
            catch (Exception ex) { AccountError("load", ex); }
        }

        private static int CountOtherActiveAdmins(MySqlConnection conn, int excludeId)
        {
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM accounts WHERE role='admin' AND is_locked=0 AND id<>@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static bool IsActiveAdmin(MySqlConnection conn, int id)
        {
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM accounts WHERE id=@id AND role='admin' AND is_locked=0", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static void CheckUsernameFree(MySqlConnection conn, string username, int excludeId)
        {
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM accounts WHERE username=@u AND id<>@id", conn))
            {
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@id", excludeId);
                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                    throw new RuleException("Username taken", "The username \"" + username + "\" is already used by another account.");
            }
        }
        // First name: letters A-Z / a-z and dot only
        private static bool IsValidFirstName(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s)
                if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.'))
                    return false;
            return true;
        }

        // Username: letters A-Z / a-z and digits 0-9 only
        private static bool IsValidUsername(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s)
                if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')))
                    return false;
            return true;
        }

        // Password: printable ASCII 32-126 (includes space)
        private static bool IsValidPassword(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s)
                if (c < 32 || c > 126)
                    return false;
            return true;
        }
        private void AddAccount()
        {
            var a = ReadAccountForm(true);
            if (a == null) return;
            try
            {
                using (var conn = Open())
                {
                    CheckUsernameFree(conn, a.Username, 0);
                    using (var cmd = new MySqlCommand(
                        "INSERT INTO accounts (firstname, username, password, role, failed_attempts, is_locked) VALUES (@f, @u, @p, @r, @fa, @lk)", conn))
                    {
                        cmd.Parameters.AddWithValue("@f", a.FirstName);
                        cmd.Parameters.AddWithValue("@u", a.Username);
                        cmd.Parameters.AddWithValue("@p", EncodePassword(txtAccPass.Text));
                        cmd.Parameters.AddWithValue("@r", a.Role);
                        cmd.Parameters.AddWithValue("@fa", a.FailedAttempts);
                        cmd.Parameters.AddWithValue("@lk", a.IsLocked ? 1 : 0);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (RuleException rx) { MessageBox.Show(rx.Message, rx.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            catch (Exception ex) { AccountError("add", ex); return; }
            ClearAccountForm();
            LoadAccountsFromDb();
        }

        private void UpdateAccount()
        {
            var cur = SelectedAccount;
            if (cur == null) return;
            var a = ReadAccountForm(false);
            if (a == null) return;
            a.Id = cur.Id;
            try
            {
                using (var conn = Open())
                {
                    CheckUsernameFree(conn, a.Username, a.Id);
                    // never leave the system without an unlocked admin
                    if ((a.Role != "admin" || a.IsLocked) && IsActiveAdmin(conn, a.Id) && CountOtherActiveAdmins(conn, a.Id) == 0)
                        throw new RuleException("Last admin", "This is the only unlocked admin account. Make another admin first before demoting or locking it.");

                    bool changePassword = txtAccPass.Text.Length > 0;
                    using (var cmd = new MySqlCommand(
                        "UPDATE accounts SET firstname=@f, username=@u, role=@r, failed_attempts=@fa, is_locked=@lk" +
                        (changePassword ? ", password=@p" : "") + " WHERE id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@f", a.FirstName);
                        cmd.Parameters.AddWithValue("@u", a.Username);
                        cmd.Parameters.AddWithValue("@r", a.Role);
                        cmd.Parameters.AddWithValue("@fa", a.FailedAttempts);
                        cmd.Parameters.AddWithValue("@lk", a.IsLocked ? 1 : 0);
                        if (changePassword) cmd.Parameters.AddWithValue("@p", EncodePassword(txtAccPass.Text));
                        cmd.Parameters.AddWithValue("@id", a.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (RuleException rx) { MessageBox.Show(rx.Message, rx.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            catch (Exception ex) { AccountError("update", ex); return; }
            LoadAccountsFromDb();
        }

        /// <summary>Sets failed_attempts = 0 and is_locked = 0 so the account can log in again.</summary>
        private void UnlockAccount()
        {
            var cur = SelectedAccount;
            if (cur == null) return;
            try
            {
                using (var conn = Open())
                using (var cmd = new MySqlCommand("UPDATE accounts SET failed_attempts=0, is_locked=0 WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", cur.Id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { AccountError("unlock", ex); return; }
            LoadAccountsFromDb();
            MessageBox.Show("\"" + cur.Username + "\" can log in again.\n\nFailed attempts: 0   |   Locked: 0", "Account unlocked", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteAccount()
        {
            var cur = SelectedAccount;
            if (cur == null) return;
            if (MessageBox.Show("Delete the account \"" + cur.Username + "\" (" + cur.Role + ")?\n\nThis cannot be undone.",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                using (var conn = Open())
                {
                    if (IsActiveAdmin(conn, cur.Id) && CountOtherActiveAdmins(conn, cur.Id) == 0)
                        throw new RuleException("Last admin", "This is the only unlocked admin account, so it can't be deleted.");
                    using (var cmd = new MySqlCommand("DELETE FROM accounts WHERE id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", cur.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (RuleException rx) { MessageBox.Show(rx.Message, rx.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            catch (MySqlException my)
            {
                if (my.Number == 1451)
                    MessageBox.Show("This account has bookings/records linked to it, so it can't be deleted. Lock it instead.", "Cannot delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else AccountError("delete", my);
                return;
            }
            catch (Exception ex) { AccountError("delete", ex); return; }
            ClearAccountForm();
            LoadAccountsFromDb();
        }

        // ---------- System Information ----------
        private Panel BuildSystemTab()
        {
            var host = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Padding = new Padding(32, 8, 32, 32), Visible = false };
            var card = new RoundedCard { Dock = DockStyle.Fill };

            card.Controls.Add(new Label
            {
                Text = "System Information",
                ForeColor = TextMain,
                Font = new Font("Segoe UI Semibold", 12F),
                AutoSize = true,
                Location = new Point(28, 22)
            });

            string[] keys = { "Application", "Version", "Runtime Framework", "Database", "Operating system", "Machine name", ".NET runtime", "Date & time" };
            int y = 70;
            foreach (var k in keys)
            {
                var name = new Label { Text = k, ForeColor = TextMuted, AutoSize = true, Location = new Point(28, y + 3) };
                var val = new Label { ForeColor = TextMain, AutoSize = true, Location = new Point(240, y + 3), Font = new Font("Segoe UI Semibold", 9.5F) };
                var line = new Panel { BackColor = CardLine, Location = new Point(28, y + 32), Size = new Size(620, 1) };
                card.Controls.Add(name); card.Controls.Add(val); card.Controls.Add(line);
                _infoValues[k] = val;
                y += 44;
            }
            host.Controls.Add(card);
            return host;
        }

        private Panel BuildDevTab()
        {
            // 2. Create a top sub-strip for developer buttons
            var devButtonStrip = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Bg,
                FlowDirection = FlowDirection.LeftToRight
            };

            // 3. Create a content area panel to hold the active profile card
            var profileContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Bg
            };
            // Push it down slightly so it clears the sub-buttons
            profileContainer.Padding = new Padding(0, 5, 0, 0);

            var host = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Bg,
                Padding = new Padding(32, 16, 32, 32),
                Visible = false
            };

            var dev1 = new GitHubProfilePanel
            {
                Dock = DockStyle.Fill,
                Username = "cristopher@ramirez",
                Title = "studensdfgsdfgsdfgsdt",
                Major = "Computer Science",
                Education = "University of Caloocan City",
                Focuss = "Web and Software Development",
                Experience = "2 yrs student",
                ProgrammingLangs = "Java, C, C#",
                RealLangs = "Tagalog, English   ",
                InterestTech = "Software Dev, Electronics",
                InterestNonTech = "Eating, Egames, Any Sports",
                FacebookUser = "Ramirez Cristopher",
                EmailPersonal = "cristopherram55@gmail.com",
                InstagramUser = "@itzm_yvon",
                GithubUser = "qtshyrie",
                RepoCount = "5",
                StarsCount = "0",
                CommitCount = "4",
                FollowersCount = "2",
                TotalLoc = "11,200",
                AddedLoc = "4,000",
                DeletedLoc = "3,800",
                Margin = new Padding(0, 0, 0, 20), // Adds a clean 20px gap between each profile card!
                Photo = Properties.Resources.ram,
            };

            var dev2 = new GitHubProfilePanel
            {
                Dock = DockStyle.Fill,
                Username = "Gabriel@Jeron",
                Title = "student",
                Major = "Computer Science",
                Education = "University of Caloocan City",
                Focuss = "Web Developing",
                Experience = "2 yrs student",
                ProgrammingLangs = "C, C++, C#",
                RealLangs = "Tagalog, English",
                InterestTech = "Cyber Security, Web Dev",
                InterestNonTech = "Nature Trip, Any Sports",
                FacebookUser = "John Gabriel Jeron",
                EmailPersonal = "jeron.johngabrielbscs2025@gmail.com ",
                InstagramUser = "@johngabriel_019",
                GithubUser = "gabz1913",
                RepoCount = "1",
                StarsCount = "0",
                CommitCount = "0",
                FollowersCount = "1",
                TotalLoc = "11,200",
                AddedLoc = "4,000",
                DeletedLoc = "3,800",
                Margin = new Padding(0, 0, 0, 20),
                Photo = Properties.Resources.gab,
            };

            // --- Developer 3 ---
            var dev3 = new GitHubProfilePanel
            {
                Dock = DockStyle.Fill,
                Username = "Timothy@Agravante",
                Title = "student",
                Major = "Computer Science",
                Education = "University of Caloocan City",
                Focuss = "Web Development",
                Experience = "2 yrs student",
                ProgrammingLangs = "Java, C, C#",
                RealLangs = "Filipino, English",
                InterestTech = "Web Dev, Data Analysis",
                InterestNonTech = "Cooking, Volleyball and Badminton",
                FacebookUser = "Timothy Agravante",
                EmailPersonal = "timothyagravante@gmail.com",
                InstagramUser = "@timagravante",
                GithubUser = "timothyagravante2-cloud",
                RepoCount = "0",
                StarsCount = "0",
                CommitCount = "0",
                FollowersCount = "1",
                TotalLoc = "800",
                AddedLoc = "800",
                DeletedLoc = "0",
                Margin = new Padding(0, 0, 0, 20),
                Photo = Properties.Resources.tim,
            };

            // --- Developer 4 ---
            var dev4 = new GitHubProfilePanel
            {
                Dock = DockStyle.Fill,
                Username = "Jullian@Millar",
                Title = "student",
                Major = "Computer Science",
                Education = "University of Caloocan City",
                Focuss = "-",
                Experience = "2 yrs student",
                ProgrammingLangs = "Java, C, C#",
                RealLangs = "Filipino, English",
                InterestTech = "-",
                InterestNonTech = "-",
                FacebookUser = "Jullian Millar",
                EmailPersonal = "-",
                InstagramUser = "-",
                GithubUser = "JABIBI-ABODO",
                RepoCount = "0",
                StarsCount = "0",
                CommitCount = "0",
                FollowersCount = "1",
                TotalLoc = "500",
                AddedLoc = "500",
                DeletedLoc = "0",
                Margin = new Padding(0, 0, 0, 20),
                Photo = Properties.Resources.juls,
            };

            void ShowDev(GitHubProfilePanel panelToShow)
            {
                profileContainer.Controls.Clear();
                profileContainer.Controls.Add(panelToShow);
            }

            // Create buttons for each dev
            var btnDev1 = new TabButton { Text = "Cristopher", Size = new Size(120, 35) };
            var btnDev2 = new TabButton { Text = "Gabriel", Size = new Size(120, 35) };
            var btnDev3 = new TabButton { Text = "Timothy", Size = new Size(120, 35) };
            var btnDev4 = new TabButton { Text = "Jullian", Size = new Size(120, 35) };

            btnDev1.Click += (s, e) => ShowDev(dev1);
            btnDev2.Click += (s, e) => ShowDev(dev2);
            btnDev3.Click += (s, e) => ShowDev(dev3);
            btnDev4.Click += (s, e) => ShowDev(dev4);

            devButtonStrip.Controls.Add(btnDev1);
            devButtonStrip.Controls.Add(btnDev2);
            devButtonStrip.Controls.Add(btnDev3);
            devButtonStrip.Controls.Add(btnDev4);

            // Default to showing Cristopher first
            ShowDev(dev1);

            // Assemble the host tab
            host.Controls.Add(profileContainer);
            host.Controls.Add(devButtonStrip); // DockTop goes on top of the fill container

            return host;
        }

        private void RefreshSystemInfo()
        {
            Action<string, string> set = (k, v) => { Label l; if (_infoValues.TryGetValue(k, out l)) l.Text = v; };
            set("Application", "MovieReserve - Grand Avenue Cinema");
            set("Version", Application.ProductVersion);
            set("Runtime Framework", ".NET Framework");
            set("Database", "MariaDB");
            set("Operating system", Environment.OSVersion.ToString());
            set("Machine name", Environment.MachineName);
            set(".NET runtime", Environment.Version.ToString());
            set("Date & time", DateTime.Now.ToString("dddd, MMMM d yyyy  hh:mm tt"));
        }

        // ---------- helpers ----------
        private void AddField(Control parent, string caption, Control input, ref int y)
        {
            parent.Controls.Add(new Label { Text = caption, ForeColor = TextMuted, AutoSize = true, Location = new Point(24, y) });
            input.Location = new Point(24, y + 22);
            input.Size = new Size(308, 28);
            input.Font = new Font("Segoe UI", 10F);
            parent.Controls.Add(input);
            y += 54;
        }

        private void AddFieldAt(Control parent, string caption, Control input, int x, int y, int width, int height)
        {
            parent.Controls.Add(new Label { Text = caption, ForeColor = TextMuted, AutoSize = true, Location = new Point(x, y) });
            input.Location = new Point(x, y + 22);
            input.Size = new Size(width, height);
            input.Font = new Font("Segoe UI", 10F);
            parent.Controls.Add(input);
        }

        // ================= custom controls =================
        private class RoundedCard : Panel
        {
            public RoundedCard()
            {
                DoubleBuffered = true;
                BackColor = Card;
                SetStyle(ControlStyles.ResizeRedraw, true);
            }
            protected override void OnPaintBackground(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(Parent != null ? Parent.BackColor : Bg);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var p = Round(r, 14))
                using (var b = new SolidBrush(Card))
                using (var pen = new Pen(CardLine))
                {
                    g.FillPath(b, p);
                    g.DrawPath(pen, p);
                }
            }
        }

        private class FlatButton : Control
        {
            public bool Primary { get; set; }
            private bool _hover, _down;
            public FlatButton()
            {
                Cursor = Cursors.Hand;
                Font = new Font("Segoe UI Semibold", 9.75F);
                SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
                MouseEnter += (s, e) => { _hover = true; Invalidate(); };
                MouseLeave += (s, e) => { _hover = false; _down = false; Invalidate(); };
                MouseDown += (s, e) => { _down = true; Invalidate(); };
                MouseUp += (s, e) => { _down = false; Invalidate(); };
                EnabledChanged += (s, e) => Invalidate();
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(Parent != null ? Parent.BackColor : Bg);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Color fill, fore;
                if (!Enabled) { fill = Color.FromArgb(38, 38, 42); fore = Color.FromArgb(100, 100, 106); }
                else if (Primary) { fill = _down ? Color.FromArgb(150, 0, 0) : _hover ? AccentHover : Accent; fore = Color.White; }
                else { fill = _down ? Color.FromArgb(70, 70, 76) : _hover ? Color.FromArgb(58, 58, 64) : Field; fore = TextMain; }

                var r = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var p = Round(r, 10))
                using (var b = new SolidBrush(fill))
                    g.FillPath(b, p);
                TextRenderer.DrawText(g, Text, Font, r, fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }

        private class TabButton : Control
        {
            private bool _active, _hover;
            public bool Active { get { return _active; } set { _active = value; Invalidate(); } }
            public TabButton()
            {
                DoubleBuffered = true;
                Cursor = Cursors.Hand;
                Font = new Font("Segoe UI Semibold", 9.75F);
                MouseEnter += (s, e) => { _hover = true; Invalidate(); };
                MouseLeave += (s, e) => { _hover = false; Invalidate(); };
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(Parent != null ? Parent.BackColor : Bg);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, Width - 1, Height - 1);
                Color fill = _active ? Accent : _hover ? Color.FromArgb(44, 44, 50) : Card;
                using (var p = Round(r, 18))
                using (var b = new SolidBrush(fill))
                using (var pen = new Pen(_active ? Accent : CardLine))
                {
                    g.FillPath(b, p);
                    g.DrawPath(pen, p);
                }
                TextRenderer.DrawText(g, Text, Font, r, _active ? Color.White : TextMain,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }

        private static GraphicsPath Round(Rectangle r, int radius)
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