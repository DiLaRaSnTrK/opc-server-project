// <copyright file="Main.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>


namespace UI
{
    // Aşama 7 — Serilog yapılandırılmış loglama
    using Core.Database;
    using Core.Models;
    using Core.Protocols;
    using Infrastructure.OPC;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using UI.Forms;
    public partial class Main : Form
    {
        private List<Channel> channelsList;
        private DatabaseService db;

        private ContextMenuStrip menuConnectivity;
        private ContextMenuStrip menuChannel;
        private ContextMenuStrip menuDevice;
        private ContextMenuStrip menuTag;

        private OpcTagUpdater _opcTagUpdater;
        private OpcServerService _opcServer;

        // ── SERILOG LOGGER (T-06) ─────────────────────────────────────────────
        private static readonly ILoggerFactory AppLoggerFactory = BuildLoggerFactory();

        private static ILoggerFactory BuildLoggerFactory()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/opc-server-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            return Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                builder.AddSerilog(Log.Logger));
        }

        public Main()
        {
            InitializeComponent();

            var opcLogger = AppLoggerFactory.CreateLogger<OpcServerService>();

            _opcTagUpdater = new OpcTagUpdater();
            db = new DatabaseService("system.db", _opcTagUpdater);
            _opcServer = new OpcServerService(_opcTagUpdater, db, opcLogger);
            _ = _opcServer.StartAsync(); // fire and forget

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
                // 1. ONAY İSTE
                var result = MessageBox.Show(
                    $"'{ch.Name}' kanalını silmek istediğinize emin misiniz?\n\n" +
                    "DİKKAT: Bu kanala bağlı TÜM CİHAZLAR ve TAGLER de silinecektir!",
                    "Kritik Silme Onayı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    // 2. SİL
                    db.DeleteChannel(ch.ChannelId);
                    channelsList.Remove(ch);
                    LoadTreeView();

                    // 3. BİLDİRİM VER (YENİ)
                    MessageBox.Show($"'{ch.Name}' kanalı ve altındaki tüm veriler başarıyla silindi.", "İşlem Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void OnDeleteDeviceClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode?.Tag is Device dev)
            {
                // 1. ONAY İSTE
                var result = MessageBox.Show(
                    $"'{dev.Name}' cihazını silmek istediğinize emin misiniz?\n\n" +
                    "Bu cihaza ait tüm tagler ve geçmiş veriler silinecektir.",
                    "Cihaz Silme Onayı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    // 2. BAĞLANTIYI KES VE SİL
                    if (_deviceConnections.ContainsKey(dev.DeviceId))
                    {
                        try { _deviceConnections[dev.DeviceId].Dispose(); } catch { }
                        _deviceConnections.Remove(dev.DeviceId);
                    }

                    db.DeleteDevice(dev.DeviceId);
                    var parentChannel = treeView1.SelectedNode.Parent?.Tag as Channel;
                    parentChannel?.Devices.Remove(dev);
                    LoadTreeView();

                    // 3. BİLDİRİM VER (YENİ)
                    MessageBox.Show($"'{dev.Name}' cihazı başarıyla silindi.", "İşlem Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void OnDeleteTagClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode?.Tag is Tag tag)
            {
                // 1. ONAY İSTE
                var result = MessageBox.Show(
                    $"'{tag.Name}' tagini silmek istediğinize emin misiniz?",
                    "Tag Silme Onayı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    db.DeleteTag(tag.TagId);
                    var parentDevice = treeView1.SelectedNode.Parent?.Tag as Device;
                    parentDevice?.Tags.Remove(tag);
                    LoadTreeView();

                    // ✅ OPC node'u da kaldır
                    _opcTagUpdater.RemoveTagNode(tag.Name);

                    MessageBox.Show($"'{tag.Name}' tagi başarıyla silindi.");
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
                var newCh = f.ChannelData;
                newCh.ChannelId = db.AddChannel(newCh);
                channelsList.Add(newCh);
                LoadTreeView();
                MessageBox.Show("Kanal eklendi.");
            }
        }

        private void OnAddDeviceClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Channel ch)
            {
                var f = new AddDeviceForm();
                if (f.ShowDialog() == DialogResult.OK)
                {
                    var newDev = f.DeviceData;
                    newDev.ChannelId = ch.ChannelId;
                    newDev.DeviceId = db.AddDevice(newDev);
                    ch.Devices.Add(newDev);
                    LoadTreeView();
                    MessageBox.Show("Cihaz eklendi.");
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
                    var newTag = f.TagData;
                    newTag.DeviceId = dev.DeviceId;
                    newTag.TagId = db.AddTag(newTag);
                    dev.Tags.Add(newTag);
                    LoadTreeView();

                    // ✅ OPC node'u runtime'da ekle — restart gerekmez
                    _opcTagUpdater.AddTagNode(newTag);

                    MessageBox.Show("Tag eklendi.");
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
                MessageBox.Show("Lütfen sol menüden bir tag seçin!");
            }
        }


        private Dictionary<int, ModbusClientWrapper> _deviceConnections = new Dictionary<int, ModbusClientWrapper>();
        private bool _isScanning = false;

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (_isScanning) return; // Çakışmayı önle
            _isScanning = true;

            try
            {
                if (channelsList == null) return;

                foreach (var channel in channelsList)
                {
                    foreach (var device in channel.Devices)
                    {
                        // 1. Bu cihaz için açık bir bağlantımız var mı? Yoksa oluştur.
                        if (!_deviceConnections.ContainsKey(device.DeviceId))
                        {
                            _deviceConnections[device.DeviceId] = new ModbusClientWrapper(device);
                        }

                        var modbusClient = _deviceConnections[device.DeviceId];

                        try
                        {
                            // 2. Bağlı değilse bağlanmayı dene
                            if (!modbusClient.IsConnected)
                            {
                                await modbusClient.ConnectAsync();
                            }

                            // 3. Tagleri Oku
                            foreach (var tag in device.Tags)
                            {
                                // Hata alırsak program patlamasın diye try-catch okuma seviyesinde de olsun
                                try
                                {
                                    var result = await modbusClient.ReadTagAsync(tag);
                                    if (result.Success)
                                    {
                                        var val = result.Values[0];
                                        tag.Value = val;
                                        tag.LastUpdated = DateTime.Now;
                                        db.UpdateTagValue(tag, val);
                                    }
                                }
                                catch (Exception)
                                {
                                    // Tek bir tag okunamazsa diğerine geç, döngüyü kırma
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Cihaza bağlanamazsa:
                            // 1. Bağlantıyı listeden sil (Bir sonraki turda temiz başlangıç yapsın)
                            if (_deviceConnections.ContainsKey(device.DeviceId))
                            {
                                try { _deviceConnections[device.DeviceId].Dispose(); } catch { }
                                _deviceConnections.Remove(device.DeviceId);
                            }

                            System.Diagnostics.Debug.WriteLine($"[HATA] {device.Name} cihazına erişilemedi: {ex.Message}");

                        }
                    }
                }

                RefreshDataGridView();
            }
            finally
            {
                _isScanning = false;
            }
        }

        private void chkAutoRead_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoRead.Checked)
            {
                timer1.Start();
                chkAutoRead.Text = "Otomatik Okuma: AKTİF";
                chkAutoRead.BackColor = Color.LightGreen; // Görsel geri bildirim
            }
            else
            {
                timer1.Stop();
                chkAutoRead.Text = "Otomatik Okuma Başlat";
                chkAutoRead.BackColor = Color.Transparent;
            }
        }

        private void RefreshDataGridView()
        {
            // Eğer DataGridView'da o an bir liste varsa, değerleri güncelle
            if (dataGridView1.Rows.Count > 0)
            {
                // Seçili olan node'a göre güncelleme yap
                // Basitçe tüm satırları gezip Tag nesnesindeki güncel değeri hücreye yazalım

                // Cihaz seçiliyse (Tag listesi vardır)
                if (treeView1.SelectedNode?.Tag is Device selectedDevice)
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        // Tag ismini ilk kolondan alıp eşleştirelim
                        string tagName = row.Cells[0].Value?.ToString();
                        var tag = selectedDevice.Tags.FirstOrDefault(t => t.Name == tagName);

                        if (tag != null)
                        {
                            row.Cells["Value"].Value = tag.Value;
                            row.Cells["Updated"].Value = tag.LastUpdated;
                        }
                    }
                }
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 1. Önce Timer'ı durdur (Arka planda yeni okuma yapmaya çalışmasın)
            if (timer1 != null)
            {
                timer1.Stop();

                _opcServer?.Stop();

                foreach (var client in _deviceConnections.Values)
                {
                    try { client.Dispose(); } catch { }
                }
                _deviceConnections.Clear();
            }

            // 2. Açık olan tüm Modbus bağlantılarını tek tek kapat
            if (_deviceConnections != null)
            {
                foreach (var client in _deviceConnections.Values)
                {
                    try
                    {
                        // Bağlantıyı güvenli bir şekilde sonlandır
                        client.Dispose();
                    }
                    catch (Exception)
                    {
                        // Kapanırken hata alırsak yutalım, programın kapanmasına engel olmasın
                    }
                }
                _deviceConnections.Clear(); // Listeyi temizle
            }
        }

        private void treeView1_NodeMouseDoubleClick_1(object sender, TreeNodeMouseClickEventArgs e)
        {
            // 1. KANAL DÜZENLEME
            if (e.Node.Tag is Channel ch)
            {
                // Formu 'ch' parametresiyle açıyoruz (Edit Modu)
                var f = new AddChannelForm(ch);
                if (f.ShowDialog() == DialogResult.OK)
                {
                    // Form kapandı, nesne güncellendi. Şimdi DB'ye yazalım.
                    db.UpdateChannel(f.ChannelData);

                    // Ağacı yenile (İsim değişmiş olabilir)
                    LoadTreeView();
                    MessageBox.Show("Kanal güncellendi.");
                }
            }
            // 2. CİHAZ DÜZENLEME
            else if (e.Node.Tag is Device dev)
            {
                var f = new AddDeviceForm(dev);
                if (f.ShowDialog() == DialogResult.OK)
                {
                    // Eğer IP/Port değiştiyse açık bağlantıyı sıfırlamak iyi olur
                    if (_deviceConnections.ContainsKey(dev.DeviceId))
                    {
                        _deviceConnections[dev.DeviceId].Dispose();
                        _deviceConnections.Remove(dev.DeviceId);
                    }

                    db.UpdateDevice(f.DeviceData);
                    LoadTreeView();
                    MessageBox.Show("Cihaz güncellendi.");
                }
            }
            // 3. TAG DÜZENLEME
            else if (e.Node.Tag is Tag tag)
            {
                var f = new AddTagForm(tag);
                if (f.ShowDialog() == DialogResult.OK)
                {
                    db.UpdateTag(f.TagData);
                    LoadTreeView();
                    MessageBox.Show("Tag güncellendi.");
                }
            }
        }
    }
}
