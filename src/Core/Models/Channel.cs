using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Channel
    {
        public int Id { get; set; }
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
