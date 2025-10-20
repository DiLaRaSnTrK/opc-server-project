using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public class ReadResult
    {
        public bool Success { get; set; }
        public double[] Values { get; set; }
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
        Task<ReadResult> ReadAsync(int address, int count, CancellationToken ct = default);
        event EventHandler<DataReceivedEventArgs> DataReceived;
    }
}
