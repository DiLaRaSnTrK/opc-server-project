namespace UI.Forms
{
    partial class AddTagForm
    {
        private TextBox txtName;
        private ComboBox cmbRegister;
        private TextBox txtAddress;
        private ComboBox cmbDataType;
        private TextBox txtDescription;

        private void InitializeComponent()
        {
            txtName = new TextBox();
            cmbRegister = new ComboBox();
            txtAddress = new TextBox();
            cmbDataType = new ComboBox();
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
            txtName.Location = new Point(173, 31);
            txtName.Name = "txtName";
            txtName.PlaceholderText = "Tag Name";
            txtName.Size = new Size(294, 27);
            txtName.TabIndex = 0;
            // 
            // cmbRegister
            // 
            cmbRegister.Location = new Point(173, 71);
            cmbRegister.Name = "cmbRegister";
            cmbRegister.Size = new Size(294, 28);
            cmbRegister.TabIndex = 1;
            // 
            // txtAddress
            // 
            txtAddress.Location = new Point(173, 111);
            txtAddress.Name = "txtAddress";
            txtAddress.PlaceholderText = "Register Address";
            txtAddress.Size = new Size(294, 27);
            txtAddress.TabIndex = 2;
            // 
            // cmbDataType
            // 
            cmbDataType.Location = new Point(173, 151);
            cmbDataType.Name = "cmbDataType";
            cmbDataType.Size = new Size(294, 28);
            cmbDataType.TabIndex = 3;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(173, 191);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.PlaceholderText = "Description";
            txtDescription.Size = new Size(294, 60);
            txtDescription.TabIndex = 4;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(381, 257);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(86, 48);
            btnSave.TabIndex = 6;
            btnSave.Text = "Kaydet";
            btnSave.Click += btnSave_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(38, 38);
            label1.Name = "label1";
            label1.Size = new Size(79, 20);
            label1.TabIndex = 7;
            label1.Text = "Tag Name:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(38, 77);
            label2.Name = "label2";
            label2.Size = new Size(101, 20);
            label2.TabIndex = 8;
            label2.Text = "Register Type:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(38, 152);
            label3.Name = "label3";
            label3.Size = new Size(79, 20);
            label3.TabIndex = 9;
            label3.Text = "Data Type:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(38, 116);
            label4.Name = "label4";
            label4.Size = new Size(123, 20);
            label4.TabIndex = 10;
            label4.Text = "Register Address:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(38, 192);
            label5.Name = "label5";
            label5.Size = new Size(88, 20);
            label5.TabIndex = 11;
            label5.Text = "Description:";
            // 
            // AddTagForm
            // 
            ClientSize = new Size(508, 329);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnSave);
            Controls.Add(txtName);
            Controls.Add(cmbRegister);
            Controls.Add(txtAddress);
            Controls.Add(cmbDataType);
            Controls.Add(txtDescription);
            Name = "AddTagForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Yeni Tag Ekle";
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
