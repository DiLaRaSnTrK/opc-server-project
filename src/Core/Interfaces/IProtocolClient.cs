// <copyright file="IProtocolClient.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Models;

    /// <summary>
    /// Protokol istemcileri için okuma/bağlantı arayüzü.
    /// </summary>
    public interface IProtocolClient : IDisposable
    {
        /// <summary>Veri alındığında tetiklenir.</summary>
        event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>Cihaza bağlanır.</summary>
        Task ConnectAsync(CancellationToken ct = default);

        /// <summary>Bağlantıyı kapatır.</summary>
        Task DisconnectAsync();

        /// <summary>Belirtilen tag'i okur.</summary>
        Task<ReadResult> ReadTagAsync(Tag tag, CancellationToken ct = default);
    }
}
