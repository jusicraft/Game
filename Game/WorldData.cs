using System.Collections.Generic;

namespace Game
{
    public class WorldData
    {
        public List<RoomData> Rooms { get; set; } = new();
        public List<PathwayData> Pathways { get; set; } = new();
    }

    public class RoomData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ItemData> Items { get; set; } = new();
        public List<NpcData> Npcs { get; set; } = new();
    }

    public class ItemData
    {
        public string Name { get; set; } = string.Empty;
        public int? KeyId { get; set; }
    }

    public class NpcData
    {
        public string Name { get; set; } = string.Empty;
        public string Dialog { get; set; } = string.Empty;
    }

    public class PathwayData
    {
        public int Room1X { get; set; }
        public int Room1Y { get; set; }
        public int Room2X { get; set; }
        public int Room2Y { get; set; }
        public int UnlockId { get; set; }
    }
}