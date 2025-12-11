using System;
using System.Windows.Forms;
using System.Xml.Linq;
using Core.Models;
using System.Net;

namespace UI.Forms
{
    public partial class AddDeviceForm : Form
    {
        public Device NewDevice { get; private set; }

        public AddDeviceForm()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1. İsim Kontrolü
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Lütfen bir cihaz adı giriniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. IP Adresi Format Kontrolü (YENİ)
            // IPAddress.TryParse metodu, girilen metnin geçerli bir IP olup olmadığını kontrol eder.
            if (string.IsNullOrWhiteSpace(txtIP.Text) || !IPAddress.TryParse(txtIP.Text, out _))
            {
                MessageBox.Show("Geçersiz IP Adresi formatı! (Örn: 192.168.1.10)", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3. Port Kontrolü
            if (!int.TryParse(txtPort.Text, out int port) || port <= 0 || port > 65535)
            {
                MessageBox.Show("Port numarası 1 ile 65535 arasında olmalıdır!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 4. Slave ID Kontrolü
            if (!byte.TryParse(txtSlaveId.Text, out byte slave))
            {
                MessageBox.Show("Slave ID 0 ile 255 arasında olmalıdır!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Nesneyi Oluştur
            NewDevice = new Device
            {
                Name = txtName.Text,
                IPAddress = txtIP.Text,
                Port = port,
                SlaveId = slave,
                Description = txtDescription.Text
            };

            // 5. Başarı Mesajı (YENİ)
            MessageBox.Show("Cihaz başarıyla oluşturuldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }

    }
}
