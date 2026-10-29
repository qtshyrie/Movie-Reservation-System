using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Text.RegularExpressions;
namespace MovieReserve
{
    // =====================================================================
    //  RecieptPanel content: customer details, seats, total and the
    //  "Continue to payment" button, which appends the receipt to file.txt
    // =====================================================================
    internal class ReceiptView : UserControl
    {
        private readonly ErrorProvider _err = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

        private static readonly Regex NameRx = new Regex(@"^(?=.*[A-Za-z])[A-Za-z. ]{1,38}$", RegexOptions.Compiled);
        private static readonly Regex PhoneRx = new Regex(@"^\+[1-9]\d{6,14}$", RegexOptions.Compiled); // E.164

        private static bool IsNameChar(char c) =>
            (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == ' ';

        private bool NameValid() => NameRx.IsMatch(_txtName.Text.Trim());
        private bool PhoneValid() => PhoneRx.IsMatch(_txtContact.Text.Trim());
        private bool InputsValid() => _admin || (NameValid() && PhoneValid());

        private void ValidateName(bool showEmptyError)
        {
            string t = _txtName.Text.Trim();
            if (t.Length == 0)
                _err.SetError(_txtName, showEmptyError ? "Full name is required." : "");
            else if (!NameValid())
                _err.SetError(_txtName, "Use letters and dots only (max 38 characters).");
            else
                _err.SetError(_txtName, "");
        }

        private void ValidatePhone(bool showEmptyError)
        {
            string t = _txtContact.Text.Trim();
            if (t.Length == 0)
                _err.SetError(_txtContact, showEmptyError ? "Contact number is required." : "");
            else if (!PhoneValid())
                _err.SetError(_txtContact, "Use + followed by 7-15 digits (e.g. +639001234567).");
            else
                _err.SetError(_txtContact, "");
        }

        private Color _payOn;   // normal (enabled) button color

        private void UpdatePayState()
        {
            if (_admin) return;
            bool ok = _s.Showtime != null && _s.SelectedSeats.Count > 0 && InputsValid();
            _btnPay.Enabled = ok;
            _btnPay.BackColor = ok ? _payOn : Color.FromArgb(205, 170, 170);
        }
        private const int Sep1 = 170, Sep2 = 212;
        private readonly bool _admin;
        private int Dy { get { return _admin ? -150 : 0; } }   

        private readonly ReservationSession _s;
        private readonly Label _title, _capName, _capContact, _seatsKey, _seatsVal, _totalKey, _totalVal, _perSeat;
        private readonly TextBox _txtName, _txtContact;

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ReceiptView
            // 
            this.Name = "ReceiptView";
            this.Load += new System.EventHandler(this.ReceiptView_Load);
            this.ResumeLayout(false);

        }

        private void ReceiptView_Load(object sender, EventArgs e)
        {
              
        }

