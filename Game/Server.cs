using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class Server
    {
        private TcpListener _server;
        private Map _map;
        private readonly string _accountsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "accounts");

        public Server(int port)
        {
            _server = new TcpListener(IPAddress.Any, port);
            _map = new Map();
            
            try
            {
                Logger.Log("Spouštění serveru a načítání herního světa...");
                WorldLoader.Load("world.json", _map);
                Logger.Log("Herní svět byl úspěšně načten.");

                if (!Directory.Exists(_accountsDir))
                {
                    Directory.CreateDirectory(_accountsDir);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Kritická chyba při načítání světa: {ex.Message}");
                Environment.Exit(1); 
            }
        }

        public async Task StartAsync()
        {
            _server.Start();
            Logger.Log("Server naslouchá na portu " + ((IPEndPoint)_server.LocalEndpoint).Port);

            _ = Task.Run(StartNpcMovementAsync);

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

                while (player == null)
                {
                    await writer.WriteLineAsync("\nVítej v MUDu!\n1. Přihlásit se\n2. Registrovat nového hráče\nVyber možnost (1/2):");
                    string? choice = await reader.ReadLineAsync();
                    if (choice == null) return;
                    choice = choice.Trim();

                    if (choice == "1")
                    {
                        await writer.WriteLineAsync("Zadej jméno:");
                        string? username = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(username)) continue;
                        username = username.Trim();

                        await writer.WriteLineAsync("Zadej heslo:");
                        string? password = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(password)) continue;

                        string hash = HashPassword(password);
                        PlayerState? state = LoadPlayerState(username);

                        if (state != null && state.PasswordHash == hash)
                        {
                            player = new Player
                            {
                                Name = state.Username,
                                PasswordHash = state.PasswordHash,
                                X = state.X,
                                Y = state.Y,
                                Writer = writer
                            };
                            player.AnsweredNpcs = state.AnsweredNpcs ?? new List<string>();

                            foreach (var saved in state.Inventory)
                            {
                                if (saved.KeyId.HasValue)
                                {
                                    player.Inventory.Add(new Key { Name = saved.Name, Id = saved.KeyId.Value });
                                }
                                else
                                {
                                    player.Inventory.Add(new Item { Name = saved.Name });
                                }
                            }
                            Logger.Log($"Hráč {player.Name} se úspěšně přihlásil.");
                        }
                        else
                        {
                            await writer.WriteLineAsync("Neplatné jméno nebo heslo.");
                        }
                    }
                    else if (choice == "2")
                    {
                        await writer.WriteLineAsync("Zadej nové jméno:");
                        string? username = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(username)) continue;
                        username = username.Trim();

                        if (username.Any(c => !char.IsLetterOrDigit(c)))
                        {
                            await writer.WriteLineAsync("Jméno může obsahovat pouze písmena a číslice.");
                            continue;
                        }

                        string filePath = Path.Combine(_accountsDir, $"{username.ToLowerInvariant()}.json");
                        if (File.Exists(filePath))
                        {
                            await writer.WriteLineAsync("Uživatel s tímto jménem již existuje.");
                            continue;
                        }

                        await writer.WriteLineAsync("Zadej nové heslo:");
                        string? password = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(password)) continue;

                        string hash = HashPassword(password);
                        player = new Player
                        {
                            Name = username,
                            PasswordHash = hash,
                            X = 0,
                            Y = 0,
                            Writer = writer
                        };

                        SavePlayerState(player);
                        Logger.Log($"Hráč {player.Name} se úspěšně registroval.");
                    }
                }

                _map.AddPlayer(player);
                var currentRoom = _map.GetPlayersCurrentRoom(player);
                if (currentRoom != null)
                {
                    var otherPlayers = currentRoom.Players.Where(p => p != player).ToList();
                    foreach (var p in otherPlayers)
                    {
                        p.Writer.WriteLine($"\nHráč {player.Name} vstoupil do hry.");
                    }
                    currentRoom.Players.Add(player);
                }

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
            catch (Exception ex)
            {
                Logger.Log($"Chyba klienta {player?.Name ?? "Neznámý"}: {ex.Message}");
            }
            finally
            {
                if (player != null)
                {
                    Logger.Log($"Hráč {player.Name} se odpojil.");
                    SavePlayerState(player);
                    _map.RemovePlayer(player);
                    var currentRoom = _map.GetPlayersCurrentRoom(player);
                    if (currentRoom != null)
                    {
                        currentRoom.Players.Remove(player);
                        var otherPlayers = currentRoom.Players.ToList();
                        foreach (var p in otherPlayers)
                        {
                            p.Writer.WriteLine($"\nHráč {player.Name} opustil hru.");
                        }
                    }
                }
                client.Close();
            }
        }

        private string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        private void SavePlayerState(Player player)
        {
            try
            {
                if (!Directory.Exists(_accountsDir))
                {
                    Directory.CreateDirectory(_accountsDir);
                }

                var state = new PlayerState
                {
                    Username = player.Name,
                    PasswordHash = player.PasswordHash,
                    X = player.X,
                    Y = player.Y,
                    Inventory = player.Inventory.Select(item => new SavedItem
                    {
                        Name = item.Name,
                        KeyId = item is Key key ? key.Id : null
                    }).ToList(),
                    AnsweredNpcs = player.AnsweredNpcs
                };

                string filePath = Path.Combine(_accountsDir, $"{player.Name.ToLowerInvariant()}.json");
                string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
                Logger.Log($"Stav hráče {player.Name} byl úspěšně uložen.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Chyba při ukládání stavu hráče {player.Name}: {ex.Message}");
            }
        }

        private PlayerState? LoadPlayerState(string username)
        {
            try
            {
                string filePath = Path.Combine(_accountsDir, $"{username.ToLowerInvariant()}.json");
                if (!File.Exists(filePath))
                {
                    return null;
                }

                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<PlayerState>(json);
            }
            catch (Exception ex)
            {
                Logger.Log($"Chyba při načítání stavu hráče {username}: {ex.Message}");
                return null;
            }
        }

        private async Task StartNpcMovementAsync()
        {
            var random = new Random();
            while (true)
            {
                await Task.Delay(15000);

                var movements = new List<(Npc npc, Room fromRoom, Room toRoom)>();
                var rooms = _map.GetAllRooms();
                foreach (var room in rooms)
                {
                    var npcs = room.Npcs.ToList();
                    foreach (var npc in npcs)
                    {
                        if (random.Next(2) == 0) continue;

                        var pathways = _map.GetPathwaysFromRoom(room);
                        var openPathways = pathways.Where(p => p.UnlockId == 0).ToList();
                        if (openPathways.Any())
                        {
                            var chosenPath = openPathways[random.Next(openPathways.Count)];
                            var nextRoom = chosenPath.Room1 == room ? chosenPath.Room2 : chosenPath.Room1;
                            movements.Add((npc, room, nextRoom));
                        }
                    }
                }

                foreach (var m in movements)
                {
                    lock (m.fromRoom.Npcs)
                    {
                        if (m.fromRoom.Npcs.Contains(m.npc))
                        {
                            m.fromRoom.Npcs.Remove(m.npc);
                            lock (m.toRoom.Npcs)
                            {
                                m.toRoom.Npcs.Add(m.npc);
                            }

                            var fromPlayers = m.fromRoom.Players.ToList();
                            foreach (var p in fromPlayers)
                            {
                                try
                                {
                                    p.Writer.WriteLine($"\n{m.npc.Name} odešel do místnosti: {m.toRoom.Name}.");
                                }
                                catch {}
                            }

                            var toPlayers = m.toRoom.Players.ToList();
                            foreach (var p in toPlayers)
                            {
                                try
                                {
                                    p.Writer.WriteLine($"\n{m.npc.Name} přišel z místnosti: {m.fromRoom.Name}.");
                                }
                                catch {}
                            }
                        }
                    }
                }
            }
        }
    }
}