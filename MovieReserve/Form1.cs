using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;

namespace MovieReserve
{
    public partial class Form1 : Form
    {
        private MoviesListPanel _movies;
        public Form1()
        {
            InitializeComponent();
            _panels = new Panel[] { MoviesPanel, ReservationPanel, SettingsPanel };
            _movies = new MoviesListPanel
            {
                Location = new Point(0, 0),
                Size = new Size(1164, 689)
            };
            MoviesPanel.Controls.Add(_movies);
            _movies.MovieSelected += (s, movie) => ShowPage(1);
            ShowPage(0);
            Shown += async (s, e) => await _movies.LoadMoviesAsync();
        }
        private readonly UserSession _user;
        private ReservationSession _res;
        public Form1(UserSession user) : this()
        {
            _user = user;
            ApplyRole();
            SetupReservation();
        }

        private void SetupReservation()
        {
            _res = ReservationPage.Attach(ReservationPanel, SeatPanel, SelectedMoviePanel, RecieptPanel, _user);
            _movies.MovieSelected += (s, movie) => { _res.SelectMovie(movie); ShowPage(1); };
        }

        private void ApplyRole()
        {
            if (_user == null) return;

            // Common to everyone
            btnReservation.Visible = true;
            btnMovies.Visible = true;

            // Staff: only Reservation + Movies. Admin: also Overview + Settings
            btnSettings.Visible = _user.IsAdmin;

            // Stack the visible buttons so there are no gaps
            int y = btnMovies.Top;          // y of the first button slot
            int gap = 6;
            foreach (var b in new[] {btnReservation, btnMovies, btnSettings })
            {
                if (!b.Visible) continue;
                b.Top = y;
                y += b.Height + gap;
            }

            // Show the user in the sidebar (use your real label names)
            // lblUserName.Text = _user.Name;
            // lblInitials.Text = _user.Initials;
        }
        private Panel[] _panels;
        
