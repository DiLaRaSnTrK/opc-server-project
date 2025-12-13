using System;
using System.Windows.Forms;
using Core.Models;

namespace UI.Forms
{
    public partial class AddTagForm : Form
    {
        // Form dışından erişilecek veri nesnemiz
        public Tag TagData { get; private set; }

        // Düzenleme modunda mıyız kontrolü (İleride gerekirse diye)
        private bool _isEditMode = false;

        // 1. STANDART CONSTRUCTOR (Yeni Ekleme İçin)
        public AddTagForm()
        {
            InitializeComponent();

            // Register türlerini doldur
            cmbRegister.Items.Add("HoldingRegister");
            cmbRegister.Items.Add("InputRegister");
            cmbRegister.Items.Add("Coil");
            cmbRegister.Items.Add("DiscreteInput");

            // Data türlerini Enum'dan doldur
            cmbDataType.Items.AddRange(Enum.GetNames(typeof(TagDataType)));

            // Yeni boş bir Tag nesnesi oluştur
            TagData = new Tag();
        }

        // 2. DÜZENLEME CONSTRUCTOR'I (Var Olanı Düzenlemek İçin)
        // DİKKAT: ": this()" sayesinde önce yukarıdaki parametresiz constructor çalışır.
        // Böylece InitializeComponent çalışır ve Combobox'lar dolar.
        public AddTagForm(Tag existingTag) : this()
        {
            _isEditMode = true;
            TagData = existingTag; // Gelen veriyi tut

            // Form alanlarını doldur
            txtName.Text = existingTag.Name;
            txtAddress.Text = existingTag.Address.ToString();
            txtDescription.Text = existingTag.Description;

            // Combobox seçimlerini yap
            if (!string.IsNullOrEmpty(existingTag.RegisterType))
            {
                cmbRegister.SelectedItem = existingTag.RegisterType;
            }

            cmbDataType.SelectedItem = existingTag.DataType.ToString();

            // Form başlığını değiştir
            this.Text = "Tag Düzenle";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // --- VALIDASYONLAR ---

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

            // 3. Adres Kontrolü
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

            // --- KAYIT / GÜNCELLEME ---

            // TagData nesnesini formdaki verilerle güncelle
            TagData.Name = txtName.Text;
            TagData.Address = address;
            TagData.RegisterType = cmbRegister.Text;
            TagData.DataType = parsedDataType;
            TagData.Description = txtDescription.Text;

            // Eğer yeni ekliyorsak (Value null ise) varsayılan değerleri koru veya ayarla
            if (!_isEditMode)
            {
                TagData.Value = null;
                TagData.LastUpdated = null;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
