using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Windows.Forms;
using Core.Database; // DatabaseService
using Core.Models;
using Core.Protocols;
using UI.Forms;


namespace UI
{
    public partial class Main : Form
    {
        private List<Channel> channelsList;
        private DatabaseService db;
        
        private ContextMenuStrip menuConnectivity;
        private ContextMenuStrip menuChannel;
        private ContextMenuStrip menuDevice;
        private ContextMenuStrip menuTag;

        public Main()
        {
            InitializeComponent();
            db = new DatabaseService();
            CreateContextMenus();

            LoadChannelsFromDb();
            LoadTreeView();
        }

        // ---------------- Context Menüler ----------------
        private void CreateContextMenus()
        {
            var addIcon = Properties.Resources.add;
            var deleteIcon = Properties.Resources.delete;

            // Connectivity menüsü (Channel ekleme/silme)
            menuConnectivity = new ContextMenuStrip();
            var addChannel = new ToolStripMenuItem("Yeni Channel Ekle", addIcon, OnAddChannelClick);

            menuConnectivity.Items.Add(addChannel);


            // Channel menüsü (Device ekleme/silme)
            menuChannel = new ContextMenuStrip();
            var addDevice = new ToolStripMenuItem("Yeni Device Ekle", addIcon, OnAddDeviceClick);
            var deleteChannel = new ToolStripMenuItem("Channel Sil", deleteIcon, OnDeleteChannelClick);

            menuChannel.Items.Add(addDevice);
            menuChannel.Items.Add(deleteChannel);

            // Device menüsü (Tag ekleme/silme)
            menuDevice = new ContextMenuStrip();
            var addTag = new ToolStripMenuItem("Yeni Tag Ekle", addIcon, OnAddTagClick);
            var deleteDevice = new ToolStripMenuItem("Device Sil", deleteIcon, OnDeleteDeviceClick); // Device silme
            menuDevice.Items.Add(addTag);
            menuDevice.Items.Add(deleteDevice);

            // Tag menüsü (sadece silme)
            menuTag = new ContextMenuStrip();
            var deleteOnlyTag = new ToolStripMenuItem("Tag Sil", deleteIcon, OnDeleteTagClick);
            menuTag.Items.Add(deleteOnlyTag);

            treeView1.NodeMouseClick += TreeView1_NodeMouseClick;
        }

        // ---------------- Sağ Tık -> Silme ----------------
        private void OnDeleteChannelClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode?.Tag is Channel ch)
            {
                if (MessageBox.Show($"'{ch.Name}' kanalını silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    db.DeleteChannel(ch.ChannelId);
                    channelsList.Remove(ch);
                    LoadTreeView();
                }
            }
        }

