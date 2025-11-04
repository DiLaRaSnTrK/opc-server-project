using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Interfaces
{
    public class ReadResult
    {
        public bool Success { get; set; }
        public double[] Values { get; set; }
        public string ErrorMessage { get; set; } // ✅ eklendi
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public int TagId { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public interface IProtocolClient : IDisposable
    {
        Task ConnectAsync(CancellationToken ct = default);
        Task DisconnectAsync();
        Task<ReadResult> ReadTagAsync(Core.Models.Tag tag, CancellationToken ct = default);

        event EventHandler<DataReceivedEventArgs> DataReceived;
    }
}


