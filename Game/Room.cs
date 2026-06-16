namespace Game;
using System.Collections.Generic;

public class Room
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; } = "Neznámá místnost";
    public string Description { get; set; } = "Zde nic není.";
    
    public List<Item> Items { get; set; } = new List<Item>();
    public List<Npc> Npcs { get; set; } = new List<Npc>();
    public List<Player> Players { get; set; } = new List<Player>();
}