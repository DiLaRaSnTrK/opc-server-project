using System;
using System.Windows.Forms;
using System.Xml.Linq;
using Core.Models;

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
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Device name boş olamaz!");
                return;
            }

            if (!int.TryParse(txtPort.Text, out int port))
            {
                MessageBox.Show("Port geçerli değil!");
                return;
            }

            if (!byte.TryParse(txtSlaveId.Text, out byte slave))
            {
                MessageBox.Show("Slave ID geçerli değil (0–255)!");
                return;
            }

            NewDevice = new Device
            {
                Name = txtName.Text,
                IPAddress = txtIP.Text,
                Port = port,
                SlaveId = slave,
                Description = txtDescription.Text
            };

            DialogResult = DialogResult.OK;
            Close();
        }

    }
}
