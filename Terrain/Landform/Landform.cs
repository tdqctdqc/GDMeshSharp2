using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Landform 
{
    public string Name { get; private set; }
    public float MinRoughness { get; private set; }
    public Color Color { get; private set; }

    public Landform(string name, float minRoughness, Color color)
    {
        Name = name;
        MinRoughness = minRoughness;
        Color = color;
    }
}