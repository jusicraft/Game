using System.Collections.Generic;

namespace Game
{
    public class PlayerState
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public List<SavedItem> Inventory { get; set; } = new();
        public List<string> AnsweredNpcs { get; set; } = new();
    }

    public class SavedItem
    {
        public string Name { get; set; } = string.Empty;
        public int? KeyId { get; set; }
    }
}
