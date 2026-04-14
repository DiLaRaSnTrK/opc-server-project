// <copyright file="Channel.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Models
{
    public class Channel
    {
        public int ChannelId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // ModbusTCP, DNP3, MQTT gibi protokoller
        public ProtocolType Protocol { get; set; }

        public List<Device> Devices { get; set; } = new();

    }

    public enum ProtocolType
    {
        ModbusTCP = 1,
        ModbusRTU = 2,
        DNP3 = 3,
        MQTT = 4
    }

}
