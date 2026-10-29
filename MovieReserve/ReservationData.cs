using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieReserve
{
    // =====================================================================
    //  Models
    // =====================================================================
    public enum SeatType { Regular, Pwd, Elderly }

    public enum SessionChange { Movie, Showtimes, Showtime, Rooms, Layout, Seats }

    public class ScreenItem
    {
        public int ScreenId { get; set; }
        public string Name { get; set; }
        public override string ToString() { return Name; }
    }

    public class ShowtimeItem
    {
        public int ShowtimeId { get; set; }
        public int MovieId { get; set; }
        public int ScreenId { get; set; }
        public string ScreenName { get; set; }
        public DateTime Start { get; set; }
        public decimal Price { get; set; }
    }

    // =====================================================================
    //  Seat layout (hardcoded design, editable by Admin, saved per room)
    //  File: %AppData%\MovieReserve\layout_screen<ID>.txt
    // =====================================================================
    public class SeatLayout
    {
        public const int MinRows = 3, MaxRows = 12, MinSide = 3, MaxSide = 8;
        public const int MaxAisle = 140, MaxMargin = 140;

        public int Rows = 6;            // A, B, C ...
        public int SeatsPerSide = 6;    // left block and right block (so 12 per row)
        public int AisleWidth = 60;     // space in the MIDDLE between the two blocks (px)
        public int SideMargin = 40;     // space on the SIDES between row letters and seats (px)
        public HashSet<string> Pwd = new HashSet<string>();
        public HashSet<string> Elderly = new HashSet<string>();

        public int Cols { get { return SeatsPerSide * 2; } }

        public static string RowLabel(int row) { return ((char)('A' + row)).ToString(); }
        public static string Code(int row, int seatNumber) { return RowLabel(row) + seatNumber; }

        public SeatType TypeOf(string code)
        {
            if (Pwd.Contains(code)) return SeatType.Pwd;
            if (Elderly.Contains(code)) return SeatType.Elderly;
            return SeatType.Regular;
        }

        public void SetType(string code, SeatType t)
        {
            Pwd.Remove(code);
            Elderly.Remove(code);
            if (t == SeatType.Pwd) Pwd.Add(code);
            else if (t == SeatType.Elderly) Elderly.Add(code);
        }

        public bool IsValid(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length < 2) return false;
            int row = code[0] - 'A';
            int num;
            if (row < 0 || row >= Rows) return false;
            return int.TryParse(code.Substring(1), out num) && num >= 1 && num <= Cols;
        }

        public void DropInvalidSeats()
        {
            Pwd.RemoveWhere(c => !IsValid(c));
            Elderly.RemoveWhere(c => !IsValid(c));
        }

        public static SeatLayout CreateDefault()
        {
            var l = new SeatLayout();
            int n = l.Cols;
            // PWD seats: first row, both ends.  Elderly seats: second row, both ends.
            foreach (int x in new[] { 1, 2, n - 1, n }) l.Pwd.Add(Code(0, x));
            foreach (int x in new[] { 1, 2, n - 1, n }) l.Elderly.Add(Code(1, x));
            return l;
        }

        private static string Folder
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MovieReserve");
            }
        }

        public static string PathFor(int screenId)
        {
            return Path.Combine(Folder, "layout_screen" + screenId + ".txt");
        }

        private static int Limit(int v, int min, int max) { return Math.Max(min, Math.Min(max, v)); }

        private static void FillSet(HashSet<string> set, string csv)
        {
            foreach (string part in csv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                set.Add(part.Trim().ToUpperInvariant());
        }

        public static SeatLayout Load(int screenId)
        {
            var l = CreateDefault();
            try
            {
                string file = PathFor(screenId);
                if (!File.Exists(file)) return l;

                l.Pwd.Clear();
                l.Elderly.Clear();
                foreach (string line in File.ReadAllLines(file))
                {
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string key = line.Substring(0, eq).Trim().ToLowerInvariant();
                    string val = line.Substring(eq + 1).Trim();
                    int n;
                    switch (key)
                    {
                        case "rows": if (int.TryParse(val, out n)) l.Rows = Limit(n, MinRows, MaxRows); break;
                        case "side": if (int.TryParse(val, out n)) l.SeatsPerSide = Limit(n, MinSide, MaxSide); break;
                        case "aisle": if (int.TryParse(val, out n)) l.AisleWidth = Limit(n, 0, MaxAisle); break;
                        case "margin": if (int.TryParse(val, out n)) l.SideMargin = Limit(n, 0, MaxMargin); break;
                        case "pwd": FillSet(l.Pwd, val); break;
                        case "elderly": FillSet(l.Elderly, val); break;
                    }
                }
                l.DropInvalidSeats();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Layout load failed: " + ex.Message);
                return CreateDefault();
            }
            return l;
        }

        public void Save(int screenId)
        {
            Directory.CreateDirectory(Folder);
            DropInvalidSeats();
            var lines = new List<string>
            {
                "rows=" + Rows,
                "side=" + SeatsPerSide,
                "aisle=" + AisleWidth,
                "margin=" + SideMargin,
                "pwd=" + string.Join(",", Pwd.OrderBy(x => x, StringComparer.Ordinal)),
                "elderly=" + string.Join(",", Elderly.OrderBy(x => x, StringComparer.Ordinal))
            };
            File.WriteAllLines(PathFor(screenId), lines);
        }
    }

    // =====================================================================
    //  Database access (movie_reserve_db)
    // =====================================================================
    public static class ReservationRepository
    {
        public static List<ScreenItem> GetScreens()
        {
            var list = new List<ScreenItem>();
            using (var con = new MySqlConnection(MovieRepository.ConnectionString))
            using (var cmd = new MySqlCommand("SELECT screen_id, screen_name FROM screens ORDER BY screen_id", con))
            {
                con.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new ScreenItem
                        {
                            ScreenId = Convert.ToInt32(r["screen_id"]),
                            Name = Convert.ToString(r["screen_name"])
                        });
            }
            return list;
        }

        public static List<ShowtimeItem> GetShowtimes(int movieId)
        {
            var list = new List<ShowtimeItem>();
            const string sql =
                "SELECT st.showtime_id, st.movie_id, st.screen_id, sc.screen_name, st.start_time, st.price " +
                "FROM showtimes st JOIN screens sc ON sc.screen_id = st.screen_id " +
                "WHERE st.movie_id = @m ORDER BY st.start_time";
            using (var con = new MySqlConnection(MovieRepository.ConnectionString))
            using (var cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@m", movieId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new ShowtimeItem
                        {
                            ShowtimeId = Convert.ToInt32(r["showtime_id"]),
                            MovieId = Convert.ToInt32(r["movie_id"]),
                            ScreenId = Convert.ToInt32(r["screen_id"]),
                            ScreenName = Convert.ToString(r["screen_name"]),
                            Start = Convert.ToDateTime(r["start_time"]),
                            Price = Convert.ToDecimal(r["price"])
                        });
            }
            return list;
        }

        // Seats already booked for a showtime (booking_seats -> seats), as codes like "E7"
        public static HashSet<string> GetOccupiedSeats(int showtimeId)
        {
            var set = new HashSet<string>();
            const string sql =
                "SELECT s.row_label, s.seat_number FROM booking_seats bs " +
                "JOIN seats s ON s.seat_id = bs.seat_id WHERE bs.showtime_id = @s";
            using (var con = new MySqlConnection(MovieRepository.ConnectionString))
            using (var cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@s", showtimeId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        set.Add((Convert.ToString(r["row_label"]) + Convert.ToString(r["seat_number"])).ToUpperInvariant());
            }
            return set;
        }

        // Saves the booking to MySQL. Returns booking id, or throws with the DB message.
        public static int BookSeats(string customer, int showtimeId, int screenId, List<string> seatCodes)
        {
            using (var con = new MySqlConnection(MovieRepository.ConnectionString))
            {
                con.Open();

                // 1. Convert codes like "E7" into seat_id values for this room
                var ids = new List<string>();
                foreach (string code in seatCodes)
                {
                    using (var cmd = new MySqlCommand(
                        "SELECT seat_id FROM seats WHERE screen_id=@sc AND row_label=@r AND seat_number=@n", con))
                    {
                        cmd.Parameters.AddWithValue("@sc", screenId);
                        cmd.Parameters.AddWithValue("@r", code.Substring(0, 1));
                        cmd.Parameters.AddWithValue("@n", int.Parse(code.Substring(1)));
                        object o = cmd.ExecuteScalar();
                        if (o == null)
                            throw new InvalidOperationException(
                                "Seat " + code + " does not exist in the database for this room.");
                        ids.Add(Convert.ToString(o));
                    }
                }

                // 2. Call the stored procedure
                using (var cmd = new MySqlCommand("book_seats", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_customer", customer);
                    cmd.Parameters.AddWithValue("p_showtime", showtimeId);
                    cmd.Parameters.AddWithValue("p_seat_ids", string.Join(",", ids));
                    var outId = new MySqlParameter("p_booking_id", MySqlDbType.Int32)
                    { Direction = System.Data.ParameterDirection.Output };
                    var outMsg = new MySqlParameter("p_message", MySqlDbType.VarChar, 255)
                    { Direction = System.Data.ParameterDirection.Output };
                    cmd.Parameters.Add(outId);
                    cmd.Parameters.Add(outMsg);
                    cmd.ExecuteNonQuery();

                    string msg = Convert.ToString(outMsg.Value);
                    if (msg != "OK") throw new InvalidOperationException(msg);
                    return Convert.ToInt32(outId.Value);
                }
            }
        }
    }

    // =====================================================================
    //  Shared state used by the three panels
    // =====================================================================
    public class ReservationSession
    {
        public const int MaxSeatsPerBooking = 10;

        // seats paid during this run, so the next customer sees them as taken
        private static readonly Dictionary<int, HashSet<string>> Paid = new Dictionary<int, HashSet<string>>();

        public UserSession User { get; private set; }
        public bool IsAdmin { get { return User != null && User.IsAdmin; } }

        public MovieItem Movie { get; private set; }
        public List<ShowtimeItem> Showtimes { get; private set; }
        public ShowtimeItem Showtime { get; private set; }
        public List<ScreenItem> Screens { get; private set; }
        public ScreenItem Room { get; private set; }
        public SeatLayout Layout { get; private set; }
        public HashSet<string> Occupied { get; private set; }
        public List<string> SelectedSeats { get; private set; }

        public decimal Total { get { return Showtime == null ? 0m : Showtime.Price * SelectedSeats.Count; } }

        public event Action<SessionChange> Changed;

        public ReservationSession(UserSession user)
        {
            User = user;
            Showtimes = new List<ShowtimeItem>();
            Screens = new List<ScreenItem>();
            Occupied = new HashSet<string>();
            SelectedSeats = new List<string>();
        }

        private void Raise(SessionChange c)
        {
            var h = Changed;
            if (h != null) h(c);
        }

        public async Task InitAsync()
        {
            try { Screens = await Task.Run(() => ReservationRepository.GetScreens()); }
            catch (Exception ex)
            {
                Debug.WriteLine("Screens failed: " + ex.Message);
                Screens = new List<ScreenItem>();
            }

            if (Room == null && Screens.Count > 0)
            {
                Room = Screens[0];
                Layout = SeatLayout.Load(Room.ScreenId);   // lets the Admin edit a room before any movie is picked
            }
            Raise(SessionChange.Rooms);
            Raise(SessionChange.Layout);
        }

        // Call this when a poster is clicked on the Movies page
        public async void SelectMovie(MovieItem movie)
        {
            Movie = movie;
            Showtimes = new List<ShowtimeItem>();
            Showtime = null;
            Occupied = new HashSet<string>();
            SelectedSeats.Clear();
            Raise(SessionChange.Movie);
            Raise(SessionChange.Showtimes);
            Raise(SessionChange.Seats);
            if (movie == null) return;

            List<ShowtimeItem> list;
            try { list = await Task.Run(() => ReservationRepository.GetShowtimes(movie.MovieId)); }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load show times:\n" + ex.Message, "MovieReserve");
                list = new List<ShowtimeItem>();
            }

            if (Movie != movie) return;   // another movie was picked while loading
            Showtimes = list;
            Raise(SessionChange.Showtimes);
            if (list.Count > 0) SelectShowtime(list[0]);
        }

        public async void SelectShowtime(ShowtimeItem st)
        {
            Showtime = st;
            SelectedSeats.Clear();
            Occupied = new HashSet<string>();

            if (st != null)
            {
                Room = Screens.FirstOrDefault(x => x.ScreenId == st.ScreenId)
                       ?? new ScreenItem { ScreenId = st.ScreenId, Name = st.ScreenName };
                Layout = SeatLayout.Load(st.ScreenId);

                HashSet<string> paid;
                if (Paid.TryGetValue(st.ShowtimeId, out paid)) Occupied.UnionWith(paid);
            }
            Raise(SessionChange.Showtime);
            Raise(SessionChange.Layout);
            Raise(SessionChange.Seats);
            if (st == null) return;

            HashSet<string> fromDb;
            try { fromDb = await Task.Run(() => ReservationRepository.GetOccupiedSeats(st.ShowtimeId)); }
            catch (Exception ex)
            {
                Debug.WriteLine("Occupied seats failed: " + ex.Message);
                fromDb = new HashSet<string>();
            }

            if (Showtime != st) return;
            Occupied.UnionWith(fromDb);
            SelectedSeats.RemoveAll(c => Occupied.Contains(c));
            Raise(SessionChange.Seats);
        }

        // Staff "Change room" / Admin room selector
        

        public void ToggleSeat(string code)
        {
            if (Showtime == null || Layout == null || !Layout.IsValid(code) || Occupied.Contains(code)) return;
            if (!SelectedSeats.Remove(code))
            {
                if (SelectedSeats.Count >= MaxSeatsPerBooking) return;
                SelectedSeats.Add(code);
                SelectedSeats.Sort(CompareSeats);
            }
            Raise(SessionChange.Seats);
        }

        private static int CompareSeats(string a, string b)
        {
            int c = a[0].CompareTo(b[0]);
            if (c != 0) return c;
            return ParseNum(a).CompareTo(ParseNum(b));
        }

        private static int ParseNum(string code)
        {
            int n;
            int.TryParse(code.Substring(1), out n);
            return n;
        }

        // ---- Admin layout operations ----
        public void NotifyLayoutEdited()
        {
            if (Layout != null) Layout.DropInvalidSeats();
            SelectedSeats.Clear();
            Raise(SessionChange.Layout);
            Raise(SessionChange.Seats);
        }

        public void SaveLayout()
        {
            if (Room == null || Layout == null) return;
            Layout.Save(Room.ScreenId);
            Raise(SessionChange.Layout);
        }

        public void ReloadLayout()
        {
            if (Room == null) return;
            Layout = SeatLayout.Load(Room.ScreenId);
            NotifyLayoutEdited();
        }

        public void ResetLayoutToDefault()
        {
            Layout = SeatLayout.CreateDefault();
            NotifyLayoutEdited();
        }

        // Called after the receipt is saved
        public void MarkPaid()
        {
            if (Showtime == null) return;
            HashSet<string> set;
            if (!Paid.TryGetValue(Showtime.ShowtimeId, out set))
            {
                set = new HashSet<string>();
                Paid[Showtime.ShowtimeId] = set;
            }
            foreach (string c in SelectedSeats) { set.Add(c); Occupied.Add(c); }
            SelectedSeats.Clear();
            Raise(SessionChange.Seats);
        }

        public int SaveBooking(string customer)
        {
            if (Showtime == null || SelectedSeats.Count == 0)
                throw new InvalidOperationException("Nothing to book.");
            return ReservationRepository.BookSeats(customer, Showtime.ShowtimeId, Showtime.ScreenId, SelectedSeats);
        }
    }

    // =====================================================================
    //  Receipt -> file.txt
    // =====================================================================
    public static class ReceiptWriter
    {
        private static string Row(string key, string value) { return key.PadRight(12) + ": " + value; }

        // cash = amount the customer handed over. change = cash - subtotal.
        public static string Append(ReservationSession s, string customer, string contact, decimal cash)
        {
            var now = DateTime.Now;
            string receiptNo = "MR-" + now.ToString("yyyyMMdd-HHmmss");
            string heavy = new string('=', 52);
            string thin = new string('-', 52);

            decimal subTotal = s.Total;              // price per seat x number of seats
            decimal change = cash - subTotal;

            var sb = new StringBuilder();
            sb.AppendLine(heavy);
            sb.AppendLine("  MOVIERESERVE  -  RECEIPT");
            sb.AppendLine(heavy);
            sb.AppendLine(Row("Receipt no", receiptNo));
            sb.AppendLine(Row("Date", now.ToString("yyyy-MM-dd hh:mm:ss tt")));
            sb.AppendLine(Row("Served by", s.User == null ? "-" : s.User.Name + " (" + s.User.Role + ")"));
            sb.AppendLine(thin);
            sb.AppendLine(Row("Movie", s.Movie == null ? "-" : s.Movie.Title));
            sb.AppendLine(Row("Genre", s.Movie == null ? "-" : s.Movie.Genre));
            sb.AppendLine(Row("Room", s.Room == null ? "-" : s.Room.Name));
            sb.AppendLine(Row("Show time", s.Showtime.Start.ToString("yyyy-MM-dd hh:mm tt")));
            sb.AppendLine(Row("Seats (" + s.SelectedSeats.Count + ")", string.Join(", ", s.SelectedSeats)));
            sb.AppendLine(Row("Price/seat", UiKit.Money(s.Showtime.Price)));
            sb.AppendLine(Row("Computation", s.SelectedSeats.Count + " x " + UiKit.Money(s.Showtime.Price)));
            sb.AppendLine(thin);
            sb.AppendLine(Row("SubTotal", UiKit.Money(subTotal)));
            sb.AppendLine(Row("Cash", UiKit.Money(cash)));
            sb.AppendLine(Row("CHANGE", UiKit.Money(change)));
            sb.AppendLine(thin);
            sb.AppendLine(Row("Customer", customer));
            sb.AppendLine(Row("Contact", contact));
            sb.AppendLine(heavy);

            string text = sb.ToString();
            string fileName = receiptNo + ".pdf";

            try
            {
                return WritePdf(Path.Combine(Application.StartupPath, "Receipts"), fileName, text);
            }
            catch (Exception ex)
            {
                if (!(ex is UnauthorizedAccessException) && !(ex is IOException)) throw;
                // exe folder not writable (e.g. Program Files): fall back to %AppData%
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MovieReserve", "Receipts");
                return WritePdf(dir, fileName, text);
            }
        }

        private static string WritePdf(string dir, string fileName, string text)
        {
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, fileName);
            string[] lines = text.TrimEnd().Replace("\r\n", "\n").Split('\n');

            var pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            if (!pd.PrinterSettings.IsValid)
                throw new InvalidOperationException(
                    "The 'Microsoft Print to PDF' printer is not installed. Enable it in Windows Features / Printers.");

            pd.PrinterSettings.PrintToFile = true;
            pd.PrinterSettings.PrintFileName = path;
            pd.DocumentName = "MovieReserve Receipt";
            pd.PrintController = new StandardPrintController();      // no "printing..." popup
            pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);

            pd.PrintPage += (s, e) =>
            {
                using (var font = new Font("Courier New", 10f))
                {
                    float lineH = font.GetHeight(e.Graphics) + 1;
                    float y = e.MarginBounds.Top;
                    foreach (string line in lines)
                    {
                        e.Graphics.DrawString(line, font, Brushes.Black, e.MarginBounds.Left, y);
                        y += lineH;
                    }
                }
                e.HasMorePages = false;
            };

            pd.Print();
            return path;
        }
    }

    // =====================================================================
    //  Poster download (same approach as MoviesListPanel, with its own cache)
    // =====================================================================
    internal static class PosterLoader
    {
        private static readonly ConcurrentDictionary<string, Image> Cache = new ConcurrentDictionary<string, Image>();
        private static readonly HttpClient Http = Create();

        private static HttpClient Create()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var c = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            return c;
        }

        public static async Task<Image> GetAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            Image img;
            if (Cache.TryGetValue(url, out img)) return img;
            try
            {
                using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                {
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
                Cache[url] = img;
                return img;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Poster failed: " + url + " -> " + ex.Message);
                return null;
            }
        }
    }

    // =====================================================================
    //  Colours, fonts and small helpers shared by the views
    // =====================================================================
    internal static class UiKit
    {
        public static readonly Color Ink = Color.FromArgb(24, 24, 24);
        public static readonly Color Muted = Color.FromArgb(125, 125, 120);
        public static readonly Color Line = Color.FromArgb(230, 229, 225);
        public static readonly Color Red = Color.FromArgb(214, 40, 40);
        public static readonly Color RedHover = Color.FromArgb(185, 28, 28);

        public static readonly Color SeatFreeBorder = Color.FromArgb(206, 206, 200);
        public static readonly Color SeatTaken = Color.FromArgb(221, 217, 211);
        public static readonly Color SeatTakenText = Color.FromArgb(165, 160, 152);
        public static readonly Color SeatPwd = Color.FromArgb(219, 234, 254);
        public static readonly Color SeatPwdBorder = Color.FromArgb(96, 150, 220);
        public static readonly Color SeatPwdText = Color.FromArgb(40, 90, 160);
        public static readonly Color SeatElder = Color.FromArgb(254, 243, 214);
        public static readonly Color SeatElderBorder = Color.FromArgb(222, 168, 58);
        public static readonly Color SeatElderText = Color.FromArgb(146, 96, 18);

        public static readonly Font FontTitle = new Font("Segoe UI Semibold", 13f);
        public static readonly Font FontBody = new Font("Segoe UI", 9.5f);
        public static readonly Font FontBold = new Font("Segoe UI Semibold", 9.5f);
        public static readonly Font FontSmall = new Font("Segoe UI", 8.5f);
        public static readonly Font FontCap = new Font("Segoe UI", 8f, FontStyle.Bold);
        public static readonly Font FontBig = new Font("Segoe UI Semibold", 18f);
        public static readonly Font FontBtn = new Font("Segoe UI Semibold", 10f);

        public static string Money(decimal v) { return "\u20B1" + v.ToString("N2"); }

        public static GraphicsPath RoundPath(Rectangle r, int radius)
        {
            int d = Math.Max(2, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void Round(Control c, int radius)
        {
            if (c.Width <= 0 || c.Height <= 0) return;
            using (var p = RoundPath(new Rectangle(0, 0, c.Width, c.Height), radius))
            {
                var old = c.Region;
                c.Region = new Region(p);
                if (old != null) old.Dispose();
            }
        }

        public static Button MakeButton(string text, bool primary)
        {
            var b = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Font = FontBtn,
                Height = 36,
                BackColor = primary ? Red : Color.White,
                ForeColor = primary ? Color.White : Ink
            };
            b.FlatAppearance.BorderSize = primary ? 0 : 1;
            b.FlatAppearance.BorderColor = Color.FromArgb(210, 209, 204);
            b.FlatAppearance.MouseOverBackColor = primary ? RedHover : Color.FromArgb(244, 243, 240);
            b.FlatAppearance.MouseDownBackColor = primary ? RedHover : Color.FromArgb(232, 231, 227);
            b.SizeChanged += (s, e) => Round(b, 8);
            if (primary)
                b.EnabledChanged += (s, e) => b.BackColor = b.Enabled ? Red : Color.FromArgb(205, 170, 170);
            return b;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        // placeholder text inside a TextBox ("Enter customer name")
        public static void SetCue(TextBox tb, string text)
        {
            Action apply = () => SendMessage(tb.Handle, 0x1501, (IntPtr)1, text);
            if (tb.IsHandleCreated) apply();
            else tb.HandleCreated += (s, e) => apply();
        }
    }

    // =====================================================================
    //  One call from Form1 wires everything into the three existing panels
    // =====================================================================
    public static class ReservationPage
    {
        public static ReservationSession Attach(Control reservationPanel, Control seatPanel,
            Control selectedMoviePanel, Control receiptPanel, UserSession user, bool autoLayout = true)
        {
            var session = new ReservationSession(user);

            Fill(seatPanel, new SeatSelectionView(session));
            Fill(selectedMoviePanel, new MovieInfoView(session));
            Fill(receiptPanel, session.IsAdmin
                ? (Control)new PosterSlideshowView()
                : new ReceiptView(session));

            seatPanel.BackColor = Color.White;
            selectedMoviePanel.BackColor = Color.White;
            receiptPanel.BackColor = Color.White;

            if (autoLayout)
            {
                reservationPanel.BackColor = Color.FromArgb(244, 243, 240);
                Action layout = () =>
                {
                    const int pad = 16, gap = 16, rightW = 300, movieH = 290;
                    int w = reservationPanel.ClientSize.Width, h = reservationPanel.ClientSize.Height;
                    if (w < 600 || h < 400) return;
                    seatPanel.SetBounds(pad, pad, w - pad * 2 - gap - rightW, h - pad * 2);
                    selectedMoviePanel.SetBounds(w - pad - rightW, pad, rightW, movieH);
                    receiptPanel.SetBounds(w - pad - rightW, pad + movieH + gap, rightW, h - pad * 2 - movieH - gap);
                };
                reservationPanel.Resize += (s, e) => layout();
                layout();
            }

            var ignore = session.InitAsync();
            return session;
        }

        // replaces whatever placeholder controls the designer left inside the panel
        private static void Fill(Control panel, Control view)
        {
            foreach (Control c in panel.Controls.Cast<Control>().ToList())
            {
                panel.Controls.Remove(c);
                c.Dispose();
            }
            view.Dock = DockStyle.Fill;
            panel.Controls.Add(view);
        }
    }
}