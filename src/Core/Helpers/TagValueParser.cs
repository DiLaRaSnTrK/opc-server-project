// <copyright file="TagValueParser.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Helpers
{
    using Core.Models;

    /// <summary>
    /// Modbus'tan gelen ham double değeri Tag'in veri tipine dönüştürür.
    /// </summary>
    public static class TagValueParser
    {
        /// <summary>
        /// Ham double değeri belirtilen veri tipine çevirir.
        /// </summary>
        /// <param name="dataType">Hedef veri tipi.</param>
        /// <param name="rawValue">Ham değer.</param>
        /// <returns>Dönüştürülmüş değer.</returns>
        public static object Convert(TagDataType dataType, double rawValue)
        {
            return dataType switch
            {
                TagDataType.Bool => Math.Abs(rawValue - 1) < 0.00001 ? 1 : 0,
                TagDataType.Int16 => (short)rawValue,
                TagDataType.UInt16 => (ushort)rawValue,
                TagDataType.Int32 => (int)rawValue,
                TagDataType.UInt32 => (uint)rawValue,
                TagDataType.Float => (float)rawValue,
                TagDataType.Double => rawValue,
                _ => rawValue,
            };
        }
    }
}
