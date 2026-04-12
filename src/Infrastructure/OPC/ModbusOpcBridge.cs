// <copyright file="ModbusOpcBridge.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Infrastructure.OPC
{
    using Core.Database;
    using Core.Models;
    using Core.Protocols;
    public class ModbusOpcBridge
    {
        private readonly DatabaseService _db;
        private readonly Device _device;
        private readonly List<Tag> _tags;
        private readonly ModbusClientWrapper _modbusClient;
        private CancellationTokenSource _cts;

        public ModbusOpcBridge(DatabaseService db, Device device, List<Tag> tags)
        {
            _db = db;
            _device = device;
            _tags = tags;
            _modbusClient = new ModbusClientWrapper(device);
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => PollLoop(_cts.Token));
            Console.WriteLine($"[Bridge] {_device.Name} başladı.");
        }

        public void Stop()
        {
            _cts?.Cancel();
            _ = _modbusClient.DisconnectAsync();
        }

        private async Task PollLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var tag in _tags)
                {
                    var result = await _modbusClient.ReadTagAsync(tag, token);

                    if (result.Success)
                    {
                        double value = result.Values[0];

                        // ✅ DB güncelle + OPC'ye yaz (UpdateTagValue içinde _tagUpdater çağrılıyor)
                        _db.UpdateTagValue(tag, value);

                        Console.WriteLine($"[{_device.Name}] {tag.Name} = {value}");
                    }
                    else
                    {
                        Console.WriteLine($"[{_device.Name}] {tag.Name} hata: {result.ErrorMessage}");
                    }
                }

                await Task.Delay(1000, token);
            }
        }
    }
}