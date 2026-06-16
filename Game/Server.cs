using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Server
    {
        private TcpListener _server;
        private Map _map;

        public Server(int port)
        {
            _server = new TcpListener(IPAddress.Any, port);
            _map = new Map();
            
            _map = new Map();
            
            try
            {
                WorldLoader.Load("world.json", _map);
                Console.WriteLine("Herní svět byl úspěšně načten z JSONu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kritická chyba při načítání světa: {ex.Message}");
                Environment.Exit(1); 
            }
        }

        public async Task StartAsync()
        {
            _server.Start();
            Console.WriteLine("Server naslouchá na portu " + ((IPEndPoint)_server.LocalEndpoint).Port);

            while (true)
            {
                TcpClient client = await _server.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            Player? player = null;
            try
            {
                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                await writer.WriteLineAsync("Vítej v MUDu! Zadej své jméno:");
                string? name = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(name)) return;

                player = new Player { Name = name, Writer = writer };
                
                //kdyz se hrac spawne
                await writer.WriteLineAsync($"\nVítej, {player.Name}! Nápovědu zobrazíš příkazem 'pomoc'.");
                Command.Execute("prozkoumej", player, _map);

                //gameloop
                while (true)
                {
                    string? input = await reader.ReadLineAsync();
                    if (input == null) break;

                    Command.Execute(input.Trim().ToLower(), player, _map);
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Klient {player?.Name ?? "Neznámý"} se neočekávaně odpojil.");
            }
            finally
            {
                client.Close();
            }
        }
    }
}