using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Modbus adresi (örnek: 3306 veya 43306)
        public int Address { get; set; }

        // Register tipi: HoldingRegister, InputRegister, Coil, DiscreteInput
        public string RegisterType { get; set; }

        // KepserverEX gibi veri tipi
        public TagDataType DataType { get; set; }

        // Son okunan değer
        public object? Value { get; set; }

        // Değerin son okunduğu zaman
        public DateTime? LastUpdated { get; set; }

        // Bağlı olduğu cihaz
        public int DeviceId { get; set; }
        public Device Device { get; set; }
    }

    public enum TagDataType
    {
        Int16,
        UInt16,
        Float,
        Bool
    }
}
