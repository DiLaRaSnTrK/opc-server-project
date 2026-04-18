// Copyright (c) OPC Server Project. All rights reserved.

namespace Infrastructure.OPC
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Database;
    using Core.Models;
    using Core.Protocols;

    /// <summary>Modbus verilerini OPC UA node'larına aktaran köprü.</summary>
    public class ModbusOpcBridge
    {
        private readonly DatabaseService db;
        private readonly Device device;
        private readonly List<Tag> tags;
        private readonly ModbusClientWrapper modbusClient;
        private CancellationTokenSource? cts;

        /// <summary>Initializes a new instance of the <see cref="ModbusOpcBridge"/> class.</summary>
        public ModbusOpcBridge(DatabaseService db, Device device, List<Tag> tags)
        {
            this.db = db;
            this.device = device;
            this.tags = tags;
            this.modbusClient = new ModbusClientWrapper(device);
        }

        /// <summary>Polling döngüsünü başlatır.</summary>
        public void Start()
        {
            this.Stop();
            this.cts = new CancellationTokenSource();
            _ = Task.Run(() => this.PollLoop(this.cts.Token));
            Console.WriteLine($"[Bridge] {this.device.Name} başladı.");
        }

        /// <summary>Polling döngüsünü durdurur.</summary>
        public void Stop()
        {
            if (this.cts != null)
            {
                this.cts.Cancel();
                this.cts.Dispose();
                this.cts = null;
            }

            if (this.modbusClient != null)
            {
                _ = this.modbusClient.DisconnectAsync();
            }
        }

        private async Task PollLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var tag in this.tags)
                {
                    var result = await this.modbusClient.ReadTagAsync(tag, token);
                    if (result.Success)
                    {
                        double value = result.Values[0];
                        this.db.UpdateTagValue(tag, value);
                        Console.WriteLine($"[{this.device.Name}] {tag.Name} = {value}");
                    }
                    else
                    {
                        Console.WriteLine($"[{this.device.Name}] {tag.Name} hata: {result.ErrorMessage}");
                    }
                }

                await Task.Delay(1000, token);
            }
        }
    }
}
