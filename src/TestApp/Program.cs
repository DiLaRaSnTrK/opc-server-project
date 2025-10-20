using Infrastructure.Protocols;

string plcIp = "192.168.1.10"; // PLC IP
int plcPort = 502;             // Modbus TCP port

var client = new ModbusClient(plcIp, plcPort);

client.DataReceived += (s, e) =>
{
    Console.WriteLine($"[Event] Tag {e.TagId}: {e.Value} ({e.Timestamp:HH:mm:ss})");
};

await client.ConnectAsync();
var result = await client.ReadAsync(0, 5);

if (result.Success)
    Console.WriteLine($"Okunan değerler: {string.Join(", ", result.Values)}");

await client.DisconnectAsync();
