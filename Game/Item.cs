namespace Game;

public class Item
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Effect { get; set; } = string.Empty;
    public string EffectValue { get; set; } = string.Empty;
    public string EffectMessage { get; set; } = string.Empty;
    public bool SingleUse { get; set; }
}