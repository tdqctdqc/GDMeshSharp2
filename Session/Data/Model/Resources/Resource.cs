
using System.Collections.Generic;
using Godot;

public abstract class Resource : IModel
{
    public string Name { get; private set; }
    public Color Color { get; private set; }

    protected Resource(string name, Color color)
    {
        Name = name;
        Color = color;
    }

}
