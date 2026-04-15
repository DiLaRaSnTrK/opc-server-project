// <copyright file="DataReceivedEventArgs.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Interfaces
{
    using System;

    /// <summary>Modbus verisi alındığında tetiklenen event argümanları.</summary>
    public class DataReceivedEventArgs : EventArgs
    {
        /// <summary>Güncellenen tag ID'si.</summary>
        public int TagId { get; set; }

        /// <summary>Okunan değer.</summary>
        public double Value { get; set; }

        /// <summary>Okuma zaman damgası.</summary>
        public DateTime Timestamp { get; set; }
    }
}
