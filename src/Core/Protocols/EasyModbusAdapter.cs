// Copyright (c) OPC Server Project. All rights reserved.

namespace Core.Protocols
{
    using Core.Interfaces;
    using EasyModbus;
    public class EasyModbusAdapter : IModbusClientAdapter
    {
        private readonly ModbusClient _client;
        public EasyModbusAdapter(string ip, int port) => _client = new ModbusClient(ip, port);

        public bool Connected => _client.Connected;
        public int UnitIdentifier { get => _client.UnitIdentifier; set => _client.UnitIdentifier = (byte)value; }
        public void Connect() => _client.Connect();
        public void Disconnect() => _client.Disconnect();
        public int[] ReadHoldingRegisters(int a, int q) => _client.ReadHoldingRegisters(a, q);
        public int[] ReadInputRegisters(int a, int q) => _client.ReadInputRegisters(a, q);
        public bool[] ReadCoils(int a, int q) => _client.ReadCoils(a, q);
        public bool[] ReadDiscreteInputs(int a, int q) => _client.ReadDiscreteInputs(a, q);
        public void Dispose() => _client?.Disconnect();
    }
}
