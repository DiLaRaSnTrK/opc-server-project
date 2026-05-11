namespace UI;

partial class Main
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();

        // ── Kontrol bildirimleri ──────────────────────────────────────────
        panel1 = new Panel();
        pnlStatus = new Panel();
        lblStatusText = new Label();
        lblClock = new Label();
        chkAutoRead = new CheckBox();
        button2 = new Button();
        btnLogout = new Button();
        label1 = new Label();
        button1 = new Button();
        treeView1 = new TreeView();
        dataGridView1 = new DataGridView();
        timer1 = new System.Windows.Forms.Timer(components);
        timerClock = new System.Windows.Forms.Timer(components);
        pictureBox1 = new PictureBox();

        panel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
        SuspendLayout();

        // ────────────────────────────────────────────────────────────────
        // panel1  —  Üst header bar
        // ────────────────────────────────────────────────────────────────
        panel1.BackColor = Color.FromArgb(30, 60, 114);   // lacivert
        panel1.Controls.Add(pictureBox1);
        panel1.Controls.Add(chkAutoRead);
        panel1.Controls.Add(button2);
        panel1.Controls.Add(label1);
        panel1.Controls.Add(button1);
        panel1.Controls.Add(btnLogout);
        panel1.Dock = DockStyle.Top;
        panel1.Location = new Point(0, 0);
        panel1.Name = "panel1";
        panel1.Size = new Size(1315, 66);
        panel1.TabIndex = 2;

        // ── Logo ─────────────────────────────────────────────────────────
        pictureBox1.Image = Properties.Resources.opc_logo;
        pictureBox1.Location = new Point(6, 10);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(44, 44);
        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        pictureBox1.TabIndex = 4;
        pictureBox1.TabStop = false;

        // ── Başlık ───────────────────────────────────────────────────────
        label1.AutoSize = true;
        label1.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
        label1.ForeColor = Color.White;
        label1.Location = new Point(56, 16);
        label1.Name = "label1";
        label1.Size = new Size(160, 30);
        label1.TabIndex = 1;
        label1.Text = "OPC Server";

        // ── Otomatik Okuma checkbox ───────────────────────────────────────
        chkAutoRead.Appearance = Appearance.Button;
        chkAutoRead.AutoSize = true;
        chkAutoRead.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chkAutoRead.FlatStyle = FlatStyle.Flat;
        chkAutoRead.FlatAppearance.BorderColor = Color.FromArgb(80, 130, 200);
        chkAutoRead.ForeColor = Color.White;
        chkAutoRead.Location = new Point(815, 18);
        chkAutoRead.MaximumSize = new Size(150, 30);
        chkAutoRead.Name = "chkAutoRead";
        chkAutoRead.Size = new Size(150, 30);
        chkAutoRead.TabIndex = 3;
        chkAutoRead.Text = "  Otomatik Okuma Başlat  ";
        chkAutoRead.UseVisualStyleBackColor = false;
        chkAutoRead.CheckedChanged += chkAutoRead_CheckedChanged;

        // ── Grafikler butonu ─────────────────────────────────────────────
        button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        button2.BackColor = Color.FromArgb(50, 80, 140);
        button2.FlatStyle = FlatStyle.Flat;
        button2.FlatAppearance.BorderColor = Color.FromArgb(80, 120, 200);
        button2.ForeColor = Color.White;
        button2.Location = new Point(971, 13);
        button2.Name = "button2";
        button2.Size = new Size(110, 40);
        button2.TabIndex = 2;
        button2.Text = "📈  Grafikler";
        button2.UseVisualStyleBackColor = false;
        button2.Click += button2_Click;

        // ── Verileri Getir butonu ─────────────────────────────────────────
        button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        button1.BackColor = Color.FromArgb(50, 80, 140);
        button1.FlatStyle = FlatStyle.Flat;
        button1.FlatAppearance.BorderColor = Color.FromArgb(80, 120, 200);
        button1.ForeColor = Color.White;
        button1.Location = new Point(1087, 13);
        button1.Name = "button1";
        button1.Size = new Size(110, 40);
        button1.TabIndex = 0;
        button1.Text = "🔄  Getir";
        button1.UseVisualStyleBackColor = false;
        button1.Click += button1_Click;

        // ── Çıkış butonu ─────────────────────────────────────────────────
        btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnLogout.BackColor = Color.FromArgb(160, 40, 40);
        btnLogout.FlatStyle = FlatStyle.Flat;
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.ForeColor = Color.White;
        btnLogout.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        btnLogout.Location = new Point(1203, 13);
        btnLogout.Name = "btnLogout";
        btnLogout.Size = new Size(100, 40);
        btnLogout.TabIndex = 5;
        btnLogout.Text = "⏻  Çıkış";
        btnLogout.UseVisualStyleBackColor = false;
        btnLogout.Cursor = Cursors.Hand;
        btnLogout.Click += btnLogout_Click;

        // ────────────────────────────────────────────────────────────────
        // pnlStatus  —  Alt durum çubuğu
        // ────────────────────────────────────────────────────────────────
        pnlStatus.BackColor = Color.FromArgb(22, 44, 84);
        pnlStatus.Dock = DockStyle.Bottom;
        pnlStatus.Height = 26;
        pnlStatus.Name = "pnlStatus";

        lblStatusText.AutoSize = false;
        lblStatusText.Dock = DockStyle.Fill;
        lblStatusText.Font = new Font("Segoe UI", 8f);
        lblStatusText.ForeColor = Color.FromArgb(160, 200, 255);
        lblStatusText.Name = "lblStatusText";
        lblStatusText.Padding = new Padding(8, 0, 0, 0);
        lblStatusText.Text = "Hazır";
        lblStatusText.TextAlign = ContentAlignment.MiddleLeft;

        lblClock.AutoSize = false;
        lblClock.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblClock.Font = new Font("Segoe UI", 8f);
        lblClock.ForeColor = Color.FromArgb(160, 200, 255);
        lblClock.Name = "lblClock";
        lblClock.Size = new Size(160, 26);
        lblClock.Location = new Point(1315 - 165, 0);
        lblClock.Text = DateTime.Now.ToString("dd.MM.yyyy  HH:mm:ss");
        lblClock.TextAlign = ContentAlignment.MiddleRight;
        lblClock.Padding = new Padding(0, 0, 8, 0);

        pnlStatus.Controls.Add(lblStatusText);
        pnlStatus.Controls.Add(lblClock);

        // ── Saat timer ────────────────────────────────────────────────────
        timerClock.Interval = 1000;
        timerClock.Tick += (s, e) =>
        {
            lblClock.Text = DateTime.Now.ToString("dd.MM.yyyy  HH:mm:ss");
        };
        timerClock.Start();

        // ────────────────────────────────────────────────────────────────
        // TreeView
        // ────────────────────────────────────────────────────────────────
        treeView1.BackColor = Color.FromArgb(28, 42, 74);
        treeView1.Dock = DockStyle.Left;
        treeView1.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point, 162);
        treeView1.ForeColor = Color.FromArgb(200, 220, 255);
        treeView1.LineColor = Color.FromArgb(80, 120, 200);
        treeView1.Location = new Point(0, 66);
        treeView1.Name = "treeView1";
        treeView1.Size = new Size(258, 572);
        treeView1.TabIndex = 3;
        treeView1.BorderStyle = BorderStyle.None;
        treeView1.AfterSelect += treeView1_AfterSelect;
        treeView1.NodeMouseDoubleClick += treeView1_NodeMouseDoubleClick_1;

        // ────────────────────────────────────────────────────────────────
        // DataGridView
        // ────────────────────────────────────────────────────────────────
        dataGridView1.AllowUserToAddRows = false;
        dataGridView1.AllowUserToDeleteRows = false;
        dataGridView1.AllowUserToResizeColumns = false;
        dataGridView1.AllowUserToResizeRows = false;
        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridView1.BackgroundColor = Color.FromArgb(245, 248, 255);
        dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dataGridView1.EnableHeadersVisualStyles = false;
        dataGridView1.GridColor = Color.FromArgb(210, 220, 240);
        dataGridView1.ReadOnly = true;
        dataGridView1.RowHeadersVisible = false;
        dataGridView1.RowHeadersWidth = 20;
        dataGridView1.RowTemplate.Height = 44;
        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridView1.Dock = DockStyle.Fill;
        dataGridView1.Location = new Point(258, 66);
        dataGridView1.Margin = new Padding(0);
        dataGridView1.Name = "dataGridView1";
        dataGridView1.TabIndex = 11;

        // Header stili
        dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dataGridViewCellStyle1.BackColor = Color.FromArgb(41, 98, 196);
        dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dataGridViewCellStyle1.ForeColor = Color.White;
        dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(41, 98, 196);
        dataGridViewCellStyle1.SelectionForeColor = Color.White;
        dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
        dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        dataGridView1.ColumnHeadersHeight = 40;
        dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        // Satır stili
        dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dataGridViewCellStyle2.BackColor = Color.White;
        dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
        dataGridViewCellStyle2.ForeColor = Color.FromArgb(30, 40, 60);
        dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(210, 225, 250);
        dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(20, 40, 100);
        dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
        dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;

        // Row header stili
        dataGridViewCellStyle3.BackColor = Color.FromArgb(30, 60, 114);
        dataGridViewCellStyle3.ForeColor = Color.White;
        dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;

        // ── timer1 (veri okuma) ───────────────────────────────────────────
        timer1.Interval = 1000;
        timer1.Tick += timer1_Tick;

        // ────────────────────────────────────────────────────────────────
        // Main Form
        // ────────────────────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1315, 664);
        Controls.Add(dataGridView1);
        Controls.Add(treeView1);
        Controls.Add(pnlStatus);
        Controls.Add(panel1);
        Name = "Main";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "OPC Server";
        FormClosing += Main_FormClosing;
        Load += Main_Load;

        panel1.ResumeLayout(false);
        panel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        ResumeLayout(false);
    }

    // ── Field bildirimleri ────────────────────────────────────────────────
    private Panel panel1;
    private Panel pnlStatus;
    private Label lblStatusText;
    private Label lblClock;
    private Button button1;
    private Button button2;
    private Button btnLogout;
    private TreeView treeView1;
    private Label label1;
    private DataGridView dataGridView1;
    private System.Windows.Forms.Timer timer1;
    private System.Windows.Forms.Timer timerClock;
    private CheckBox chkAutoRead;
    private PictureBox pictureBox1;
}
