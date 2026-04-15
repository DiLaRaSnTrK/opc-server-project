// Copyright (c) PlaceholderCompany. All rights reserved.

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
        /// Her dal açıkça (object) olarak döndürülür — switch expression
        /// kullanılmaz çünkü derleyici tüm dalları ortak türe (double) yükseltir.
        /// </summary>
        /// <param name="dataType">Hedef veri tipi.</param>
        /// <param name="rawValue">Ham değer.</param>
        /// <returns>Doğru C# türünde kutulanmış değer.</returns>
        public static object Convert(TagDataType dataType, double rawValue)
        {
            switch (dataType)
            {
                case TagDataType.Bool:
                    // 1.0'a 0.00001'den yakınsa true(1), değilse false(0)
                    return Math.Abs(rawValue - 1) < 0.00001 ? (object)1 : (object)0;

                case TagDataType.Int16:
                    return (object)(short)rawValue;

                case TagDataType.UInt16:
                    return (object)(ushort)rawValue;

                case TagDataType.Int32:
                    return (object)(int)rawValue;

                case TagDataType.UInt32:
                    return (object)(uint)rawValue;

                case TagDataType.Float:
                    return (object)(float)rawValue;

                case TagDataType.Double:
                    return (object)rawValue;

                default:
                    return (object)rawValue;
            }
        }
    }
}
