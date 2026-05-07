namespace Game;

public class Room
{
    private int _x;
    private int _y;
    private List<Item> _items;
    
    public int X
    {
        get => _x;
        set => _x = value;
    }

    public int Y
    {
        get => _y;
        set => _y = value;
    }
}