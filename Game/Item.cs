namespace Game;

public class Item
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(value));
    }
}