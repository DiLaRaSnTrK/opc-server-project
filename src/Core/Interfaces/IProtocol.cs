// Copyright (c) OPC Server Project. All rights reserved.

namespace Core.Interfaces
{
    using System.Threading.Tasks;
    using Core.Models;

    /// <summary>Endüstriyel protokol istemcileri için temel arayüz.</summary>
    public interface IProtocol
    {
        /// <summary>Bağlantı durumunu döndürür.</summary>
        bool IsConnected { get; }

        /// <summary>Cihaza bağlantı kurar.</summary>
        Task<bool> ConnectAsync(Device device);

        /// <summary>Bağlantıyı sonlandırır.</summary>
        Task DisconnectAsync();

        /// <summary>Belirtilen tag'in değerini okur.</summary>
        Task<object?> ReadTagAsync(Tag tag);

        /// <summary>Tag'e değer yazar.</summary>
        Task<bool> WriteTagAsync(Tag tag, object value);
    }
}
