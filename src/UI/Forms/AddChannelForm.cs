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
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Channel name boş olamaz!");
                return;
            }

            if (!Enum.TryParse<ProtocolType>(cmbProtocol.Text, out var protocol))
            {
                MessageBox.Show("Geçerli bir protocol seçiniz!");
                return;
            }

            NewChannel = new Channel
            {
                Name = txtName.Text,
                Protocol = protocol,
                Description = txtDescription.Text
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private void AddChannelForm_Load(object sender, EventArgs e)
        {

        }
    }
}
