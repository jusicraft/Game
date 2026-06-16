namespace Game;

public class Pathway
{
    private string _name = string.Empty;
    private Room _room1 = null!;
    private Room _room2 = null!;
    private int _unlockId;

    public string Name
    {
        get => _name;
    }

    public Room Room1
    {
        get => _room1;
        set => _room1 = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Room Room2
    {
        get => _room2;
        set => _room2 = value ?? throw new ArgumentNullException(nameof(value));
    }

    public int UnlockId
    {
        get => _unlockId;
        set => _unlockId = value;
    }

    public bool HasRooms(Room room1, Room room2)
    {
        if (_room1 == room1 && _room2 == room2 || _room1 == room2 && _room2 == room1)
        {
            return true;
        }
        return false;
    }
}