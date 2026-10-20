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
        public Form1()
        {
            InitializeComponent();
            _panels = new Panel[] { OverviewPanel, ReservationPanel, MoviesPanel, SettingsPanel };
            ShowPage(0);
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panel8 = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.RightMotherPanel = new System.Windows.Forms.Panel();
            this.OverviewPanel = new System.Windows.Forms.Panel();
            this.SettingsPanel = new System.Windows.Forms.Panel();
            this.MoviesPanel = new System.Windows.Forms.Panel();
            this.ReservationPanel = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel22 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label10 = new System.Windows.Forms.Label();
            this.panel7 = new System.Windows.Forms.Panel();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.roundLabel2 = new RoundLabel();
            this.New_Reserve = new RoundButton();
            this.roundPanel5 = new RoundPanel();
            this.cornerPanel2 = new CornerPanel();
            this.cornerPanel1 = new CornerPanel();
            this.label24 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.roundPanel2 = new RoundPanel();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.roundPanel3 = new RoundPanel();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.roundPanel4 = new RoundPanel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.roundPanel1 = new RoundPanel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.roundLabel1 = new RoundLabel();
            this.roundButton4 = new RoundButton();
            this.roundButton3 = new RoundButton();
            this.roundButton2 = new RoundButton();
            this.roundButton1 = new RoundButton();
            this.roundButton5 = new RoundButton();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel8.SuspendLayout();
            this.RightMotherPanel.SuspendLayout();
            this.OverviewPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel7.SuspendLayout();
            this.roundPanel5.SuspendLayout();
            this.cornerPanel1.SuspendLayout();
            this.roundPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            this.roundPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.roundPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.roundPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
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
            this.RightMotherPanel.Controls.Add(this.OverviewPanel);
            this.RightMotherPanel.Controls.Add(this.SettingsPanel);
            this.RightMotherPanel.Controls.Add(this.MoviesPanel);
            this.RightMotherPanel.Controls.Add(this.ReservationPanel);
            this.RightMotherPanel.Location = new System.Drawing.Point(0, 102);
            this.RightMotherPanel.Name = "RightMotherPanel";
            this.RightMotherPanel.Size = new System.Drawing.Size(1167, 707);
            this.RightMotherPanel.TabIndex = 8;
            this.RightMotherPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.panel5_Paint);
            // 
            // OverviewPanel
            // 
            this.OverviewPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.OverviewPanel.Controls.Add(this.roundPanel5);
            this.OverviewPanel.Controls.Add(this.roundPanel2);
            this.OverviewPanel.Controls.Add(this.roundPanel3);
            this.OverviewPanel.Controls.Add(this.roundPanel4);
            this.OverviewPanel.Controls.Add(this.roundPanel1);
            this.OverviewPanel.Location = new System.Drawing.Point(3, 3);
            this.OverviewPanel.Name = "OverviewPanel";
            this.OverviewPanel.Size = new System.Drawing.Size(1161, 686);
            this.OverviewPanel.TabIndex = 0;
            // 
            // SettingsPanel
            // 
            this.SettingsPanel.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.SettingsPanel.Location = new System.Drawing.Point(3, 3);
            this.SettingsPanel.Name = "SettingsPanel";
            this.SettingsPanel.Size = new System.Drawing.Size(1161, 686);
            this.SettingsPanel.TabIndex = 3;
            // 
            // MoviesPanel
            // 
            this.MoviesPanel.BackColor = System.Drawing.SystemColors.Highlight;
            this.MoviesPanel.Location = new System.Drawing.Point(3, 3);
            this.MoviesPanel.Name = "MoviesPanel";
            this.MoviesPanel.Size = new System.Drawing.Size(1161, 686);
            this.MoviesPanel.TabIndex = 2;
            // 
            // ReservationPanel
            // 
            this.ReservationPanel.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ReservationPanel.Location = new System.Drawing.Point(3, 3);
            this.ReservationPanel.Name = "ReservationPanel";
            this.ReservationPanel.Size = new System.Drawing.Size(1161, 686);
            this.ReservationPanel.TabIndex = 1;
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
            this.panel6.Controls.Add(this.roundButton4);
            this.panel6.Controls.Add(this.roundButton3);
            this.panel6.Controls.Add(this.roundButton2);
            this.panel6.Controls.Add(this.roundButton1);
            this.panel6.Controls.Add(this.label10);
            this.panel6.Location = new System.Drawing.Point(3, 225);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(261, 518);
            this.panel6.TabIndex = 2;
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
            // roundPanel5
            // 
            this.roundPanel5.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.roundPanel5.BorderColor = System.Drawing.Color.Gray;
            this.roundPanel5.BorderSize = 1;
            this.roundPanel5.Controls.Add(this.cornerPanel2);
            this.roundPanel5.Controls.Add(this.cornerPanel1);
            this.roundPanel5.Location = new System.Drawing.Point(49, 178);
            this.roundPanel5.Name = "roundPanel5";
            this.roundPanel5.Size = new System.Drawing.Size(1070, 478);
            this.roundPanel5.TabIndex = 10;
            // 
            // cornerPanel2
            // 
            this.cornerPanel2.BorderBottom = 1;
            this.cornerPanel2.BorderLeft = 1;
            this.cornerPanel2.BorderRight = 1;
            this.cornerPanel2.BorderSize = 1;
            this.cornerPanel2.BorderTop = 1;
            this.cornerPanel2.Location = new System.Drawing.Point(0, 62);
            this.cornerPanel2.Name = "cornerPanel2";
            this.cornerPanel2.Size = new System.Drawing.Size(1070, 416);
            this.cornerPanel2.TabIndex = 3;
            this.cornerPanel2.TopLeftRadius = 0;
            this.cornerPanel2.TopRightRadius = 0;
            this.cornerPanel2.Paint += new System.Windows.Forms.PaintEventHandler(this.cornerPanel2_Paint);
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
            // roundPanel2
            // 
            this.roundPanel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.roundPanel2.BorderColor = System.Drawing.Color.Gray;
            this.roundPanel2.BorderSize = 1;
            this.roundPanel2.Controls.Add(this.pictureBox5);
            this.roundPanel2.Controls.Add(this.label20);
            this.roundPanel2.Controls.Add(this.label21);
            this.roundPanel2.Controls.Add(this.label22);
            this.roundPanel2.CornerRadius = 15;
            this.roundPanel2.Location = new System.Drawing.Point(870, 32);
            this.roundPanel2.Name = "roundPanel2";
            this.roundPanel2.Size = new System.Drawing.Size(249, 100);
            this.roundPanel2.TabIndex = 9;
            // 
            // pictureBox5
            // 
            this.pictureBox5.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox5.Image")));
            this.pictureBox5.Location = new System.Drawing.Point(12, 33);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(52, 47);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox5.TabIndex = 3;
            this.pictureBox5.TabStop = false;
            // 
            // label20
            // 
            this.label20.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.ForeColor = System.Drawing.Color.Black;
            this.label20.Location = new System.Drawing.Point(75, 69);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(158, 23);
            this.label20.TabIndex = 2;
            this.label20.Text = "For the next screening";
            // 
            // label21
            // 
            this.label21.Font = new System.Drawing.Font("Microsoft YaHei UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.ForeColor = System.Drawing.Color.Black;
            this.label21.Location = new System.Drawing.Point(70, 33);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(163, 32);
            this.label21.TabIndex = 1;
            this.label21.Text = "42";
            this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label22
            // 
            this.label22.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.ForeColor = System.Drawing.Color.Black;
            this.label22.Location = new System.Drawing.Point(72, 6);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(100, 23);
            this.label22.TabIndex = 0;
            this.label22.Text = "Available Seats";
            this.label22.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // roundPanel3
            // 
            this.roundPanel3.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.roundPanel3.BorderColor = System.Drawing.Color.Gray;
            this.roundPanel3.BorderSize = 1;
            this.roundPanel3.Controls.Add(this.pictureBox4);
            this.roundPanel3.Controls.Add(this.label17);
            this.roundPanel3.Controls.Add(this.label18);
            this.roundPanel3.Controls.Add(this.label19);
            this.roundPanel3.CornerRadius = 15;
            this.roundPanel3.Location = new System.Drawing.Point(598, 32);
            this.roundPanel3.Name = "roundPanel3";
            this.roundPanel3.Size = new System.Drawing.Size(249, 100);
            this.roundPanel3.TabIndex = 8;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox4.Image")));
            this.pictureBox4.Location = new System.Drawing.Point(12, 33);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(52, 47);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox4.TabIndex = 3;
            this.pictureBox4.TabStop = false;
            // 
            // label17
            // 
            this.label17.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.ForeColor = System.Drawing.Color.Black;
            this.label17.Location = new System.Drawing.Point(75, 69);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(158, 23);
            this.label17.TabIndex = 2;
            this.label17.Text = "18th Rose - Hall 1";
            // 
            // label18
            // 
            this.label18.Font = new System.Drawing.Font("Microsoft YaHei UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ForeColor = System.Drawing.Color.Black;
            this.label18.Location = new System.Drawing.Point(70, 33);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(163, 32);
            this.label18.TabIndex = 1;
            this.label18.Text = "12:30";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label19
            // 
            this.label19.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.ForeColor = System.Drawing.Color.Black;
            this.label19.Location = new System.Drawing.Point(72, 6);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(100, 23);
            this.label19.TabIndex = 0;
            this.label19.Text = "Next Screening";
            this.label19.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // roundPanel4
            // 
            this.roundPanel4.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.roundPanel4.BorderColor = System.Drawing.Color.Gray;
            this.roundPanel4.BorderSize = 1;
            this.roundPanel4.Controls.Add(this.pictureBox3);
            this.roundPanel4.Controls.Add(this.label14);
            this.roundPanel4.Controls.Add(this.label15);
            this.roundPanel4.Controls.Add(this.label16);
            this.roundPanel4.CornerRadius = 15;
            this.roundPanel4.Location = new System.Drawing.Point(325, 32);
            this.roundPanel4.Name = "roundPanel4";
            this.roundPanel4.Size = new System.Drawing.Size(249, 100);
            this.roundPanel4.TabIndex = 7;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(12, 33);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(52, 47);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 3;
            this.pictureBox3.TabStop = false;
            // 
            // label14
            // 
            this.label14.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.Color.Black;
            this.label14.Location = new System.Drawing.Point(75, 69);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(147, 23);
            this.label14.TabIndex = 2;
            this.label14.Text = "Across 34 Reservation";
            // 
            // label15
            // 
            this.label15.Font = new System.Drawing.Font("Microsoft YaHei UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.Black;
            this.label15.Location = new System.Drawing.Point(70, 33);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(163, 32);
            this.label15.TabIndex = 1;
            this.label15.Text = "78";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label16
            // 
            this.label16.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.Color.Black;
            this.label16.Location = new System.Drawing.Point(72, 6);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(100, 23);
            this.label16.TabIndex = 0;
            this.label16.Text = "Tickets Issued";
            this.label16.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // roundPanel1
            // 
            this.roundPanel1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.roundPanel1.BorderColor = System.Drawing.Color.Gray;
            this.roundPanel1.BorderSize = 1;
            this.roundPanel1.Controls.Add(this.pictureBox2);
            this.roundPanel1.Controls.Add(this.label13);
            this.roundPanel1.Controls.Add(this.label7);
            this.roundPanel1.Controls.Add(this.label1);
            this.roundPanel1.CornerRadius = 15;
            this.roundPanel1.Location = new System.Drawing.Point(52, 32);
            this.roundPanel1.Name = "roundPanel1";
            this.roundPanel1.Size = new System.Drawing.Size(249, 100);
            this.roundPanel1.TabIndex = 4;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(12, 33);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(52, 47);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 3;
            this.pictureBox2.TabStop = false;
            // 
            // label13
            // 
            this.label13.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.Black;
            this.label13.Location = new System.Drawing.Point(75, 69);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(100, 23);
            this.label13.TabIndex = 2;
            this.label13.Text = "Since 9:00 AM ";
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Microsoft YaHei UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(70, 33);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(163, 32);
            this.label7.TabIndex = 1;
            this.label7.Text = "₱ 2300";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(72, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "My Sales Today";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.label1.Click += new System.EventHandler(this.label1_Click);
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
            // roundButton4
            // 
            this.roundButton4.BackColor = System.Drawing.Color.Black;
            this.roundButton4.BorderColor = System.Drawing.Color.Gray;
            this.roundButton4.BorderSize = 1;
            this.roundButton4.CornerRadius = 10;
            this.roundButton4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton4.FlatAppearance.BorderSize = 0;
            this.roundButton4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton4.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roundButton4.ForeColor = System.Drawing.Color.White;
            this.roundButton4.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.roundButton4.Location = new System.Drawing.Point(20, 191);
            this.roundButton4.Name = "roundButton4";
            this.roundButton4.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.roundButton4.Size = new System.Drawing.Size(216, 41);
            this.roundButton4.TabIndex = 11;
            this.roundButton4.Text = "Settings";
            this.roundButton4.UseVisualStyleBackColor = false;
            this.roundButton4.Click += new System.EventHandler(this.roundButton4_Click);
            // 
            // roundButton3
            // 
            this.roundButton3.BackColor = System.Drawing.Color.Black;
            this.roundButton3.BorderColor = System.Drawing.Color.Gray;
            this.roundButton3.BorderSize = 1;
            this.roundButton3.CornerRadius = 10;
            this.roundButton3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton3.FlatAppearance.BorderSize = 0;
            this.roundButton3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton3.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roundButton3.ForeColor = System.Drawing.Color.White;
            this.roundButton3.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.roundButton3.Location = new System.Drawing.Point(20, 144);
            this.roundButton3.Name = "roundButton3";
            this.roundButton3.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.roundButton3.Size = new System.Drawing.Size(216, 41);
            this.roundButton3.TabIndex = 10;
            this.roundButton3.Text = "Movies";
            this.roundButton3.UseVisualStyleBackColor = false;
            this.roundButton3.Click += new System.EventHandler(this.roundButton3_Click);
            // 
            // roundButton2
            // 
            this.roundButton2.BackColor = System.Drawing.Color.Black;
            this.roundButton2.BorderColor = System.Drawing.Color.Gray;
            this.roundButton2.BorderSize = 1;
            this.roundButton2.CornerRadius = 10;
            this.roundButton2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton2.FlatAppearance.BorderSize = 0;
            this.roundButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton2.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roundButton2.ForeColor = System.Drawing.Color.White;
            this.roundButton2.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.roundButton2.Location = new System.Drawing.Point(20, 97);
            this.roundButton2.Name = "roundButton2";
            this.roundButton2.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.roundButton2.Size = new System.Drawing.Size(216, 41);
            this.roundButton2.TabIndex = 9;
            this.roundButton2.Text = "Reservation";
            this.roundButton2.UseVisualStyleBackColor = false;
            this.roundButton2.Click += new System.EventHandler(this.roundButton2_Click);
            // 
            // roundButton1
            // 
            this.roundButton1.BackColor = System.Drawing.Color.Black;
            this.roundButton1.BorderColor = System.Drawing.Color.Gray;
            this.roundButton1.BorderSize = 1;
            this.roundButton1.CornerRadius = 10;
            this.roundButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roundButton1.FlatAppearance.BorderSize = 0;
            this.roundButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.roundButton1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roundButton1.ForeColor = System.Drawing.Color.White;
            this.roundButton1.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.roundButton1.Location = new System.Drawing.Point(20, 50);
            this.roundButton1.Name = "roundButton1";
            this.roundButton1.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.roundButton1.Size = new System.Drawing.Size(216, 41);
            this.roundButton1.TabIndex = 8;
            this.roundButton1.Text = "Overview";
            this.roundButton1.UseVisualStyleBackColor = false;
            this.roundButton1.Click += new System.EventHandler(this.roundButton1_Click);
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
            this.OverviewPanel.ResumeLayout(false);
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
            this.roundPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            this.roundPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.roundPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.roundPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        private void roundButton1_Click(object sender, EventArgs e)
        {
            ShowPage(0);
        }

        private void roundButton3_Click(object sender, EventArgs e)
        {
            ShowPage(2);
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
    }
}
