using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Landform : TerrainAspect
{
    public string Name { get; private set; }
    public float MinRoughness { get; private set; }
    public Color Color { get; private set; }

    public Landform(string name, float minRoughness, Color color, Func<GeologyPolygon, HashSet<GeologyPolygon>, List<Triangle>> BuildTrisForPoly)
    {
        Name = name;
        MinRoughness = minRoughness;
        Color = color;
    }

    public override Func<GeologyPolygon, HashSet<GeologyPolygon>, List<Triangle>> BuildTrisForPoly
    {
        get;
        protected set;
    }
}