        private void OnDeleteDeviceClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode?.Tag is Device dev)
            {
                if (MessageBox.Show($"'{dev.Name}' cihazını silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    db.DeleteDevice(dev.DeviceId);
                    var parentChannel = treeView1.SelectedNode.Parent?.Tag as Channel;
                    parentChannel?.Devices.Remove(dev);
                    LoadTreeView();
                }
            }
        }

        private void OnDeleteTagClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode?.Tag is Tag tag)
            {
                if (MessageBox.Show($"'{tag.Name}' tagini silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    db.DeleteTag(tag.TagId);
                    var parentDevice = treeView1.SelectedNode.Parent?.Tag as Device;
                    parentDevice?.Tags.Remove(tag);
                    LoadTreeView();
                }
            }
        }

        private void TreeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;

            if (e.Button == MouseButtons.Right)
            {
                if (e.Node.Text == "Connectivity")
                    menuConnectivity.Show(treeView1, e.Location);
                else if (e.Node.Tag is Channel)
                    menuChannel.Show(treeView1, e.Location);
                else if (e.Node.Tag is Device)
                    menuDevice.Show(treeView1, e.Location);
                else if (e.Node.Tag is Tag)
                    menuTag.Show(treeView1, e.Location);
            }
        }

        // ---------------- DB’den veri yükle ----------------
        private void LoadChannelsFromDb()
        {
            channelsList = db.GetChannels();
            foreach (var ch in channelsList)
            {
                ch.Devices = db.GetDevicesByChannelId(ch.ChannelId);
                foreach (var dev in ch.Devices)
                {
                    dev.Tags = db.GetTagsByDeviceId(dev.DeviceId);
                }
            }
        }

        // ---------------- TreeView ----------------
        private void LoadTreeView()
        {
            treeView1.Nodes.Clear();
            var root = new TreeNode("Connectivity");

            foreach (var ch in channelsList)
            {
                var chNode = new TreeNode(ch.Name) { Tag = ch };
                foreach (var dev in ch.Devices)
                {
                    var devNode = new TreeNode(dev.Name) { Tag = dev };
                    foreach (var tag in dev.Tags)
                        devNode.Nodes.Add(new TreeNode(tag.Name) { Tag = tag });
                    chNode.Nodes.Add(devNode);
                }
                root.Nodes.Add(chNode);
            }

            treeView1.Nodes.Add(root);
            treeView1.ExpandAll();
        }

        // ---------------- TreeView Selection ----------------
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            if (e.Node.Text == "Connectivity")
            {
                dataGridView1.Columns.Add("Name", "Channel Adı");
                dataGridView1.Columns.Add("Protocol", "Protokol");
                dataGridView1.Columns.Add("Desc", "Açıklama");

                foreach (var ch in channelsList)
                    dataGridView1.Rows.Add(ch.Name, ch.Protocol, ch.Description);
            }
            else if (e.Node.Tag is Channel ch)
            {
                dataGridView1.Columns.Add("DevName", "Cihaz Adı");
                dataGridView1.Columns.Add("IP", "IP Adresi");
                dataGridView1.Columns.Add("Port", "Port");
                dataGridView1.Columns.Add("Slave", "Slave ID");
                dataGridView1.Columns.Add("TagCount", "Tag Sayısı");
                dataGridView1.Columns.Add("Desc", "Açıklama");

                foreach (var dev in ch.Devices)
                    dataGridView1.Rows.Add(dev.Name, dev.IPAddress, dev.Port, dev.SlaveId, dev.Tags.Count, dev.Description);
            }
            else if (e.Node.Tag is Device dev)
            {
                dataGridView1.Columns.Add("TagName", "Tag Adı");
                dataGridView1.Columns.Add("RegType", "Register");
                dataGridView1.Columns.Add("Address", "Adres");
                dataGridView1.Columns.Add("Type", "Veri Türü");
                dataGridView1.Columns.Add("Value", "Değer");
                dataGridView1.Columns.Add("Updated", "Son Okuma");
                dataGridView1.Columns.Add("Desc", "Açıklama");

                foreach (var tag in dev.Tags)
                    dataGridView1.Rows.Add(tag.Name, tag.RegisterType, tag.Address, tag.DataType, tag.Value, tag.LastUpdated, tag.Description);
            }
            else if (e.Node.Tag is Tag tag)
            {
                dataGridView1.Columns.Add("Prop", "Özellik");
                dataGridView1.Columns.Add("Val", "Değer");

                dataGridView1.Rows.Add("Tag Adı", tag.Name);
                dataGridView1.Rows.Add("Register", tag.RegisterType);
                dataGridView1.Rows.Add("Adres", tag.Address);
                dataGridView1.Rows.Add("Veri Türü", tag.DataType);
                dataGridView1.Rows.Add("Değer", tag.Value);
                dataGridView1.Rows.Add("Son Okuma", tag.LastUpdated);
                dataGridView1.Rows.Add("Açıklama", tag.Description);
            }
        }

        // ---------------- Sağ Tık -> Ekleme ----------------
        private void OnAddChannelClick(object sender, EventArgs e)
        {
            var f = new AddChannelForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                var newCh = f.NewChannel;
                newCh.ChannelId = db.AddChannel(newCh);
                channelsList.Add(newCh);
                LoadTreeView();
            }
        }

        private void OnAddDeviceClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Channel ch)
            {
                var f = new AddDeviceForm();
                if (f.ShowDialog() == DialogResult.OK)
                {
                    var newDev = f.NewDevice;
                    newDev.ChannelId = ch.ChannelId;
                    newDev.DeviceId = db.AddDevice(newDev);
                    ch.Devices.Add(newDev);
                    LoadTreeView();
                }
            }
        }

        private void OnAddTagClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Device dev)
            {
                var f = new AddTagForm();
                if (f.ShowDialog() == DialogResult.OK)
                {
                    var newTag = f.NewTag;
                    newTag.DeviceId = dev.DeviceId;
                    newTag.TagId = db.AddTag(newTag);
                    dev.Tags.Add(newTag);
                    LoadTreeView();
                }
            }
        }

        // ---------------- Modbus Okuma (button1_Click) ----------------
        private async void button1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                MessageBox.Show("Lütfen bir cihaz veya tag seçin!");
                return;
            }

            // ===================== DEVICE OKUMA =====================
            if (treeView1.SelectedNode.Tag is Device selectedDevice)
            {
                var modbus = new ModbusClientWrapper(selectedDevice);

                try
                {
                    await modbus.ConnectAsync();

                    foreach (var tag in selectedDevice.Tags)
                    {
                        var result = await modbus.ReadTagAsync(tag);

                        if (result.Success)
                        {
                            var val = result.Values[0];
                            tag.Value = val;
                            tag.LastUpdated = DateTime.Now;

                            db.UpdateTagValue(tag, val);

                            // DGV güncelle
                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                if (row.Cells[0].Value?.ToString() == tag.Name) // TagName
                                {
                                    row.Cells["Value"].Value = val;
                                    row.Cells["Updated"].Value = tag.LastUpdated;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Modbus bağlantı hatası: " + ex.Message);
                }
                finally
                {
                    await modbus.DisconnectAsync();
                }

                return;
            }

            // ===================== TAG OKUMA =====================
            if (treeView1.SelectedNode.Tag is Tag selectedTag)
            {
                var parentDevice = treeView1.SelectedNode.Parent?.Tag as Device;

                var modbus = new ModbusClientWrapper(parentDevice);

                try
                {
                    await modbus.ConnectAsync();
                    var result = await modbus.ReadTagAsync(selectedTag);

                    if (result.Success)
                    {
                        var val = result.Values[0];
                        selectedTag.Value = val;
                        selectedTag.LastUpdated = DateTime.Now;

                        db.UpdateTagValue(selectedTag, val);
                        //db.InsertHistory(selectedTag.TagId, val, selectedTag.LastUpdated.Value);


                        // DGV güncelle
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.Cells[0].Value?.ToString() == "Değer")
                                row.Cells[1].Value = val;

                            if (row.Cells[0].Value?.ToString() == "Son Okuma")
                                row.Cells[1].Value = selectedTag.LastUpdated;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Modbus bağlantı hatası: " + ex.Message);
                }
                finally
                {
                    await modbus.DisconnectAsync();
                }
                return;
            }

            MessageBox.Show("Lütfen bir cihaz veya tag seçin!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode?.Tag is Tag tag)
            {
                var chartForm = new HistoryChartForm(tag);
                chartForm.Show();
            }
            else
            {
                MessageBox.Show("Lütfen bir tag seçin!");
            }
            /*if (treeView1.SelectedNode == null)
            {
                MessageBox.Show("Lütfen bir cihaz veya tag seçin!");
                return;
            }
            // ===================== DEVICE GEÇMİŞ GÖSTERME =====================
            if (treeView1.SelectedNode.Tag is Device selectedDevice)
            {
                var historyForm = new HistoryForm(selectedDevice);
                historyForm.ShowDialog();
                return;
            }
            // ===================== TAG GEÇMİŞ GÖSTERME =====================
            if (treeView1.SelectedNode.Tag is Tag selectedTag)
            {
                var historyForm = new HistoryForm(selectedTag);
                historyForm.ShowDialog();
                return;
            }
            MessageBox.Show("Lütfen bir cihaz veya tag seçin!");*/
        }
    }
}