        private readonly Button _btnPay;
        // Small popup that asks for the cash received. Returns false if cancelled.
        private bool AskCash(decimal total, out decimal cash)
        {
            cash = 0m;
            using (var f = new Form())
            {
                f.Text = "Payment";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.ClientSize = new Size(320, 170);
                f.Font = UiKit.FontBody;

                var lblTotal = new Label { Text = "Total: " + UiKit.Money(total), AutoSize = true, Left = 16, Top = 16, Font = UiKit.FontBold };
                var lblCash = new Label { Text = "Cash received", AutoSize = true, Left = 16, Top = 50 };
                var txt = new TextBox { Left = 16, Top = 70, Width = 288 };
                var lblChange = new Label { Text = "Change: -", AutoSize = true, Left = 16, Top = 104, Font = UiKit.FontBold };
                var ok = new Button { Text = "Confirm", Left = 148, Top = 130, Width = 75, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", Left = 229, Top = 130, Width = 75, DialogResult = DialogResult.Cancel };

                Func<string, decimal?> parse = t =>
                {
                    decimal v;
                    return decimal.TryParse(t.Replace("\u20B1", "").Replace(",", "").Trim(), out v) ? (decimal?)v : null;
                };

                txt.TextChanged += (s, e) =>
                {
                    var v = parse(txt.Text);
                    lblChange.Text = v.HasValue && v.Value >= total
                        ? "Change: " + UiKit.Money(v.Value - total)
                        : "Change: -";
                };

                f.Controls.AddRange(new Control[] { lblTotal, lblCash, txt, lblChange, ok, cancel });
                f.AcceptButton = ok;
                f.CancelButton = cancel;

                while (f.ShowDialog(this) == DialogResult.OK)
                {
                    var v = parse(txt.Text);
                    if (!v.HasValue)
                    {
                        MessageBox.Show("Enter the cash amount received.", "MovieReserve");
                        continue;
                    }
                    if (v.Value < total)
                    {
                        MessageBox.Show("Cash is less than the total (" + UiKit.Money(total) + ").", "MovieReserve");
                        continue;
                    }
                    cash = v.Value;
                    return true;
                }
                return false;
            }
        }
        public ReceiptView(ReservationSession s)
        {
            _s = s;
            _admin = s.IsAdmin;
            BackColor = Color.White;

            _title = new Label { Text = "Customer details", AutoSize = true, Font = UiKit.FontTitle, ForeColor = UiKit.Ink };
            _capName = Cap("Full name");
            _capContact = Cap("Contact number");

            // ---- Full name: letters, dot, space; max 38 ----
            _txtName = Box();
            UiKit.SetCue(_txtName, "Enter customer name");
            _txtName.MaxLength = 28;
            _txtName.KeyPress += (a, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !IsNameChar(e.KeyChar))
                    e.Handled = true;
            };
            _txtName.TextChanged += (a, e) =>
            {
                // clean up pasted text
                string clean = new string(_txtName.Text.Where(IsNameChar).ToArray());
                if (clean != _txtName.Text)
                {
                    int pos = Math.Max(0, _txtName.SelectionStart - (_txtName.Text.Length - clean.Length));
                    _txtName.Text = clean;
                    _txtName.SelectionStart = pos;
                }
                ValidateName(false);
                UpdatePayState();
            };
            _txtName.Validating += (a, e) => ValidateName(true);

            // ---- Contact: E.164 (+ then 7-15 digits) ----
            _txtContact = Box();
            UiKit.SetCue(_txtContact, "+639001234567");
            _txtContact.MaxLength = 16; // "+" + 15 digits
            _txtContact.KeyPress += (a, e) =>
            {
                if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar)) return;

                // "+" only as the very first character, once
                bool plusOk = e.KeyChar == '+' && _txtContact.SelectionStart == 0
                              && (_txtContact.Text.IndexOf('+') < 0 || _txtContact.SelectionLength > 0);
                if (!plusOk) e.Handled = true;
            };
            _txtContact.TextChanged += (a, e) =>
            {
                // clean up pasted text
                string t = _txtContact.Text;
                string clean = (t.StartsWith("+") ? "+" : "") + new string(t.Where(char.IsDigit).ToArray());
                if (clean != t)
                {
                    _txtContact.Text = clean;
                    _txtContact.SelectionStart = clean.Length;
                }
                ValidatePhone(false);
                UpdatePayState();
            };
            _txtContact.Validating += (a, e) => ValidatePhone(true);

            _seatsKey = new Label { Text = "Seats (0)", AutoSize = true, Font = UiKit.FontBody, ForeColor = UiKit.Muted };
            _seatsVal = new Label
            {
                AutoSize = false,
                AutoEllipsis = true,
                Font = UiKit.FontBold,
                ForeColor = UiKit.Ink,
                TextAlign = ContentAlignment.TopRight,
                Text = "-"
            };
            _totalKey = new Label { Text = "Total", AutoSize = true, Font = UiKit.FontBody, ForeColor = UiKit.Ink };
            _totalVal = new Label
            {
                AutoSize = false,
                Font = UiKit.FontBig,
                ForeColor = UiKit.Ink,
                TextAlign = ContentAlignment.TopRight,
                Text = UiKit.Money(0)
            };
            _perSeat = new Label
            {
                AutoSize = false,
                Font = UiKit.FontSmall,
                ForeColor = UiKit.Muted,
                TextAlign = ContentAlignment.TopRight
            };

            _btnPay = UiKit.MakeButton("Continue to payment  \u2192", true);
            _payOn = _btnPay.BackColor;          // <- add this line
            _btnPay.Height = 44;
            _btnPay.Enabled = false;
            _btnPay.BackColor = Color.FromArgb(205, 170, 170);
            _btnPay.Click += (a, e) => Pay();

            Controls.AddRange(new Control[]
            {
        _title, _capName, _txtName, _capContact, _txtContact,
        _seatsKey, _seatsVal, _totalKey, _totalVal, _perSeat, _btnPay
            });

