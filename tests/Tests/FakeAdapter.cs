// <copyright file="FakeAdapter.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Tests
{
    using System;
    using Core.Interfaces;

    /// <summary>
    /// IModbusClientAdapter'ın test implementasyonu.
    /// Gerçek ağ bağlantısı gerektirmez — istenen davranışı simüle eder.
    /// </summary>
    internal sealed class FakeAdapter : IModbusClientAdapter
    {
        private readonly bool connectSucceeds;
        private readonly int[]? holdingRegisters;
        private readonly bool[]? coils;
        private readonly Exception? readException;

        public FakeAdapter(
            bool connectSucceeds = true,
            int[]? holdingRegisters = null,
            bool[]? coils = null,
            Exception? readException = null)
        {
            this.connectSucceeds = connectSucceeds;
            this.holdingRegisters = holdingRegisters;
            this.coils = coils;
            this.readException = readException;
        }

        public bool Connected { get; private set; }

        // HATA ÇÖZÜMÜ 1: UnitIdentifier tipi int olmalı (Arayüz öyle bekliyor)
        public int UnitIdentifier { get; set; }

        public void Connect()
        {
            if (!this.connectSucceeds)
            {
                throw new InvalidOperationException("Bağlantı başarısız (fake)");
            }

            this.Connected = true;
        }

        public void Disconnect() => this.Connected = false;

        // HATA ÇÖZÜMÜ 2: IDisposable arayüzünden gelen Dispose metodu eklenmeli
        public void Dispose()
        {
            this.Disconnect();
        }

        public int[] ReadHoldingRegisters(int addr, int qty)
        {
            if (this.readException != null)
            {
                throw this.readException;
            }

            return this.holdingRegisters ?? new[] { 42 };
        }

        public int[] ReadInputRegisters(int addr, int qty) =>
            this.holdingRegisters ?? new[] { 99 };

        public bool[] ReadCoils(int addr, int qty) =>
            this.coils ?? new[] { true };

        public bool[] ReadDiscreteInputs(int addr, int qty) =>
            this.coils ?? new[] { false };
    }
}
