using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;

namespace MovieReserve
{
    // =====================================================================
    //  "Bookings" list shown under the movie posters (staff and admin).
    //  Reads every row of the bookings table, newest first, with a search box.
    // =====================================================================
    internal class BookingsListPanel : UserControl
    {
        // TODO: use the same connection string your MoviesListPanel uses
        private const string ConnStr = "Server=127.0.0.1;Database=movie_reserve_db;Uid=root;Pwd=;";
        private const string BlockedChars = "!@#$%^&*()_+;.,/'[]";   // remove from Form1
        private static readonly Color Card = Color.FromArgb(30, 30, 34);
        private static readonly Color Line = Color.FromArgb(48, 48, 54);
        private static readonly Color Muted = Color.FromArgb(140, 140, 150);
        private static readonly Color Red = Color.FromArgb(229, 9, 20);

        private readonly Label _title, _sub;
        private readonly TextBox _search;
        private readonly DataGridView _grid;
        private readonly DataTable _table = new DataTable();

        public BookingsListPanel()
        {
            BackColor = Card;

            _table.Columns.Add("id", typeof(int));
            _table.Columns.Add("name", typeof(string));
            _table.Columns.Add("booked_on", typeof(string));

            _title = new Label
            {
                Text = "Bookings",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 14f),
                ForeColor = Color.White,
                Location = new Point(24, 12)
            };
            _sub = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Muted,
                Location = new Point(26, 40)
            };

            _search = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Card,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f),
                Width = 280
            };
            UiKit.SetCue(_search, "Search booking #, name, date...");
            _search.MaxLength = 40;

            _search.KeyPress += (a, e) =>
            {
                if (!char.IsControl(e.KeyChar) && BlockedChars.IndexOf(e.KeyChar) >= 0)
                    e.Handled = true;
            };

            _search.TextChanged += (a, e) =>
            {
                var sb = new StringBuilder();
                foreach (char c in _search.Text)
                    if (BlockedChars.IndexOf(c) < 0) sb.Append(c);
                string clean = sb.ToString();
                if (clean != _search.Text)
                {
                    int pos = Math.Max(0, _search.SelectionStart - (_search.Text.Length - clean.Length));
                    _search.Text = clean;
                    _search.SelectionStart = pos;
                }
                ApplyFilter();
            };

            _grid = new DataGridView
            {
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Card,
                GridColor = Line,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 32,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _grid.RowTemplate.Height = 36;

            _grid.ColumnHeadersDefaultCellStyle.BackColor = Card;
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Card;
            _grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Muted;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9f);
            _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);

            _grid.DefaultCellStyle.BackColor = Card;
            _grid.DefaultCellStyle.ForeColor = Color.White;
            _grid.DefaultCellStyle.SelectionBackColor = Red;
            _grid.DefaultCellStyle.SelectionForeColor = Color.White;
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10f);
            _grid.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);

            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "id", HeaderText = "Booking #", FillWeight = 15 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "name", HeaderText = "Customer", FillWeight = 45 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "booked_on", HeaderText = "Booked on", FillWeight = 40 });
            _grid.DataSource = _table.DefaultView;

            Controls.AddRange(new Control[] { _title, _sub, _search, _grid });

            Resize += (a, e) => DoLayout();
            DoLayout();
            ApplyFilter();
        }

        private void DoLayout()
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            if (w < 100) return;
            _search.Location = new Point(w - 24 - _search.Width, 18);
            _grid.SetBounds(20, 68, w - 40, Math.Max(0, h - 68 - 12));
        }

        // Call on startup and whenever the Movies page is shown again
        public async Task LoadAsync()
        {
            try
            {
                using (var conn = new MySqlConnection(ConnStr))
                {
                    await conn.OpenAsync();
                    const string sql =
                        "SELECT booking_id, customer_name, booking_date FROM bookings ORDER BY booking_id DESC";
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        _table.Rows.Clear();
                        while (await r.ReadAsync())
                        {
                            _table.Rows.Add(
                                r.GetInt32(0),
                                r.GetString(1),
                                r.GetDateTime(2).ToString("MMM dd yyyy  hh:mm tt", CultureInfo.InvariantCulture));
                        }
                    }
                }
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _sub.Text = "Could not load bookings: " + ex.Message;
            }
        }

        private void ApplyFilter()
        {
            string q = _search.Text.Trim();
            if (q.Length == 0)
            {
                _table.DefaultView.RowFilter = "";
            }
            else
            {
                string e = Escape(q);
                _table.DefaultView.RowFilter =
                    "Convert(id, 'System.String') LIKE '%" + e + "%' OR name LIKE '%" + e +
                    "%' OR booked_on LIKE '%" + e + "%'";
            }
            int n = _table.DefaultView.Count;
            _sub.Text = n + (n == 1 ? " booking" : " bookings");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // BookingsListPanel
            // 
            this.Name = "BookingsListPanel";
            this.Load += new System.EventHandler(this.BookingsListPanel_Load);
            this.ResumeLayout(false);

        }

        private void BookingsListPanel_Load(object sender, EventArgs e)
        {

        }

        // escapes ' and the LIKE wildcards so typed text is matched literally
        private static string Escape(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (c == '\'') sb.Append("''");
                else if ("%*[]".IndexOf(c) >= 0) sb.Append('[').Append(c).Append(']');
                else sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
