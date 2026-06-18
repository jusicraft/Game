namespace Game;

public class Player
{
    private string _name = string.Empty;
    private int _x;
    private int _y;
    
    private List<Item> _inventory = new List<Item>();
    private StreamWriter _writer = null!;

    public List<Item> Inventory => _inventory;

    public string PasswordHash { get; set; } = string.Empty;

    public List<string> AnsweredNpcs { get; set; } = new();

    public string ActiveQuestionNpc { get; set; } = string.Empty;

    public string Name
    {
        get => _name;
        set  => _name = value;
    }
    
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

    public StreamWriter Writer
    {
        get => _writer; 
        set  => _writer = value;
    }

    public int MaxCapacity { get; set; } = 5;
}