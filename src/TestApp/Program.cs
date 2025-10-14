using System;
using System.Threading.Tasks;
using Infrastructure.Protocols; // ModbusClient
using Core.Interfaces;         // IProtocolClient, ReadResult, DataReceivedEventArgs

class Program
{
    static async Task Main(string[] args)
    {
        // Test cihazı bilgileri (simülasyon)
        string ip = "192.168.1.10";
        int port = 502;

        // ModbusClient oluştur
        var client = new ModbusClient(ip, port);

        // DataReceived eventini yakala
        client.DataReceived += (sender, e) =>
        {
            Console.WriteLine($"[Event] Tag {e.TagId}: {e.Value} ({e.Timestamp:HH:mm:ss})");
        };

        // Bağlan
        await client.ConnectAsync();

        // ReadAsync ile veri oku
        var result = await client.ReadAsync(0, 5); // adres 0, 5 değer oku
        if (result.Success)
        {
            Console.WriteLine("Okunan değerler: " + string.Join(", ", result.Values));
        }
        else
        {
            Console.WriteLine("Okuma başarısız!");
        }

        // Bir süre eventleri görmek için bekle
        Console.WriteLine("10 saniye boyunca eventleri izliyoruz...");
        await Task.Delay(10000); // 10 saniye

        // Bağlantıyı kapat
        await client.DisconnectAsync();
    }
}
