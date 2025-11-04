using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Device
    {
        public int Id { get; set; }
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