            if (_admin)
            {
                _title.Visible = false;
                _capName.Visible = false;
                _txtName.Visible = false;
                _capContact.Visible = false;
                _txtContact.Visible = false;
                _btnPay.Visible = false;
            }

            Resize += (a, e) => { DoLayout(); Invalidate(); };
            DoLayout();

            s.Changed += c => RefreshSummary();
            RefreshSummary();
            UpdatePayState();
        }

        private static Label Cap(string text)
        {
            return new Label { Text = text, AutoSize = true, Font = UiKit.FontCap, ForeColor = UiKit.Muted };
        }

        private static TextBox Box()
        {
            return new TextBox { BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10.5f), ForeColor = UiKit.Ink };
        }

        private void DoLayout()
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            if (w < 100) return;
            _title.Location = new Point(16, 14);
            _capName.Location = new Point(16, 52);
            _txtName.SetBounds(16, 70, w - 32, _txtName.Height);
            _capContact.Location = new Point(16, 108);
            _txtContact.SetBounds(16, 126, w - 32, _txtContact.Height);

            _seatsKey.Location = new Point(16, 183 + Dy);
            _seatsVal.SetBounds(w - 16 - 190, 182 + Dy, 190, 20);
            _totalKey.Location = new Point(16, 232 + Dy);
            _totalVal.SetBounds(w - 16 - 190, 220 + Dy, 190, 34);
            _perSeat.SetBounds(w - 16 - 190, 256 + Dy, 190, 18);
            _btnPay.SetBounds(16, h - 14 - 44, w - 32, 44);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var p = new Pen(UiKit.Line))
            {
                if (!_admin)
                    e.Graphics.DrawLine(p, 16, Sep1, Width - 16, Sep1);
                e.Graphics.DrawLine(p, 16, Sep2 + Dy, Width - 16, Sep2 + Dy);
            }
        }

        private void RefreshSummary()
        {
            int n = _s.SelectedSeats.Count;
            _seatsKey.Text = "Seats (" + n + ")";
            _seatsVal.Text = n == 0 ? "-" : string.Join(", ", _s.SelectedSeats);
            _totalVal.Text = UiKit.Money(_s.Total);
            _perSeat.Text = (_s.Showtime != null && n > 0) ? n + " \u00D7 " + UiKit.Money(_s.Showtime.Price) : "";
            UpdatePayState();   // was: _btnPay.Enabled = _s.Showtime != null && n > 0;
        }

        private void Pay()
        {
            ValidateName(true);
            ValidatePhone(true);
            if (!InputsValid()) return;

            _txtName.Text = _txtName.Text.Trim();
            if (_admin) return;
            if (_s.Showtime == null || _s.SelectedSeats.Count == 0)
            {
                MessageBox.Show("Please select at least one seat first.", "MovieReserve",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = _txtName.Text.Trim();
            string contact = _txtContact.Text.Trim();

            if (name.Length == 0)
            {
                MessageBox.Show("Please enter the customer's full name.", "MovieReserve",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _txtName.Focus();
                return;
            }
            if (contact.Count(char.IsDigit) < 7)
            {
                MessageBox.Show("Please enter a valid contact number (at least 7 digits).", "MovieReserve",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _txtContact.Focus();
                return;
            }
            decimal cash;
            if (!AskCash(_s.Total, out cash)) return;   // cancelled: nothing is saved
                                                        // 1) Save to the database first
            try
            {
                _s.SaveBooking(name);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Booking failed:\n" + ex.Message, "MovieReserve",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                // seats may have just been taken by another cashier: refresh them
                _s.SelectShowtime(_s.Showtime);
                return;
            }

            // 2) Receipt (uses the still-selected seats)
            string seats = string.Join(", ", _s.SelectedSeats);
            decimal total = _s.Total;
            string receiptError = null;
            try
            {
                ReceiptWriter.Append(_s, name, contact, cash);
            }
            catch (Exception ex)
            {
                receiptError = ex.Message;   // booking is already saved, so don't undo it
            }

            // 3) Mark seats as taken in the UI
            _s.MarkPaid();
            _txtName.Clear();
            _txtContact.Clear();

            string msg = "Payment complete.\n\nSeats: " + seats + "\nTotal: " + UiKit.Money(total);
            if (receiptError != null)
                msg += "\n\nWarning: the booking was saved but the receipt PDF could not be created:\n" + receiptError;
            MessageBox.Show(msg, "MovieReserve", MessageBoxButtons.OK,
                receiptError == null ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
    }
}
