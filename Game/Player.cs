namespace Game;

public class Player
{
    private int _x;
    private int _y;
    
    private List<Item> _inventory;

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

    public Key? FindKeyById(int id)
    {
        foreach (Item item in _inventory)
        {
            if (item is Key key)
            {
                if (key.Id == id)
                {
                    return key;
                }
            }
        }
        return null;
    }
}