using System;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Game
{
    public static class Command
    {
        public static void Execute(string input, Player player, Map map)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            Logger.Log($"Hráč {player.Name} zadal příkaz: {input}");

            if (!string.IsNullOrEmpty(player.ActiveQuestionNpc))
            {
                string npcName = player.ActiveQuestionNpc;
                player.ActiveQuestionNpc = string.Empty;

                string normalizedAnswer = NormalizeString(input);
                if (CheckNpcAnswer(npcName, normalizedAnswer))
                {
                    player.AnsweredNpcs.Add(npcName);
                    player.Writer.WriteLine($"{npcName} říká: \"Správně!\"");

                    var allNpcNames = map.GetAllNpcNames();
                    bool won = allNpcNames.All(name => player.AnsweredNpcs.Any(a => NormalizeString(a) == NormalizeString(name)));
                    if (won)
                    {
                        player.Writer.WriteLine("\n=== GRATULUJEME! ===");
                        player.Writer.WriteLine("Zodpověděl jsi správně všechny otázky a vyhrál jsi celou hru!");
                        player.Writer.WriteLine("====================\n");
                    }
                }
                else
                {
                    player.Writer.WriteLine($"{npcName} říká: \"Špatně! Zkus to znovu.\"");
                }
                return;
            }

            string cleanedInput = NormalizeString(input);
            string[] parts = cleanedInput.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

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
                    {
                        if (string.IsNullOrWhiteSpace(argument))
                        {
                            player.Writer.WriteLine("Co chceš vzít?");
                            break;
                        }
                        Item? item = currentRoom.Items.FirstOrDefault(i => NormalizeString(i.Name) == argument);
                        if (item == null)
                        {
                            player.Writer.WriteLine("Nic takového tu není.");
                        }
                        else if (player.Inventory.Count >= player.MaxCapacity)
                        {
                            player.Writer.WriteLine("Nemáš místo v inventáři.");
                        }
                        else
                        {
                            currentRoom.Items.Remove(item);
                            player.Inventory.Add(item);
                            player.Writer.WriteLine($"Zvedáš předmět {item.Name}.");
                        }
                    }
                    break;
                case "poloz":
                    {
                        if (string.IsNullOrWhiteSpace(argument))
                        {
                            player.Writer.WriteLine("Co chceš položit?");
                            break;
                        }
                        Item? item = player.Inventory.FirstOrDefault(i => NormalizeString(i.Name) == argument);
                        if (item == null)
                        {
                            player.Writer.WriteLine("Nic takového u sebe nemáš.");
                        }
                        else
                        {
                            player.Inventory.Remove(item);
                            currentRoom.Items.Add(item);
                            player.Writer.WriteLine($"Pokládáš předmět {item.Name}.");
                        }
                    }
                    break;
                case "inventar":
                    if (player.Inventory.Any())
                    {
                        player.Writer.WriteLine("Máš u sebe: " + string.Join(", ", player.Inventory.Select(i => i.Name)));
                    }
                    else
                    {
                        player.Writer.WriteLine("Tvůj inventář je prázdný.");
                    }
                    break;
                case "mluv":
                    {
                        if (string.IsNullOrWhiteSpace(argument))
                        {
                            player.Writer.WriteLine("S kým chceš mluvit?");
                            break;
                        }
                        Npc? npc = currentRoom.Npcs.FirstOrDefault(n => NormalizeString(n.Name) == argument);
                        if (npc != null)
                        {
                            if (player.AnsweredNpcs.Any(a => NormalizeString(a) == NormalizeString(npc.Name)))
                            {
                                string dialogText = npc.Dialog;
                                if (npc.DialogOptions != null && npc.DialogOptions.Any())
                                {
                                    bool hasKey = player.Inventory.Any(i => i is Key);
                                    bool answered = player.AnsweredNpcs.Any(a => NormalizeString(a) == NormalizeString(npc.Name));

                                    var matched = npc.DialogOptions.FirstOrDefault(opt =>
                                        (opt.Condition == "answered" && answered) ||
                                        (opt.Condition == "has_key" && hasKey) ||
                                        (opt.Condition == "no_key" && !hasKey) ||
                                        (opt.Condition == "default" || string.IsNullOrEmpty(opt.Condition))
                                    );
                                    if (matched != null)
                                    {
                                        dialogText = matched.Text;
                                    }
                                }
                                player.Writer.WriteLine($"{npc.Name} říká: \"{dialogText}\"");
                            }
                            else
                            {
                                player.ActiveQuestionNpc = npc.Name;
                                player.Writer.WriteLine($"{npc.Name} tě chce vyzkoušet: \"{GetNpcQuestion(npc.Name)}\"");
                                player.Writer.WriteLine("Napiš svou odpověď:");
                            }
                        }
                        else
                        {
                            player.Writer.WriteLine("Nikoho takového tu nevidím.");
                        }
                    }
                    break;
                case "cesty":
                    {
                        var pathways = map.GetPathwaysFromRoom(currentRoom);
                        if (pathways.Any())
                        {
                            player.Writer.WriteLine("Dostupné cesty:");
                            for (int i = 0; i < pathways.Count; i++)
                            {
                                var path = pathways[i];
                                var dest = path.Room1 == currentRoom ? path.Room2 : path.Room1;
                                string pathName = !string.IsNullOrEmpty(path.Name) ? path.Name : dest.Name;
                                player.Writer.WriteLine($"{i + 1}. {pathName}");
                            }
                        }
                        else
                        {
                            player.Writer.WriteLine("Odtud nevedou žádné cesty.");
                        }
                    }
                    break;
                case "pouzij":
                    {
                        if (string.IsNullOrWhiteSpace(argument))
                        {
                            player.Writer.WriteLine("Co chceš použít?");
                            break;
                        }
                        Item? item = player.Inventory.FirstOrDefault(i => NormalizeString(i.Name) == argument);
                        if (item == null)
                        {
                            player.Writer.WriteLine("Nic takového u sebe nemáš.");
                        }
                        else if (string.IsNullOrEmpty(item.Effect))
                        {
                            player.Writer.WriteLine("Tento předmět nemá žádné využití.");
                        }
                        else
                        {
                            if (item.Effect == "teleport")
                            {
                                string[] coords = item.EffectValue.Split(',');
                                if (coords.Length == 2 && int.TryParse(coords[0], out int tx) && int.TryParse(coords[1], out int ty))
                                {
                                    Room? targetRoom = map.FindRoom(tx, ty);
                                    if (targetRoom != null)
                                    {
                                        if (!string.IsNullOrEmpty(item.EffectMessage))
                                            player.Writer.WriteLine(item.EffectMessage);
                                        else
                                            player.Writer.WriteLine("Byl jsi teleportován!");

                                        var oldRoom = map.GetPlayersCurrentRoom(player);
                                        if (oldRoom != null)
                                        {
                                            var fromPlayers = oldRoom.Players.Where(p => p != player).ToList();
                                            foreach (var p in fromPlayers)
                                            {
                                                p.Writer.WriteLine($"\nHráč {player.Name} zmizel v záblesku světla.");
                                            }
                                            oldRoom.Players.Remove(player);
                                        }

                                        player.X = tx;
                                        player.Y = ty;
                                        targetRoom.Players.Add(player);

                                        var toPlayers = targetRoom.Players.Where(p => p != player).ToList();
                                        foreach (var p in toPlayers)
                                        {
                                            p.Writer.WriteLine($"\nHráč {player.Name} se objevil v záblesku světla.");
                                        }

                                        if (item.SingleUse)
                                            player.Inventory.Remove(item);

                                        Execute("prozkoumej", player, map);
                                    }
                                    else
                                    {
                                        player.Writer.WriteLine("Cílové místo teleportu neexistuje.");
                                    }
                                }
                            }
                            else if (item.Effect == "zprava")
                            {
                                player.Writer.WriteLine(item.EffectMessage);
                                if (item.SingleUse)
                                    player.Inventory.Remove(item);
                            }
                        }
                    }
                    break;
                case "rekni":
                    {
                        if (string.IsNullOrWhiteSpace(argument))
                        {
                            player.Writer.WriteLine("Co chceš říct?");
                            break;
                        }
                        var otherPlayers = currentRoom.Players.Where(p => p != player).ToList();
                        foreach (var p in otherPlayers)
                        {
                            p.Writer.WriteLine($"\n[Místnost] {player.Name} říká: \"{argument}\"");
                        }
                        player.Writer.WriteLine($"Říkáš: \"{argument}\"");
                    }
                    break;
                case "krik":
                    {
                        if (string.IsNullOrWhiteSpace(argument))
                        {
                            player.Writer.WriteLine("Co chceš zakřičet?");
                            break;
                        }
                        var allPlayers = map.GetAllPlayers().Where(p => p != player).ToList();
                        foreach (var p in allPlayers)
                        {
                            p.Writer.WriteLine($"\n[Křik] Hráč {player.Name} křičí: \"{argument}\"");
                        }
                        player.Writer.WriteLine($"Křičíš: \"{argument}\"");
                    }
                    break;
                case "septat":
                    {
                        if (string.IsNullOrWhiteSpace(argument))
                        {
                            player.Writer.WriteLine("Komu chceš šeptat a co?");
                            break;
                        }
                        string[] msgParts = argument.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (msgParts.Length < 2)
                        {
                            player.Writer.WriteLine("Zadej jméno hráče a zprávu.");
                            break;
                        }
                        string targetName = msgParts[0];
                        string msg = msgParts[1];

                        Player? target = map.GetAllPlayers().FirstOrDefault(p => NormalizeString(p.Name) == NormalizeString(targetName));
                        if (target == null)
                        {
                            player.Writer.WriteLine($"Hráč {targetName} není připojen.");
                        }
                        else
                        {
                            target.Writer.WriteLine($"\n[Soukromě] Hráč {player.Name} ti šeptá: \"{msg}\"");
                            player.Writer.WriteLine($"Šeptáš hráči {target.Name}: \"{msg}\"");
                        }
                    }
                    break;
                case "pomoc":
                    player.Writer.WriteLine("=== NÁPOVĚDA ===");
                    player.Writer.WriteLine("jdi <směr>   - Přesune tě jinam (sever, jih, vychod, zapad).");
                    player.Writer.WriteLine("jdi <číslo>  - Přesune tě vybranou cestou.");
                    player.Writer.WriteLine("cesty        - Vypíše dostupné cesty.");
                    player.Writer.WriteLine("prozkoumej   - Rozhlédneš se po místnosti.");
                    player.Writer.WriteLine("vezmi <věc>  - Sebereš předmět.");
                    player.Writer.WriteLine("poloz <věc>  - Odložíš předmět.");
                    player.Writer.WriteLine("pouzij <věc> - Použiješ předmět.");
                    player.Writer.WriteLine("inventar     - Ukáže, co máš u sebe.");
                    player.Writer.WriteLine("mluv <jméno> - Promluvíš s postavou.");
                    player.Writer.WriteLine("rekni <zpr.> - Pošle zprávu všem v místnosti.");
                    player.Writer.WriteLine("krik <zpr.>  - Pošle zprávu všem připojeným hráčům.");
                    player.Writer.WriteLine("septat <hr.> <zpr.> - Pošle soukromou zprávu.");
                    break;
                default:
                    player.Writer.WriteLine("Neznámý příkaz. Napiš 'pomoc' pro seznam příkazů.");
                    break;
            }
        }

        private static string NormalizeString(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    if (!char.IsPunctuation(c))
                    {
                        sb.Append(c);
                    }
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        private static string GetNpcQuestion(string npcName)
        {
            string norm = NormalizeString(npcName);
            if (norm == "strazny")
            {
                return "Který smyčcový hudební nástroj je největší?";
            }
            if (norm == "hudebnik")
            {
                return "Který rakouský hudební skladatel složil operu Kouzelná flétna?";
            }
            return "Máš rád hudbu?";
        }

        private static bool CheckNpcAnswer(string npcName, string normalizedAnswer)
        {
            string norm = NormalizeString(npcName);
            if (norm == "strazny")
            {
                return normalizedAnswer == "kontrabas" || normalizedAnswer == "basa";
            }
            if (norm == "hudebnik")
            {
                return normalizedAnswer == "mozart" || normalizedAnswer == "wolfgang amadeus mozart";
            }
            return normalizedAnswer == "ano";
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
            Room? currentRoom = map.GetPlayersCurrentRoom(player);
            if (currentRoom == null) return;

            if (int.TryParse(direction, out int index))
            {
                var pathways = map.GetPathwaysFromRoom(currentRoom);
                if (index > 0 && index <= pathways.Count)
                {
                    var pathway = pathways[index - 1];
                    if (pathway.UnlockId > 0 && player.FindKeyById(pathway.UnlockId) == null)
                    {
                        string keyName = map.GetKeyName(pathway.UnlockId, player);
                        player.Writer.WriteLine($"Tato cesta je zamčená. K jejímu odemčení potřebuješ předmět: {keyName}.");
                        return;
                    }

                    var targetRoom = pathway.Room1 == currentRoom ? pathway.Room2 : pathway.Room1;

                    var fromPlayers = currentRoom.Players.Where(p => p != player).ToList();
                    foreach (var p in fromPlayers)
                    {
                        p.Writer.WriteLine($"\nHráč {player.Name} odešel do místnosti: {targetRoom.Name}.");
                    }

                    currentRoom.Players.Remove(player);
                    player.X = targetRoom.X;
                    player.Y = targetRoom.Y;
                    targetRoom.Players.Add(player);

                    var toPlayers = targetRoom.Players.Where(p => p != player).ToList();
                    foreach (var p in toPlayers)
                    {
                        p.Writer.WriteLine($"\nHráč {player.Name} vstoupil do místnosti.");
                    }

                    Execute("prozkoumej", player, map);
                    return;
                }
                else
                {
                    player.Writer.WriteLine("Neplatné číslo cesty.");
                    return;
                }
            }

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

            Room? nextRoom = map.FindRoom(player.X + dx, player.Y + dy);
            if (nextRoom != null)
            {
                Pathway? connection = map.FindConnection(currentRoom, nextRoom);
                if (connection != null && connection.UnlockId > 0 && player.FindKeyById(connection.UnlockId) == null)
                {
                    string keyName = map.GetKeyName(connection.UnlockId, player);
                    player.Writer.WriteLine($"Tato cesta je zamčená. K jejímu odemčení potřebuješ předmět: {keyName}.");
                    return;
                }
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