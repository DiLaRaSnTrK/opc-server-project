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
        DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
        panel1 = new Panel();
        label1 = new Label();
        button1 = new Button();
        treeView1 = new TreeView();
        dataGridView1 = new DataGridView();
        panel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
        SuspendLayout();
        // 
        // panel1
        // 
        panel1.BackColor = Color.FromArgb(55, 67, 109);
        panel1.Controls.Add(label1);
        panel1.Controls.Add(button1);
        panel1.Dock = DockStyle.Top;
        panel1.Location = new Point(0, 0);
        panel1.Name = "panel1";
        panel1.Size = new Size(1315, 66);
        panel1.TabIndex = 2;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Font = new Font("Showcard Gothic", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
        label1.ForeColor = SystemColors.ButtonHighlight;
        label1.Location = new Point(26, 15);
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
        dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dataGridViewCellStyle4.BackColor = Color.SteelBlue;
        dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F);
        dataGridViewCellStyle4.ForeColor = SystemColors.ButtonHighlight;
        dataGridViewCellStyle4.SelectionBackColor = Color.SteelBlue;
        dataGridViewCellStyle4.SelectionForeColor = SystemColors.ButtonHighlight;
        dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
        dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
        dataGridView1.ColumnHeadersHeight = 40;
        dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dataGridViewCellStyle5.BackColor = Color.White;
        dataGridViewCellStyle5.Font = new Font("Segoe UI", 9F);
        dataGridViewCellStyle5.ForeColor = Color.Black;
        dataGridViewCellStyle5.SelectionBackColor = Color.LightSteelBlue;
        dataGridViewCellStyle5.SelectionForeColor = SystemColors.Desktop;
        dataGridViewCellStyle5.WrapMode = DataGridViewTriState.False;
        dataGridView1.DefaultCellStyle = dataGridViewCellStyle5;
        dataGridView1.Dock = DockStyle.Fill;
        dataGridView1.EnableHeadersVisualStyles = false;
        dataGridView1.GridColor = SystemColors.HighlightText;
        dataGridView1.Location = new Point(258, 66);
        dataGridView1.Margin = new Padding(3, 3, 30, 3);
        dataGridView1.Name = "dataGridView1";
        dataGridView1.ReadOnly = true;
        dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle6.BackColor = SystemColors.ControlDarkDark;
        dataGridViewCellStyle6.Font = new Font("Segoe UI", 9F);
        dataGridViewCellStyle6.ForeColor = SystemColors.ButtonHighlight;
        dataGridViewCellStyle6.SelectionBackColor = SystemColors.ControlLight;
        dataGridViewCellStyle6.SelectionForeColor = SystemColors.ControlText;
        dataGridViewCellStyle6.WrapMode = DataGridViewTriState.True;
        dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
        dataGridView1.RowHeadersVisible = false;
        dataGridView1.RowHeadersWidth = 20;
        dataGridView1.RowTemplate.Height = 50;
        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridView1.Size = new Size(1057, 598);
        dataGridView1.TabIndex = 9;
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
        Text = "Main Page";
        panel1.ResumeLayout(false);
        panel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel panel1;
    private Button button1;
    private TreeView treeView1;
    private DataGridView dataGridView1;
    private Label label1;
}
