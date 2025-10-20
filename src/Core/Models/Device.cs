using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Device
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Protocol { get; set; } // "Modbus", "DNP3", "S7"
        public string Ip { get; set; }
        public int Port { get; set; }
        public int PollIntervalMs { get; set; }
    }
}
