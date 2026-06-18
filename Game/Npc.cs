using System.Collections.Generic;

namespace Game;

public class Npc
{
    public string Name { get; set; }
    public string Dialog { get; set; }
    public List<NpcDialogOption> DialogOptions { get; set; } = new();

    public Npc(string name, string dialog)
    {
        Name = name;
        Dialog = dialog;
    }
}

public class NpcDialogOption
{
    public string Condition { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}