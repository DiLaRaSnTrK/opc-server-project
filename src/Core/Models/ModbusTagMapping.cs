// Copyright (c) OPC Server Project. All rights reserved.

namespace Core.Models
{
    /// <summary>Modbus tag adres eşleştirme modeli.</summary>
    public class ModbusTagMapping
    {
        /// <summary>Tag adı.</summary>
        public string TagName { get; set; } = default!;

        /// <summary>Modbus register adresi.</summary>
        public ushort Address { get; set; }

        /// <summary>Veri tipi.</summary>
        public string DataType { get; set; } = "double";
    }
}
