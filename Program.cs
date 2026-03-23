using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketLogger {
    class Program {
        static async Task Main(string[] args) {
            string wsUri = "ws://relay:8081";
            using (ClientWebSocket client = new ClientWebSocket()) {
                try {
                    await client.ConnectAsync(new Uri(wsUri), CancellationToken.None);
                    Console.WriteLine("Connected to ELK Relay!");

                    while (true) {
                        string logJson = "{\"level\":\"INFO\", \"message\":\"Hello from .NET 4.8\", \"timestamp\":\"" + DateTime.UtcNow.ToString("o") + "\"}";
                        var bytes = Encoding.UTF8.GetBytes(logJson);
                        await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                        
                        Console.WriteLine("Log sent.");
                        await Task.Delay(5000);
                    }
                } catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
            }
        }
    }
}
