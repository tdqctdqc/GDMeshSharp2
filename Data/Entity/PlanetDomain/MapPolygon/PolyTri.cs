
using System;
using Godot;

public class PolyTri : Triangle
{
    public Landform Landform { get; set; }
    public Vegetation Vegetation { get; set; }

    public PolyTri(Vector2 a, Vector2 b, Vector2 c, Landform landform, Vegetation vegetation)
    : base(a,b,c)
    {
        Landform = landform;
        Vegetation = vegetation;
    }
    public PolyTri(Vector2 a, Vector2 b, Vector2 c)
        : base(a,b,c)
    {
    }
}
