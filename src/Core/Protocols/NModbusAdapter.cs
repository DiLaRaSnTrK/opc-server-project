// Copyright (c) OPC Server Project. All rights reserved.

namespace Core.Protocols
{
    using System.Net.Sockets;
    using Core.Interfaces;
    using NModbus;

    public class NModbusAdapter : IModbusClientAdapter
    {
        private TcpClient _tcpClient;
        private IModbusMaster _master;
        private readonly string _ip;
        private readonly int _port;

        public NModbusAdapter(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public bool Connected => _tcpClient?.Connected ?? false;
        public int UnitIdentifier { get; set; }

        public void Connect()
        {
            _tcpClient = new TcpClient(_ip, _port);
            var factory = new ModbusFactory();
            _master = factory.CreateMaster(_tcpClient);
        }

        public void Disconnect()
        {
            _tcpClient?.Close();
            _tcpClient?.Dispose();
        }

        public int[] ReadHoldingRegisters(int address, int quantity)
        {
            var result = _master.ReadHoldingRegisters((byte)UnitIdentifier, (ushort)address, (ushort)quantity);
            return Array.ConvertAll(result, x => (int)x);
        }

        // EKSİK OLAN VE HATA VEREN METODLARI EKLEDİK:
        public int[] ReadInputRegisters(int address, int quantity)
        {
            var result = _master.ReadInputRegisters((byte)UnitIdentifier, (ushort)address, (ushort)quantity);
            return Array.ConvertAll(result, x => (int)x);
        }

        public bool[] ReadCoils(int address, int quantity)
        {
            return _master.ReadCoils((byte)UnitIdentifier, (ushort)address, (ushort)quantity);
        }

        public bool[] ReadDiscreteInputs(int address, int quantity)
        {
            return _master.ReadInputs((byte)UnitIdentifier, (ushort)address, (ushort)quantity);
        }

        public void Dispose() => Disconnect();
    }
}