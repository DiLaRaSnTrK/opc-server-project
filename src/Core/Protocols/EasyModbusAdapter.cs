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
        public int[] ReadHoldingRegisters(int address, int quantity) => _client.ReadHoldingRegisters(address, quantity);
        public int[] ReadInputRegisters(int address, int quantity) => _client.ReadInputRegisters(address, quantity);
        public bool[] ReadCoils(int address, int quantity) => _client.ReadCoils(address, quantity);
        public bool[] ReadDiscreteInputs(int address, int quantity) => _client.ReadDiscreteInputs(address, quantity);
        public void Dispose() => _client?.Disconnect();
    }
}
