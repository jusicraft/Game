namespace Game;

public class Pathway
{
    private Room _room1;
    private Room _room2;
    private int _unlockID;

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
        get => _unlockID;
        set => _unlockID = value;
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