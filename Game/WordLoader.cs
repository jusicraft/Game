using System;
using System.IO;
using System.Text.Json;

namespace Game
{
    public static class WorldLoader
    {
        public static void Load(string filePath, Map map)
        {
            string fullPath = filePath;
            if (!Path.IsPathRooted(filePath))
            {
                if (!File.Exists(fullPath))
                {
                    fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
                }
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Soubor s herním světem nenalezen: {filePath} (hledáno také v {fullPath})");
            }

            string json = File.ReadAllText(fullPath);
            WorldData? data = JsonSerializer.Deserialize<WorldData>(json);

            if (data == null)
            {
                throw new InvalidOperationException("Nepodařilo se načíst data herního světa.");
            }
            
            foreach (var roomData in data.Rooms)
            {
                var room = new Room
                {
                    X = roomData.X,
                    Y = roomData.Y,
                    Name = roomData.Name,
                    Description = roomData.Description
                };

                foreach (var itemData in roomData.Items)
                {
                    if (itemData.KeyId.HasValue)
                    {
                        room.Items.Add(new Key { Name = itemData.Name, Id = itemData.KeyId.Value });
                    }
                    else
                    {
                        room.Items.Add(new Item { Name = itemData.Name });
                    }
                }

                foreach (var npcData in roomData.Npcs)
                {
                    room.Npcs.Add(new Npc(npcData.Name, npcData.Dialog));
                }

                map.AddRoom(room);
            }
            
            foreach (var pathData in data.Pathways)
            {
                Room? room1 = map.FindRoom(pathData.Room1X, pathData.Room1Y);
                Room? room2 = map.FindRoom(pathData.Room2X, pathData.Room2Y);

                if (room1 != null && room2 != null)
                {
                    var pathway = new Pathway
                    {
                        Room1 = room1,
                        Room2 = room2,
                        UnlockId = pathData.UnlockId
                    };
                    map.AddPathway(pathway);
                }
                else
                {
                    Console.WriteLine($"Varování: Cesta propojuje neexistující místnosti ({pathData.Room1X},{pathData.Room1Y}) a ({pathData.Room2X},{pathData.Room2Y}).");
                }
            }
        }
    }
}