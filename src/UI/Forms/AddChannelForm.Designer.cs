namespace UI.Forms
{
    partial class AddChannelForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtName;
        private ComboBox cmbProtocol;
        private TextBox txtDescription;
        private Button btnSave;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtName = new TextBox();
            cmbProtocol = new ComboBox();
            txtDescription = new TextBox();
            btnSave = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            SuspendLayout();
            // 
            // txtName
            // 
            txtName.Location = new Point(138, 36);
            txtName.Name = "txtName";
            txtName.PlaceholderText = "Channel Name";
            txtName.Size = new Size(294, 27);
            txtName.TabIndex = 0;
            // 
            // cmbProtocol
            // 
            cmbProtocol.Items.AddRange(new object[] { "ModbusTCP", "ModbusRTU", "DNP3", "MQTT" });
            cmbProtocol.Location = new Point(138, 71);
            cmbProtocol.Name = "cmbProtocol";
            cmbProtocol.Size = new Size(294, 28);
            cmbProtocol.TabIndex = 1;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(138, 107);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.PlaceholderText = "Description";
            txtDescription.Size = new Size(294, 60);
            txtDescription.TabIndex = 2;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(346, 184);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(86, 48);
            btnSave.TabIndex = 3;
            btnSave.Text = "Kaydet";
            btnSave.Click += btnSave_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(23, 39);
            label1.Name = "label1";
            label1.Size = new Size(109, 20);
            label1.TabIndex = 4;
            label1.Text = "Channel Name:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(23, 75);
            label2.Name = "label2";
            label2.Size = new Size(103, 20);
            label2.TabIndex = 5;
            label2.Text = "Protocol Type:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(23, 111);
            label3.Name = "label3";
            label3.Size = new Size(88, 20);
            label3.TabIndex = 6;
            label3.Text = "Description:";
            // 
            // AddChannelForm
            // 
            ClientSize = new Size(462, 260);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtName);
            Controls.Add(cmbProtocol);
            Controls.Add(txtDescription);
            Controls.Add(btnSave);
            Name = "AddChannelForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Yeni Channel Ekle";
            Load += AddChannelForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }
        private Label label1;
        private Label label2;
        private Label label3;
    }
}
