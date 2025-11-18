namespace UI.Forms
{
    partial class AddDeviceForm
    {
        private TextBox txtName;
        private TextBox txtIP;
        private TextBox txtPort;
        private TextBox txtSlaveId;
        private TextBox txtDescription;

        private void InitializeComponent()
        {
            txtName = new TextBox();
            txtIP = new TextBox();
            txtPort = new TextBox();
            txtSlaveId = new TextBox();
            txtDescription = new TextBox();
            btnSave = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            SuspendLayout();
            // 
            // txtName
            // 
            txtName.Location = new Point(142, 23);
            txtName.Name = "txtName";
            txtName.PlaceholderText = "Device Name";
            txtName.Size = new Size(294, 27);
            txtName.TabIndex = 0;
            // 
            // txtIP
            // 
            txtIP.Location = new Point(142, 63);
            txtIP.Name = "txtIP";
            txtIP.PlaceholderText = "IP Address";
            txtIP.Size = new Size(294, 27);
            txtIP.TabIndex = 1;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(142, 103);
            txtPort.Name = "txtPort";
            txtPort.PlaceholderText = "Port";
            txtPort.Size = new Size(294, 27);
            txtPort.TabIndex = 2;
            // 
            // txtSlaveId
            // 
            txtSlaveId.Location = new Point(142, 143);
            txtSlaveId.Name = "txtSlaveId";
            txtSlaveId.PlaceholderText = "Slave ID";
            txtSlaveId.Size = new Size(294, 27);
            txtSlaveId.TabIndex = 3;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(142, 183);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.PlaceholderText = "Description";
            txtDescription.Size = new Size(294, 60);
            txtDescription.TabIndex = 4;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(350, 251);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(86, 48);
            btnSave.TabIndex = 5;
            btnSave.Text = "Kaydet";
            btnSave.Click += btnSave_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(35, 30);
            label1.Name = "label1";
            label1.Size = new Size(101, 20);
            label1.TabIndex = 6;
            label1.Text = "Device Name:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(35, 68);
            label2.Name = "label2";
            label2.Size = new Size(81, 20);
            label2.TabIndex = 7;
            label2.Text = "IP Address:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(35, 182);
            label3.Name = "label3";
            label3.Size = new Size(88, 20);
            label3.TabIndex = 8;
            label3.Text = "Description:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(35, 144);
            label4.Name = "label4";
            label4.Size = new Size(66, 20);
            label4.TabIndex = 9;
            label4.Text = "Sleve ID:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(35, 106);
            label5.Name = "label5";
            label5.Size = new Size(38, 20);
            label5.TabIndex = 10;
            label5.Text = "Port:";
            // 
            // AddDeviceForm
            // 
            ClientSize = new Size(481, 320);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnSave);
            Controls.Add(txtName);
            Controls.Add(txtIP);
            Controls.Add(txtPort);
            Controls.Add(txtSlaveId);
            Controls.Add(txtDescription);
            Name = "AddDeviceForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Yeni Device Ekle";
            ResumeLayout(false);
            PerformLayout();
        }
        private Button btnSave;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
    }
}
