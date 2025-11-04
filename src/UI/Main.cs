using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Core.Interfaces;
using Core.Models;
using Core.Protocols;

namespace UI
{
    public partial class Main : Form
    {
        private List<Channel> channelsList;
        private ModbusClientWrapper modbusWrapper;
        public Main()
        {
            InitializeComponent();

            LoadSampleData();
            LoadTreeView();
        }

        private void LoadSampleData()
        {
            channelsList = new List<Channel>
            {
                new Channel
                {
                    Name = "Channel 1",
                    Protocol= ProtocolType.ModbusRTU,
                    Description = "Su pompası sistemi",
                    Devices = new List<Device>
                    {
                        new Device
                        {
                            Name = "Pompa 1",
                            IPAddress = "192.168.1.10",
                            Port = 502,
                            SlaveId = 1,
                            Description = "Ana pompa",
                            Tags = new List<Tag>
                            {
                                new Tag { Name = "Basınç", Address = 40001, DataType = TagDataType.Float, RegisterType = "HoldingRegister", Description = "Pompa basınç değeri", Value = 3.4, LastUpdated = DateTime.Now },
                                new Tag { Name = "Sıcaklık", Address = 40002, DataType = TagDataType.Float, RegisterType = "HoldingRegister", Description = "Motor sıcaklığı", Value = 65.2, LastUpdated = DateTime.Now }
                            }
                        },
                        new Device
                        {
                            Name = "Pompa 2",
                            IPAddress = "192.168.1.11",
                            Port = 502,
                            SlaveId = 2,
                            Description = "Yedek pompa",
                            Tags = new List<Tag>
                            {
                                new Tag { Name = "Basınç", Address = 40005, DataType = TagDataType.Float, RegisterType = "HoldingRegister", Description = "Yedek pompa basıncı", Value = 3.1, LastUpdated = DateTime.Now }
                            }
                        }
                    }
                },
                new Channel
                {
                    Name = "Channel 2",
                    Protocol = ProtocolType.ModbusRTU,
                    Description = "Havalandırma sistemi",
                    Devices = new List<Device>()
                }
            };
        }

        private void LoadTreeView()
        {
            treeView1.Nodes.Clear();

            var rootNode = new TreeNode("Connectivity");

            foreach (var channel in channelsList)
            {
                var channelNode = new TreeNode(channel.Name) { Tag = channel };

                foreach (var device in channel.Devices)
                {
                    var deviceNode = new TreeNode(device.Name) { Tag = device };

                    foreach (var tag in device.Tags)
                    {
                        var tagNode = new TreeNode(tag.Name) { Tag = tag };
                        deviceNode.Nodes.Add(tagNode);
                    }

                    channelNode.Nodes.Add(deviceNode);
                }

                rootNode.Nodes.Add(channelNode);
            }

            treeView1.Nodes.Add(rootNode);
            treeView1.ExpandAll();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            if (e.Node.Text == "Connectivity")
            {
                // --- CONNECTIVITY seçildi ---
                dataGridView1.Columns.Add("ChannelName", "Channel Adı");
                dataGridView1.Columns.Add("Protocol", "Protokol");
                dataGridView1.Columns.Add("Description", "Açıklama");

                foreach (var ch in channelsList)
                {
                    dataGridView1.Rows.Add(ch.Name, ch.Protocol, ch.Description);
                }
            }
            else if (e.Node.Tag is Channel channel)
            {
                // --- CHANNEL seçildi ---
                dataGridView1.Columns.Add("DeviceName", "Cihaz Adı");
                dataGridView1.Columns.Add("IP", "IP Adresi");
                dataGridView1.Columns.Add("Port", "Port");
                dataGridView1.Columns.Add("SlaveId", "Slave ID");
                dataGridView1.Columns.Add("TagCount", "Tag Sayısı");
                dataGridView1.Columns.Add("Description", "Açıklama");

                foreach (var dev in channel.Devices)
                {
                    dataGridView1.Rows.Add(dev.Name, dev.IPAddress, dev.Port, dev.SlaveId, dev.Tags.Count, dev.Description);
                }
            }
            else if (e.Node.Tag is Device device)
            {
                // --- DEVICE seçildi ---
                dataGridView1.Columns.Add("TagName", "Tag Adı");
                dataGridView1.Columns.Add("RegisterType", "Register Türü");
                dataGridView1.Columns.Add("Address", "Adres");
                dataGridView1.Columns.Add("DataType", "Veri Türü");
                dataGridView1.Columns.Add("Value", "Değer");
                dataGridView1.Columns.Add("LastRead", "Son Okunma");
                dataGridView1.Columns.Add("Description", "Açıklama");

                foreach (var tag in device.Tags)
                {
                    dataGridView1.Rows.Add(tag.Name, tag.RegisterType, tag.Address, tag.DataType, tag.Value, tag.LastUpdated, tag.Description);
                }
            }
            else if (e.Node.Tag is Tag tag)
            {
                // --- TAG seçildi ---
                dataGridView1.Columns.Add("Property", "Özellik");
                dataGridView1.Columns.Add("Value", "Değer");

                dataGridView1.Rows.Add("Ad", tag.Name);
                dataGridView1.Rows.Add("Register Türü", tag.RegisterType);
                dataGridView1.Rows.Add("Adres", tag.Address);
                dataGridView1.Rows.Add("Veri Türü", tag.DataType);
                dataGridView1.Rows.Add("Değer", tag.Value);
                dataGridView1.Rows.Add("Son Okunma", tag.LastUpdated);
                dataGridView1.Rows.Add("Açıklama", tag.Description);
            }
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode?.Tag is Device selectedDevice)
            {
                // Cihaz seçilmiş → tüm tag'ları oku
                dataGridView1.Rows.Clear();
                modbusWrapper = new ModbusClientWrapper(selectedDevice);

                try
                {
                    await modbusWrapper.ConnectAsync();

                    foreach (var tag in selectedDevice.Tags)
                    {
                        var result = await modbusWrapper.ReadTagAsync(tag);

                        if (result.Success)
                        {
                            dataGridView1.Rows.Add(tag.Name, result.Values[0]);
                        }
                        else
                        {
                            dataGridView1.Rows.Add(tag.Name, "Hata: " + result.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Modbus bağlantı hatası: " + ex.Message);
                }
                finally
                {
                    await modbusWrapper.DisconnectAsync();
                }
            }
            else if (treeView1.SelectedNode?.Tag is Tag selectedTag)
            {
                // Sadece bir tag seçilmiş → sadece onu oku
                dataGridView1.Rows.Clear();

                var parentDevice = treeView1.SelectedNode.Parent?.Tag as Device;
                if (parentDevice == null)
                {
                    MessageBox.Show("Tag'ın bağlı olduğu cihaz bulunamadı!");
                    return;
                }

                modbusWrapper = new ModbusClientWrapper(parentDevice);

                try
                {
                    await modbusWrapper.ConnectAsync();

                    var result = await modbusWrapper.ReadTagAsync(selectedTag);

                    if (result.Success)
                    {
                        dataGridView1.Rows.Add(selectedTag.Name, result.Values[0]);
                    }
                    else
                    {
                        dataGridView1.Rows.Add(selectedTag.Name, "Hata: " + result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Modbus bağlantı hatası: " + ex.Message);
                }
                finally
                {
                    await modbusWrapper.DisconnectAsync();
                }
            }
            else
            {
                MessageBox.Show("Lütfen bir cihaz veya tag seçin!");
            }
        }
    }
}
