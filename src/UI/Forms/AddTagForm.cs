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
            // 1. İsim Kontrolü
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Tag adı zorunludur!", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Register Tipi Kontrolü
            if (cmbRegister.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen bir Register Tipi seçiniz!", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Adres Kontrolü (Negatif olamaz)
            if (!int.TryParse(txtAddress.Text, out int address) || address < 0)
            {
                MessageBox.Show("Geçerli bir Register Adresi giriniz (Pozitif tam sayı)!", "Format Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 4. Veri Tipi Kontrolü
            if (cmbDataType.SelectedIndex == -1 || !Enum.TryParse(cmbDataType.Text, out TagDataType parsedDataType))
            {
                MessageBox.Show("Lütfen bir Veri Tipi (Data Type) seçiniz!", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NewTag = new Tag
            {
                Name = txtName.Text,
                Description = txtDescription.Text,
                Address = address,
                RegisterType = cmbRegister.Text,
                DataType = parsedDataType,
                Value = null,
                LastUpdated = null
            };

            MessageBox.Show("Tag başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }

    }
}
