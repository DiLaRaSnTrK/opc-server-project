using System;
using System.Windows.Forms;
using Core.Models;

namespace UI.Forms
{
    public partial class AddTagForm : Form
    {
        public Tag NewTag { get; private set; }

        public AddTagForm()
        {
            InitializeComponent();

            // Register türlerini manuel ekliyoruz (string olduğu için enum'a gerek yok)
            cmbRegister.Items.Add("HoldingRegister");
            cmbRegister.Items.Add("InputRegister");
            cmbRegister.Items.Add("Coil");
            cmbRegister.Items.Add("DiscreteInput");

            // DataType enum ile dolduruluyor
            cmbDataType.Items.AddRange(Enum.GetNames(typeof(TagDataType)));
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Tag adı zorunlu!");
                return;
            }

            if (!int.TryParse(txtAddress.Text, out int address))
            {
                MessageBox.Show("Adres sayısal olmalı!");
                return;
            }

            if (string.IsNullOrWhiteSpace(cmbRegister.Text))
            {
                MessageBox.Show("Bir Register Type seçiniz!");
                return;
            }

            if (!Enum.TryParse(cmbDataType.Text, out TagDataType parsedDataType))
            {
                MessageBox.Show("Bir Data Type seçiniz!");
                return;
            }

            NewTag = new Tag
            {
                Name = txtName.Text,
                Description = txtDescription.Text,
                Address = address,
                RegisterType = cmbRegister.Text,        // STRING ✔
                DataType = parsedDataType,              // ENUM ✔
                Value = null,                           // Cihazdan okunacak
                LastUpdated = null                      // İlk başta boş
            };

            DialogResult = DialogResult.OK;
            Close();
        }

    }
}
