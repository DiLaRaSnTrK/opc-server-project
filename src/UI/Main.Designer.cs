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

    #region Windows Form Designer generated code

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
        panel1 = new Panel();
        chkAutoRead = new CheckBox();
        button2 = new Button();
        label1 = new Label();
        button1 = new Button();
        treeView1 = new TreeView();
        dataGridView1 = new DataGridView();
        timer1 = new System.Windows.Forms.Timer(components);
        pictureBox1 = new PictureBox();
        panel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
        SuspendLayout();
        // 
        // panel1
        // 
        panel1.BackColor = Color.FromArgb(55, 67, 109);
        panel1.Controls.Add(pictureBox1);
        panel1.Controls.Add(chkAutoRead);
        panel1.Controls.Add(button2);
        panel1.Controls.Add(label1);
        panel1.Controls.Add(button1);
        panel1.Dock = DockStyle.Top;
        panel1.Location = new Point(0, 0);
        panel1.Name = "panel1";
        panel1.Size = new Size(1315, 66);
        panel1.TabIndex = 2;
        // 
        // chkAutoRead
        // 
        chkAutoRead.Appearance = Appearance.Button;
        chkAutoRead.AutoSize = true;
        chkAutoRead.ForeColor = SystemColors.ActiveCaptionText;
        chkAutoRead.Location = new Point(872, 20);
        chkAutoRead.MaximumSize = new Size(200, 30);
        chkAutoRead.Name = "chkAutoRead";
        chkAutoRead.Size = new Size(200, 30);
        chkAutoRead.TabIndex = 3;
        chkAutoRead.Text = "   Otomatik Okuma Başlat   ";
        chkAutoRead.UseVisualStyleBackColor = true;
        chkAutoRead.CheckedChanged += chkAutoRead_CheckedChanged;
        // 
        // button2
        // 
        button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        button2.BackColor = Color.FromArgb(55, 67, 109);
        button2.ForeColor = SystemColors.ButtonHighlight;
        button2.Location = new Point(1102, 10);
        button2.Name = "button2";
        button2.Size = new Size(79, 50);
        button2.TabIndex = 2;
        button2.Text = "Grafikler";
        button2.UseVisualStyleBackColor = false;
        button2.Click += button2_Click;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Font = new Font("Showcard Gothic", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
        label1.ForeColor = SystemColors.ButtonHighlight;
        label1.Location = new Point(55, 13);
        label1.Name = "label1";
        label1.Size = new Size(203, 37);
        label1.TabIndex = 1;
        label1.Text = "OPC Server";
        // 
        // button1
        // 
        button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        button1.Location = new Point(1211, 10);
        button1.Name = "button1";
        button1.Size = new Size(79, 50);
        button1.TabIndex = 0;
        button1.Text = "Verileri\r\nGetir";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // treeView1
        // 
        treeView1.BackColor = Color.FromArgb(41, 50, 81);
        treeView1.Dock = DockStyle.Left;
        treeView1.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 162);
        treeView1.ForeColor = SystemColors.Window;
        treeView1.LineColor = Color.White;
        treeView1.Location = new Point(0, 66);
        treeView1.Name = "treeView1";
        treeView1.Size = new Size(258, 598);
        treeView1.TabIndex = 3;
        treeView1.AfterSelect += treeView1_AfterSelect;
        treeView1.NodeMouseDoubleClick += treeView1_NodeMouseDoubleClick_1;
        // 
        // dataGridView1
        // 
        dataGridView1.AllowUserToAddRows = false;
        dataGridView1.AllowUserToDeleteRows = false;
        dataGridView1.AllowUserToResizeColumns = false;
        dataGridView1.AllowUserToResizeRows = false;
        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridView1.BackgroundColor = Color.White;
        dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.RaisedHorizontal;
        dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dataGridViewCellStyle1.BackColor = Color.SteelBlue;
        dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
        dataGridViewCellStyle1.ForeColor = SystemColors.ButtonHighlight;
        dataGridViewCellStyle1.SelectionBackColor = Color.SteelBlue;
        dataGridViewCellStyle1.SelectionForeColor = SystemColors.ButtonHighlight;
        dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
        dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        dataGridView1.ColumnHeadersHeight = 40;
        dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dataGridViewCellStyle2.BackColor = Color.White;
        dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
        dataGridViewCellStyle2.ForeColor = Color.Black;
        dataGridViewCellStyle2.SelectionBackColor = Color.LightSteelBlue;
        dataGridViewCellStyle2.SelectionForeColor = SystemColors.Desktop;
        dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
        dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
        dataGridView1.Dock = DockStyle.Fill;
        dataGridView1.EnableHeadersVisualStyles = false;
        dataGridView1.GridColor = SystemColors.HighlightText;
        dataGridView1.Location = new Point(258, 66);
        dataGridView1.Margin = new Padding(3, 3, 30, 3);
        dataGridView1.Name = "dataGridView1";
        dataGridView1.ReadOnly = true;
        dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle3.BackColor = SystemColors.ControlDarkDark;
        dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
        dataGridViewCellStyle3.ForeColor = SystemColors.ButtonHighlight;
        dataGridViewCellStyle3.SelectionBackColor = SystemColors.ControlLight;
        dataGridViewCellStyle3.SelectionForeColor = SystemColors.ControlText;
        dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
        dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
        dataGridView1.RowHeadersVisible = false;
        dataGridView1.RowHeadersWidth = 20;
        dataGridView1.RowTemplate.Height = 50;
        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridView1.Size = new Size(1057, 598);
        dataGridView1.TabIndex = 11;
        // 
        // timer1
        // 
        timer1.Interval = 1000;
        timer1.Tick += timer1_Tick;
        // 
        // pictureBox1
        // 
        pictureBox1.Image = Properties.Resources.opc_logo;
        pictureBox1.Location = new Point(3, 10);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(51, 46);
        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        pictureBox1.TabIndex = 4;
        pictureBox1.TabStop = false;
        // 
        // Main
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1315, 664);
        Controls.Add(dataGridView1);
        Controls.Add(treeView1);
        Controls.Add(panel1);
        Name = "Main";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Main Page";
        FormClosing += Main_FormClosing;
        Load += Main_Load;
        panel1.ResumeLayout(false);
        panel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel panel1;
    private Button button1;
    private TreeView treeView1;
    private Label label1;
    private Button button2;
    private DataGridView dataGridView1;
    private System.Windows.Forms.Timer timer1;
    private CheckBox chkAutoRead;
    private PictureBox pictureBox1;
}
