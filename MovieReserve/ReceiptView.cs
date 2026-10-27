using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MovieReserve
{
    // =====================================================================
    //  RecieptPanel content: customer details, seats, total and the
    //  "Continue to payment" button, which appends the receipt to file.txt
    // =====================================================================
    internal class ReceiptView : UserControl
    {
        private const int Sep1 = 170, Sep2 = 212;

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
            BackColor = Color.White;

            _title = new Label { Text = "Customer details", AutoSize = true, Font = UiKit.FontTitle, ForeColor = UiKit.Ink };
            _capName = Cap("Full name");
            _capContact = Cap("Contact number");

            _txtName = Box();
            UiKit.SetCue(_txtName, "Enter customer name");
            _txtContact = Box();
            UiKit.SetCue(_txtContact, "+63 900 000 0000");
            _txtContact.MaxLength = 20;
            _txtContact.KeyPress += (a, e) =>
            {
                // digits and the usual phone symbols only
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && "+-() ".IndexOf(e.KeyChar) < 0)
                    e.Handled = true;
            };

            _seatsKey = new Label { Text = "Seats (0)", AutoSize = true, Font = UiKit.FontBody, ForeColor = UiKit.Muted };
            _seatsVal = new Label
            {
                AutoSize = false, AutoEllipsis = true, Font = UiKit.FontBold, ForeColor = UiKit.Ink,
                TextAlign = ContentAlignment.TopRight, Text = "-"
            };
            _totalKey = new Label { Text = "Total", AutoSize = true, Font = UiKit.FontBody, ForeColor = UiKit.Ink };
            _totalVal = new Label
            {
                AutoSize = false, Font = UiKit.FontBig, ForeColor = UiKit.Ink,
                TextAlign = ContentAlignment.TopRight, Text = UiKit.Money(0)
            };
            _perSeat = new Label
            {
                AutoSize = false, Font = UiKit.FontSmall, ForeColor = UiKit.Muted, TextAlign = ContentAlignment.TopRight
            };

            _btnPay = UiKit.MakeButton("Continue to payment  \u2192", true);
            _btnPay.Height = 44;
            _btnPay.Enabled = false;
            _btnPay.BackColor = Color.FromArgb(205, 170, 170);
            _btnPay.Click += (a, e) => Pay();

            Controls.AddRange(new Control[]
            {
                _title, _capName, _txtName, _capContact, _txtContact,
                _seatsKey, _seatsVal, _totalKey, _totalVal, _perSeat, _btnPay
            });

            Resize += (a, e) => { DoLayout(); Invalidate(); };
            DoLayout();

            s.Changed += c => RefreshSummary();
            RefreshSummary();
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

            _seatsKey.Location = new Point(16, 183);
            _seatsVal.SetBounds(w - 16 - 190, 182, 190, 20);
            _totalKey.Location = new Point(16, 232);
            _totalVal.SetBounds(w - 16 - 190, 220, 190, 34);
            _perSeat.SetBounds(w - 16 - 190, 256, 190, 18);
            _btnPay.SetBounds(16, h - 14 - 44, w - 32, 44);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var p = new Pen(UiKit.Line))
            {
                e.Graphics.DrawLine(p, 16, Sep1, Width - 16, Sep1);
                e.Graphics.DrawLine(p, 16, Sep2, Width - 16, Sep2);
            }
        }

        private void RefreshSummary()
        {
            int n = _s.SelectedSeats.Count;
            _seatsKey.Text = "Seats (" + n + ")";
            _seatsVal.Text = n == 0 ? "-" : string.Join(", ", _s.SelectedSeats);
            _totalVal.Text = UiKit.Money(_s.Total);
            _perSeat.Text = (_s.Showtime != null && n > 0) ? n + " \u00D7 " + UiKit.Money(_s.Showtime.Price) : "";
            _btnPay.Enabled = _s.Showtime != null && n > 0;
        }

        private void Pay()
        {
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
