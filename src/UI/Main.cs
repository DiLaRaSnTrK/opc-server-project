using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Core.Models;
using Core.Protocols;
using Core.Database; // DatabaseService için
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

        public Main()
        {
            InitializeComponent();

            db = new DatabaseService(); // system.db kullanılıyor
            CreateContextMenus();

            LoadChannelsFromDb();
            LoadTreeView();
        }

        // ---------------- Context Menüler ----------------
        private void CreateContextMenus()
        {
            menuConnectivity = new ContextMenuStrip();
            menuConnectivity.Items.Add("Yeni Channel Ekle", null, OnAddChannelClick);

            menuChannel = new ContextMenuStrip();
            menuChannel.Items.Add("Yeni Device Ekle", null, OnAddDeviceClick);

            menuDevice = new ContextMenuStrip();
            menuDevice.Items.Add("Yeni Tag Ekle", null, OnAddTagClick);

            treeView1.NodeMouseClick += TreeView1_NodeMouseClick;
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
                    {
                        devNode.Nodes.Add(new TreeNode(tag.Name) { Tag = tag });
                    }
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

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
