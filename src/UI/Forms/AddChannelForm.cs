using System;
using System.Windows.Forms;
using Core.Models;

namespace UI.Forms
{
    public partial class AddChannelForm : Form
    {
        public Channel ChannelData { get; private set; } // İsmini genel yaptım
        private bool _isEditMode = false;

        // 1. Boş Constructor (Ekleme için)
        public AddChannelForm()
        {
            InitializeComponent();
            ChannelData = new Channel(); // Yeni boş nesne
        }

        // 2. Dolu Constructor (Düzenleme için)
        public AddChannelForm(Channel existingChannel)
        {
            InitializeComponent();
            _isEditMode = true;
            ChannelData = existingChannel;

            // Formu doldur
            txtName.Text = existingChannel.Name;
            txtDescription.Text = existingChannel.Description;
            cmbProtocol.Text = existingChannel.Protocol.ToString(); // Enum'ı string'e çevirip seç

            this.Text = "Kanal Düzenle"; // Başlığı değiştir
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validasyonlar (Aynen kalsın)
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("İsim giriniz"); return; }
            if (!Enum.TryParse<ProtocolType>(cmbProtocol.Text, out var protocol)) { MessageBox.Show("Protokol seçiniz"); return; }

            // Nesneyi güncelle
            ChannelData.Name = txtName.Text;
            ChannelData.Protocol = protocol;
            ChannelData.Description = txtDescription.Text;

            DialogResult = DialogResult.OK;
            Close();
        }


        private void AddChannelForm_Load(object sender, EventArgs e)
        {

        }
    }
}
