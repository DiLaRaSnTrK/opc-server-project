// Copyright (c) OPC Server Project. All rights reserved.

namespace Core.Interfaces
{
    public interface IModbusClientAdapter : IDisposable
    {
        bool Connected { get; }
        void Connect();
        void Disconnect();
        int UnitIdentifier { get; set; }
        int[] ReadHoldingRegisters(int address, int quantity);
        int[] ReadInputRegisters(int address, int quantity);
        bool[] ReadCoils(int address, int quantity);
        bool[] ReadDiscreteInputs(int address, int quantity);
    }
}
