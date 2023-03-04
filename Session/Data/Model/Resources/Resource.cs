
using Godot;

public class Resource : IModel
{
    public string Name { get; private set; }
    public Color Color { get; private set; }

    public Resource(string name, Color color)
    {
        Name = name;
        Color = color;
    }
}
