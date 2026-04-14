// <copyright file="Device.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Models
{
    public class Device
    {
        public int DeviceId { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        // Bağlantı bilgileri
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public byte SlaveId { get; set; }

        // İlişkiler
        public int ChannelId { get; set; }
        public Channel Channel { get; set; }

        public List<Tag> Tags { get; set; } = new();
    }

}
