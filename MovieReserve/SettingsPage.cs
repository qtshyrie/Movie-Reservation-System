using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MovieReserve
{   
    /// <summary>Data carried by the Manage Movies form / events.</summary>
    public class MovieInput
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public int Duration { get; set; }
        public DateTime Showtime { get; set; }
        public string ShowtimeText { get { return Showtime.ToString("hh:mm tt"); } }
    }

    /// <summary>
    /// Redesigned Settings page (Manage Movies + System Information).
    /// Usage in Form1 (after InitializeComponent):
    ///     SettingsPanel.Controls.Clear();
    ///     var settings = new SettingsPage { Dock = DockStyle.Fill };
    ///     SettingsPanel.Controls.Add(settings);
    /// Hook settings.MovieAdded / MovieUpdated / MovieRemoved to save to MySQL,
    /// and call settings.SetMovies(...) to load rows.
    /// </summary>
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

        public void SetMovies(IEnumerable<MovieInput> movies)
        {
            _movies.Clear();
            foreach (var m in movies) _movies.Add(m);
            UpdateCount();
        }

      
        // ---- state ----
        private readonly BindingList<MovieInput> _movies = new BindingList<MovieInput>();
        private int _nextId = 1;
        private Panel _tabMovies, _tabSystem, _TabDev;
        private TabButton _btnTabMovies, _btnTabSystem, _btnTabDev;
        private TextBox txtTitle;
        private ComboBox cboGenre;
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
            _btnTabDev = new TabButton { Text = "Developers", Location = new Point(388, 6), Size = new Size(140, 38) }; // Non-overlapping X coordinate

            _btnTabMovies.Click += (s, e) => ShowTab(0);
            _btnTabSystem.Click += (s, e) => ShowTab(1);
            _btnTabDev.Click += (s, e) => ShowTab(2);

            strip.Controls.Add(_btnTabMovies);
            strip.Controls.Add(_btnTabSystem);
            strip.Controls.Add(_btnTabDev);

            _tabMovies = BuildMoviesTab();
            _tabSystem = BuildSystemTab();
            _TabDev = BuildDevTab(); // <--- Initialize your Developer tab here!

            // Docking order: add tabs first, then strips/headers on top
            Controls.Add(_tabMovies);
            Controls.Add(_tabSystem);
            Controls.Add(_TabDev);   // <--- Add the dev tab to the form controls
            Controls.Add(strip);
            Controls.Add(header);
        }   

        private void ShowTab(int index)
        {
            _tabMovies.Visible = index == 0;
            _tabSystem.Visible = index == 1;
            _TabDev.Visible = index == 2;

            _btnTabMovies.Active = index == 0;
            _btnTabSystem.Active = index == 1;
            _btnTabDev.Active = index == 2;
            if (index == 1) RefreshSystemInfo();
        }

        // ---------- Manage Movies ----------
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
            y += 40;

            txtTitle = new TextBox { BackColor = Field, ForeColor = TextMain, BorderStyle = BorderStyle.FixedSingle };
            AddField(form, "Title", txtTitle, ref y);

            cboGenre = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Field, ForeColor = TextMain };
            cboGenre.Items.AddRange(new object[] { "Action", "Adventure", "Animation", "Comedy", "Drama", "Fantasy", "Horror", "Romance", "Sci-Fi", "Thriller" });
            cboGenre.SelectedIndex = 0;
            AddField(form, "Genre", cboGenre, ref y);

            numDuration = new NumericUpDown { Minimum = 1, Maximum = 400, Value = 120, BackColor = Field, ForeColor = TextMain, BorderStyle = BorderStyle.FixedSingle };
            AddField(form, "Duration (minutes)", numDuration, ref y);

            dtpShowtime = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "hh:mm tt", ShowUpDown = true };
            AddField(form, "Showtime", dtpShowtime, ref y);

            y += 8;
            btnAdd = new FlatButton { Text = "Add", Primary = true, Location = new Point(24, y), Size = new Size(148, 40) };
            btnUpdate = new FlatButton { Text = "Update", Location = new Point(184, y), Size = new Size(148, 40) };
            y += 50;
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

            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Title", DataPropertyName = "Title", FillWeight = 40 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Genre", DataPropertyName = "Genre", FillWeight = 20 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Duration", DataPropertyName = "Duration", FillWeight = 15 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Showtime", DataPropertyName = "ShowtimeText", FillWeight = 25 });

            g.DataSource = _movies;
            g.SelectionChanged += (s, e) => LoadSelectedIntoForm();
            _movies.ListChanged += (s, e) => UpdateCount();
            UpdateCount();
            return g;
        }

        private void UpdateCount()
        {
            if (lblCount != null)
                lblCount.Text = _movies.Count + (_movies.Count == 1 ? " movie" : " movies") + " scheduled today";
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
            var m = Selected;
            SetSelectionState(m != null);
            if (m == null) return;
            txtTitle.Text = m.Title;
            cboGenre.SelectedItem = m.Genre;
            numDuration.Value = Math.Max(numDuration.Minimum, Math.Min(numDuration.Maximum, m.Duration));
            dtpShowtime.Value = m.Showtime;
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
            return new MovieInput
            {
                Title = txtTitle.Text.Trim(),
                Genre = cboGenre.SelectedItem as string ?? "",
                Duration = (int)numDuration.Value,
                Showtime = dtpShowtime.Value
            };
        }

        private void AddMovie()
        {
            var m = ReadForm();
            if (m == null) return;
            m.Id = _nextId++;
            _movies.Add(m);
            var h = MovieAdded; if (h != null) h(this, m);
            ClearForm();
        }

        private void UpdateMovie()
        {
            var cur = Selected;
            if (cur == null) return;
            var m = ReadForm();
            if (m == null) return;
            cur.Title = m.Title; cur.Genre = m.Genre; cur.Duration = m.Duration; cur.Showtime = m.Showtime;
            _movies.ResetBindings();
            var h = MovieUpdated; if (h != null) h(this, cur);
        }

        private void RemoveMovie()
        {
            var cur = Selected;
            if (cur == null) return;
            if (MessageBox.Show("Remove \"" + cur.Title + "\"?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _movies.Remove(cur);
            var h = MovieRemoved; if (h != null) h(this, cur);
            ClearForm();
        }

        private void ClearForm()
        {
            grid.ClearSelection();
            txtTitle.Clear();
            cboGenre.SelectedIndex = 0;
            numDuration.Value = 120;
            dtpShowtime.Value = DateTime.Now;
            SetSelectionState(false);
            txtTitle.Focus();
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

                CustomAsciiArt = new string[]
                {
                @"         g@M%@%%@N%Nw,,        ",
                @"      ,M*`||*%gNM=]mM%g||%N,   ",
                @"      p!`` '! |```''|||jhlj%w  ",
                @"    ,@L `  ,,       ''!`|j%M]%M",
                @"   ]j`  .,wp@pw,    `.  ''''|%Wg",
                @"  /{||]@NNNNNNNN@pp.        |||||",
                @"  `  ']@NNNNNNNNNNNNN@p    ,   ",
                @"  ,  :]%@@@@@@%%%%%k%h  '*||mkr   *",
                @"  `  j%M`    |jkk'   ~nrn=|i   ;` ",
                @"  !  jrr*^`          `""! L'':! ",
                @"     j lp;,.  / @@   ,;\nmy  ~ ",
                @"  i r @@@@mmHM @@@@  ^*****M*,p ;, ",
                @"  |  ]@NNNNHHH]g@M%%%%H,jmgpmb% j ",
                @"  ;;%%%%%k%@[,.n|;.;j%%k|k%%',[ ",
                @"   H|%%k%%%j%k||,;;j;!!'|ij}]@",
                @"   ""djjmkL,""]] [,,,,wwxw;|#kjk`",
                @"   %;%km%%%%M%M|%%jkkii|||[ ",
                @"     kjj%kkkl|!||||||j|||""",
                @"      |jm%H@@b%%kkmk%i!,[ ",
                @"      @p|j%%%%jkk|||j*``;j[",
                @"     ]@@@g|'```''...,j%k",
                @"     @@@@@mgmp;,,,,;jj%%k%",
                @"    @@@@@@@@%%kgki!|jjjj%k%@ .",
                @" .^[`` %@@@@HH%b%k{illljkjj%%%% ; `.",
                @"=[' ` . %HH%%%%%H@gkilljjj%kk""." , "i"
                }
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

                CustomAsciiArt = new string[]
                {
                @"         g@M%@%%@N%Nw,,        ",
                @"      ,M*`||*%gNM=]mM%g||%N,   ",
                @"      p!`` '! |```''|||jhlj%w  ",
                @"    ,@L `  ,,       ''!`|j%M]%M",
                @"   ]j`  .,wp@pw,    `.  ''''|%Wg",
                @"  /{||]@NNNNNNNN@pp.        |||||",
                @"  `  ']@NNNNNNNNNNNNN@p    ,   ",
                @"  ,  :]%@@@@@@%%%%%k%h  '*||mkr   *",
                @"  `  j%M`    |jkk'   ~nrn=|i   ;` ",
                @"  !  jrr*^`          `""! L'':! ",
                @"     j lp;,.  / @@   ,;\nmy  ~ ",
                @"  i r @@@@mmHM @@@@  ^*****M*,p ;, ",
                @"  |  ]@NNNNHHH]g@M%%%%H,jmgpmb% j ",
                @"  ;;%%%%%k%@[,.n|;.;j%%k|k%%',[ ",
                @"   H|%%k%%%j%k||,;;j;!!'|ij}]@",
                @"   ""djjmkL,""]] [,,,,wwxw;|#kjk`",
                @"   %;%km%%%%M%M|%%jkkii|||[ ",
                @"     kjj%kkkl|!||||||j|||""",
                @"      |jm%H@@b%%kkmk%i!,[ ",
                @"      @p|j%%%%jkk|||j*``;j[",
                @"     ]@@@g|'```''...,j%k",
                @"     @@@@@mgmp;,,,,;jj%%k%",
                @"    @@@@@@@@%%kgki!|jjjj%k%@ .",
                @" .^[`` %@@@@HH%b%k{illljkjj%%%% ; `.",
                @"=[' ` . %HH%%%%%H@gkilljjj%kk""." , "i"
                }
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

                CustomAsciiArt = new string[]
                {
                @"         g@M%@%%@N%Nw,,        ",
                @"      ,M*`||*%gNM=]mM%g||%N,   ",
                @"      p!`` '! |```''|||jhlj%w  ",
                @"    ,@L `  ,,       ''!`|j%M]%M",
                @"   ]j`  .,wp@pw,    `.  ''''|%Wg",
                @"  /{||]@NNNNNNNN@pp.        |||||",
                @"  `  ']@NNNNNNNNNNNNN@p    ,   ",
                @"  ,  :]%@@@@@@%%%%%k%h  '*||mkr   *",
                @"  `  j%M`    |jkk'   ~nrn=|i   ;` ",
                @"  !  jrr*^`          `""! L'':! ",
                @"     j lp;,.  / @@   ,;\nmy  ~ ",
                @"  i r @@@@mmHM @@@@  ^*****M*,p ;, ",
                @"  |  ]@NNNNHHH]g@M%%%%H,jmgpmb% j ",
                @"  ;;%%%%%k%@[,.n|;.;j%%k|k%%',[ ",
                @"   H|%%k%%%j%k||,;;j;!!'|ij}]@",
                @"   ""djjmkL,""]] [,,,,wwxw;|#kjk`",
                @"   %;%km%%%%M%M|%%jkkii|||[ ",
                @"     kjj%kkkl|!||||||j|||""",
                @"      |jm%H@@b%%kkmk%i!,[ ",
                @"      @p|j%%%%jkk|||j*``;j[",
                @"     ]@@@g|'```''...,j%k",
                @"     @@@@@mgmp;,,,,;jj%%k%",
                @"    @@@@@@@@%%kgki!|jjjj%k%@ .",
                @" .^[`` %@@@@HH%b%k{illljkjj%%%% ; `.",
                @"=[' ` . %HH%%%%%H@gkilljjj%kk""." , "i"
                }
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

                CustomAsciiArt = new string[]
                {
                @"         g@M%@%%@N%Nw,,        ",
                @"      ,M*`||*%gNM=]mM%g||%N,   ",
                @"      p!`` '! |```''|||jhlj%w  ",
                @"    ,@L `  ,,       ''!`|j%M]%M",
                @"   ]j`  .,wp@pw,    `.  ''''|%Wg",
                @"  /{||]@NNNNNNNN@pp.        |||||",
                @"  `  ']@NNNNNNNNNNNNN@p    ,   ",
                @"  ,  :]%@@@@@@%%%%%k%h  '*||mkr   *",
                @"  `  j%M`    |jkk'   ~nrn=|i   ;` ",
                @"  !  jrr*^`          `""! L'':! ",
                @"     j lp;,.  / @@   ,;\nmy  ~ ",
                @"  i r @@@@mmHM @@@@  ^*****M*,p ;, ",
                @"  |  ]@NNNNHHH]g@M%%%%H,jmgpmb% j ",
                @"  ;;%%%%%k%@[,.n|;.;j%%k|k%%',[ ",
                @"   H|%%k%%%j%k||,;;j;!!'|ij}]@",
                @"   ""djjmkL,""]] [,,,,wwxw;|#kjk`",
                @"   %;%km%%%%M%M|%%jkkii|||[ ",
                @"     kjj%kkkl|!||||||j|||""",
                @"      |jm%H@@b%%kkmk%i!,[ ",
                @"      @p|j%%%%jkk|||j*``;j[",
                @"     ]@@@g|'```''...,j%k",
                @"     @@@@@mgmp;,,,,;jj%%k%",
                @"    @@@@@@@@%%kgki!|jjjj%k%@ .",
                @" .^[`` %@@@@HH%b%k{illljkjj%%%% ; `.",
                @"=[' ` . %HH%%%%%H@gkilljjj%kk""." , "i"
                }
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
            y += 66;
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
