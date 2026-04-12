// <copyright file="IProtocol.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Core.Interfaces
{
    using Core.Models;
    public interface IProtocol
    {
        /// <summary>
        /// Cihaza bağlantı kurar.
        /// </summary>
        Task<bool> ConnectAsync(Device device);

        /// <summary>
        /// Bağlantıyı sonlandırır.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Belirtilen tag’in değerini okur.
        /// </summary>
        Task<object?> ReadTagAsync(Tag tag);

        /// <summary>
        /// Tag’e değer yazar (destekleyen protokoller için).
        /// </summary>
        Task<bool> WriteTagAsync(Tag tag, object value);

        /// <summary>
        /// Bağlantı durumu (true = bağlı)
        /// </summary>
        bool IsConnected { get; }
    }
}
