using System;
using System.Windows.Forms;
using Core.Models;

namespace UI.Forms
{
    public partial class AddChannelForm : Form
    {
        public Channel NewChannel { get; private set; }

        public AddChannelForm()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1. İsim Kontrolü
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Kanal adı boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Protokol Seçimi Kontrolü
            if (cmbProtocol.SelectedIndex == -1 || !Enum.TryParse<ProtocolType>(cmbProtocol.Text, out var protocol))
            {
                MessageBox.Show("Lütfen listeden geçerli bir protokol seçiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NewChannel = new Channel
            {
                Name = txtName.Text,
                Protocol = protocol,
                Description = txtDescription.Text
            };

            // 3. Başarı Mesajı
            MessageBox.Show("Kanal başarıyla eklendi.", "İşlem Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void AddChannelForm_Load(object sender, EventArgs e)
        {

        }
    }
}
