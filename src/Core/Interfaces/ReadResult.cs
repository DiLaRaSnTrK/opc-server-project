// Copyright (c) PlaceholderCompany. All rights reserved.

namespace Core.Interfaces
{
    using System;

    /// <summary>Modbus okuma işleminin sonucunu taşır.</summary>
    public class ReadResult
    {
        /// <summary>Okuma başarılı mı?</summary>
        public bool Success { get; set; }

        /// <summary>Okunan değer dizisi.</summary>
        public double[] Values { get; set; } = Array.Empty<double>();

        /// <summary>Hata durumunda açıklama.</summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
