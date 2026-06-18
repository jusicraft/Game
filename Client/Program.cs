using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("=== MUD Klient ===");
            Console.Write("Zadej IP adresu serveru (výchozí 127.0.0.1): ");
            string? ip = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";

            Console.Write("Zadej port serveru (výchozí 65526): ");
            string? portStr = Console.ReadLine();
            if (!int.TryParse(portStr, out int port)) port = 65526;

            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(ip, port);
                Console.WriteLine("Připojeno k serveru.");

                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                Task readTask = Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            string? line = await reader.ReadLineAsync();
                            if (line == null) break;
                            Console.WriteLine(line);
                        }
                    }
                    catch
                    {
                    }
                    Console.WriteLine("Spojení se serverem bylo ukončeno.");
                    Environment.Exit(0);
                });

                while (true)
                {
                    string? input = Console.ReadLine();
                    if (input == null) break;
                    await writer.WriteLineAsync(input);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba připojení: {ex.Message}");
            }
        }
    }
}