        private void ShowPage(int page) {
            for (int i = 0; i < _panels.Length; i++)
            {
                _panels[i].Visible = (i == page);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void New_Reserve_Click(object sender, EventArgs e)
        {
            ShowPage(1);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.roundLabel2 = new RoundLabel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panel8 = new System.Windows.Forms.Panel();
            this.New_Reserve = new RoundButton();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.RightMotherPanel = new System.Windows.Forms.Panel();
            this.ReservationPanel = new System.Windows.Forms.Panel();
            this.MoviesPanel = new System.Windows.Forms.Panel();
            this.SettingsPanel = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel22 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.roundLabel1 = new RoundLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.btnSettings = new RoundButton();
            this.btnMovies = new RoundButton();
            this.btnReservation = new RoundButton();
            this.label10 = new System.Windows.Forms.Label();
            this.panel7 = new System.Windows.Forms.Panel();
            this.roundButton5 = new RoundButton();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.roundPanel5 = new RoundPanel();
            this.cornerPanel1 = new CornerPanel();
            this.label24 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.SeatPanel = new CornerPanel();
            this.SelectedMoviePanel = new CornerPanel();
            this.RecieptPanel = new CornerPanel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.roundButton1 = new RoundButton();
            this.roundButton2 = new RoundButton();
            this.roundButton3 = new RoundButton();
            this.roundButton4 = new RoundButton();
            this.roundButton6 = new RoundButton();
            this.roundButton7 = new RoundButton();
            this.roundButton8 = new RoundButton();
            this.roundButton9 = new RoundButton();
            this.roundButton10 = new RoundButton();
            this.roundButton11 = new RoundButton();
            this.roundButton12 = new RoundButton();
            this.roundButton13 = new RoundButton();
            this.roundButton15 = new RoundButton();
            this.roundButton16 = new RoundButton();
            this.roundButton17 = new RoundButton();
            this.roundButton18 = new RoundButton();
            this.roundButton19 = new RoundButton();
            this.roundButton20 = new RoundButton();
            this.roundButton21 = new RoundButton();
            this.roundButton22 = new RoundButton();
            this.roundButton23 = new RoundButton();
            this.panel9 = new System.Windows.Forms.Panel();
            this.roundButton14 = new RoundButton();
            this.roundButton24 = new RoundButton();
            this.roundButton25 = new RoundButton();
            this.roundButton26 = new RoundButton();
            this.roundButton27 = new RoundButton();
            this.roundButton28 = new RoundButton();
            this.roundButton29 = new RoundButton();
            this.roundButton30 = new RoundButton();
            this.roundButton31 = new RoundButton();
            this.roundButton32 = new RoundButton();
            this.roundButton33 = new RoundButton();
            this.roundButton34 = new RoundButton();
            this.roundButton35 = new RoundButton();
            this.roundButton36 = new RoundButton();
            this.roundButton37 = new RoundButton();
            this.roundButton38 = new RoundButton();
            this.roundButton39 = new RoundButton();
            this.roundButton40 = new RoundButton();
            this.roundButton41 = new RoundButton();
            this.panel10 = new System.Windows.Forms.Panel();
            this.panel11 = new System.Windows.Forms.Panel();
            this.roundButton42 = new RoundButton();
            this.roundButton43 = new RoundButton();
            this.roundButton44 = new RoundButton();
            this.roundButton45 = new RoundButton();
            this.roundButton46 = new RoundButton();
            this.roundButton47 = new RoundButton();
            this.roundButton48 = new RoundButton();
            this.roundButton49 = new RoundButton();
            this.roundButton50 = new RoundButton();
            this.roundButton51 = new RoundButton();
            this.roundButton52 = new RoundButton();
            this.roundButton53 = new RoundButton();
            this.roundButton54 = new RoundButton();
            this.roundButton55 = new RoundButton();
            this.roundButton56 = new RoundButton();
            this.roundButton57 = new RoundButton();
            this.roundButton58 = new RoundButton();
            this.roundButton59 = new RoundButton();
            this.roundButton60 = new RoundButton();
            this.roundButton61 = new RoundButton();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel8.SuspendLayout();
            this.RightMotherPanel.SuspendLayout();
            this.ReservationPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel7.SuspendLayout();
            this.roundPanel5.SuspendLayout();
            this.cornerPanel1.SuspendLayout();
            this.SeatPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panel11.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Desktop;
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1446, 900);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint_1);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel3.Controls.Add(this.roundLabel2);
            this.panel3.Controls.Add(this.textBox1);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.panel8);
            this.panel3.Location = new System.Drawing.Point(273, 3);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1170, 899);
            this.panel3.TabIndex = 1;
            // 
            // roundLabel2
            // 
            this.roundLabel2.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.roundLabel2.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roundLabel2.Location = new System.Drawing.Point(1071, 39);
            this.roundLabel2.Name = "roundLabel2";
            this.roundLabel2.Size = new System.Drawing.Size(51, 44);
            this.roundLabel2.TabIndex = 6;
            this.roundLabel2.Text = "RC";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(889, 55);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(170, 20);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = "Search anything...";
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft YaHei UI", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label6.Location = new System.Drawing.Point(47, 52);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(147, 23);
            this.label6.TabIndex = 2;
            this.label6.Text = "OVERVIEW";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.label5.Location = new System.Drawing.Point(62, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(242, 16);
            this.label5.TabIndex = 1;
            this.label5.Text = "Thursday October 16";
            // 
            // panel8
            // 
            this.panel8.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel8.Controls.Add(this.New_Reserve);
            this.panel8.Controls.Add(this.label9);
            this.panel8.Controls.Add(this.label8);
            this.panel8.Controls.Add(this.RightMotherPanel);
            this.panel8.ForeColor = System.Drawing.Color.White;
            this.panel8.Location = new System.Drawing.Point(0, 100);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(1170, 809);
            this.panel8.TabIndex = 0;
            // 
            // New_Reserve
            // 
            this.New_Reserve.BackColor = System.Drawing.Color.Red;
            this.New_Reserve.BorderColor = System.Drawing.Color.Gray;
            this.New_Reserve.CornerRadius = 10;
            this.New_Reserve.Cursor = System.Windows.Forms.Cursors.Hand;
            this.New_Reserve.FlatAppearance.BorderSize = 0;
            this.New_Reserve.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.New_Reserve.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.New_Reserve.ForeColor = System.Drawing.Color.White;
            this.New_Reserve.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.New_Reserve.Location = new System.Drawing.Point(953, 35);
            this.New_Reserve.Name = "New_Reserve";
            this.New_Reserve.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.New_Reserve.Size = new System.Drawing.Size(169, 41);
            this.New_Reserve.TabIndex = 7;
            this.New_Reserve.Text = "+ New Reserve";
            this.New_Reserve.UseVisualStyleBackColor = false;
            this.New_Reserve.Click += new System.EventHandler(this.New_Reserve_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label9.Location = new System.Drawing.Point(52, 63);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(279, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "Your counter is ready. Here’s a quick look at today’s shift.";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Microsoft YaHei UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(48, 19);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(316, 44);
            this.label8.TabIndex = 5;
            this.label8.Text = "Good Day, Staff";
            // 
            // RightMotherPanel
            // 
            this.RightMotherPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.RightMotherPanel.Controls.Add(this.ReservationPanel);
            this.RightMotherPanel.Controls.Add(this.MoviesPanel);
            this.RightMotherPanel.Controls.Add(this.SettingsPanel);
            this.RightMotherPanel.Location = new System.Drawing.Point(0, 102);
            this.RightMotherPanel.Name = "RightMotherPanel";
            this.RightMotherPanel.Size = new System.Drawing.Size(1167, 707);
            this.RightMotherPanel.TabIndex = 8;
            this.RightMotherPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.panel5_Paint);
            // 
            // ReservationPanel
            // 
            this.ReservationPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ReservationPanel.Controls.Add(this.RecieptPanel);
            this.ReservationPanel.Controls.Add(this.SelectedMoviePanel);
            this.ReservationPanel.Controls.Add(this.SeatPanel);
            this.ReservationPanel.Location = new System.Drawing.Point(0, 0);
            this.ReservationPanel.Name = "ReservationPanel";
            this.ReservationPanel.Size = new System.Drawing.Size(1164, 689);
            this.ReservationPanel.TabIndex = 1;
            // 
            // MoviesPanel
            // 
            this.MoviesPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.MoviesPanel.Location = new System.Drawing.Point(0, 0);
            this.MoviesPanel.Name = "MoviesPanel";
            this.MoviesPanel.Size = new System.Drawing.Size(1164, 689);
            this.MoviesPanel.TabIndex = 2;
            this.MoviesPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.MoviesPanel_Paint);
            // 
            // SettingsPanel
            // 
            this.SettingsPanel.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.SettingsPanel.Location = new System.Drawing.Point(0, 0);
            this.SettingsPanel.Name = "SettingsPanel";
            this.SettingsPanel.Size = new System.Drawing.Size(1164, 689);
            this.SettingsPanel.TabIndex = 3;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panel2.Controls.Add(this.panel22);
            this.panel2.Controls.Add(this.tableLayoutPanel1);
            this.panel2.Location = new System.Drawing.Point(0, 3);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(701, 897);
            this.panel2.TabIndex = 0;
            // 
            // panel22
            // 
            this.panel22.Location = new System.Drawing.Point(273, 202);
            this.panel22.Name = "panel22";
            this.panel22.Size = new System.Drawing.Size(381, 426);
            this.panel22.TabIndex = 6;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel4, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel5, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel6, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel7, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58.72483F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.44295F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(267, 894);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel4.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panel4.Controls.Add(this.pictureBox1);
            this.panel4.Controls.Add(this.label2);
            this.panel4.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.panel4.Location = new System.Drawing.Point(3, 10);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(0, 200, 0, 0);
            this.panel4.Size = new System.Drawing.Size(261, 98);
            this.panel4.TabIndex = 0;
            this.panel4.Paint += new System.Windows.Forms.PaintEventHandler(this.panel4_Paint);
            // 
            // pictureBox1
            // 
            this.pictureBox1.ErrorImage = global::MovieReserve.Properties.Resources.SysLogo;
            this.pictureBox1.Image = global::MovieReserve.Properties.Resources.SysLogo;
            this.pictureBox1.InitialImage = global::MovieReserve.Properties.Resources.SysLogo;
            this.pictureBox1.Location = new System.Drawing.Point(20, 15);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(50, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label2.Location = new System.Drawing.Point(76, 29);
            this.label2.MaximumSize = new System.Drawing.Size(500, 500);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(171, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "MovieReserve";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // panel5
            // 
            this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel5.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panel5.Controls.Add(this.roundLabel1);
            this.panel5.Controls.Add(this.label4);
            this.panel5.Controls.Add(this.label3);
            this.panel5.Location = new System.Drawing.Point(3, 114);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(261, 105);
            this.panel5.TabIndex = 1;
            this.panel5.Paint += new System.Windows.Forms.PaintEventHandler(this.panel5_Paint);
            // 
            // roundLabel1
            // 
            this.roundLabel1.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.roundLabel1.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roundLabel1.Location = new System.Drawing.Point(16, 5);
            this.roundLabel1.Name = "roundLabel1";
            this.roundLabel1.Size = new System.Drawing.Size(51, 44);
            this.roundLabel1.TabIndex = 5;
            this.roundLabel1.Text = "RC";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label4.Location = new System.Drawing.Point(73, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(185, 23);
            this.label4.TabIndex = 2;
            this.label4.Text = "Ramirez Cristopher\r\n";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label3.Location = new System.Drawing.Point(94, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "signed as";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // panel6
            // 
            this.panel6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel6.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panel6.Controls.Add(this.btnSettings);
            this.panel6.Controls.Add(this.btnMovies);
            this.panel6.Controls.Add(this.btnReservation);
            this.panel6.Controls.Add(this.label10);
            this.panel6.Location = new System.Drawing.Point(3, 225);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(261, 518);
            this.panel6.TabIndex = 2;
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.Color.Black;
            this.btnSettings.BorderColor = System.Drawing.Color.Gray;
            this.btnSettings.BorderSize = 1;
            this.btnSettings.CornerRadius = 10;
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettings.ForeColor = System.Drawing.Color.White;
            this.btnSettings.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnSettings.Location = new System.Drawing.Point(20, 144);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSettings.Size = new System.Drawing.Size(216, 41);
            this.btnSettings.TabIndex = 11;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.roundButton4_Click);
            // 
            // btnMovies
            // 
            this.btnMovies.BackColor = System.Drawing.Color.Black;
            this.btnMovies.BorderColor = System.Drawing.Color.Gray;
            this.btnMovies.BorderSize = 1;
            this.btnMovies.CornerRadius = 10;
            this.btnMovies.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMovies.FlatAppearance.BorderSize = 0;
            this.btnMovies.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMovies.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMovies.ForeColor = System.Drawing.Color.White;
            this.btnMovies.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnMovies.Location = new System.Drawing.Point(20, 50);
            this.btnMovies.Name = "btnMovies";
            this.btnMovies.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnMovies.Size = new System.Drawing.Size(216, 41);
            this.btnMovies.TabIndex = 10;
            this.btnMovies.Text = "Movies";
            this.btnMovies.UseVisualStyleBackColor = false;
            this.btnMovies.Click += new System.EventHandler(this.roundButton3_Click);
            // 
            // btnReservation
            // 
            this.btnReservation.BackColor = System.Drawing.Color.Black;
            this.btnReservation.BorderColor = System.Drawing.Color.Gray;
            this.btnReservation.BorderSize = 1;
            this.btnReservation.CornerRadius = 10;
            this.btnReservation.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReservation.FlatAppearance.BorderSize = 0;
            this.btnReservation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReservation.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReservation.ForeColor = System.Drawing.Color.White;
            this.btnReservation.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnReservation.Location = new System.Drawing.Point(20, 97);
            this.btnReservation.Name = "btnReservation";
            this.btnReservation.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnReservation.Size = new System.Drawing.Size(216, 41);
            this.btnReservation.TabIndex = 9;
            this.btnReservation.Text = "Reservation";
            this.btnReservation.UseVisualStyleBackColor = false;
            this.btnReservation.Click += new System.EventHandler(this.roundButton2_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.label10.Location = new System.Drawing.Point(43, 12);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(116, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "Counter workspace";
            // 
            // panel7
            // 
            this.panel7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel7.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panel7.Controls.Add(this.roundButton5);
            this.panel7.Controls.Add(this.label12);
            this.panel7.Controls.Add(this.label11);
            this.panel7.Location = new System.Drawing.Point(3, 749);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(261, 142);
            this.panel7.TabIndex = 3;
            // 
            // roundButton5
            // 
            this.roundButton5.BackColor = System.Drawing.Color.Black;
            this.roundButton5.BorderColor = System.Drawing.Color.Gray;
            this.roundButton5.BorderSize = 1;
            this.roundButton5.CornerRadius = 10;
            this.roundButton5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton5.FlatAppearance.BorderSize = 0;
            this.roundButton5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton5.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roundButton5.ForeColor = System.Drawing.Color.White;
            this.roundButton5.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.roundButton5.Location = new System.Drawing.Point(20, 62);
            this.roundButton5.Name = "roundButton5";
            this.roundButton5.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.roundButton5.Size = new System.Drawing.Size(216, 41);
            this.roundButton5.TabIndex = 10;
            this.roundButton5.Text = "LogOut";
            this.roundButton5.UseVisualStyleBackColor = false;
            this.roundButton5.Click += new System.EventHandler(this.roundButton5_Click);
            // 
            // label12
            // 
            this.label12.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label12.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label12.Location = new System.Drawing.Point(27, 10);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(28, 29);
            this.label12.TabIndex = 1;
            this.label12.Text = "•";
            this.label12.Click += new System.EventHandler(this.label12_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.SystemColors.WindowText;
            this.label11.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.label11.Location = new System.Drawing.Point(52, 20);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(158, 19);
            this.label11.TabIndex = 0;
            this.label11.Text = "Grand Avenue Cinema";
            this.label11.Click += new System.EventHandler(this.label11_Click);
            // 
            // roundPanel5
            // 
            this.roundPanel5.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.roundPanel5.BorderColor = System.Drawing.Color.Gray;
            this.roundPanel5.BorderSize = 1;
            this.roundPanel5.Controls.Add(this.cornerPanel1);
            this.roundPanel5.Location = new System.Drawing.Point(49, 20);
            this.roundPanel5.Name = "roundPanel5";
            this.roundPanel5.Size = new System.Drawing.Size(1070, 636);
            this.roundPanel5.TabIndex = 10;
            // 
            // cornerPanel1
            // 
            this.cornerPanel1.BorderBottom = 1;
            this.cornerPanel1.BorderLeft = 1;
            this.cornerPanel1.BorderRight = 1;
            this.cornerPanel1.BorderSize = 1;
            this.cornerPanel1.BorderTop = 1;
            this.cornerPanel1.BottomLeftRadius = 0;
            this.cornerPanel1.BottomRightRadius = 0;
            this.cornerPanel1.Controls.Add(this.label24);
            this.cornerPanel1.Controls.Add(this.label23);
            this.cornerPanel1.Location = new System.Drawing.Point(0, 0);
            this.cornerPanel1.Name = "cornerPanel1";
            this.cornerPanel1.Size = new System.Drawing.Size(1070, 62);
            this.cornerPanel1.TabIndex = 1;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.ForeColor = System.Drawing.Color.Black;
            this.label24.Location = new System.Drawing.Point(10, 36);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(251, 16);
            this.label24.TabIndex = 1;
            this.label24.Text = "All movies and screening times available today";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.ForeColor = System.Drawing.Color.Black;
            this.label23.Location = new System.Drawing.Point(11, 17);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(115, 19);
            this.label23.TabIndex = 0;
            this.label23.Text = "Now Showing";
            // 
            // SeatPanel
            // 
            this.SeatPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.SeatPanel.BottomLeftRadius = 10;
            this.SeatPanel.BottomRightRadius = 10;
            this.SeatPanel.Controls.Add(this.roundButton61);
            this.SeatPanel.Controls.Add(this.roundButton60);
            this.SeatPanel.Controls.Add(this.roundButton59);
            this.SeatPanel.Controls.Add(this.roundButton58);
            this.SeatPanel.Controls.Add(this.roundButton57);
            this.SeatPanel.Controls.Add(this.roundButton56);
            this.SeatPanel.Controls.Add(this.roundButton55);
            this.SeatPanel.Controls.Add(this.roundButton54);
            this.SeatPanel.Controls.Add(this.roundButton53);
            this.SeatPanel.Controls.Add(this.roundButton52);
            this.SeatPanel.Controls.Add(this.roundButton51);
            this.SeatPanel.Controls.Add(this.roundButton50);
            this.SeatPanel.Controls.Add(this.roundButton49);
            this.SeatPanel.Controls.Add(this.roundButton48);
            this.SeatPanel.Controls.Add(this.roundButton47);
            this.SeatPanel.Controls.Add(this.roundButton46);
            this.SeatPanel.Controls.Add(this.roundButton45);
            this.SeatPanel.Controls.Add(this.roundButton44);
            this.SeatPanel.Controls.Add(this.roundButton43);
            this.SeatPanel.Controls.Add(this.roundButton42);
            this.SeatPanel.Controls.Add(this.panel11);
            this.SeatPanel.Controls.Add(this.roundButton41);
            this.SeatPanel.Controls.Add(this.roundButton40);
            this.SeatPanel.Controls.Add(this.roundButton39);
            this.SeatPanel.Controls.Add(this.roundButton38);
            this.SeatPanel.Controls.Add(this.roundButton37);
            this.SeatPanel.Controls.Add(this.roundButton36);
            this.SeatPanel.Controls.Add(this.roundButton35);
            this.SeatPanel.Controls.Add(this.roundButton34);
            this.SeatPanel.Controls.Add(this.roundButton33);
            this.SeatPanel.Controls.Add(this.roundButton32);
            this.SeatPanel.Controls.Add(this.roundButton31);
            this.SeatPanel.Controls.Add(this.roundButton30);
            this.SeatPanel.Controls.Add(this.roundButton29);
            this.SeatPanel.Controls.Add(this.roundButton28);
            this.SeatPanel.Controls.Add(this.roundButton27);
            this.SeatPanel.Controls.Add(this.roundButton26);
            this.SeatPanel.Controls.Add(this.roundButton25);
            this.SeatPanel.Controls.Add(this.roundButton24);
            this.SeatPanel.Controls.Add(this.roundButton14);
            this.SeatPanel.Controls.Add(this.panel9);
            this.SeatPanel.Controls.Add(this.roundButton23);
            this.SeatPanel.Controls.Add(this.roundButton22);
            this.SeatPanel.Controls.Add(this.roundButton21);
            this.SeatPanel.Controls.Add(this.roundButton20);
            this.SeatPanel.Controls.Add(this.roundButton19);
            this.SeatPanel.Controls.Add(this.roundButton18);
            this.SeatPanel.Controls.Add(this.roundButton17);
            this.SeatPanel.Controls.Add(this.roundButton16);
            this.SeatPanel.Controls.Add(this.roundButton15);
            this.SeatPanel.Controls.Add(this.roundButton13);
            this.SeatPanel.Controls.Add(this.roundButton12);
            this.SeatPanel.Controls.Add(this.roundButton11);
            this.SeatPanel.Controls.Add(this.roundButton10);
            this.SeatPanel.Controls.Add(this.roundButton9);
            this.SeatPanel.Controls.Add(this.roundButton8);
            this.SeatPanel.Controls.Add(this.roundButton7);
            this.SeatPanel.Controls.Add(this.roundButton6);
            this.SeatPanel.Controls.Add(this.roundButton4);
            this.SeatPanel.Controls.Add(this.roundButton3);
            this.SeatPanel.Controls.Add(this.roundButton2);
            this.SeatPanel.Controls.Add(this.roundButton1);
            this.SeatPanel.Controls.Add(this.pictureBox2);
            this.SeatPanel.Location = new System.Drawing.Point(41, 35);
            this.SeatPanel.Name = "SeatPanel";
            this.SeatPanel.Size = new System.Drawing.Size(780, 627);
            this.SeatPanel.TabIndex = 0;
            this.SeatPanel.TopLeftRadius = 10;
            this.SeatPanel.TopRightRadius = 10;
            this.SeatPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.SeatPanel_Paint);
            // 
            // SelectedMoviePanel
            // 
            this.SelectedMoviePanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.SelectedMoviePanel.BottomLeftRadius = 10;
            this.SelectedMoviePanel.BottomRightRadius = 10;
            this.SelectedMoviePanel.Location = new System.Drawing.Point(838, 35);
            this.SelectedMoviePanel.Name = "SelectedMoviePanel";
            this.SelectedMoviePanel.Size = new System.Drawing.Size(284, 202);
            this.SelectedMoviePanel.TabIndex = 1;
            this.SelectedMoviePanel.TopLeftRadius = 10;
            this.SelectedMoviePanel.TopRightRadius = 10;
            // 
            // RecieptPanel
            // 
            this.RecieptPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.RecieptPanel.BottomLeftRadius = 10;
            this.RecieptPanel.BottomRightRadius = 10;
            this.RecieptPanel.Location = new System.Drawing.Point(838, 260);
            this.RecieptPanel.Name = "RecieptPanel";
            this.RecieptPanel.Size = new System.Drawing.Size(284, 402);
            this.RecieptPanel.TabIndex = 2;
            this.RecieptPanel.TopLeftRadius = 10;
            this.RecieptPanel.TopRightRadius = 10;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(780, 30);
            this.pictureBox2.TabIndex = 0;
            this.pictureBox2.TabStop = false;
            // 
            // roundButton1
            // 
            this.roundButton1.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton1.BorderColor = System.Drawing.Color.Gray;
            this.roundButton1.CornerRadius = 10;
            this.roundButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton1.FlatAppearance.BorderSize = 0;
            this.roundButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton1.ForeColor = System.Drawing.Color.White;
            this.roundButton1.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton1.Location = new System.Drawing.Point(62, 116);
            this.roundButton1.Name = "roundButton1";
            this.roundButton1.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton1.Size = new System.Drawing.Size(51, 46);
            this.roundButton1.TabIndex = 2;
            this.roundButton1.Text = "1";
            this.roundButton1.UseVisualStyleBackColor = false;
            // 
            // roundButton2
            // 
            this.roundButton2.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton2.BorderColor = System.Drawing.Color.Gray;
            this.roundButton2.CornerRadius = 10;
            this.roundButton2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton2.FlatAppearance.BorderSize = 0;
            this.roundButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton2.ForeColor = System.Drawing.Color.White;
            this.roundButton2.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton2.Location = new System.Drawing.Point(119, 116);
            this.roundButton2.Name = "roundButton2";
            this.roundButton2.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton2.Size = new System.Drawing.Size(51, 46);
            this.roundButton2.TabIndex = 3;
            this.roundButton2.Text = "1";
            this.roundButton2.UseVisualStyleBackColor = false;
            // 
            // roundButton3
            // 
            this.roundButton3.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton3.BorderColor = System.Drawing.Color.Gray;
            this.roundButton3.CornerRadius = 10;
            this.roundButton3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton3.FlatAppearance.BorderSize = 0;
            this.roundButton3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton3.ForeColor = System.Drawing.Color.White;
            this.roundButton3.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton3.Location = new System.Drawing.Point(176, 116);
            this.roundButton3.Name = "roundButton3";
            this.roundButton3.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton3.Size = new System.Drawing.Size(51, 46);
            this.roundButton3.TabIndex = 4;
            this.roundButton3.Text = "1";
            this.roundButton3.UseVisualStyleBackColor = false;
            // 
            // roundButton4
            // 
            this.roundButton4.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton4.BorderColor = System.Drawing.Color.Gray;
            this.roundButton4.CornerRadius = 10;
            this.roundButton4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton4.FlatAppearance.BorderSize = 0;
            this.roundButton4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton4.ForeColor = System.Drawing.Color.White;
            this.roundButton4.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton4.Location = new System.Drawing.Point(233, 116);
            this.roundButton4.Name = "roundButton4";
            this.roundButton4.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton4.Size = new System.Drawing.Size(51, 46);
            this.roundButton4.TabIndex = 5;
            this.roundButton4.Text = "1";
            this.roundButton4.UseVisualStyleBackColor = false;
            // 
            // roundButton6
            // 
            this.roundButton6.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton6.BorderColor = System.Drawing.Color.Gray;
            this.roundButton6.CornerRadius = 10;
            this.roundButton6.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton6.FlatAppearance.BorderSize = 0;
            this.roundButton6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton6.ForeColor = System.Drawing.Color.White;
            this.roundButton6.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton6.Location = new System.Drawing.Point(290, 116);
            this.roundButton6.Name = "roundButton6";
            this.roundButton6.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton6.Size = new System.Drawing.Size(51, 46);
            this.roundButton6.TabIndex = 6;
            this.roundButton6.Text = "1";
            this.roundButton6.UseVisualStyleBackColor = false;
            // 
            // roundButton7
            // 
            this.roundButton7.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton7.BorderColor = System.Drawing.Color.Gray;
            this.roundButton7.CornerRadius = 10;
            this.roundButton7.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton7.FlatAppearance.BorderSize = 0;
            this.roundButton7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton7.ForeColor = System.Drawing.Color.White;
            this.roundButton7.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton7.Location = new System.Drawing.Point(62, 168);
            this.roundButton7.Name = "roundButton7";
            this.roundButton7.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton7.Size = new System.Drawing.Size(51, 46);
            this.roundButton7.TabIndex = 7;
            this.roundButton7.Text = "1";
            this.roundButton7.UseVisualStyleBackColor = false;
            // 
            // roundButton8
            // 
            this.roundButton8.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton8.BorderColor = System.Drawing.Color.Gray;
            this.roundButton8.CornerRadius = 10;
            this.roundButton8.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton8.FlatAppearance.BorderSize = 0;
            this.roundButton8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton8.ForeColor = System.Drawing.Color.White;
            this.roundButton8.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton8.Location = new System.Drawing.Point(119, 168);
            this.roundButton8.Name = "roundButton8";
            this.roundButton8.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton8.Size = new System.Drawing.Size(51, 46);
            this.roundButton8.TabIndex = 8;
            this.roundButton8.Text = "1";
            this.roundButton8.UseVisualStyleBackColor = false;
            // 
            // roundButton9
            // 
            this.roundButton9.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton9.BorderColor = System.Drawing.Color.Gray;
            this.roundButton9.CornerRadius = 10;
            this.roundButton9.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton9.FlatAppearance.BorderSize = 0;
            this.roundButton9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton9.ForeColor = System.Drawing.Color.White;
            this.roundButton9.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton9.Location = new System.Drawing.Point(661, 116);
            this.roundButton9.Name = "roundButton9";
            this.roundButton9.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton9.Size = new System.Drawing.Size(51, 46);
            this.roundButton9.TabIndex = 9;
            this.roundButton9.Text = "1";
            this.roundButton9.UseVisualStyleBackColor = false;
            // 
            // roundButton10
            // 
            this.roundButton10.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton10.BorderColor = System.Drawing.Color.Gray;
            this.roundButton10.CornerRadius = 10;
            this.roundButton10.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton10.FlatAppearance.BorderSize = 0;
            this.roundButton10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton10.ForeColor = System.Drawing.Color.White;
            this.roundButton10.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton10.Location = new System.Drawing.Point(604, 116);
            this.roundButton10.Name = "roundButton10";
            this.roundButton10.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton10.Size = new System.Drawing.Size(51, 46);
            this.roundButton10.TabIndex = 10;
            this.roundButton10.Text = "1";
            this.roundButton10.UseVisualStyleBackColor = false;
            // 
            // roundButton11
            // 
            this.roundButton11.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton11.BorderColor = System.Drawing.Color.Gray;
            this.roundButton11.CornerRadius = 10;
            this.roundButton11.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton11.FlatAppearance.BorderSize = 0;
            this.roundButton11.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton11.ForeColor = System.Drawing.Color.White;
            this.roundButton11.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton11.Location = new System.Drawing.Point(547, 116);
            this.roundButton11.Name = "roundButton11";
            this.roundButton11.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton11.Size = new System.Drawing.Size(51, 46);
            this.roundButton11.TabIndex = 11;
            this.roundButton11.Text = "1";
            this.roundButton11.UseVisualStyleBackColor = false;
            // 
            // roundButton12
            // 
            this.roundButton12.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton12.BorderColor = System.Drawing.Color.Gray;
            this.roundButton12.CornerRadius = 10;
            this.roundButton12.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton12.FlatAppearance.BorderSize = 0;
            this.roundButton12.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton12.ForeColor = System.Drawing.Color.White;
            this.roundButton12.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton12.Location = new System.Drawing.Point(490, 116);
            this.roundButton12.Name = "roundButton12";
            this.roundButton12.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton12.Size = new System.Drawing.Size(51, 46);
            this.roundButton12.TabIndex = 12;
            this.roundButton12.Text = "1";
            this.roundButton12.UseVisualStyleBackColor = false;
            // 
            // roundButton13
            // 
            this.roundButton13.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton13.BorderColor = System.Drawing.Color.Gray;
            this.roundButton13.CornerRadius = 10;
            this.roundButton13.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton13.FlatAppearance.BorderSize = 0;
            this.roundButton13.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton13.ForeColor = System.Drawing.Color.White;
            this.roundButton13.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton13.Location = new System.Drawing.Point(433, 116);
            this.roundButton13.Name = "roundButton13";
            this.roundButton13.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton13.Size = new System.Drawing.Size(51, 46);
            this.roundButton13.TabIndex = 13;
            this.roundButton13.Text = "1";
            this.roundButton13.UseVisualStyleBackColor = false;
            // 
            // roundButton15
            // 
            this.roundButton15.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton15.BorderColor = System.Drawing.Color.Gray;
            this.roundButton15.CornerRadius = 10;
            this.roundButton15.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton15.FlatAppearance.BorderSize = 0;
            this.roundButton15.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton15.ForeColor = System.Drawing.Color.White;
            this.roundButton15.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton15.Location = new System.Drawing.Point(62, 254);
            this.roundButton15.Name = "roundButton15";
            this.roundButton15.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton15.Size = new System.Drawing.Size(51, 46);
            this.roundButton15.TabIndex = 15;
            this.roundButton15.Text = "1";
            this.roundButton15.UseVisualStyleBackColor = false;
            // 
            // roundButton16
            // 
            this.roundButton16.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton16.BorderColor = System.Drawing.Color.Gray;
            this.roundButton16.CornerRadius = 10;
            this.roundButton16.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton16.FlatAppearance.BorderSize = 0;
            this.roundButton16.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton16.ForeColor = System.Drawing.Color.White;
            this.roundButton16.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton16.Location = new System.Drawing.Point(661, 168);
            this.roundButton16.Name = "roundButton16";
            this.roundButton16.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton16.Size = new System.Drawing.Size(51, 46);
            this.roundButton16.TabIndex = 16;
            this.roundButton16.Text = "1";
            this.roundButton16.UseVisualStyleBackColor = false;
            // 
            // roundButton17
            // 
            this.roundButton17.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton17.BorderColor = System.Drawing.Color.Gray;
            this.roundButton17.CornerRadius = 10;
            this.roundButton17.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton17.FlatAppearance.BorderSize = 0;
            this.roundButton17.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton17.ForeColor = System.Drawing.Color.White;
            this.roundButton17.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton17.Location = new System.Drawing.Point(604, 168);
            this.roundButton17.Name = "roundButton17";
            this.roundButton17.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton17.Size = new System.Drawing.Size(51, 46);
            this.roundButton17.TabIndex = 17;
            this.roundButton17.Text = "1";
            this.roundButton17.UseVisualStyleBackColor = false;
            // 
            // roundButton18
            // 
            this.roundButton18.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton18.BorderColor = System.Drawing.Color.Gray;
            this.roundButton18.CornerRadius = 10;
            this.roundButton18.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton18.FlatAppearance.BorderSize = 0;
            this.roundButton18.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton18.ForeColor = System.Drawing.Color.White;
            this.roundButton18.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton18.Location = new System.Drawing.Point(547, 168);
            this.roundButton18.Name = "roundButton18";
            this.roundButton18.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton18.Size = new System.Drawing.Size(51, 46);
            this.roundButton18.TabIndex = 18;
            this.roundButton18.Text = "1";
            this.roundButton18.UseVisualStyleBackColor = false;
            // 
            // roundButton19
            // 
            this.roundButton19.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton19.BorderColor = System.Drawing.Color.Gray;
            this.roundButton19.CornerRadius = 10;
            this.roundButton19.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton19.FlatAppearance.BorderSize = 0;
            this.roundButton19.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton19.ForeColor = System.Drawing.Color.White;
            this.roundButton19.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton19.Location = new System.Drawing.Point(490, 168);
            this.roundButton19.Name = "roundButton19";
            this.roundButton19.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton19.Size = new System.Drawing.Size(51, 46);
            this.roundButton19.TabIndex = 19;
            this.roundButton19.Text = "1";
            this.roundButton19.UseVisualStyleBackColor = false;
            // 
            // roundButton20
            // 
            this.roundButton20.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton20.BorderColor = System.Drawing.Color.Gray;
            this.roundButton20.CornerRadius = 10;
            this.roundButton20.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton20.FlatAppearance.BorderSize = 0;
            this.roundButton20.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton20.ForeColor = System.Drawing.Color.White;
            this.roundButton20.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton20.Location = new System.Drawing.Point(433, 168);
            this.roundButton20.Name = "roundButton20";
            this.roundButton20.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton20.Size = new System.Drawing.Size(51, 46);
            this.roundButton20.TabIndex = 20;
            this.roundButton20.Text = "1";
            this.roundButton20.UseVisualStyleBackColor = false;
            // 
            // roundButton21
            // 
            this.roundButton21.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton21.BorderColor = System.Drawing.Color.Gray;
            this.roundButton21.CornerRadius = 10;
            this.roundButton21.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton21.FlatAppearance.BorderSize = 0;
            this.roundButton21.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton21.ForeColor = System.Drawing.Color.White;
            this.roundButton21.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton21.Location = new System.Drawing.Point(290, 168);
            this.roundButton21.Name = "roundButton21";
            this.roundButton21.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton21.Size = new System.Drawing.Size(51, 46);
            this.roundButton21.TabIndex = 21;
            this.roundButton21.Text = "1";
            this.roundButton21.UseVisualStyleBackColor = false;
            // 
            // roundButton22
            // 
            this.roundButton22.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton22.BorderColor = System.Drawing.Color.Gray;
            this.roundButton22.CornerRadius = 10;
            this.roundButton22.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton22.FlatAppearance.BorderSize = 0;
            this.roundButton22.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton22.ForeColor = System.Drawing.Color.White;
            this.roundButton22.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton22.Location = new System.Drawing.Point(233, 168);
            this.roundButton22.Name = "roundButton22";
            this.roundButton22.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton22.Size = new System.Drawing.Size(51, 46);
            this.roundButton22.TabIndex = 22;
            this.roundButton22.Text = "1";
            this.roundButton22.UseVisualStyleBackColor = false;
            // 
            // roundButton23
            // 
            this.roundButton23.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton23.BorderColor = System.Drawing.Color.Gray;
            this.roundButton23.CornerRadius = 10;
            this.roundButton23.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton23.FlatAppearance.BorderSize = 0;
            this.roundButton23.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton23.ForeColor = System.Drawing.Color.White;
            this.roundButton23.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton23.Location = new System.Drawing.Point(176, 168);
            this.roundButton23.Name = "roundButton23";
            this.roundButton23.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton23.Size = new System.Drawing.Size(51, 46);
            this.roundButton23.TabIndex = 23;
            this.roundButton23.Text = "1";
            this.roundButton23.UseVisualStyleBackColor = false;
            // 
            // panel9
            // 
            this.panel9.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.panel9.Location = new System.Drawing.Point(62, 225);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(650, 13);
            this.panel9.TabIndex = 25;
            // 
            // roundButton14
            // 
            this.roundButton14.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton14.BorderColor = System.Drawing.Color.Gray;
            this.roundButton14.CornerRadius = 10;
            this.roundButton14.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton14.FlatAppearance.BorderSize = 0;
            this.roundButton14.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton14.ForeColor = System.Drawing.Color.White;
            this.roundButton14.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton14.Location = new System.Drawing.Point(62, 306);
            this.roundButton14.Name = "roundButton14";
            this.roundButton14.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton14.Size = new System.Drawing.Size(51, 46);
            this.roundButton14.TabIndex = 26;
            this.roundButton14.Text = "1";
            this.roundButton14.UseVisualStyleBackColor = false;
            // 
            // roundButton24
            // 
            this.roundButton24.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton24.BorderColor = System.Drawing.Color.Gray;
            this.roundButton24.CornerRadius = 10;
            this.roundButton24.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton24.FlatAppearance.BorderSize = 0;
            this.roundButton24.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton24.ForeColor = System.Drawing.Color.White;
            this.roundButton24.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton24.Location = new System.Drawing.Point(661, 254);
            this.roundButton24.Name = "roundButton24";
            this.roundButton24.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton24.Size = new System.Drawing.Size(51, 46);
            this.roundButton24.TabIndex = 27;
            this.roundButton24.Text = "1";
            this.roundButton24.UseVisualStyleBackColor = false;
            // 
            // roundButton25
            // 
            this.roundButton25.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton25.BorderColor = System.Drawing.Color.Gray;
            this.roundButton25.CornerRadius = 10;
            this.roundButton25.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton25.FlatAppearance.BorderSize = 0;
            this.roundButton25.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton25.ForeColor = System.Drawing.Color.White;
            this.roundButton25.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton25.Location = new System.Drawing.Point(604, 254);
            this.roundButton25.Name = "roundButton25";
            this.roundButton25.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton25.Size = new System.Drawing.Size(51, 46);
            this.roundButton25.TabIndex = 28;
            this.roundButton25.Text = "1";
            this.roundButton25.UseVisualStyleBackColor = false;
            // 
            // roundButton26
            // 
            this.roundButton26.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton26.BorderColor = System.Drawing.Color.Gray;
            this.roundButton26.CornerRadius = 10;
            this.roundButton26.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton26.FlatAppearance.BorderSize = 0;
            this.roundButton26.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton26.ForeColor = System.Drawing.Color.White;
            this.roundButton26.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton26.Location = new System.Drawing.Point(547, 254);
            this.roundButton26.Name = "roundButton26";
            this.roundButton26.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton26.Size = new System.Drawing.Size(51, 46);
            this.roundButton26.TabIndex = 29;
            this.roundButton26.Text = "1";
            this.roundButton26.UseVisualStyleBackColor = false;
            // 
            // roundButton27
            // 
            this.roundButton27.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton27.BorderColor = System.Drawing.Color.Gray;
            this.roundButton27.CornerRadius = 10;
            this.roundButton27.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton27.FlatAppearance.BorderSize = 0;
            this.roundButton27.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton27.ForeColor = System.Drawing.Color.White;
            this.roundButton27.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton27.Location = new System.Drawing.Point(490, 254);
            this.roundButton27.Name = "roundButton27";
            this.roundButton27.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton27.Size = new System.Drawing.Size(51, 46);
            this.roundButton27.TabIndex = 30;
            this.roundButton27.Text = "1";
            this.roundButton27.UseVisualStyleBackColor = false;
            // 
            // roundButton28
            // 
            this.roundButton28.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton28.BorderColor = System.Drawing.Color.Gray;
            this.roundButton28.CornerRadius = 10;
            this.roundButton28.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton28.FlatAppearance.BorderSize = 0;
            this.roundButton28.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton28.ForeColor = System.Drawing.Color.White;
            this.roundButton28.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton28.Location = new System.Drawing.Point(433, 254);
            this.roundButton28.Name = "roundButton28";
            this.roundButton28.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton28.Size = new System.Drawing.Size(51, 46);
            this.roundButton28.TabIndex = 31;
            this.roundButton28.Text = "1";
            this.roundButton28.UseVisualStyleBackColor = false;
            // 
            // roundButton29
            // 
            this.roundButton29.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton29.BorderColor = System.Drawing.Color.Gray;
            this.roundButton29.CornerRadius = 10;
            this.roundButton29.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton29.FlatAppearance.BorderSize = 0;
            this.roundButton29.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton29.ForeColor = System.Drawing.Color.White;
            this.roundButton29.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton29.Location = new System.Drawing.Point(290, 254);
            this.roundButton29.Name = "roundButton29";
            this.roundButton29.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton29.Size = new System.Drawing.Size(51, 46);
            this.roundButton29.TabIndex = 32;
            this.roundButton29.Text = "1";
            this.roundButton29.UseVisualStyleBackColor = false;
            // 
            // roundButton30
            // 
            this.roundButton30.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton30.BorderColor = System.Drawing.Color.Gray;
            this.roundButton30.CornerRadius = 10;
            this.roundButton30.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton30.FlatAppearance.BorderSize = 0;
            this.roundButton30.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton30.ForeColor = System.Drawing.Color.White;
            this.roundButton30.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton30.Location = new System.Drawing.Point(233, 254);
            this.roundButton30.Name = "roundButton30";
            this.roundButton30.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton30.Size = new System.Drawing.Size(51, 46);
            this.roundButton30.TabIndex = 33;
            this.roundButton30.Text = "1";
            this.roundButton30.UseVisualStyleBackColor = false;
            // 
            // roundButton31
            // 
            this.roundButton31.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton31.BorderColor = System.Drawing.Color.Gray;
            this.roundButton31.CornerRadius = 10;
            this.roundButton31.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton31.FlatAppearance.BorderSize = 0;
            this.roundButton31.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton31.ForeColor = System.Drawing.Color.White;
            this.roundButton31.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton31.Location = new System.Drawing.Point(176, 254);
            this.roundButton31.Name = "roundButton31";
            this.roundButton31.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton31.Size = new System.Drawing.Size(51, 46);
            this.roundButton31.TabIndex = 34;
            this.roundButton31.Text = "1";
            this.roundButton31.UseVisualStyleBackColor = false;
            // 
            // roundButton32
            // 
            this.roundButton32.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton32.BorderColor = System.Drawing.Color.Gray;
            this.roundButton32.CornerRadius = 10;
            this.roundButton32.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton32.FlatAppearance.BorderSize = 0;
            this.roundButton32.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton32.ForeColor = System.Drawing.Color.White;
            this.roundButton32.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton32.Location = new System.Drawing.Point(119, 254);
            this.roundButton32.Name = "roundButton32";
            this.roundButton32.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton32.Size = new System.Drawing.Size(51, 46);
            this.roundButton32.TabIndex = 35;
            this.roundButton32.Text = "1";
            this.roundButton32.UseVisualStyleBackColor = false;
            // 
            // roundButton33
            // 
            this.roundButton33.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton33.BorderColor = System.Drawing.Color.Gray;
            this.roundButton33.CornerRadius = 10;
            this.roundButton33.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton33.FlatAppearance.BorderSize = 0;
            this.roundButton33.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton33.ForeColor = System.Drawing.Color.White;
            this.roundButton33.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton33.Location = new System.Drawing.Point(176, 306);
            this.roundButton33.Name = "roundButton33";
            this.roundButton33.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton33.Size = new System.Drawing.Size(51, 46);
            this.roundButton33.TabIndex = 36;
            this.roundButton33.Text = "1";
            this.roundButton33.UseVisualStyleBackColor = false;
            // 
            // roundButton34
            // 
            this.roundButton34.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton34.BorderColor = System.Drawing.Color.Gray;
            this.roundButton34.CornerRadius = 10;
            this.roundButton34.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton34.FlatAppearance.BorderSize = 0;
            this.roundButton34.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton34.ForeColor = System.Drawing.Color.White;
            this.roundButton34.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton34.Location = new System.Drawing.Point(119, 306);
            this.roundButton34.Name = "roundButton34";
            this.roundButton34.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton34.Size = new System.Drawing.Size(51, 46);
            this.roundButton34.TabIndex = 37;
            this.roundButton34.Text = "1";
            this.roundButton34.UseVisualStyleBackColor = false;
            // 
            // roundButton35
            // 
            this.roundButton35.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton35.BorderColor = System.Drawing.Color.Gray;
            this.roundButton35.CornerRadius = 10;
            this.roundButton35.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton35.FlatAppearance.BorderSize = 0;
            this.roundButton35.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton35.ForeColor = System.Drawing.Color.White;
            this.roundButton35.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton35.Location = new System.Drawing.Point(433, 306);
            this.roundButton35.Name = "roundButton35";
            this.roundButton35.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton35.Size = new System.Drawing.Size(51, 46);
            this.roundButton35.TabIndex = 38;
            this.roundButton35.Text = "1";
            this.roundButton35.UseVisualStyleBackColor = false;
            // 
            // roundButton36
            // 
            this.roundButton36.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton36.BorderColor = System.Drawing.Color.Gray;
            this.roundButton36.CornerRadius = 10;
            this.roundButton36.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton36.FlatAppearance.BorderSize = 0;
            this.roundButton36.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton36.ForeColor = System.Drawing.Color.White;
            this.roundButton36.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton36.Location = new System.Drawing.Point(490, 306);
            this.roundButton36.Name = "roundButton36";
            this.roundButton36.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton36.Size = new System.Drawing.Size(51, 46);
            this.roundButton36.TabIndex = 39;
            this.roundButton36.Text = "1";
            this.roundButton36.UseVisualStyleBackColor = false;
            this.roundButton36.Click += new System.EventHandler(this.roundButton36_Click);
            // 
            // roundButton37
            // 
            this.roundButton37.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton37.BorderColor = System.Drawing.Color.Gray;
            this.roundButton37.CornerRadius = 10;
            this.roundButton37.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton37.FlatAppearance.BorderSize = 0;
            this.roundButton37.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton37.ForeColor = System.Drawing.Color.White;
            this.roundButton37.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton37.Location = new System.Drawing.Point(547, 306);
            this.roundButton37.Name = "roundButton37";
            this.roundButton37.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton37.Size = new System.Drawing.Size(51, 46);
            this.roundButton37.TabIndex = 40;
            this.roundButton37.Text = "1";
            this.roundButton37.UseVisualStyleBackColor = false;
            // 
            // roundButton38
            // 
            this.roundButton38.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton38.BorderColor = System.Drawing.Color.Gray;
            this.roundButton38.CornerRadius = 10;
            this.roundButton38.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton38.FlatAppearance.BorderSize = 0;
            this.roundButton38.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton38.ForeColor = System.Drawing.Color.White;
            this.roundButton38.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton38.Location = new System.Drawing.Point(604, 306);
            this.roundButton38.Name = "roundButton38";
            this.roundButton38.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton38.Size = new System.Drawing.Size(51, 46);
            this.roundButton38.TabIndex = 41;
            this.roundButton38.Text = "1";
            this.roundButton38.UseVisualStyleBackColor = false;
            // 
            // roundButton39
            // 
            this.roundButton39.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton39.BorderColor = System.Drawing.Color.Gray;
            this.roundButton39.CornerRadius = 10;
            this.roundButton39.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton39.FlatAppearance.BorderSize = 0;
            this.roundButton39.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton39.ForeColor = System.Drawing.Color.White;
            this.roundButton39.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton39.Location = new System.Drawing.Point(661, 306);
            this.roundButton39.Name = "roundButton39";
            this.roundButton39.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton39.Size = new System.Drawing.Size(51, 46);
            this.roundButton39.TabIndex = 42;
            this.roundButton39.Text = "1";
            this.roundButton39.UseVisualStyleBackColor = false;
            // 
            // roundButton40
            // 
            this.roundButton40.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton40.BorderColor = System.Drawing.Color.Gray;
            this.roundButton40.CornerRadius = 10;
            this.roundButton40.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton40.FlatAppearance.BorderSize = 0;
            this.roundButton40.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton40.ForeColor = System.Drawing.Color.White;
            this.roundButton40.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton40.Location = new System.Drawing.Point(289, 306);
            this.roundButton40.Name = "roundButton40";
            this.roundButton40.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton40.Size = new System.Drawing.Size(51, 46);
            this.roundButton40.TabIndex = 43;
            this.roundButton40.Text = "1";
            this.roundButton40.UseVisualStyleBackColor = false;
            // 
            // roundButton41
            // 
            this.roundButton41.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton41.BorderColor = System.Drawing.Color.Gray;
            this.roundButton41.CornerRadius = 10;
            this.roundButton41.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton41.FlatAppearance.BorderSize = 0;
            this.roundButton41.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton41.ForeColor = System.Drawing.Color.White;
            this.roundButton41.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton41.Location = new System.Drawing.Point(233, 306);
            this.roundButton41.Name = "roundButton41";
            this.roundButton41.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton41.Size = new System.Drawing.Size(51, 46);
            this.roundButton41.TabIndex = 44;
            this.roundButton41.Text = "1";
            this.roundButton41.UseVisualStyleBackColor = false;
            // 
            // panel10
            // 
            this.panel10.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel10.Location = new System.Drawing.Point(0, 3);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(650, 13);
            this.panel10.TabIndex = 45;
            // 
            // panel11
            // 
            this.panel11.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.panel11.Controls.Add(this.panel10);
            this.panel11.Location = new System.Drawing.Point(65, 358);
            this.panel11.Name = "panel11";
            this.panel11.Size = new System.Drawing.Size(650, 13);
            this.panel11.TabIndex = 46;
            // 
            // roundButton42
            // 
            this.roundButton42.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton42.BorderColor = System.Drawing.Color.Gray;
            this.roundButton42.CornerRadius = 10;
            this.roundButton42.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton42.FlatAppearance.BorderSize = 0;
            this.roundButton42.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton42.ForeColor = System.Drawing.Color.White;
            this.roundButton42.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton42.Location = new System.Drawing.Point(62, 380);
            this.roundButton42.Name = "roundButton42";
            this.roundButton42.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton42.Size = new System.Drawing.Size(51, 46);
            this.roundButton42.TabIndex = 47;
            this.roundButton42.Text = "1";
            this.roundButton42.UseVisualStyleBackColor = false;
            // 
            // roundButton43
            // 
            this.roundButton43.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton43.BorderColor = System.Drawing.Color.Gray;
            this.roundButton43.CornerRadius = 10;
            this.roundButton43.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton43.FlatAppearance.BorderSize = 0;
            this.roundButton43.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton43.ForeColor = System.Drawing.Color.White;
            this.roundButton43.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton43.Location = new System.Drawing.Point(661, 380);
            this.roundButton43.Name = "roundButton43";
            this.roundButton43.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton43.Size = new System.Drawing.Size(51, 46);
            this.roundButton43.TabIndex = 48;
            this.roundButton43.Text = "1";
            this.roundButton43.UseVisualStyleBackColor = false;
            // 
            // roundButton44
            // 
            this.roundButton44.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton44.BorderColor = System.Drawing.Color.Gray;
            this.roundButton44.CornerRadius = 10;
            this.roundButton44.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton44.FlatAppearance.BorderSize = 0;
            this.roundButton44.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton44.ForeColor = System.Drawing.Color.White;
            this.roundButton44.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton44.Location = new System.Drawing.Point(604, 377);
            this.roundButton44.Name = "roundButton44";
            this.roundButton44.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton44.Size = new System.Drawing.Size(51, 46);
            this.roundButton44.TabIndex = 49;
            this.roundButton44.Text = "1";
            this.roundButton44.UseVisualStyleBackColor = false;
            // 
            // roundButton45
            // 
            this.roundButton45.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton45.BorderColor = System.Drawing.Color.Gray;
            this.roundButton45.CornerRadius = 10;
            this.roundButton45.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton45.FlatAppearance.BorderSize = 0;
            this.roundButton45.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton45.ForeColor = System.Drawing.Color.White;
            this.roundButton45.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton45.Location = new System.Drawing.Point(547, 377);
            this.roundButton45.Name = "roundButton45";
            this.roundButton45.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton45.Size = new System.Drawing.Size(51, 46);
            this.roundButton45.TabIndex = 50;
            this.roundButton45.Text = "1";
            this.roundButton45.UseVisualStyleBackColor = false;
            // 
            // roundButton46
            // 
            this.roundButton46.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton46.BorderColor = System.Drawing.Color.Gray;
            this.roundButton46.CornerRadius = 10;
            this.roundButton46.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton46.FlatAppearance.BorderSize = 0;
            this.roundButton46.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton46.ForeColor = System.Drawing.Color.White;
            this.roundButton46.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton46.Location = new System.Drawing.Point(490, 380);
            this.roundButton46.Name = "roundButton46";
            this.roundButton46.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton46.Size = new System.Drawing.Size(51, 46);
            this.roundButton46.TabIndex = 51;
            this.roundButton46.Text = "1";
            this.roundButton46.UseVisualStyleBackColor = false;
            // 
            // roundButton47
            // 
            this.roundButton47.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton47.BorderColor = System.Drawing.Color.Gray;
            this.roundButton47.CornerRadius = 10;
            this.roundButton47.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton47.FlatAppearance.BorderSize = 0;
            this.roundButton47.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton47.ForeColor = System.Drawing.Color.White;
            this.roundButton47.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton47.Location = new System.Drawing.Point(433, 380);
            this.roundButton47.Name = "roundButton47";
            this.roundButton47.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton47.Size = new System.Drawing.Size(51, 46);
            this.roundButton47.TabIndex = 52;
            this.roundButton47.Text = "1";
            this.roundButton47.UseVisualStyleBackColor = false;
            // 
            // roundButton48
            // 
            this.roundButton48.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton48.BorderColor = System.Drawing.Color.Gray;
            this.roundButton48.CornerRadius = 10;
            this.roundButton48.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton48.FlatAppearance.BorderSize = 0;
            this.roundButton48.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton48.ForeColor = System.Drawing.Color.White;
            this.roundButton48.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton48.Location = new System.Drawing.Point(290, 377);
            this.roundButton48.Name = "roundButton48";
            this.roundButton48.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton48.Size = new System.Drawing.Size(51, 46);
            this.roundButton48.TabIndex = 53;
            this.roundButton48.Text = "1";
            this.roundButton48.UseVisualStyleBackColor = false;
            this.roundButton48.Click += new System.EventHandler(this.roundButton48_Click);
            // 
            // roundButton49
            // 
            this.roundButton49.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton49.BorderColor = System.Drawing.Color.Gray;
            this.roundButton49.CornerRadius = 10;
            this.roundButton49.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton49.FlatAppearance.BorderSize = 0;
            this.roundButton49.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton49.ForeColor = System.Drawing.Color.White;
            this.roundButton49.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton49.Location = new System.Drawing.Point(233, 380);
            this.roundButton49.Name = "roundButton49";
            this.roundButton49.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton49.Size = new System.Drawing.Size(51, 46);
            this.roundButton49.TabIndex = 54;
            this.roundButton49.Text = "1";
            this.roundButton49.UseVisualStyleBackColor = false;
            // 
            // roundButton50
            // 
            this.roundButton50.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton50.BorderColor = System.Drawing.Color.Gray;
            this.roundButton50.CornerRadius = 10;
            this.roundButton50.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton50.FlatAppearance.BorderSize = 0;
            this.roundButton50.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton50.ForeColor = System.Drawing.Color.White;
            this.roundButton50.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton50.Location = new System.Drawing.Point(176, 380);
            this.roundButton50.Name = "roundButton50";
            this.roundButton50.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton50.Size = new System.Drawing.Size(51, 46);
            this.roundButton50.TabIndex = 55;
            this.roundButton50.Text = "1";
            this.roundButton50.UseVisualStyleBackColor = false;
            // 
            // roundButton51
            // 
            this.roundButton51.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton51.BorderColor = System.Drawing.Color.Gray;
            this.roundButton51.CornerRadius = 10;
            this.roundButton51.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton51.FlatAppearance.BorderSize = 0;
            this.roundButton51.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton51.ForeColor = System.Drawing.Color.White;
            this.roundButton51.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton51.Location = new System.Drawing.Point(119, 380);
            this.roundButton51.Name = "roundButton51";
            this.roundButton51.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton51.Size = new System.Drawing.Size(51, 46);
            this.roundButton51.TabIndex = 56;
            this.roundButton51.Text = "1";
            this.roundButton51.UseVisualStyleBackColor = false;
            // 
            // roundButton52
            // 
            this.roundButton52.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton52.BorderColor = System.Drawing.Color.Gray;
            this.roundButton52.CornerRadius = 10;
            this.roundButton52.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton52.FlatAppearance.BorderSize = 0;
            this.roundButton52.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton52.ForeColor = System.Drawing.Color.White;
            this.roundButton52.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton52.Location = new System.Drawing.Point(661, 432);
            this.roundButton52.Name = "roundButton52";
            this.roundButton52.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton52.Size = new System.Drawing.Size(51, 46);
            this.roundButton52.TabIndex = 57;
            this.roundButton52.Text = "1";
            this.roundButton52.UseVisualStyleBackColor = false;
            // 
            // roundButton53
            // 
            this.roundButton53.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton53.BorderColor = System.Drawing.Color.Gray;
            this.roundButton53.CornerRadius = 10;
            this.roundButton53.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton53.FlatAppearance.BorderSize = 0;
            this.roundButton53.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton53.ForeColor = System.Drawing.Color.White;
            this.roundButton53.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton53.Location = new System.Drawing.Point(604, 432);
            this.roundButton53.Name = "roundButton53";
            this.roundButton53.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton53.Size = new System.Drawing.Size(51, 46);
            this.roundButton53.TabIndex = 58;
            this.roundButton53.Text = "1";
            this.roundButton53.UseVisualStyleBackColor = false;
            // 
            // roundButton54
            // 
            this.roundButton54.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton54.BorderColor = System.Drawing.Color.Gray;
            this.roundButton54.CornerRadius = 10;
            this.roundButton54.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton54.FlatAppearance.BorderSize = 0;
            this.roundButton54.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton54.ForeColor = System.Drawing.Color.White;
            this.roundButton54.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton54.Location = new System.Drawing.Point(547, 432);
            this.roundButton54.Name = "roundButton54";
            this.roundButton54.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton54.Size = new System.Drawing.Size(51, 46);
            this.roundButton54.TabIndex = 59;
            this.roundButton54.Text = "1";
            this.roundButton54.UseVisualStyleBackColor = false;
            // 
            // roundButton55
            // 
            this.roundButton55.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton55.BorderColor = System.Drawing.Color.Gray;
            this.roundButton55.CornerRadius = 10;
            this.roundButton55.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton55.FlatAppearance.BorderSize = 0;
            this.roundButton55.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton55.ForeColor = System.Drawing.Color.White;
            this.roundButton55.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton55.Location = new System.Drawing.Point(490, 432);
            this.roundButton55.Name = "roundButton55";
            this.roundButton55.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton55.Size = new System.Drawing.Size(51, 46);
            this.roundButton55.TabIndex = 60;
            this.roundButton55.Text = "1";
            this.roundButton55.UseVisualStyleBackColor = false;
            // 
            // roundButton56
            // 
            this.roundButton56.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton56.BorderColor = System.Drawing.Color.Gray;
            this.roundButton56.CornerRadius = 10;
            this.roundButton56.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton56.FlatAppearance.BorderSize = 0;
            this.roundButton56.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton56.ForeColor = System.Drawing.Color.White;
            this.roundButton56.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton56.Location = new System.Drawing.Point(433, 432);
            this.roundButton56.Name = "roundButton56";
            this.roundButton56.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton56.Size = new System.Drawing.Size(51, 46);
            this.roundButton56.TabIndex = 61;
            this.roundButton56.Text = "1";
            this.roundButton56.UseVisualStyleBackColor = false;
            // 
            // roundButton57
            // 
            this.roundButton57.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton57.BorderColor = System.Drawing.Color.Gray;
            this.roundButton57.CornerRadius = 10;
            this.roundButton57.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton57.FlatAppearance.BorderSize = 0;
            this.roundButton57.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton57.ForeColor = System.Drawing.Color.White;
            this.roundButton57.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton57.Location = new System.Drawing.Point(290, 432);
            this.roundButton57.Name = "roundButton57";
            this.roundButton57.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton57.Size = new System.Drawing.Size(51, 46);
            this.roundButton57.TabIndex = 62;
            this.roundButton57.Text = "1";
            this.roundButton57.UseVisualStyleBackColor = false;
            // 
            // roundButton58
            // 
            this.roundButton58.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton58.BorderColor = System.Drawing.Color.Gray;
            this.roundButton58.CornerRadius = 10;
            this.roundButton58.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton58.FlatAppearance.BorderSize = 0;
            this.roundButton58.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton58.ForeColor = System.Drawing.Color.White;
            this.roundButton58.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton58.Location = new System.Drawing.Point(233, 432);
            this.roundButton58.Name = "roundButton58";
            this.roundButton58.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton58.Size = new System.Drawing.Size(51, 46);
            this.roundButton58.TabIndex = 63;
            this.roundButton58.Text = "1";
            this.roundButton58.UseVisualStyleBackColor = false;
            // 
            // roundButton59
            // 
            this.roundButton59.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton59.BorderColor = System.Drawing.Color.Gray;
            this.roundButton59.CornerRadius = 10;
            this.roundButton59.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton59.FlatAppearance.BorderSize = 0;
            this.roundButton59.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton59.ForeColor = System.Drawing.Color.White;
            this.roundButton59.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton59.Location = new System.Drawing.Point(176, 432);
            this.roundButton59.Name = "roundButton59";
            this.roundButton59.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton59.Size = new System.Drawing.Size(51, 46);
            this.roundButton59.TabIndex = 64;
            this.roundButton59.Text = "1";
            this.roundButton59.UseVisualStyleBackColor = false;
            // 
            // roundButton60
            // 
            this.roundButton60.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton60.BorderColor = System.Drawing.Color.Gray;
            this.roundButton60.CornerRadius = 10;
            this.roundButton60.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton60.FlatAppearance.BorderSize = 0;
            this.roundButton60.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton60.ForeColor = System.Drawing.Color.White;
            this.roundButton60.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton60.Location = new System.Drawing.Point(119, 432);
            this.roundButton60.Name = "roundButton60";
            this.roundButton60.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton60.Size = new System.Drawing.Size(51, 46);
            this.roundButton60.TabIndex = 65;
            this.roundButton60.Text = "1";
            this.roundButton60.UseVisualStyleBackColor = false;
            // 
            // roundButton61
            // 
            this.roundButton61.BackColor = System.Drawing.Color.SteelBlue;
            this.roundButton61.BorderColor = System.Drawing.Color.Gray;
            this.roundButton61.CornerRadius = 10;
            this.roundButton61.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton61.FlatAppearance.BorderSize = 0;
            this.roundButton61.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton61.ForeColor = System.Drawing.Color.White;
            this.roundButton61.HoverBackColor = System.Drawing.Color.Empty;
            this.roundButton61.Location = new System.Drawing.Point(62, 432);
            this.roundButton61.Name = "roundButton61";
            this.roundButton61.PressedBackColor = System.Drawing.Color.Empty;
            this.roundButton61.Size = new System.Drawing.Size(51, 46);
            this.roundButton61.TabIndex = 66;
            this.roundButton61.Text = "1";
            this.roundButton61.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1440, 900);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel8.ResumeLayout(false);
            this.panel8.PerformLayout();
            this.RightMotherPanel.ResumeLayout(false);
            this.ReservationPanel.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel5.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.roundPanel5.ResumeLayout(false);
            this.cornerPanel1.ResumeLayout(false);
            this.cornerPanel1.PerformLayout();
            this.SeatPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panel11.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void roundButton1_Click(object sender, EventArgs e)
        {
            ShowPage(0);
        }

        private async void roundButton3_Click(object sender, EventArgs e)
        {
            ShowPage(0);
            await _movies.LoadMoviesAsync();
        }

        private void roundButton4_Click(object sender, EventArgs e)
        {
            ShowPage(3);
            try
            {
                using (var conn = Db.GetConnection())
                {
                    conn.Open();
                    MessageBox.Show("Connected!");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        private void roundButton2_Click(object sender, EventArgs e)
        {
            ShowPage(1);
        }

        private void roundButton5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void cornerPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void MoviesPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void roundButton1_Click_1(object sender, EventArgs e)
        {

        }

        private void roundButton36_Click(object sender, EventArgs e)
        {

        }

        private void roundButton48_Click(object sender, EventArgs e)
        {

        }

        private void SeatPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
