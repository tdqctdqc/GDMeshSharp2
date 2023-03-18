
using System.Collections.Generic;
using Godot;

public abstract class Item : IModel
{
    public string Name { get; private set; }
    public Color Color { get; private set; }
    public Icon ResIcon { get; } 
    protected Item(string name, Color color)
    {
        Name = name;
        Color = color;
        ResIcon = Icon.Create(Name, Icon.AspectRatio.Square);
    }
}
