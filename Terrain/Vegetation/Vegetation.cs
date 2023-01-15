using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Vegetation : TerrainAspect
{
    public HashSet<Landform> AllowedLandforms { get; private set; }
    public float MinMoisture { get; private set; }
    public Color Color { get; private set; }
    public string Name { get; private set; }

    public Vegetation(HashSet<Landform> allowedLandforms, float minMoisture, Color color, string name)
    {
        AllowedLandforms = allowedLandforms;
        MinMoisture = minMoisture;
        Color = color;
        Name = name;
    }

    public override Func<GeologyPolygon, HashSet<GeologyPolygon>, List<Triangle>> BuildTrisForPoly
    {
        get;
        protected set;
    }
}