namespace Game;

public class Npc
{
    public string Name { get; set; }
    public string Dialog { get; set; }

    public Npc(string name, string dialog)
    {
        Name = name;
        Dialog = dialog;
    }
}