namespace Game
{
    public static class Command
    {
        public static void Execute(string input, Player player, Map map)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            string[] parts = input.Split(' ', 2);
            string command = parts[0];
            string argument = parts.Length > 1 ? parts[1] : string.Empty;

            Room? currentRoom = map.GetPlayersCurrentRoom(player);
            if (currentRoom == null) return;

            switch (command)
            {
                case "jdi":
                    HandleMovement(argument, player, map);
                    break;
                case "prozkoumej":
                    HandleLook(currentRoom, player);
                    break;
                case "vezmi":
                    player.Writer.WriteLine($"Zvedáš předmět {argument}.");
                    break;
                case "poloz":
                    player.Writer.WriteLine($"Pokládáš předmět {argument}.");
                    break;
                case "inventar":
                    player.Writer.WriteLine("Máš u sebe: ...");
                    break;
                case "mluv":
                    Npc? npc = currentRoom.Npcs.FirstOrDefault(n => n.Name.ToLower() == argument);
                    if (npc != null)
                        player.Writer.WriteLine($"{npc.Name} říká: \"{npc.Dialog}\"");
                    else
                        player.Writer.WriteLine("Nikoho takového tu nevidím.");
                    break;
                case "pomoc":
                    player.Writer.WriteLine("=== NÁPOVĚDA ===");
                    player.Writer.WriteLine("jdi <směr>   - Přesune tě jinam (sever, jih, vychod, zapad).");
                    player.Writer.WriteLine("prozkoumej   - Rozhlédneš se po místnosti.");
                    player.Writer.WriteLine("vezmi <věc>  - Sebereš předmět.");
                    player.Writer.WriteLine("poloz <věc>  - Odložíš předmět.");
                    player.Writer.WriteLine("inventar     - Ukáže, co máš u sebe.");
                    player.Writer.WriteLine("mluv <jméno> - Promluvíš s postavou.");
                    break;
                default:
                    player.Writer.WriteLine("Neznámý příkaz. Napiš 'pomoc' pro seznam příkazů.");
                    break;
            }
        }

        private static void HandleLook(Room room, Player player)
        {
            player.Writer.WriteLine($"--- {room.Name} ---");
            player.Writer.WriteLine(room.Description);
            
            if (room.Items.Any())
                player.Writer.WriteLine("Leží tu: " + string.Join(", ", room.Items.Select(i => i.Name)));
            
            if (room.Npcs.Any())
                player.Writer.WriteLine("Stojí tu: " + string.Join(", ", room.Npcs.Select(n => n.Name)));
            
            var otherPlayers = room.Players.Where(p => p != player).Select(p => p.Name).ToList();
            if (otherPlayers.Any())
                player.Writer.WriteLine("Ostatní hráči: " + string.Join(", ", otherPlayers));
        }

        private static void HandleMovement(string direction, Player player, Map map)
        {
            int dx = 0, dy = 0;
            switch (direction)
            {
                case "sever": dy = 1; break;
                case "jih": dy = -1; break;
                case "vychod": dx = 1; break;
                case "zapad": dx = -1; break;
                default:
                    player.Writer.WriteLine("Neznámý směr. Zkus: sever, jih, vychod, zapad.");
                    return;
            }

            if (map.Move(dx, dy, player))
            {
                Execute("prozkoumej", player, map);
            }
            else
            {
                player.Writer.WriteLine("Tímto směrem nemůžeš jít.");
            }
        }
    }
}